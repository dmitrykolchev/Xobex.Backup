using Microsoft.Win32.SafeHandles;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Ioctl;
using static Windows.Win32.PInvoke;

namespace PartBackup;

public struct VolumeClusterInfo
{
    public uint bytesPerSector;
    public uint sectorsPerCluster;
    public long totalClusters;
};


[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class VolumeBackupEngine
{
    public static bool QueryVolumeClusterInfo(SafeFileHandle hVolume, string volumePath, ref VolumeClusterInfo outInfo)
    {
        // 1. Попытка получить точные данные NTFS напрямую через дескриптор устройства
        NTFS_VOLUME_DATA_BUFFER ntfsData;
        if (DeviceIoControl(
            hVolume,
            FSCTL_GET_NTFS_VOLUME_DATA,
            lpOutBuffer: new Span<byte>(&ntfsData, sizeof(NTFS_VOLUME_DATA_BUFFER)),
            lpBytesReturned: out uint bytesReturned))
        {
            outInfo.bytesPerSector = ntfsData.BytesPerSector;
            outInfo.sectorsPerCluster = ntfsData.BytesPerCluster / ntfsData.BytesPerSector;
            outInfo.totalClusters = ntfsData.TotalClusters;
            return true;
        }

        // 2. Fallback для FAT32/exFAT через GetDiskFreeSpaceW (до блокировки тома)
        string rootPath = volumePath;
        if (rootPath.LastIndexOf("\\\\.\\", 0) == 0)
        {
            rootPath = rootPath[4..];
        }
        if (rootPath[^1] != '\\')
        {
            rootPath += '\\';
        }

        if (GetDiskFreeSpace(rootPath, out uint sectorsPerCluster, out uint bytesPerSector, out _, out uint totalClusters))
        {
            outInfo.bytesPerSector = bytesPerSector;
            outInfo.sectorsPerCluster = sectorsPerCluster;
            outInfo.totalClusters = totalClusters;
            return true;
        }

        // 3. Fallback геометрии диска через IOCTL_DISK_GET_DRIVE_GEOMETRY_EX
        DISK_GEOMETRY_EX geometry;
        if (DeviceIoControl(
            hVolume,
            IOCTL_DISK_GET_DRIVE_GEOMETRY_EX,
            lpOutBuffer: new Span<byte>(&geometry, sizeof(DISK_GEOMETRY_EX)),
            lpBytesReturned: out bytesReturned))
        {
            outInfo.bytesPerSector = geometry.Geometry.BytesPerSector;
            outInfo.sectorsPerCluster = 8; // Стандартное значение по умолчанию (4KB кластер при 512B секторе)
            outInfo.totalClusters = geometry.DiskSize / (outInfo.bytesPerSector * outInfo.sectorsPerCluster);
            return true;
        }

        return false;
    }

    public static bool CreateBackup(string volumePath, string imagePath)
    {
        Console.WriteLine($"[*] Opening source volume: {volumePath}");

        // 1. Открытие тома
        SafeFileHandle hVolume = CreateFile(
            volumePath,
            (uint)(GENERIC_ACCESS_RIGHTS.GENERIC_READ | GENERIC_ACCESS_RIGHTS.GENERIC_WRITE),
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_NO_BUFFERING |
            FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_WRITE_THROUGH |
            FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_BACKUP_SEMANTICS,
            null);

        if (hVolume.IsInvalid)
        {
            Console.WriteLine($"[-] Failed to open volume. Error: {Marshal.GetLastPInvokeError()}");
            return false;
        }

        // 2. Сброс системных буферов файловой системы
        _ = FlushFileBuffers(hVolume);

        // 3. Получение параметров кластеризации (на открытом смонтированном томе)
        VolumeClusterInfo clusterInfo = new();
        if (!QueryVolumeClusterInfo(hVolume, volumePath, ref clusterInfo))
        {
            Console.WriteLine($"[-] Failed to query volume cluster info. Error: {Marshal.GetLastPInvokeError()}");
            return false;
        }

        long volumeLength = VolumeManager.GetVolumeLength(hVolume);
        uint clusterSize = clusterInfo.bytesPerSector * clusterInfo.sectorsPerCluster;

        // 4. Считывание карты занятых кластеров ДО вызова FSCTL_LOCK_VOLUME
        Console.WriteLine("[*] Fetching volume bitmap (pre-lock)...");
        STARTING_LCN_INPUT_BUFFER inputLcn;
        inputLcn.StartingLcn = 0;
        long bitmapCount = (clusterInfo.totalClusters + 7) / 8;
        using var bitBuffer = new ReadOnlyVolumeBitmap(bitmapCount);

        if (!DeviceIoControl(
            hVolume,
            FSCTL_GET_VOLUME_BITMAP,
            lpInBuffer: new ReadOnlySpan<byte>(&inputLcn, sizeof(STARTING_LCN_INPUT_BUFFER)),
            lpOutBuffer: bitBuffer.AsSpan(),
            lpBytesReturned: out var _))
        {
            Console.WriteLine($"[-] FSCTL_GET_VOLUME_BITMAP failed. Error: {Marshal.GetLastPInvokeError()}");
            return false;
        }
        ulong totalBitmapClusters = (ulong)bitBuffer.BitmapSize;
        Console.WriteLine($"[+] Bitmap acquired successfully. Total clusters: {totalBitmapClusters}");

        // 5. Захват эксклюзивной блокировки для безопасного прямого чтения блоков
        Console.WriteLine($"[*] Acquiring exclusive volume lock for streaming...");

        using var volumeLock = VolumeManager.LockVolumeForBackup(hVolume);
        Console.WriteLine("[+] Exclusive volume lock acquired.");

        using var stream = File.Create(imagePath, 4 * 1024 * 1024);
        using var brotli = new BrotliStream(stream, CompressionLevel.Optimal, true);
        using var writer = new BinaryWriter(brotli, System.Text.Encoding.UTF8, true);

        ImageHeader header = new()
        {
            Magic = ImageHeader.IMAGE_MAGIC,
            Version = ImageHeader.IMAGE_VERSION,
            BytesPerSector = clusterInfo.bytesPerSector,
            SectorsPerCluster = clusterInfo.sectorsPerCluster,
            TotalClusters = totalBitmapClusters,
            VolumeLengthBytes = (ulong)volumeLength
        };
        writer.Write(new ReadOnlySpan<byte>((byte*)&header, sizeof(ImageHeader)));

        // 7. Потоковое чтение занятых экстентов (Direct I/O, 4MB Chunks)
        int chunkSize = 4 * 1024 * 1024;
        long clustersPerChunk = chunkSize / clusterSize;
        using AlignedBuffer readBuffer = new((nuint)chunkSize);

        nuint currentLcn = 0;
        ulong totalAllocatedClustersCopied = 0;

        Console.WriteLine("[*] Streaming allocated extents...");

        while (currentLcn < totalBitmapClusters)
        {
            if (!bitBuffer.IsEmpty(currentLcn))
            {
                currentLcn++;
                continue;
            }

            ulong startLcn = currentLcn;
            while (currentLcn < totalBitmapClusters && bitBuffer.IsAllocated(currentLcn))
            {
                currentLcn++;
            }
            ulong extentLength = currentLcn - startLcn;

            ulong offsetInExtent = 0;
            while (offsetInExtent < extentLength)
            {
                ulong batchClusters = Math.Min(extentLength - offsetInExtent, (ulong)clustersPerChunk);
                ulong batchLcn = startLcn + offsetInExtent;
                uint bytesToRead = (uint)(batchClusters * clusterSize);

                long seekPos = (long)(batchLcn * clusterSize);
                _ = SetFilePointerEx(hVolume, seekPos, SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);

                var bytesRead = 0u;
                var overlapped = new NativeOverlapped();

                if (!ReadFile(
                    hFile: hVolume, 
                    lpBuffer: new Span<byte>(readBuffer.Pointer, (int)bytesToRead), 
                    lpNumberOfBytesRead: out bytesRead, 
                    lpOverlapped: ref overlapped))
                {
                    Console.WriteLine($"\n[-] Direct Read error at offset {seekPos} Error: {Marshal.GetLastPInvokeError()}");
                    return false;
                }
                Console.Write($"\r{batchLcn:X16}");

                BlockRecordHeader blockHeader = new()
                {
                    Type = BlockType.ClusterExtent,
                    TargetOffset = (ulong)seekPos,
                    DataSize = bytesRead
                };

                writer.Write(new ReadOnlySpan<byte>((byte*)&blockHeader, sizeof(BlockRecordHeader)));
                writer.Write(new ReadOnlySpan<byte>(readBuffer.Pointer, (int)bytesRead));

                offsetInExtent += batchClusters;
                totalAllocatedClustersCopied += batchClusters;
            }
        }

        // 8. Сохранение Backup Boot Sector (последний сектор тома)
        if (volumeLength > clusterInfo.bytesPerSector)
        {
            long lastSectorOffset = volumeLength - clusterInfo.bytesPerSector; ;

            _ = SetFilePointerEx(hVolume, lastSectorOffset, SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);
            var bytesRead = 0u;
            var overlapped = new NativeOverlapped();
            if (ReadFile(
                hFile: hVolume, 
                lpBuffer: new Span<byte>(readBuffer.Pointer, (int)clusterInfo.bytesPerSector), 
                lpNumberOfBytesRead: out bytesRead, 
                lpOverlapped: ref overlapped))
            {
                BlockRecordHeader backupVbrHdr = new()
                {
                    Type = BlockType.RawByteOffset,
                    TargetOffset = (ulong)lastSectorOffset,
                    DataSize = bytesRead
                };
                writer.Write(new ReadOnlySpan<byte>((byte*)&backupVbrHdr, sizeof(BlockRecordHeader)));
                writer.Write(new ReadOnlySpan<byte>(readBuffer.Pointer, (int)bytesRead));
            }
        }

        // 9. Завершающий маркер потока
        BlockRecordHeader endMarker = new() { Type = BlockType.EndOfStream, TargetOffset = 0, DataSize = 0 };
        writer.Write(new ReadOnlySpan<byte>((byte*)&endMarker, sizeof(BlockRecordHeader)));

        Console.WriteLine("\n[+] Image creation complete!");
        Console.WriteLine($"    Total clusters backed up: {totalAllocatedClustersCopied}/{totalBitmapClusters} ({totalAllocatedClustersCopied * clusterSize / (1024 * 1024)} MB)");
        return true;
    }
}
