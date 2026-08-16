#pragma once

class VolumeManager {
public:
    static bool EnablePrivilege(LPCWSTR privilegeName) {
        HANDLE hToken = INVALID_HANDLE_VALUE;
        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
            return false;
        }
        SafeHandle token(hToken);

        TOKEN_PRIVILEGES tp{};
        LUID luid{};
        if (!LookupPrivilegeValueW(nullptr, privilegeName, &luid)) {
            return false;
        }

        tp.PrivilegeCount = 1;
        tp.Privileges[0].Luid = luid;
        tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

        SetLastError(ERROR_SUCCESS);
        AdjustTokenPrivileges(token.get(), FALSE, &tp, sizeof(TOKEN_PRIVILEGES), nullptr, nullptr);
        return (GetLastError() == ERROR_SUCCESS);
    }

    // Блокировка для БЭКАПА (только LOCK, без DISMOUNT, чтобы не убить Ntfs.sys)
    static bool TryLockVolumeForBackup(HANDLE hVolume) {
        DWORD bytesReturned = 0;
        FlushFileBuffers(hVolume);

        if (DeviceIoControl(hVolume, FSCTL_LOCK_VOLUME, nullptr, 0, nullptr, 0, &bytesReturned, nullptr)) {
            return true;
        }
        return false;
    }

    // Блокировка для ВОССТАНОВЛЕНИЯ (жесткий LOCK + DISMOUNT для записи секторов)
    static bool LockAndDismountForRestore(HANDLE hVolume, int maxRetries = 5) {
        DWORD bytesReturned = 0;
        FlushFileBuffers(hVolume);

        for (int i = 0; i < maxRetries; ++i) {
            if (DeviceIoControl(hVolume, FSCTL_LOCK_VOLUME, nullptr, 0, nullptr, 0, &bytesReturned, nullptr)) {
                return true;
            }
            // Разрываем дескрипторы для разблокировки записи
            DeviceIoControl(hVolume, FSCTL_DISMOUNT_VOLUME, nullptr, 0, nullptr, 0, &bytesReturned, nullptr);
            Sleep(300);
        }
        return false;
    }

    static uint64_t GetVolumeLength(HANDLE hVolume) {
        GET_LENGTH_INFORMATION gli{};
        DWORD bytesReturned = 0;
        if (DeviceIoControl(hVolume, IOCTL_DISK_GET_LENGTH_INFO, nullptr, 0, &gli, sizeof(gli), &bytesReturned, nullptr)) {
            return static_cast<uint64_t>(gli.Length.QuadPart);
        }
        return 0;
    }
};
