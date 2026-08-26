// <copyright file="LinuxTerminalParser.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Buffers;
using System.Text;
using Xobex.Console.Abstractions;
using Key = System.ConsoleKey;
using Mod = System.ConsoleModifiers;

namespace Xobex.Console.Linux;

#pragma warning disable format
public class LinuxTerminalParser: ITerminalParser
{
    private static readonly Dictionary<char, (ConsoleKey Key, ConsoleModifiers Mod)> _charToConsoleKey = new()
    {
        ['\u0001'] = (Key.A, Mod.Control),
        ['\u0002'] = (Key.B, Mod.Control),
        ['\u0003'] = (Key.C, Mod.Control),
        ['\u0004'] = (Key.D, Mod.Control),
        ['\u0005'] = (Key.E, Mod.Control),
        ['\u0006'] = (Key.F, Mod.Control),
        ['\u0007'] = (Key.G, Mod.Control),
        ['\u0008'] = (Key.Backspace, Mod.Control),
        ['\u0009'] = (Key.Tab, Mod.None),
        ['\u000A'] = (Key.J, Mod.Control),
        ['\u000B'] = (Key.K, Mod.Control),
        ['\u000C'] = (Key.L, Mod.Control),
        ['\u000D'] = (Key.Enter, Mod.None),
        ['\u000E'] = (Key.N, Mod.Control),
        ['\u000F'] = (Key.O, Mod.Control),
        ['\u0010'] = (Key.P, Mod.Control),
        ['\u0011'] = (Key.Q, Mod.Control),
        ['\u0012'] = (Key.R, Mod.Control),
        ['\u0013'] = (Key.S, Mod.Control),
        ['\u0014'] = (Key.T, Mod.Control),
        ['\u0015'] = (Key.U, Mod.Control),
        ['\u0016'] = (Key.V, Mod.Control),
        ['\u0017'] = (Key.W, Mod.Control),
        ['\u0018'] = (Key.X, Mod.Control),
        ['\u0019'] = (Key.Y, Mod.Control),
        ['\u001A'] = (Key.Z, Mod.Control),
        ['\u001B'] = (Key.Escape, Mod.None),
        ['\u001C'] = (Key.Oem5, Mod.Control),
        ['\u001D'] = (Key.Oem6, Mod.Control),
        ['\u001E'] = (Key.NumPad6, Mod.Control),
        ['\u001F'] = (Key.Oem2, Mod.Control),
        ['\u0020'] = (Key.Spacebar, Mod.None),
        ['-'] = (Key.OemMinus, Mod.None),
        ['_'] = (Key.OemMinus, Mod.Shift),
        ['='] = (Key.OemPlus, Mod.None),
        ['+'] = (Key.OemPlus, Mod.Shift),
        ['\\'] = (Key.Oem5, Mod.None),
        ['|'] = (Key.Oem5, Mod.Shift),
        [']'] = (Key.Oem6, Mod.None),
        ['}'] = (Key.Oem6, Mod.Shift),
        ['['] = (Key.Oem4, Mod.None),
        ['{'] = (Key.Oem4, Mod.Shift),
        ['\''] = (Key.Oem7, Mod.None),
        ['\"'] = (Key.Oem7, Mod.Shift),
        [';'] = (Key.Oem1, Mod.None),
        [':'] = (Key.Oem1, Mod.Shift),
        ['/'] = (Key.Oem2, Mod.None),
        ['?'] = (Key.Oem2, Mod.Shift),
        ['.'] = (Key.OemPeriod, Mod.None),
        ['>'] = (Key.OemPeriod, Mod.Shift),
        [','] = (Key.OemComma, Mod.None),
        ['<'] = (Key.OemComma, Mod.Shift),
        ['`'] = (Key.Oem3, Mod.None),
        ['~'] = (Key.Oem3, Mod.Shift),
        ['1'] = (Key.D1, Mod.None),
        ['!'] = (Key.D1, Mod.Shift),
        ['2'] = (Key.D2, Mod.None),
        ['@'] = (Key.D2, Mod.Shift),
        ['3'] = (Key.D3, Mod.None),
        ['#'] = (Key.D3, Mod.Shift),
        ['4'] = (Key.D4, Mod.None),
        ['$'] = (Key.D4, Mod.Shift),
        ['5'] = (Key.D5, Mod.None),
        ['%'] = (Key.D5, Mod.Shift),
        ['6'] = (Key.D6, Mod.None),
        ['^'] = (Key.D6, Mod.Shift),
        ['7'] = (Key.D7, Mod.None),
        ['&'] = (Key.D7, Mod.Shift),
        ['8'] = (Key.D8, Mod.None),
        ['*'] = (Key.D8, Mod.Shift),
        ['9'] = (Key.D9, Mod.None),
        ['('] = (Key.D9, Mod.Shift),
        ['0'] = (Key.D0, Mod.None),
        [')'] = (Key.D0, Mod.Shift),
        ['\u007F'] = (Key.Backspace, Mod.None),
    };

