// <copyright file="Program.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using Xobex.Console.Abstractions;
using Xobex.Console.Linux;
using Xobex.Console.Windows;

namespace Xobex.Console;

internal class Program
{
    private static IDisposable? _mouseInput;

    private static void Main(string[] args)
    {
        using var notifier = new TerminalResizeNotifier();
        using var con = TerminalAdapter.Create();
        using var altScreen = con.Out.AlternateScreen();
        notifier.Resized += (sender, e) =>
        {
            con.Out.WriteLine($"Terminal size changed: {con.Out.Width}x{con.Out.Height}");
            con.Out.Flush();
        };
        WritePalette(con.Out);
        PrintAsciiTable(con.Out);
        ProcessConsoleEvents(con);
        _mouseInput?.Dispose();
    }

    private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
    }

    private static void WritePalette(ITerminalOutputAdapter conOut)
    {
        for (var b = TerminalColor.Black; b <= TerminalColor.White; ++b)
        {
            for (var f = TerminalColor.Black; f <= TerminalColor.White; ++f)
            {
                conOut.SetColor(b, f);
                conOut.Write(" ■");
            }
            conOut.WriteLine(" ");
        }
        conOut.Write("\e[0m");
        conOut.Flush();
    }

    private static void PrintAsciiTable(ITerminalOutputAdapter conOut)
    {
        //conOut.SetCursorPosition(0, 0);
        conOut.WriteLine("ASCII");
        conOut.WriteLine("-----");
        for (var h = 0; h < 16; ++h)
        {
            conOut.Write(' ');
            for (var l = 0; l < 16; ++l)
            {
                var ch = (char)(byte)((h << 4) | l);
                if(ch <= 32)
                {
                    ch = (char)((short)'\u2400' + (short)ch);
                }
                else if(ch == 127)
                {
                    ch = '\u2421';
                }
                conOut.Write($"{ch} ");
            }
            conOut.WriteLine();
        }
        conOut.WriteLine("Box Drawings");
        conOut.WriteLine("------------");
        conOut.Write(' ');
        for (var ch  = 0x2500; ch < 0x2580; ++ch)
        {
            conOut.Write($"{(char)ch} ");
            if(ch % 0x10 == 0xF)
            {
                conOut.WriteLine();
                conOut.Write(' ');
            }
        }
        conOut.WriteLine();
        conOut.WriteLine("Block Elements");
        conOut.WriteLine("--------------");
        conOut.Write(' ');
        for (var ch = 0x2580; ch < 0x2600; ++ch)
        {
            conOut.Write($"{(char)ch} ");
            if (ch % 0x10 == 0xF)
            {
                conOut.WriteLine();
                conOut.Write(' ');
            }
        }
        conOut.Flush();
    }

    ///  ▀ ▁ ▂ ▃ ▄ ▅ ▆ ▇ █ ▉ ▊ ▋ ▌ ▍ ▎ ▏
    ///  
    ///  ▐ ░ ▒ ▓ ▔ ▕ ▖ ▗ ▘ ▙ ▚ ▛ ▜ ▝ ▞ ▟
    ///  
    ///  ■ □ ▢ ▣ ▤ ▥ ▦ ▧ ▨ ▩ ▪ ▫ ▬ ▭ ▮ ▯
    ///  
    ///  ▰ ▱ ▲ △ ▴ ▵ ▶ ▷ ▸ ▹ ► ▻ ▼ ▽ ▾ ▿
    ///  
    ///  ◀ ◁ ◂ ◃ ◄ ◅ ◆ ◇ ◈ ◉ ◊ ○ ◌ ◍ ◎ ●
    ///  
    ///  ◐ ◑ ◒ ◓ ◔ ◕ ◖ ◗ ◘ ◙ ◚ ◛ ◜ ◝ ◞ ◟
    ///  
    ///  ◠ ◡ ◢ ◣ ◤ ◥ ◦ ◧ ◨ ◩ ◪ ◫ ◬ ◭ ◮ ◯
    ///  
    ///  ◰ ◱ ◲ ◳ ◴ ◵ ◶ ◷ ◸ ◹ ◺ ◻ ◼ ◽ ◾ ◿

    //  ─ ━ │ ┃ ┄ ┅ ┆ ┇ ┈ ┉ ┊ ┋ ┌ ┍ ┎ ┏
    //
    //  ┐ ┑ ┒ ┓ └ ┕ ┖ ┗ ┘ ┙ ┚ ┛ ├ ┝ ┞ ┟
    //
    //  ┠ ┡ ┢ ┣ ┤ ┥ ┦ ┧ ┨ ┩ ┪ ┫ ┬ ┭ ┮ ┯
    //
    //  ┰ ┱ ┲ ┳ ┴ ┵ ┶ ┷ ┸ ┹ ┺ ┻ ┼ ┽ ┾ ┿
    //
    //  ╀ ╁ ╂ ╃ ╄ ╅ ╆ ╇ ╈ ╉ ╊ ╋ ╌ ╍ ╎ ╏
    //
    //  ═ ║ ╒ ╓ ╔ ╕ ╖ ╗ ╘ ╙ ╚ ╛ ╜ ╝ ╞ ╟
    //
    //  ╠ ╡ ╢ ╣ ╤ ╥ ╦ ╧ ╨ ╩ ╪ ╫ ╬ ╭ ╮ ╯
    //
    //  ╰ ╱ ╲ ╳ ╴ ╵ ╶ ╷ ╸ ╹ ╺ ╻ ╼ ╽ ╾ ╿


    /// ┏━━━━━┳━━━━┯━━━┓
    /// ┃     ┃    │   ┃   ╭───────────────────────────╮
    /// ┃     ┃    │   ┃   │                           │
    /// ┠─────╂────┼───┨   │                           │
    /// ┃     ┃    │   ┃   │                           │
    /// ┃     ┃    │   ┃   │                           │
    /// ┣━━━━━╋━━━━┿━━━┫   │                           │
    /// ┃     ┃    │   ┃   ╰───────────────────────────╯
    /// ┃     ┃    │   ┃
    /// ┗━━━━━┻━━━━┷━━━┛
    /// # ┏━[x]━┳━━━━┯━━[↕]━┓
    /// # ┃     ┃    │      ┃   ╭───────────────────────────╮
    /// # ┃     ┃    │      ┃   │                           │
    /// # ┠─────╂────┼──────┨   │                           │
    /// # ┃     ┃    │      ┃   │                           │
    /// # ┃     ┃    │      ┃   │                           │
    /// # ┣━━━━━╋━━━━┿━━━━━━┫   │                           │
    /// # ┃     ┃    │      ┃   ╰───────────────────────────╯
    /// # ┃     ┃    │      ┃
    /// # └━━━━━┻━━━━┷━━━━━━┘

    private static void ProcessConsoleEvents(TerminalAdapter con)
    {
        var terminalParser = con.In.CreateParser();
        for (; ; )
        {
            if (terminalParser.TryGetInputEvent(out var ev))
            {
                con.Out.WriteLine($"{ev}");
                con.Out.Flush();
                if (ev?.EventType == InputEventType.Key)
                {
                    if (ev.Key.Key == ConsoleKey.C && ev.Key.Mod == ConsoleModifiers.Control)
                    {
                        con.Out.WriteLine($"Ctrl-C pressed. Exiting");
                        con.Out.Flush();
                        break;
                    }
                    else if (ev.Key.Ch == 'M' && ev.Key.KeyDown)
                    {
                        _mouseInput = con.In.EnableMouseInput();
                        con.Out.WriteLine($"Mouse input enabled");
                        con.Out.Flush();
                    }
                    else if (ev.Key.Ch == 'm' && ev.Key.KeyDown)
                    {
                        _mouseInput?.Dispose();
                        _mouseInput = null;
                        con.Out.WriteLine($"Mouse input disabled");
                        con.Out.Flush();
                    }
                }
            }
        }
    }
}
