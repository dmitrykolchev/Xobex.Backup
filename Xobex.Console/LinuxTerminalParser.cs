using System;
using System.Collections.Generic;
using System.Text;

namespace Xobex.Console;

public enum ParserState
{
    Initial,
    StartOfEscapeSequence,

    KeyReady,
    StartOfSS3Sequence
}

internal class LinuxTerminalParser
{
    private readonly LinuxInputAdapter _conIn;
    private readonly InputBuffer _buffer;
    private readonly byte[] _rawData;
    private int _rawDataPosition = 0;
    private readonly Stack<InputToken> _ungetBuffer = new();


    public LinuxTerminalParser(LinuxInputAdapter conIn)
    {
        _conIn = conIn;
        _buffer = new InputBuffer(conIn);
        _rawData = new byte[128];
        _rawDataPosition = 0;
    }

    private InputToken NextToken()
    {
        InputToken token;
        if (_ungetBuffer.Count > 0)
        {
            token = _ungetBuffer.Pop();
        }
        else
        {
            token = _buffer.NextToken();
        }
        if (token.TokenType != InputTokenType.Separator)
        {
            _rawData[_rawDataPosition] = token.Ch;
            _rawDataPosition++;
        }
        return token;
    }

    private void UngetToken(InputToken token)
    {
        if (token.TokenType == InputTokenType.Separator)
        {
            throw new InvalidOperationException("cannot unget separatop token");
        }
        if (_rawDataPosition <= 0)
        {
            throw new InvalidOperationException("cannot unget token");
        }
        _ungetBuffer.Push(token);
        _rawDataPosition--;
    }

