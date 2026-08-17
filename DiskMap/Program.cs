using DiskMap;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.System.Console;
using Windows.Win32.System.Ioctl;
using static Windows.Win32.PInvoke;


[SupportedOSPlatform("windows5.1.2600")]
unsafe class Program
{
    // Win32 константа для получения битовой карты тома
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.Out.WriteLine(" ");

        // 1. Активируем поддержку ANSI и TrueColor в терминале Windows
        EnableTrueColorSupport();

        char driveLetter = 'E';
        if (args.Length > 0 && args[0].Length > 0)
        {
            driveLetter = char.ToUpper(args[0][0]);
        }

        Console.Title = $"Дефраг-карта диска {driveLetter}:";
        Console.Out.WriteLine($"\x1b[1mАнализ диска {driveLetter}:... (Требуются права Администратора)\x1b[0m\n");

        // 2. Получаем геометрию диска (размер кластера)
        string rootPath = $"{driveLetter}:\\";
        uint sectorsPerCluster, bytesPerSector, numberOfFreeClusters, totalNumberOfClusters;

        if (!GetDiskFreeSpace(rootPath, out sectorsPerCluster, out bytesPerSector, out numberOfFreeClusters, out totalNumberOfClusters))
        {
            Console.Error.WriteLine($"\x1b[31mОшибка получения геометрии диска. Код: {Marshal.GetLastPInvokeError()}\x1b[0m");
            return;
        }

        long clusterSize = (long)sectorsPerCluster * bytesPerSector;

        // 3. Открываем дескриптор тома для IOCTL запросов
        string volumePath = $"\\\\.\\{driveLetter}:";

        var hVolume = CreateFile(
            volumePath,
            (uint)GENERIC_ACCESS_RIGHTS.GENERIC_READ, // Минимальные права для чтения карты
            FILE_SHARE_MODE.FILE_SHARE_READ | FILE_SHARE_MODE.FILE_SHARE_WRITE,
            lpSecurityAttributes: null,
            FILE_CREATION_DISPOSITION.OPEN_EXISTING,
            FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL
        );

        if (hVolume.IsInvalid)
        {
            Console.Error.WriteLine($"\x1b[31mОшибка открытия тома. Код: {Marshal.GetLastPInvokeError()}\nУбедитесь, что запустили от Администратора.\x1b[0m");
            return;
        }

