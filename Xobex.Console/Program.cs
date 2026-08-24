using System.Diagnostics;
using System.Globalization;

namespace Xobex.Console;

internal class Program
{
    static void Main(string[] args)
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

            for(; ; )
            {
                if(terminalParser.TryGetInputEvent(out var ev))
                {
                    conOut.WriteLine($"{ev}");
                    conOut.Flush();
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