    private ReadOnlySpan<byte> GetRawData()
    {
        return new ReadOnlySpan<byte>(_rawData, 0, _rawDataPosition);
    }
    private static readonly Dictionary<char, (ConsoleKey, ConsoleModifiers)> _charToConsoleKey = new()
    {
        ['\u0001'] = (ConsoleKey.A, ConsoleModifiers.Control),
        ['\u0002'] = (ConsoleKey.B, ConsoleModifiers.Control),
        ['\u0003'] = (ConsoleKey.C, ConsoleModifiers.Control),
        ['\u0004'] = (ConsoleKey.D, ConsoleModifiers.Control),
        ['\u0005'] = (ConsoleKey.E, ConsoleModifiers.Control),
        ['\u0006'] = (ConsoleKey.F, ConsoleModifiers.Control),
        ['\u0007'] = (ConsoleKey.G, ConsoleModifiers.Control),
        ['\u0008'] = (ConsoleKey.Backspace, ConsoleModifiers.Control),
        ['\u0009'] = (ConsoleKey.I, ConsoleModifiers.Control),
        ['\u000A'] = (ConsoleKey.J, ConsoleModifiers.Control),
        ['\u000B'] = (ConsoleKey.K, ConsoleModifiers.Control),
        ['\u000C'] = (ConsoleKey.L, ConsoleModifiers.Control),
        ['\u000D'] = (ConsoleKey.Enter, ConsoleModifiers.None),
        ['\u000E'] = (ConsoleKey.N, ConsoleModifiers.Control),
        ['\u000F'] = (ConsoleKey.O, ConsoleModifiers.Control),
        ['\u0010'] = (ConsoleKey.P, ConsoleModifiers.Control),
        ['\u0011'] = (ConsoleKey.Q, ConsoleModifiers.Control),
        ['\u0012'] = (ConsoleKey.R, ConsoleModifiers.Control),
        ['\u0013'] = (ConsoleKey.S, ConsoleModifiers.Control),
        ['\u0014'] = (ConsoleKey.T, ConsoleModifiers.Control),
        ['\u0015'] = (ConsoleKey.U, ConsoleModifiers.Control),
        ['\u0016'] = (ConsoleKey.V, ConsoleModifiers.Control),
        ['\u0017'] = (ConsoleKey.W, ConsoleModifiers.Control),
        ['\u0018'] = (ConsoleKey.X, ConsoleModifiers.Control),
        ['\u0019'] = (ConsoleKey.Y, ConsoleModifiers.Control),
        ['\u001A'] = (ConsoleKey.Z, ConsoleModifiers.Control),
        ['\u001B'] = (ConsoleKey.Escape, ConsoleModifiers.None),
        ['\u001C'] = (ConsoleKey.Oem5, ConsoleModifiers.Control),
        ['\u001D'] = (ConsoleKey.Oem6, ConsoleModifiers.Control),
        ['\u001E'] = (ConsoleKey.NumPad6, ConsoleModifiers.Control),
        ['\u001F'] = (ConsoleKey.Oem2, ConsoleModifiers.Control),
        ['\u0020'] = (ConsoleKey.Spacebar, ConsoleModifiers.None),
        ['-'] = (ConsoleKey.OemMinus, ConsoleModifiers.None),
        ['_'] = (ConsoleKey.OemMinus, ConsoleModifiers.Shift),
        ['='] = (ConsoleKey.OemPlus, ConsoleModifiers.None),
        ['+'] = (ConsoleKey.OemPlus, ConsoleModifiers.Shift),
        ['\\'] = (ConsoleKey.Oem5, ConsoleModifiers.None),
        ['|'] = (ConsoleKey.Oem5, ConsoleModifiers.Shift),
        [']'] = (ConsoleKey.Oem6, ConsoleModifiers.None),
        ['}'] = (ConsoleKey.Oem6, ConsoleModifiers.Shift),
        ['['] = (ConsoleKey.Oem4, ConsoleModifiers.None),
        ['{'] = (ConsoleKey.Oem4, ConsoleModifiers.Shift),
        ['\''] = (ConsoleKey.Oem7, ConsoleModifiers.None),
        ['\"'] = (ConsoleKey.Oem7, ConsoleModifiers.Shift),
        [';'] = (ConsoleKey.Oem1, ConsoleModifiers.None),
        [':'] = (ConsoleKey.Oem1, ConsoleModifiers.Shift),
        ['/'] = (ConsoleKey.Oem2, ConsoleModifiers.None),
        ['?'] = (ConsoleKey.Oem2, ConsoleModifiers.Shift),
        ['.'] = (ConsoleKey.OemPeriod, ConsoleModifiers.None),
        ['>'] = (ConsoleKey.OemPeriod, ConsoleModifiers.Shift),
        [','] = (ConsoleKey.OemComma, ConsoleModifiers.None),
        ['<'] = (ConsoleKey.OemComma, ConsoleModifiers.Shift),
        ['`'] = (ConsoleKey.Oem3, ConsoleModifiers.None),
        ['~'] = (ConsoleKey.Oem3, ConsoleModifiers.Shift),
        ['1'] = (ConsoleKey.D1, ConsoleModifiers.None),
        ['!'] = (ConsoleKey.D1, ConsoleModifiers.Shift),
        ['2'] = (ConsoleKey.D2, ConsoleModifiers.None),
        ['@'] = (ConsoleKey.D2, ConsoleModifiers.Shift),
        ['3'] = (ConsoleKey.D3, ConsoleModifiers.None),
        ['#'] = (ConsoleKey.D3, ConsoleModifiers.Shift),
        ['4'] = (ConsoleKey.D4, ConsoleModifiers.None),
        ['$'] = (ConsoleKey.D4, ConsoleModifiers.Shift),
        ['5'] = (ConsoleKey.D5, ConsoleModifiers.None),
        ['%'] = (ConsoleKey.D5, ConsoleModifiers.Shift),
        ['6'] = (ConsoleKey.D6, ConsoleModifiers.None),
        ['^'] = (ConsoleKey.D6, ConsoleModifiers.Shift),
        ['7'] = (ConsoleKey.D7, ConsoleModifiers.None),
        ['&'] = (ConsoleKey.D7, ConsoleModifiers.Shift),
        ['8'] = (ConsoleKey.D8, ConsoleModifiers.None),
        ['*'] = (ConsoleKey.D8, ConsoleModifiers.Shift),
        ['9'] = (ConsoleKey.D9, ConsoleModifiers.None),
        ['('] = (ConsoleKey.D9, ConsoleModifiers.Shift),
        ['0'] = (ConsoleKey.D0, ConsoleModifiers.None),
        [')'] = (ConsoleKey.D0, ConsoleModifiers.Shift),
        ['\u007F'] = (ConsoleKey.Backspace, ConsoleModifiers.None),
    };

