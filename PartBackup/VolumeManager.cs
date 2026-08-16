using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.System.Ioctl;
using static Windows.Win32.PInvoke;

namespace PartBackup;

[SupportedOSPlatform("windows5.1.2600")]
internal static unsafe class VolumeManager
{
    public readonly ref struct VolumeLockHandle
    {
        private readonly SafeFileHandle _handle;
        internal VolumeLockHandle(SafeFileHandle handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            VolumeManager.Unlock(_handle);
        }
    }

    // Блокировка для БЭКАПА (только LOCK, без DISMOUNT, чтобы не убить Ntfs.sys)
    public static VolumeLockHandle LockVolumeForBackup(SafeFileHandle hVolume)
    {
        FlushFileBuffers(hVolume);

        if (DeviceIoControl(hVolume, FSCTL_LOCK_VOLUME, lpBytesReturned: out uint _))
        {
            return new VolumeLockHandle(hVolume);
        }
        throw new InvalidOperationException("cannot lock volume");
    }

    // Блокировка для ВОССТАНОВЛЕНИЯ (жесткий LOCK + DISMOUNT для записи секторов)
    public static VolumeLockHandle LockAndDismountForRestore(SafeFileHandle hVolume, int maxRetries = 5)
    {
        FlushFileBuffers(hVolume);
        for (int i = 0; i < maxRetries; ++i)
        {
            if (DeviceIoControl(hVolume, FSCTL_LOCK_VOLUME, lpBytesReturned: out uint _))
            {
                return new VolumeLockHandle(hVolume);
            }
            // Разрываем дескрипторы для разблокировки записи
            DeviceIoControl(hVolume, FSCTL_DISMOUNT_VOLUME, lpBytesReturned: out uint _);
            Thread.Sleep(300);
        }
        throw new InvalidOperationException("cannot lock volume");
    }

    public static long GetVolumeLength(SafeFileHandle hVolume)
    {
        GET_LENGTH_INFORMATION gli;
        if (DeviceIoControl(
            hVolume, 
            IOCTL_DISK_GET_LENGTH_INFO, 
            lpOutBuffer: new Span<byte>(&gli, sizeof(GET_LENGTH_INFORMATION)), 
            lpBytesReturned: out uint _))
        {
            return gli.Length;
        }
        return 0;
    }

    public static bool Unlock(SafeFileHandle hVolume)
    {
        if (DeviceIoControl(hVolume, FSCTL_UNLOCK_VOLUME, lpBytesReturned: out var _))
        {
            return true;
        }
        return false;
    }
}
