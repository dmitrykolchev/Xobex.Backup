using System.Runtime.CompilerServices;

namespace TermIn;

public class AnsiParser
{
    private readonly InputBuffer _inputBuffer;

    private readonly Dictionary<string, KeyCode> _ascKeyMap = new()
    {
        ["0"] = KeyCode.Key0, ["1"] = KeyCode.Key1, ["2"] = KeyCode.Key2, ["3"] = KeyCode.Key3, ["4"] = KeyCode.Key4,
        ["5"] = KeyCode.Key5, ["6"] = KeyCode.Key6, ["7"] = KeyCode.Key7, ["8"] = KeyCode.Key8, ["9"] = KeyCode.Key9,

        ["A"] = KeyCode.KeyA, ["B"] = KeyCode.KeyB, ["C"] = KeyCode.KeyC, ["D"] = KeyCode.KeyD, ["E"] = KeyCode.KeyE,
        ["F"] = KeyCode.KeyF, ["G"] = KeyCode.KeyG, ["H"] = KeyCode.KeyH, ["I"] = KeyCode.KeyI, ["J"] = KeyCode.KeyJ,
        ["K"] = KeyCode.KeyK, ["L"] = KeyCode.KeyL, ["M"] = KeyCode.KeyM, ["N"] = KeyCode.KeyN, ["O"] = KeyCode.KeyO,
        ["P"] = KeyCode.KeyP, ["Q"] = KeyCode.KeyQ, ["R"] = KeyCode.KeyR, ["S"] = KeyCode.KeyS, ["T"] = KeyCode.KeyT,
        ["U"] = KeyCode.KeyU, ["V"] = KeyCode.KeyV, ["W"] = KeyCode.KeyW, ["X"] = KeyCode.KeyX, ["Y"] = KeyCode.KeyY, 
        ["Z"] = KeyCode.KeyZ,

        ["a"] = KeyCode.KeyA, ["b"] = KeyCode.KeyB, ["c"] = KeyCode.KeyC, ["d"] = KeyCode.KeyD, ["e"] = KeyCode.KeyE,
        ["f"] = KeyCode.KeyF, ["g"] = KeyCode.KeyG, ["h"] = KeyCode.KeyH, ["i"] = KeyCode.KeyI, ["j"] = KeyCode.KeyJ,
        ["k"] = KeyCode.KeyK, ["l"] = KeyCode.KeyL, ["m"] = KeyCode.KeyM, ["n"] = KeyCode.KeyN, ["o"] = KeyCode.KeyO,
        ["p"] = KeyCode.KeyP, ["q"] = KeyCode.KeyQ, ["r"] = KeyCode.KeyR, ["s"] = KeyCode.KeyS, ["t"] = KeyCode.KeyT,
        ["u"] = KeyCode.KeyU, ["v"] = KeyCode.KeyV, ["w"] = KeyCode.KeyW, ["x"] = KeyCode.KeyX, ["y"] = KeyCode.KeyY,
        ["z"] = KeyCode.KeyZ,
    };

    private readonly Dictionary<string, KeyCode> _csiKeyMap = new ()
    {
        ["15"] = KeyCode.KeyF5,
        ["16"] = KeyCode.KeyF5,
        ["17"] = KeyCode.KeyF6,
        ["18"] = KeyCode.KeyF7,
        ["19"] = KeyCode.KeyF8,
        ["20"] = KeyCode.KeyF9,
        ["21"] = KeyCode.KeyF10,
        ["22"] = KeyCode.KeyF11,
        ["23"] = KeyCode.KeyF11,
        ["24"] = KeyCode.KeyF12,
    };

    private readonly Dictionary<string, KeyCode> _ss3KeyMap = new()
    {
        ["P"] = KeyCode.KeyF1,
        ["Q"] = KeyCode.KeyF2,
        ["R"] = KeyCode.KeyF3,
        ["S"] = KeyCode.KeyF4,
    };

    public enum ParseResult
    {
        Rejected = 0,
        Accepted,
        Ignored
    }

    public AnsiParser(LinuxInputAdapter adapter, LinuxConsoleAdapter con)
    {
        _inputBuffer = new InputBuffer(adapter, con);
    }

    public ParseResult TryParseEvent(out InputEvent ev)
    {
        int ch = _inputBuffer.GetChar();
        if (ch == (byte)'\x1B')
        {
            return ParseEscapeSequence(out ev);
        }
        ev = InputEvent.CreateKeyEvent(ch, _ascKeyMap["" + (char)ch], 0, "ASC", _inputBuffer.GetRawBuffer());
        return ParseResult.Accepted;
    }

