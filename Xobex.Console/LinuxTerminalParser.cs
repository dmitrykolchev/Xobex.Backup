// <copyright file="LinuxTerminalParser.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers;
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
    private byte[] _rawData;
    private int _rawDataPosition;
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
            if (_rawDataPosition == _rawData.Length)
            {
                var rawData = new byte[_rawData.Length * 2];
                Array.Copy(_rawData, rawData, _rawData.Length);
                _rawData = rawData;
            }
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

    // Final byte of a parameterless CSI sequence (<ESC>[A ...) mapped to a console key.
    private static readonly Dictionary<char, ConsoleKey> _csiFinalToConsoleKey = new()
    {
        ['A'] = ConsoleKey.UpArrow,
        ['B'] = ConsoleKey.DownArrow,
        ['C'] = ConsoleKey.RightArrow,
        ['D'] = ConsoleKey.LeftArrow,
        ['E'] = ConsoleKey.Clear,
        ['F'] = ConsoleKey.End,
        ['H'] = ConsoleKey.Home,
        ['P'] = ConsoleKey.F1,
        ['Q'] = ConsoleKey.F2,
        ['S'] = ConsoleKey.F4,
    };

    // First numeric parameter of a <ESC>[<n>~ sequence mapped to a console key.
    private static readonly Dictionary<int, ConsoleKey> _csiTildeToConsoleKey = new()
    {
        [1] = ConsoleKey.Home,
        [2] = ConsoleKey.Insert,
        [3] = ConsoleKey.Delete,
        [4] = ConsoleKey.End,
        [5] = ConsoleKey.PageUp,
        [6] = ConsoleKey.PageDown,
        [7] = ConsoleKey.Home,
        [8] = ConsoleKey.End,
        [11] = ConsoleKey.F1,
        [12] = ConsoleKey.F2,
        [13] = ConsoleKey.F3,
        [14] = ConsoleKey.F4,
        [15] = ConsoleKey.F5,
        [17] = ConsoleKey.F6,
        [18] = ConsoleKey.F7,
        [19] = ConsoleKey.F8,
        [20] = ConsoleKey.F9,
        [21] = ConsoleKey.F10,
        [23] = ConsoleKey.F11,
        [24] = ConsoleKey.F12,
    };

    #pragma warning disable format
    public bool TryGetInputEvent(out InputEvent? ev)
    {
        try
        {
            var token = NextToken();
            var result = true;
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
                        (var key, var mod) = _charToConsoleKey[(char)token.Ch];
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
                    result = ParseUtf8Character(token.Ch, out ev);
                    break;
                default:
                    if (token.TokenType == InputTokenType.UpperCase)
                    {
                        ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'A'), ConsoleModifiers.Shift, (char)token.Ch, GetRawData());
                    }
                    else if (token.TokenType == InputTokenType.LowerCase)
                    {
                        ev = InputEvent.Create(ConsoleKey.A + (token.Ch - 'a'), ConsoleModifiers.None, (char)token.Ch, GetRawData());
                    }
                    else
                    {
                        (var key, var mod) = _charToConsoleKey[(char)token.Ch];
                        ev = InputEvent.Create(key, mod, (char)token.Ch, GetRawData());
                    }
                    break;
            }
            return result;
        }
        finally
        {
            // reset raw buffer when exit
            _rawDataPosition = 0;
        }
    }

    private bool ParseEscapeSequence(out InputEvent? ev)
    {
        var result = true;
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
                    (var key, var mod) = _charToConsoleKey[(char)token.Ch];
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
                            (var key, var mod) = _charToConsoleKey[(char)token.Ch];
                            ev = InputEvent.Create(key, mod | ConsoleModifiers.Alt, (char)token.Ch, GetRawData());
                        }
                        break;
                }
                break;
        }
        return result;
    }

    // <ESC>]... string sequence terminated by BEL or ST, consumed and ignored
    private bool ParseOSCSequence(out InputEvent? ev)
    {
        var token = NextToken();
        if (token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.Oem6, ConsoleModifiers.Alt, '\u0000', GetRawData());
            return true;
        }
        UngetToken(token);
        SkipStringSequence();
        ev = null;
        return false;
    }

    // Consumes a string sequence terminated by BEL (\u0007) or ST (\u001b\\).
    private void SkipStringSequence()
    {
        for (; ; )
        {
            var token = NextToken();
            if (token.TokenType is InputTokenType.BEL or InputTokenType.Separator)
            {
                return;
            }
            if (token.TokenType == InputTokenType.ESC)
            {
                var next = NextToken();
                if ((char)next.Ch == '\\')
                {
                    return;
                }
            }
        }
    }

    // <ESC>[...
    private bool ParseCSISequence(out InputEvent? ev)
    {
        var token = NextToken();
        if (token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.Oem4, ConsoleModifiers.Alt, '\u0000', GetRawData());
            return true;
        }
        if ((char)token.Ch == '<')
        {
            return ParseSGRMouseSequence(out ev);
        }
        UngetToken(token);
        var result = ReadParameters(out var parameters, out var final);
        if (!result || (final.TokenType == InputTokenType.Separator))
        {
            // Sequence interrupted by a separator token, cannot be completed
            ev = null;
            return false;
        }
        var finalChar = (char)final.Ch;
        if (parameters.Count == 0 && finalChar == 'M')
        {
            return ParseX10MouseSequence(out ev);
        }
        ConsoleKey key;
        var modifiers = ConsoleModifiers.None;
        if (finalChar == 'Z')
        {
            // <ESC>[Z is back tab
            key = ConsoleKey.Tab;
            modifiers = ConsoleModifiers.Shift;
        }
        else if (finalChar == '~' && parameters.Count > 0)
        {
            if (!_csiTildeToConsoleKey.TryGetValue(parameters[0], out key))
            {
                ev = null;
                return false;
            }
            modifiers = parameters.Count > 1 ? GetModifiers(parameters[1]) : ConsoleModifiers.None;
        }
        else if (_csiFinalToConsoleKey.TryGetValue(finalChar, out key))
        {
            modifiers = parameters.Count > 1 ? GetModifiers(parameters[1]) : ConsoleModifiers.None;
        }
        else
        {
            // Cursor position report (<ESC>[<n>;<m>R) and unknown sequences are consumed silently
            ev = null;
            return false;
        }
        ev = InputEvent.Create(key, modifiers, '\u0000', GetRawData());
        return true;
    }

    // Reads numeric CSI parameters separated by ';' up to the terminating token.
    // Returns false if the sequence was interrupted by a separator token.
    private bool ReadParameters(out List<int> parameters, out InputToken final)
    {
        parameters = [];
        var current = -1;
        for (; ; )
        {
            var token = NextToken();
            switch (token.TokenType)
            {
                case InputTokenType.Digit:
                    current = (Math.Max(current, 0) * 10) + (token.Ch - '0');
                    break;
                case InputTokenType.Symbol when (char)token.Ch == ';':
                    parameters.Add(current);
                    current = -1;
                    break;
                default:
                    if (current >= 0 || parameters.Count > 0)
                    {
                        parameters.Add(current);
                    }
                    final = token;
                    return token.TokenType != InputTokenType.Separator;
            }
        }
    }

    // Decodes an xterm modifier parameter, shift=1, alt=2, ctrl=4 encoded as parameter - 1.
    private static ConsoleModifiers GetModifiers(int parameter)
    {
        var value = Math.Max(parameter, 1) - 1;
        var modifiers = ConsoleModifiers.None;
        if ((value & 0x01) != 0)
        {
            modifiers |= ConsoleModifiers.Shift;
        }
        if ((value & 0x02) != 0)
        {
            modifiers |= ConsoleModifiers.Alt;
        }
        if ((value & 0x04) != 0)
        {
            modifiers |= ConsoleModifiers.Control;
        }
        return modifiers;
    }

    // Parses SGR mouse encoding <ESC>[<{code};{column};{row}(M|m).
    private bool ParseSGRMouseSequence(out InputEvent? ev)
    {
        var result = ReadParameters(out var parameters, out var final);
        if (!result || parameters.Count < 3)
        {
            ev = null;
            return false;
        }
        var released = (char)final.Ch == 'm';
        if (!released && (char)final.Ch != 'M')
        {
            ev = null;
            return false;
        }
        var column = Math.Max(parameters[1], 1);
        var row = Math.Max(parameters[2], 1);
        if (!TryCreateMouseEvent(parameters[0], column, row, released, out var mouse))
        {
            ev = null;
            return false;
        }
        ev = InputEvent.Create(mouse, GetRawData());
        return true;
    }

    // Parses legacy X10 mouse encoding <ESC>[M{code}{column}{row}.
    private bool ParseX10MouseSequence(out InputEvent? ev)
    {
        var codeToken = NextToken();
        var columnToken = NextToken();
        var rowToken = NextToken();
        if (codeToken.TokenType == InputTokenType.Separator || columnToken.TokenType == InputTokenType.Separator || rowToken.TokenType == InputTokenType.Separator ||
            codeToken.Ch < 0x20 || columnToken.Ch < 0x20 || rowToken.Ch < 0x20)
        {
            ev = null;
            return false;
        }
        var code = codeToken.Ch - 0x20;
        var released = (code & 0x03) == 0x03;
        if (!TryCreateMouseEvent(code, columnToken.Ch - 0x20, rowToken.Ch - 0x20, released, out var mouse))
        {
            ev = null;
            return false;
        }
        ev = InputEvent.Create(mouse, GetRawData());
        return true;
    }

    // Decodes an xterm mouse button code into a mouse event. Column and row are one based.
    private static bool TryCreateMouseEvent(int code, int column, int row, bool released, out InputEvent.MouseEvent mouse)
    {
        mouse = default;
        if ((uint)code > 0x7F)
        {
            return false;
        }
        var modifiers = ConsoleModifiers.None;
        if ((code & 0x04) != 0)
        {
            modifiers |= ConsoleModifiers.Shift;
        }
        if ((code & 0x08) != 0)
        {
            modifiers |= ConsoleModifiers.Alt;
        }
        if ((code & 0x10) != 0)
        {
            modifiers |= ConsoleModifiers.Control;
        }
        var buttonIndex = code & 0x03;
        var button = buttonIndex switch
        {
            0 => MouseButton.Left,
            1 => MouseButton.Middle,
            2 => MouseButton.Right,
            _ => MouseButton.None,
        };
        MouseAction action;
        if ((code & 0x40) != 0)
        {
            button = (MouseButton)((int)MouseButton.WheelUp + buttonIndex);
            action = MouseAction.Down;
        }
        else if (released)
        {
            action = MouseAction.Up;
        }
        else if ((code & 0x20) != 0 || button == MouseButton.None)
        {
            action = MouseAction.Move;
        }
        else
        {
            action = MouseAction.Down;
        }
        mouse = new InputEvent.MouseEvent
        {
            Button = button,
            Action = action,
            Mod = modifiers,
            X = column - 1,
            Y = row - 1,
        };
        return true;
    }

    // <ESC>P... string sequence terminated by ST, consumed and ignored
    private bool ParseDCSSequence(out InputEvent? ev)
    {
        var token = NextToken();
        if (token.TokenType == InputTokenType.Separator)
        {
            ev = InputEvent.Create(ConsoleKey.P, ConsoleModifiers.Alt, '\u0000', GetRawData());
            return true;
        }
        UngetToken(token);
        SkipStringSequence();
        ev = null;
        return false;
    }

    // <ESC>O
    private bool ParseSS3Sequence(out InputEvent? ev)
    {
        var result = true;
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
                    ev = InputEvent.Create(ConsoleKey.F4, ConsoleModifiers.None, '\u0000', GetRawData());
                    break;
                default:
                    UngetToken(token);
                    ev = InputEvent.Create(ConsoleKey.O, ConsoleModifiers.Alt | ConsoleModifiers.Shift, '\u0000', GetRawData());
                    break;
            }
        }
        return result;
    }

    // Assembles a multi byte UTF-8 character started with the given lead byte.
    private bool ParseUtf8Character(byte leadByte, out InputEvent? ev)
    {
        Span<byte> bytes = stackalloc byte[4];
        var count = GetUtf8SequenceLength(leadByte);
        if (count == 0)
        {
            // Unexpected continuation byte or invalid lead byte
            ev = null;
            return false;
        }
        bytes[0] = leadByte;
        for (var i = 1; i < count; ++i)
        {
            var token = NextToken();
            if (token.TokenType != InputTokenType.Char8Bit || (token.Ch & 0xC0) != 0x80)
            {
                ev = null;
                return false;
            }
            bytes[i] = token.Ch;
        }
        if (Rune.DecodeFromUtf8(bytes[..count], out var rune, out var consumed) != OperationStatus.Done || consumed != count)
        {
            ev = null;
            return false;
        }
        ev = InputEvent.Create(ConsoleKey.None, ConsoleModifiers.None, (char)rune.Value, GetRawData());
        return true;
    }

    // Returns the number of bytes in a UTF-8 sequence started with the given lead byte, 0 if the lead byte is invalid.
    private static int GetUtf8SequenceLength(byte leadByte)
    {
        return leadByte switch
        {
            >= 0xC2 and <= 0xDF => 2,
            >= 0xE0 and <= 0xEF => 3,
            >= 0xF0 and <= 0xF4 => 4,
            _ => 0,
        };
    }

#pragma warning restore format
}