    // Final byte of a parameterless CSI sequence (<ESC>[A ...) mapped to a console key.
    private static readonly Dictionary<char, Key> _csiFinalToConsoleKey = new()
    {
        ['A'] = Key.UpArrow,
        ['B'] = Key.DownArrow,
        ['C'] = Key.RightArrow,
        ['D'] = Key.LeftArrow,
        ['E'] = Key.Clear,
        ['F'] = Key.End,
        ['H'] = Key.Home,
        ['P'] = Key.F1,
        ['Q'] = Key.F2,
        ['S'] = Key.F4,
    };

    // First numeric parameter of a <ESC>[<n>~ sequence mapped to a console key.
    private static readonly Dictionary<int, ConsoleKey> _csiTildeToConsoleKey = new()
    {
        [1] = Key.Home,
        [2] = Key.Insert,
        [3] = Key.Delete,
        [4] = Key.End,
        [5] = Key.PageUp,
        [6] = Key.PageDown,
        [7] = Key.Home,
        [8] = Key.End,
        [11] = Key.F1,
        [12] = Key.F2,
        [13] = Key.F3,
        [14] = Key.F4,
        [15] = Key.F5,
        [17] = Key.F6,
        [18] = Key.F7,
        [19] = Key.F8,
        [20] = Key.F9,
        [21] = Key.F10,
        [23] = Key.F11,
        [24] = Key.F12,
    };

    private readonly LinuxInputBuffer _buffer;
    private byte[] _rawData;
    private int _rawDataPosition;
    private readonly Stack<InputToken> _ungetBuffer = new();

    internal LinuxTerminalParser(LinuxInputBuffer inputBuffer)
    {
        _buffer = inputBuffer;
        _rawData = new byte[128];
        _rawDataPosition = 0;
    }

