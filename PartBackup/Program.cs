using System.Runtime.Versioning;

namespace PartBackup;


[SupportedOSPlatform("windows5.1.2600")]
internal class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length != 3)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(@"Usage:
    PartBackup backup  \\.\D:  C:\backup.img
    PartBackup restore C:\backup.img \\.\D:");
                return;
            }

            // Инициализация всех привилегий токена до работы с подсистемой хранения
            if (!SecurityManager.EnableRequiredPrivileges())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Warning: Some storage privileges could not be enabled. " +
                    "Execution might fail on restricted/locked volumes.");
                return;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Info: All required storage privileges has been enabled. ");
            }

            string mode = args[0];
            string arg1 = args[1];
            string arg2 = args[2];

            if (mode == "backup")
            {
                VolumeBackupEngine.CreateBackup(arg1, arg2);
                return;
            }
            else if (mode == "restore")
            {
                //VolumeRestoreEngine.RestoreBackup(arg1, arg2);
                throw new NotImplementedException();
            }

        }
        finally
        {
            Console.ResetColor();
        }
    }
}