        try
        {
            // Запрашиваем карту, начиная с 0-го кластера
            STARTING_LCN_INPUT_BUFFER inputLcn;
            inputLcn.StartingLcn = 0;
            long bitmapCount = (totalNumberOfClusters + 7) / 8;
            var bitBuffer = new ReadOnlyVolumeBitmap(bitmapCount);

            if (!DeviceIoControl(
                hVolume,
                FSCTL_GET_VOLUME_BITMAP,
                lpInBuffer: new ReadOnlySpan<byte>(&inputLcn, sizeof(STARTING_LCN_INPUT_BUFFER)),
                lpOutBuffer: bitBuffer.AsSpan(),
                lpBytesReturned: out var bytesReturned))
            {
                Console.Error.WriteLine($"[-] FSCTL_GET_VOLUME_BITMAP failed. Error: {Marshal.GetLastPInvokeError()}");
                return;
            }

            // 4. Отрисовка карты
            DrawDiskMap(ref bitBuffer, sectorsPerCluster * bytesPerSector);

            bitBuffer.Dispose();
        }
        finally
        {
            hVolume.Close();
        }
    }

    private static void DrawDiskMap(ref ReadOnlyVolumeBitmap bitmap, uint clusterSize)
    {
        // Получаем размеры консоли
        int windowWidth = Console.WindowWidth;
        if (windowWidth <= 0) windowWidth = 80;

        // Резервируем место под рамку
        int mapWidth = windowWidth - 4;
        int mapHeight = Console.WindowHeight - 8;
        if (mapHeight <= 0) mapHeight = 20;

        long totalCells = (long)mapWidth * mapHeight;

        // Вычисляем, сколько кластеров входит в один символ на экране (Пул кластеров)
        long clustersPerCell = (bitmap.BitmapSize + totalCells - 1) / totalCells;
        var totalClusters = bitmap.BitmapSize;
        // Открываем прямой доступ к потоку вывода терминала с буфером в 128 КБ
        using Stream baseStream = Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        using StreamWriter writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);


        writer.WriteLine($"Размер кластера: {clusterSize / 1024} КБ");
        writer.WriteLine($"Всего кластеров: {totalClusters:N0}");
        writer.WriteLine($"Один символ [■] равен пулу из {clustersPerCell:F1} кластеров (~{clustersPerCell * clusterSize / 1024 / 1024:F1} МБ)\n");

        // Отрисовка верхней рамки
        writer.Write('┌');
        for(int i = 0; i < mapWidth;++i) writer.Write('─');
        writer.WriteLine('┐');
        long totalAllocated = 0;
        for (int y = 0; y < mapHeight; y++)
        {
            writer.Write("│"); // Левая граница

            for (int x = 0; x < mapWidth; x++)
            {
                long cellIndex = (long)y * mapWidth + x;

                // Диапазон кластеров для текущей ячейки терминала
                long startClusterForCell = (long)(cellIndex * clustersPerCell);
                long endClusterForCell = (long)((cellIndex + 1) * clustersPerCell);
                if (endClusterForCell > totalClusters) endClusterForCell = totalClusters;

                long checkedClusters = 0;
                long usedClusters = 0;
                
                // Анализируем биты занятости для пула кластеров этой ячейки
                for (long c = startClusterForCell; c < endClusterForCell; c++)
                {
                    if (bitmap.IsAllocated((nuint)c))
                    {
                        usedClusters++;
                        totalAllocated++;
                    }
                    checkedClusters++;
                }

                // Вычисляем процент занятости пула кластеров в ячейке
                double usageRatio = checkedClusters > 0 ? (double)usedClusters / checkedClusters : 0;

                // 5. Генерация TrueColor цвета (Градиент от Темно-Синего через Зеленый к Ярко-Красному)
                // Свободный (0%) -> Глубокий темно-серый/синий
                // Частично занят (1%-50%) -> Переход в зеленый
                // Плотная занятость (51%-100%) -> Переход в ярко-красный
                byte r, g, b;
                if (usageRatio == 0)
                {
                    r = 35; g = 35; b = 45; // Свободный кластер (фоновый цвет)
                }
                else if (usageRatio < 0.5)
                {
                    double t = usageRatio / 0.5;
                    r = (byte)(0 + t * 0);
                    g = (byte)(100 + t * 155); // Уходим в насыщенный зеленый
                    b = (byte)(200 - t * 200);
                }
                else
                {
                    double t = (usageRatio - 0.5) / 0.5;
                    r = (byte)(0 + t * 255);   // Переходим в ярко-красный при 100%
                    g = (byte)(255 - t * 255);
                    b = 0;
                }

                // Выводим закрашенный блок ■ с TrueColor фоном и текстом
                // Используем ANSI последовательность ESC[38;2;R;G;Bm для Foreground текста
                writer.Write($"\x1b[38;2;{r};{g};{b}m■");
            }

            writer.WriteLine("\x1b[0m│"); // Правая граница и сброс цвета
        }

        // Отрисовка нижней рамки
        writer.WriteLine("└" + new string('─', mapWidth) + "┘");

        // Легенда
        writer.WriteLine("\nЛегенда занятости пула кластеров:");
        writer.WriteLine("\x1b[38;2;35;35;45m■ 0% (Пусто) \x1b[38;2;0;150;100m■ ~30% (Редко) \x1b[38;2;120;120;0m■ ~60% (Средне) \x1b[38;2;255;0;0m■ 100% (Плотная запись)\x1b[0m\n");

        writer.WriteLine($"Занято {totalAllocated:#,##0} кластеров ({totalAllocated * clusterSize / 1024.0 / 1024.0 / 1024.0:#,##0.0} GB)");
        writer.Flush();
    }

    private static void EnableTrueColorSupport()
    {
        // Получаем стандартный дескриптор вывода консоли
        var hOut = GetStdHandle(STD_HANDLE.STD_OUTPUT_HANDLE);
        if (hOut == HANDLE.INVALID_HANDLE_VALUE) return;
        CONSOLE_MODE mode = 0;
        if (GetConsoleMode(hOut, &mode))
        {
            // Включаем флаг ENABLE_VIRTUAL_TERMINAL_PROCESSING для обработки ANSI/TrueColor последовательностей
            mode |= CONSOLE_MODE.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(hOut, mode);
        }
    }
}
