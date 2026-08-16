using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using static Windows.Win32.PInvoke;

namespace PartBackup;

[SupportedOSPlatform("windows5.1.2600")]
public static unsafe class SecurityManager
{
    private static readonly string[] _requiredPrivileges = [
        SE_BACKUP_NAME,          // L"SeBackupPrivilege"
        SE_RESTORE_NAME,         // L"SeRestorePrivilege"
        SE_MANAGE_VOLUME_NAME,   // L"SeManageVolumePrivilege"
        SE_SECURITY_NAME         // L"SeSecurityPrivilege"
    ];

    public static bool EnableRequiredPrivileges()
    {
        HANDLE hToken = HANDLE.INVALID_HANDLE_VALUE;
        if (!OpenProcessToken(
            GetCurrentProcess(),
            TOKEN_ACCESS_MASK.TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ACCESS_MASK.TOKEN_QUERY,
            &hToken))
        {
            Console.WriteLine($"[-] OpenProcessToken failed: {Marshal.GetLastPInvokeError()}");
            return false;
        }

        bool allEnabled = true;

        foreach (string privName in _requiredPrivileges)
        {
            if (!LookupPrivilegeValue(null, privName, out LUID luid))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[-] LookupPrivilegeValue failed for {privName} Error: {Marshal.GetLastPInvokeError()}");
                allEnabled = false;
                continue;
            }

            TOKEN_PRIVILEGES tp = new()
            {
                PrivilegeCount = 1
            };
            tp.Privileges[0].Luid = luid;
            tp.Privileges[0].Attributes = TOKEN_PRIVILEGES_ATTRIBUTES.SE_PRIVILEGE_ENABLED;

            // Обязательный сброс ошибки перед вызовом
            Marshal.SetLastSystemError((int)WIN32_ERROR.ERROR_SUCCESS);

            _ = AdjustTokenPrivileges(
                hToken,
                false,
                &tp,
                (uint)sizeof(TOKEN_PRIVILEGES),
                null,
                null
            );

            WIN32_ERROR err = (WIN32_ERROR)Marshal.GetLastSystemError();
            if (err == WIN32_ERROR.ERROR_NOT_ALL_ASSIGNED)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[!] Privilege not held by process token: {privName} (Ensure running as Administrator)");
                allEnabled = false;
            }
            else if (err != WIN32_ERROR.ERROR_SUCCESS)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[-] AdjustTokenPrivileges failed for {privName} Error: {err}");
                allEnabled = false;
            }
        }

        _ = CloseHandle(hToken);
        return allEnabled;
    }
}
