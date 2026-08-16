using System.Drawing;
using System.Runtime.Versioning;
using Windows.Win32.System.Memory;
using static Windows.Win32.PInvoke;

namespace PartBackup;

internal struct ImageHeader
{
    public const ulong IMAGE_MAGIC = 0x474D494D554C4F56UL; // "VOLUMIMG"
    public const uint IMAGE_VERSION = 2;

    public ulong Magic;
    public uint Version;
    public uint BytesPerSector;
    public uint SectorsPerCluster;
    public ulong TotalClusters;
    public ulong VolumeLengthBytes;
};

public enum BlockType : uint
{
    ClusterExtent = 1,
    RawByteOffset = 2,
    EndOfStream = 0xFFFFFFFF
};

internal struct BlockRecordHeader
{
    public BlockType Type;
    public ulong TargetOffset; // Абсолютное смещение на томе в байтах
    public ulong DataSize;     // Размер полезной нагрузки в байтах
};


[SupportedOSPlatform("windows5.1.2600")]
public unsafe class AlignedBuffer: IDisposable
{
    private byte* _ptr;
    private nuint _size;
    public AlignedBuffer(nuint size)
    {
        _size = size;
        _ptr = (byte*)VirtualAlloc(null, size, VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE, PAGE_PROTECTION_FLAGS.PAGE_READWRITE);
    }

    ~AlignedBuffer()
    {
        Free();
    }

    public void Dispose()
    {
        Free();
        GC.SuppressFinalize(this);
    }

    public void Free()
    {
        var ptr = _ptr;
        _ptr = null;
        if (ptr != null)
        {
            VirtualFree(ptr, 0, VIRTUAL_FREE_TYPE.MEM_RELEASE);
        }
    }


    public byte* Pointer => _ptr;

    public nuint Length => _size;
}
