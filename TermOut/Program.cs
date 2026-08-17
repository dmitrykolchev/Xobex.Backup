using System.Runtime.Versioning;

namespace TermOut;

[SupportedOSPlatform("windows5.1.2600")]
internal unsafe class Program
{
    private static WindowsConsole _console = new();
    private static ConsoleBuffer _buffer;

    private static void Main(string[] args)
    {
        using TerminalResizeNotifier resizeNotifier = new();
        resizeNotifier.Resized += ResizeNotifier_Resized;

        _buffer = new(_console.Width, _console.Height, Colors.White, Colors.Black);
        _buffer.Clear();
        _buffer.Write(10, 10, "Hello World!");

        _buffer.Background = new Color(127, 127, 127);
        _buffer.Foreground = new Color(0, 0, 128);
        _buffer.Fill(0, _console.Height - 1, ' ', _console.Width);
        _buffer.Write(1, _console.Height - 1, $"Новый размер: {Console.WindowHeight} строк, {Console.WindowWidth} столбцов");

        _console.Render(_buffer);

        Console.ReadLine();
    }

    private static void ResizeNotifier_Resized(object? sender, EventArgs e)
    {
        _buffer = new(_console.Width, _console.Height, Colors.White, Colors.Black);
        _buffer.Clear();
        _buffer.Write(10, 10, "Hello World!");
        _buffer.Background = new Color(127, 127, 127);
        _buffer.Foreground = new Color(0, 0, 128);
        _buffer.Fill(0, _console.Height - 1, ' ', _console.Width);
        _buffer.Write(1, _console.Height - 1, $"Новый размер: {Console.WindowHeight} строк, {Console.WindowWidth} столбцов");
        _console.Render(_buffer);
    }
}