    #pragma warning disable format
    public bool TryGetInputEvent(out InputEvent ev)
    {
        var token = NextToken();
        bool result = true;
        switch (token.TokenType)
        {
            case InputTokenType.SOH: case InputTokenType.STX: case InputTokenType.ETX: case InputTokenType.EOT:
            case InputTokenType.ENQ: case InputTokenType.ACK: case InputTokenType.BEL: case InputTokenType.BS:
            case InputTokenType.HT:  case InputTokenType.LF:  case InputTokenType.VT:  case InputTokenType.FF:
            case InputTokenType.CR:  case InputTokenType.SO:  case InputTokenType.SI:  case InputTokenType.DLE:
            case InputTokenType.DC1: case InputTokenType.DC2: case InputTokenType.DC3: case InputTokenType.DC4:
            case InputTokenType.NAK: case InputTokenType.SYN: case InputTokenType.ETB: case InputTokenType.CAN:
            case InputTokenType.EM:  case InputTokenType.SUB: case InputTokenType.IS4: case InputTokenType.IS3:
            case InputTokenType.IS2: case InputTokenType.IS1:
                {
                    (ConsoleKey key, ConsoleModifiers mod) = _charToConsoleKey[(char)token.Ch];
                    ev = InputEvent.Create(key, mod, (char)token.Ch, GetRawData());
                    break;
                }
            case InputTokenType.ESC:
                result = ParseEscapeSequence(out ev);
                break;
            case InputTokenType.SP:
                ev = InputEvent.Create(ConsoleKey.Spacebar, ConsoleModifiers.None, (char)token.Ch, GetRawData());
                break;
            case InputTokenType.DEL:
                ev = InputEvent.Create(ConsoleKey.Backspace, ConsoleModifiers.None, (char)token.Ch, GetRawData());
                break;
            case InputTokenType.Char8Bit:
                throw new NotImplementedException();
            default:
                if(token.TokenType == InputTokenType.UpperCase)
                {
                    ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'A'), ConsoleModifiers.Shift, (char)token.Ch, GetRawData());
                }
                else if(token.TokenType == InputTokenType.LowerCase)
                {
                    ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'a'), ConsoleModifiers.None, (char)token.Ch, GetRawData());
                }
                else 
                {
                    (ConsoleKey key, ConsoleModifiers mod) = _charToConsoleKey[(char)token.Ch];
                    ev = InputEvent.Create(key, mod, (char)token.Ch, GetRawData());
                }
                break;
        }
        return result;
    }


    private bool ParseEscapeSequence(out InputEvent ev)
    {
        bool result = true;
        var token = NextToken();
        switch(token.TokenType)
        {
            case InputTokenType.Separator:
                ev = InputEvent.Create(ConsoleKey.Escape, ConsoleModifiers.None, (char)token.Ch, GetRawData());
                return true;
            case InputTokenType.SOH: case InputTokenType.STX: case InputTokenType.ETX: case InputTokenType.EOT:
            case InputTokenType.ENQ: case InputTokenType.ACK: case InputTokenType.BEL: case InputTokenType.BS:
            case InputTokenType.HT:  case InputTokenType.LF:  case InputTokenType.VT:  case InputTokenType.FF:
            case InputTokenType.CR:  case InputTokenType.SO:  case InputTokenType.SI:  case InputTokenType.DLE:
            case InputTokenType.DC1: case InputTokenType.DC2: case InputTokenType.DC3: case InputTokenType.DC4:
            case InputTokenType.NAK: case InputTokenType.SYN: case InputTokenType.ETB: case InputTokenType.CAN:
            case InputTokenType.EM:  case InputTokenType.SUB: case InputTokenType.IS4: case InputTokenType.IS3:
            case InputTokenType.IS2: case InputTokenType.IS1:
                {
                    (ConsoleKey key, ConsoleModifiers mod) = _charToConsoleKey[(char)token.Ch];
                    ev = InputEvent.Create(key, mod | ConsoleModifiers.Alt, '\u0000', GetRawData());
                    break;
                }
            case InputTokenType.ESC:
                ev = InputEvent.Create(ConsoleKey.Oem4, ConsoleModifiers.Alt | ConsoleModifiers.Control, '\u0000', GetRawData());
                break;
            default:
                switch((char)token.Ch)
                {
                    case 'O':
                        result = ParseSS3Sequence(out ev);
                        break;
                    case 'P':
                        result = ParseDCSSequence(out ev);
                        break;
                    case '[':
                        result = ParseCSISequence(out ev);
                        break;
                    case ']':
                        result = ParseOSCSequence(out ev);
                        break;
                    default:
                        if (token.TokenType == InputTokenType.UpperCase)
                        {
                            ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'A'), ConsoleModifiers.Alt | ConsoleModifiers.Shift, '\u0000', GetRawData());
                        }
                        else if (token.TokenType == InputTokenType.LowerCase)
                        {
                            ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'a'), ConsoleModifiers.Alt, '\u0000', GetRawData());
                        }
                        else
                        {
                            (ConsoleKey key, ConsoleModifiers mod) = _charToConsoleKey[(char)token.Ch];
                            ev = InputEvent.Create(key, mod | ConsoleModifiers.Alt, (char)token.Ch, GetRawData());
                        }
                        break;
                }
                break;
        }
        return result;
    }

    // <ESC>]...
    private bool ParseOSCSequence(out InputEvent ev)
    {
        bool result = true;
        var token = NextToken();
        if(token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.Oem6, ConsoleModifiers.Alt, '\u0000', GetRawData());
        }
        else
        {
            throw new NotImplementedException();
        }
        return result;
    }

    // <ESC>[...
    private bool ParseCSISequence(out InputEvent ev)
    {
        bool result = true;
        var token = NextToken();
        if (token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.Oem4, ConsoleModifiers.Alt, '\u0000', GetRawData());
        }
        else
        {
            throw new NotImplementedException();
        }
        return result;
    }

    private bool ParseDCSSequence(out InputEvent ev)
    {
        throw new NotImplementedException();
    }

    // <ESC>O
    private bool ParseSS3Sequence(out InputEvent ev)
    {
        bool result = true;
        var token = NextToken();
        if (token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.O, ConsoleModifiers.Alt | ConsoleModifiers.Shift, '\u0000', GetRawData());
        }
        else
        {
            switch((char)token.Ch)
            {
                case 'A':
                    ev = InputEvent.Create(ConsoleKey.UpArrow, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'B':
                    ev = InputEvent.Create(ConsoleKey.DownArrow, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'C':
                    ev = InputEvent.Create(ConsoleKey.RightArrow, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'D':
                    ev = InputEvent.Create(ConsoleKey.LeftArrow, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'E':
                    ev = InputEvent.Create(ConsoleKey.Clear, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'F':
                    ev = InputEvent.Create(ConsoleKey.End, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'H':
                    ev = InputEvent.Create(ConsoleKey.Home, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'P':
                    ev = InputEvent.Create(ConsoleKey.F1, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'Q':
                    ev = InputEvent.Create(ConsoleKey.F2, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'R':
                    ev = InputEvent.Create(ConsoleKey.F3, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                case 'S':
                    ev = InputEvent.Create(ConsoleKey.F3, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                default:
                    UngetToken(token);
                    ev = InputEvent.Create(ConsoleKey.O, ConsoleModifiers.Alt | ConsoleModifiers.Shift, '\u0000', GetRawData());
                    break;
            }
        }
        return result;
    }

#pragma warning restore format
}
