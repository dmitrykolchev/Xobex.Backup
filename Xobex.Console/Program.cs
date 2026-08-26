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
    private static void Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            var conOut = WindowsOutputAdapter.Create();
            using var conIn = WindowsInputAdapter.Create();
            using var mouse = conIn.EnableMouseInput();
            var terminalParser = conIn.CreateParser();
            for (; ; )
            {
                if (terminalParser.TryGetInputEvent(out var ev))
                {
                    conOut.WriteLine($"{ev}");
                    conOut.Flush();
                    if (ev?.EventType == InputEventType.Key)
                    {
                        if (ev.Key.Key == ConsoleKey.C && ev.Key.Mod == ConsoleModifiers.Control)
                        {
                            conOut.WriteLine($"Ctrl-C pressed. Exiting");
                            conOut.Flush();
                            break;
                        }
                        else if (ev.Key.Ch == 'M')
                        {
                            conIn.EnableMouseInput();
                            conOut.WriteLine($"Mouse input enabled");
                            conOut.Flush();
                        }
                        else if (ev.Key.Ch == 'm')
                        {
                            conIn.DisableMouseInput();
                            conOut.WriteLine($"Mouse input disabled");
                            conOut.Flush();
                        }
                    }
                }
            }
        }
        else if(OperatingSystem.IsLinux())
        {
            var conOut = LinuxOutputAdapter.Create();
            using ITerminalInputAdapter conIn = LinuxInputAdapter.Create(conOut);
            using var mouse = conIn.EnableMouseInput();
            var terminalParser = conIn.CreateParser();

            for (; ; )
            {
                if (terminalParser.TryGetInputEvent(out var ev))
                {
                    conOut.WriteLine($"{ev}");
                    conOut.Flush();
                    if (ev?.EventType == InputEventType.Key)
                    {
                        if (ev.Key.Key == ConsoleKey.C && ev.Key.Mod == ConsoleModifiers.Control)
                        {
                            conOut.WriteLine($"Ctrl-C pressed. Exiting");
                            conOut.Flush();
                            break;
                        }
                        else if (ev.Key.Ch == 'M')
                        {
                            conIn.EnableMouseInput();
                            conOut.WriteLine($"Mouse input enabled");
                            conOut.Flush();
                        }
                        else if (ev.Key.Ch == 'm')
                        {
                            conIn.DisableMouseInput();
                            conOut.WriteLine($"Mouse input disabled");
                            conOut.Flush();
                        }
                    }
                }
            }
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
    }
}
