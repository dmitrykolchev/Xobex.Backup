// <copyright file="Program.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
        {
            for (; ; )
            {
                var keyInfo = System.Console.ReadKey(true);
                System.Console.WriteLine($"{keyInfo.Key}, {keyInfo.Modifiers}, \\u{(ushort)keyInfo.KeyChar:x4}");
            }
        }
        else
        {

            var conOut = new LinuxOutputAdapter();
            using var conIn = new LinuxInputAdapter(conOut);
            var buffer = new InputBuffer(conIn);

            var terminalParser = new LinuxTerminalParser(conIn);

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
                            break;
                        }
                        else if (ev.Key.Ch == 'M')
                        {
                            conOut.EnableMouseInput();
                        }
                        else if (ev.Key.Ch == 'm')
                        {
                            conOut.DisableMouseInput();
                        }
                    }
                }
            }

            //bool sep = false;
            //for (; ; )
            //{
            //    InputToken token = buffer.NextToken();
            //    if (token.TokenType == InputTokenType.Separator)
            //    {
            //        sep = true;
            //        conOut.WriteLine("<SEP>");
            //        conOut.Flush();
            //        continue;
            //    }
            //    if (token.TokenType == InputTokenType.ETX)
            //    {
            //        break;
            //    }
            //    else if ((token.TokenType & InputTokenType.Char7Bit) != 0)
            //    {
            //        if (token.Ch == 'm' && sep)
            //        {
            //            conOut.DisableMouseInput();
            //            conOut.WriteLine("Mouse input disabled");
            //            conOut.Flush();
            //        }
            //        else if (token.Ch == 'M' && sep)
            //        {
            //            conOut.EnableMouseInput();
            //            conOut.WriteLine("Mouse input enabled");
            //            conOut.Flush();
            //        }
            //    }
            //    sep = false;
            //    var text = token.TokenType switch
            //    {
            //        InputTokenType.Char7Bit or 
            //        InputTokenType.Digit or 
            //        InputTokenType.UpperCase or 
            //        InputTokenType.LowerCase or 
            //        InputTokenType.Symbol => new string((char)token.Ch, 1),
            //        InputTokenType.Char8Bit => $"\\u{token.Ch:x4}",
            //        _ => $"<{token.TokenType}>"
            //    };
            //    conOut.Write(text);
            //    conOut.Flush();
            //}
        }
    }
}
