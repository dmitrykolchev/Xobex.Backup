#pragma once

#include <vector>

class SecurityManager {
public:
    static bool EnableRequiredPrivileges() {
        HANDLE hToken = INVALID_HANDLE_VALUE;
        if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
            std::cerr << "[-] OpenProcessToken failed: " << GetLastError() << std::endl;
            return false;
        }

        const wchar_t* requiredPrivileges[] = {
            SE_BACKUP_NAME,          // L"SeBackupPrivilege"
            SE_RESTORE_NAME,         // L"SeRestorePrivilege"
            SE_MANAGE_VOLUME_NAME,   // L"SeManageVolumePrivilege"
            SE_SECURITY_NAME         // L"SeSecurityPrivilege"
        };

        bool allEnabled = true;

        for (const auto* privName : requiredPrivileges) {
            TOKEN_PRIVILEGES tp{};
            LUID luid{};

            if (!LookupPrivilegeValueW(nullptr, privName, &luid)) {
                std::wcerr << L"[-] LookupPrivilegeValue failed for " << privName
                    << L" Error: " << GetLastError() << std::endl;
                allEnabled = false;
                continue;
            }

            tp.PrivilegeCount = 1;
            tp.Privileges[0].Luid = luid;
            tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

            // Обязательный сброс ошибки перед вызовом
            SetLastError(ERROR_SUCCESS);

            AdjustTokenPrivileges(
                hToken,
                FALSE,
                &tp,
                sizeof(TOKEN_PRIVILEGES),
                nullptr,
                nullptr
            );

            DWORD err = GetLastError();
            if (err == ERROR_NOT_ALL_ASSIGNED) {
                std::wcerr << L"[!] Privilege not held by process token: " << privName
                    << L" (Ensure running as Administrator)" << std::endl;
                allEnabled = false;
            }
            else if (err != ERROR_SUCCESS) {
                std::wcerr << L"[-] AdjustTokenPrivileges failed for " << privName
                    << L" Error: " << err << std::endl;
                allEnabled = false;
            }
        }

        CloseHandle(hToken);
        return allEnabled;
    }
};