    private ParseResult ParseEscapeSequence(out InputEvent ev)
    {
        int ch = _inputBuffer.GetChar();
        ParseResult result;
        switch (ch)
        {
            case (byte)'[':
                ch = _inputBuffer.GetChar();
                switch (ch)
                {
                    case (byte)'M':
                        return ParseX10Mouse(out ev) == ParseResult.Accepted ? ParseResult.Accepted : ParseResult.Ignored;
                    case (byte)'<':
                        return ParseSgrMouse(out ev) == ParseResult.Accepted ? ParseResult.Accepted : ParseResult.Ignored;
                    default:
                        _inputBuffer.Unget();
                        return ParseCSI(out ev);
                }
            case (byte)'O':
                return ParseSS3Key(out ev);
            case (byte)'P':
                return ParseDCS(out ev);
            case (byte)']':
                return ParseOSC(out ev);
            case (byte)'\x1B':
                result = ParseEscapeSequence(out ev);
                if (result == ParseResult.Accepted && ev.EventType == InputEventType.Key)
                {
                    throw new NotImplementedException("normalize keydown event");
                }
                ev = default;
                break;
            default:
                throw new InvalidOperationException($"unexpected char \\x{ch:X2}");
        }
        return result;
    }

    private ParseResult ParseCSI(out InputEvent ev)
    {
        ev = default;
        if (!ReadInt(out int keyCode))
        {
            return ParseResult.Rejected;
        }
        string key = keyCode.ToString();
        int ch = _inputBuffer.GetChar();
        if(ch != '~' && ch != ';')
        {
            _inputBuffer.Unget();
            return ParseResult.Rejected;
        }
        if(ch == '~')
        {
            ev = InputEvent.CreateKeyEvent(0, _csiKeyMap[key], 0, "CSI", _inputBuffer.GetRawBuffer());
        }
        else
        {
            if (!ReadInt(out int shiftState))
            {
                return ParseResult.Rejected;
            }
            if(!Read('~'))
            {
                return ParseResult.Rejected;
            }
            ev = InputEvent.CreateKeyEvent(0, _csiKeyMap[key], shiftState, "CSI", _inputBuffer.GetRawBuffer());
        }
        return ParseResult.Accepted;
    }

    private ParseResult ParseOSC(out InputEvent ev)
    {
        throw new NotImplementedException();
    }

    private ParseResult ParseDCS(out InputEvent ev)
    {
        throw new NotImplementedException();
    }

    private ParseResult ParseSS3Key(out InputEvent ev)
    {
        ev = default;
        int ch = _inputBuffer.GetChar();
        KeyCode keyCode;
        switch(ch)
        {
            case 'P':
                keyCode = _ss3KeyMap["P"];
                break;
            case 'Q':
                keyCode = _ss3KeyMap["Q"];
                break;
            case 'R':
                keyCode = _ss3KeyMap["R"];
                break;
            case 'S':
                keyCode = _ss3KeyMap["S"];
                break;
            default:
                return ParseResult.Rejected;
        }
        ev = InputEvent.CreateKeyEvent(0, keyCode, 0, "SS3", _inputBuffer.GetRawBuffer());
        return ParseResult.Accepted;
    }
    /// <summary>
    /// Reads \x1B[<35;54;22M
    /// </summary>
    /// <param name="ev"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private ParseResult ParseSgrMouse(out InputEvent ev)
    {
        ev = default;
        if(!ReadInt(out int state))
        {
            return ParseResult.Rejected;
        }
        if (!Read(';'))
        {
            return ParseResult.Rejected;
        }
        if(!ReadInt(out int x))
        {
            return ParseResult.Rejected;
        }    
        if(!Read(';'))
        {
            return ParseResult.Rejected;
        }
        if(!ReadInt(out int y))
        {
            return ParseResult.Rejected;
        }
        int type = _inputBuffer.GetChar();
        if(type != 'M' && type != 'm')
        {
            return ParseResult.Rejected;
        }
        ev = InputEvent.CreateMouseEvent(--x, --y, state, type, _inputBuffer.GetRawBuffer());
        return ParseResult.Accepted;
    }

    private ParseResult ParseX10Mouse(out InputEvent ev)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Read(char ch)
    {
        int current = _inputBuffer.GetChar();
        if (ch != current)
        {
            _inputBuffer.Unget();
            return false;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool Read(string text)
    {
        for (int i = 0; i < text.Length; ++i)
        {
            int current = _inputBuffer.GetChar();
            if (text[i] != current)
            {
                _inputBuffer.Unget();
                return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool ReadInt(out int value)
    {
        bool result = false;
        value = 0;
        for (; ; )
        {
            int ch = _inputBuffer.GetChar();
            if (ch >= (byte)'0' && ch <= (byte)'9')
            {
                value = value * 10 + (ch - (byte)'0');
                result = true;
            }
            else
            {
                _inputBuffer.Unget();
                break;
            }
        }
        return result;
    }
}