    public bool TryGetInputEvent(out InputEvent? ev)
    {
        try
        {
            var token = NextToken();
            var result = true;
            switch (token.TokenType)
            {
                case InputTokenType.Separator:
                    // Idle timeout marker, not an input event
                    ev = null;
                    return false;
                case InputTokenType.SOH: case InputTokenType.STX: case InputTokenType.ETX: case InputTokenType.EOT:
                case InputTokenType.ENQ: case InputTokenType.ACK: case InputTokenType.BEL: case InputTokenType.BS:
                case InputTokenType.HT:  case InputTokenType.LF:  case InputTokenType.VT:  case InputTokenType.FF:
                case InputTokenType.CR:  case InputTokenType.SO:  case InputTokenType.SI:  case InputTokenType.DLE:
                case InputTokenType.DC1: case InputTokenType.DC2: case InputTokenType.DC3: case InputTokenType.DC4:
                case InputTokenType.NAK: case InputTokenType.SYN: case InputTokenType.ETB: case InputTokenType.CAN:
                case InputTokenType.EM:  case InputTokenType.SUB: case InputTokenType.IS4: case InputTokenType.IS3:
                case InputTokenType.IS2: case InputTokenType.IS1:
                    {
                        ev = CreateKeyEvent((char)token.Ch, Mod.None, GetRawData());
                        break;
                    }
                case InputTokenType.ESC:
                    result = ParseEscapeSequence(out ev);
                    break;
                case InputTokenType.SP:
                    ev = InputEvent.Create(Key.Spacebar, Mod.None, (char)token.Ch, true, GetRawData());
                    break;
                case InputTokenType.DEL:
                    ev = InputEvent.Create(Key.Backspace, Mod.None, (char)token.Ch, true, GetRawData());
                    break;
                case InputTokenType.Char8Bit:
                    result = ParseUtf8Character(token.Ch, Mod.None, out ev);
                    break;
                default:
                    if (token.TokenType == InputTokenType.UpperCase)
                    {
                        ev = InputEvent.Create(Key.A + (token.Ch - 'A'), Mod.Shift, (char)token.Ch, true, GetRawData());
                    }
                    else if (token.TokenType == InputTokenType.LowerCase)
                    {
                        ev = InputEvent.Create(Key.A + (token.Ch - 'a'), Mod.None, (char)token.Ch, true, GetRawData());
                    }
                    else
                    {
                        ev = CreateKeyEvent((char)token.Ch, Mod.None, GetRawData());
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

    // Maps a character to a key event, unmapped characters produce an event with Key.None.
    private static InputEvent CreateKeyEvent(char ch, ConsoleModifiers additionalModifiers, ReadOnlySpan<byte> rawData)
    {
        if (_charToConsoleKey.TryGetValue(ch, out var entry))
        {
            return InputEvent.Create(entry.Key, entry.Mod | additionalModifiers, ch, true, rawData);
        }
        return InputEvent.Create(Key.None, additionalModifiers, ch, true, rawData);
    }

    private bool ParseEscapeSequence(out InputEvent? ev)
    {
        var result = true;
        var token = NextToken();
        switch(token.TokenType)
        {
            case InputTokenType.Separator:
                ev = InputEvent.Create(Key.Escape, Mod.None, (char)token.Ch, true, GetRawData());
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
                    ev = InputEvent.Create(key, mod | Mod.Alt, '\u0000', true, GetRawData());
                    break;
                }
            case InputTokenType.ESC:
                ev = InputEvent.Create(Key.Oem4, Mod.Alt | Mod.Control, '\u0000', true, GetRawData());
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
                            ev = InputEvent.Create(Key.A + (token.Ch - 'A'), Mod.Alt | Mod.Shift, '\u0000', true, GetRawData());
                        }
                        else if (token.TokenType == InputTokenType.LowerCase)
                        {
                            ev = InputEvent.Create(Key.A + (token.Ch - 'a'), Mod.Alt, '\u0000', true, GetRawData());
                        }
                        else if (token.TokenType == InputTokenType.Char8Bit)
                        {
                            // Alt plus a multi byte UTF-8 character
                            result = ParseUtf8Character(token.Ch, Mod.Alt, out ev);
                        }
                        else
                        {
                            ev = CreateKeyEvent((char)token.Ch, Mod.Alt, GetRawData());
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
            ev = InputEvent.Create(Key.Oem6, Mod.Alt, '\u0000', true, GetRawData());
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
            ev = InputEvent.Create(Key.Oem4, Mod.Alt, '\u0000', true, GetRawData());
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
        var modifiers = Mod.None;
        if (finalChar == 'Z')
        {
            // <ESC>[Z is back tab
            key = Key.Tab;
            modifiers = Mod.Shift;
        }
        else if (finalChar == '~' && parameters.Count > 0)
        {
            if (!_csiTildeToConsoleKey.TryGetValue(parameters[0], out key))
            {
                ev = null;
                return false;
            }
            modifiers = parameters.Count > 1 ? GetModifiers(parameters[1]) : Mod.None;
        }
        else if (_csiFinalToConsoleKey.TryGetValue(finalChar, out key))
        {
            modifiers = parameters.Count > 1 ? GetModifiers(parameters[1]) : Mod.None;
        }
        else
        {
            // Cursor position report (<ESC>[<n>;<m>R) and unknown sequences are consumed silently
            ev = null;
            return false;
        }
        ev = InputEvent.Create(key, modifiers, '\u0000', true, GetRawData());
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
        var modifiers = Mod.None;
        if ((value & 0x01) != 0)
        {
            modifiers |= Mod.Shift;
        }
        if ((value & 0x02) != 0)
        {
            modifiers |= Mod.Alt;
        }
        if ((value & 0x04) != 0)
        {
            modifiers |= Mod.Control;
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
        var modifiers = Mod.None;
        if ((code & 0x04) != 0)
        {
            modifiers |= Mod.Shift;
        }
        if ((code & 0x08) != 0)
        {
            modifiers |= Mod.Alt;
        }
        if ((code & 0x10) != 0)
        {
            modifiers |= Mod.Control;
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
            action = (MouseAction)((int)MouseAction.WheelUp + buttonIndex);
            button = MouseButton.None;
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
            ev = InputEvent.Create(Key.P, Mod.Alt, '\u0000', true, GetRawData());
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
            ev = InputEvent.Create(Key.O, Mod.Alt | Mod.Shift, '\u0000', true, GetRawData());
        }
        else
        {
            switch((char)token.Ch)
            {
                case 'A':
                    ev = InputEvent.Create(Key.UpArrow, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'B':
                    ev = InputEvent.Create(Key.DownArrow, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'C':
                    ev = InputEvent.Create(Key.RightArrow, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'D':
                    ev = InputEvent.Create(Key.LeftArrow, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'E':
                    ev = InputEvent.Create(Key.Clear, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'F':
                    ev = InputEvent.Create(Key.End, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'H':
                    ev = InputEvent.Create(Key.Home, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'P':
                    ev = InputEvent.Create(Key.F1, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'Q':
                    ev = InputEvent.Create(Key.F2, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'R':
                    ev = InputEvent.Create(Key.F3, Mod.None, '\u0000', true, GetRawData());
                    break;
                case 'S':
                    ev = InputEvent.Create(Key.F4, Mod.None, '\u0000', true, GetRawData());
                    break;
                default:
                    UngetToken(token);
                    ev = InputEvent.Create(Key.O, Mod.Alt | Mod.Shift, '\u0000', true, GetRawData());
                    break;
            }
        }
        return result;
    }

    // Assembles a multi byte UTF-8 character started with the given lead byte.
    private bool ParseUtf8Character(byte leadByte, ConsoleModifiers modifiers, out InputEvent? ev)
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
        ev = InputEvent.Create(Key.None, modifiers, (char)rune.Value, true, GetRawData());
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
}
#pragma warning restore format
