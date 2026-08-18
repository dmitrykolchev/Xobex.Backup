using System;
using System.Text;


namespace TermIn;

public sealed class AnsiInputParser
{
    private enum State
    {
        Ground,
        Escape,
        Csi,
        CsiParam,
        Ss3,
        SgrMouseParam
    }

    private State _state = State.Ground;

    // Буфер параметров CSI/SGR (например, CSI 1;5 A -> params: [1, 5])
    private readonly int[] _params = new int[8];
    private int _paramCount = 0;
    private int _currentParam = 0;
    private bool _hasParam = false;
    private bool _isSgrMouse = false;

    // Буфер для сборки UTF-8 символов
    private readonly byte[] _utf8Buffer = new byte[4];
    private int _utf8Count = 0;
    private int _utf8Expected = 0;

    /// <summary>
    /// Парсит входной поток байтов. Генерирует события через callback (для исключения аллокаций).
    /// </summary>
    public void Parse(ReadOnlySpan<byte> data, Action<InputRecord> onEvent)
    {
        for (int i = 0; i < data.Length; i++)
        {
            byte b = data[i];
            ProcessByte(b, onEvent);
        }
    }

    /// <summary>
    /// Сброс зависшего ESC по таймауту.
    /// </summary>
    public void FlushEscape(Action<InputRecord> onEvent)
    {
        if (_state == State.Escape)
        {
            _state = State.Ground;
            onEvent(new InputRecord(new KeyEventRecord(ConsoleKey.Escape, '\x1b', ModifierKeys.None)));
        }
    }

    private void ResetParams()
    {
        _paramCount = 0;
        _currentParam = 0;
        _hasParam = false;
        _isSgrMouse = false;
    }

    private void PushParam()
    {
        if (_paramCount < _params.Length)
        {
            _params[_paramCount++] = _hasParam ? _currentParam : 0;
        }
        _currentParam = 0;
        _hasParam = false;
    }

    private void ProcessByte(byte b, Action<InputRecord> onEvent)
    {
        switch (_state)
        {
            case State.Ground:
                ProcessGround(b, onEvent);
                break;

            case State.Escape:
                ProcessEscape(b, onEvent);
                break;

            case State.Csi:
            case State.CsiParam:
                ProcessCsi(b, onEvent);
                break;

            case State.Ss3:
                ProcessSs3(b, onEvent);
                break;

            case State.SgrMouseParam:
                ProcessSgrMouse(b, onEvent);
                break;
        }
    }

    private void ProcessGround(byte b, Action<InputRecord> onEvent)
    {
        if (b == 0x1B) // ESC
        {
            _state = State.Escape;
            return;
        }

        // Обработка UTF-8 потока
        if (_utf8Expected == 0)
        {
            if ((b & 0x80) == 0) // 1-byte ASCII
            {
                DispatchControlOrAscii(b, onEvent);
            }
            else if ((b & 0xE0) == 0xC0) { _utf8Buffer[0] = b; _utf8Count = 1; _utf8Expected = 2; }
            else if ((b & 0xF0) == 0xE0) { _utf8Buffer[0] = b; _utf8Count = 1; _utf8Expected = 3; }
            else if ((b & 0xF8) == 0xF0) { _utf8Buffer[0] = b; _utf8Count = 1; _utf8Expected = 4; }
        }
        else
        {
            _utf8Buffer[_utf8Count++] = b;
            if (_utf8Count == _utf8Expected)
            {
                Span<char> chars = stackalloc char[2];
                int decoded = Encoding.UTF8.GetChars(_utf8Buffer.AsSpan(0, _utf8Count), chars);
                if (decoded > 0)
                {
                    onEvent(new InputRecord(new KeyEventRecord((ConsoleKey)chars[0], chars[0], ModifierKeys.None)));
                }
                _utf8Expected = 0;
                _utf8Count = 0;
            }
        }
    }

    private void ProcessEscape(byte b, Action<InputRecord> onEvent)
    {
        ResetParams();

        switch (b)
        {
            case (byte)'[':
                _state = State.Csi;
                break;
            case (byte)'O':
                _state = State.Ss3;
                break;
            case 0x1B: // Double Escape
                onEvent(new InputRecord(new KeyEventRecord(ConsoleKey.Escape, '\x1b', ModifierKeys.None)));
                break;
            default:
                // Alt + символ (Meta-префикс)
                _state = State.Ground;
                if (b < 128)
                {
                    char ch = (char)b;
                    onEvent(new InputRecord(new KeyEventRecord((ConsoleKey)ch, ch, ModifierKeys.Alt)));
                }
                break;
        }
    }

    private void ProcessCsi(byte b, Action<InputRecord> onEvent)
    {
        if (b == (byte)'<')
        {
            _isSgrMouse = true;
            _state = State.SgrMouseParam;
            return;
        }

        if (b >= (byte)'0' && b <= (byte)'9')
        {
            _currentParam = _currentParam * 10 + (b - '0');
            _hasParam = true;
            _state = State.CsiParam;
            return;
        }

        if (b == (byte)';')
        {
            PushParam();
            return;
        }

        // Финальный байт команды CSI
        PushParam();
        _state = State.Ground;
        DispatchCsiKey((char)b, onEvent);
    }

    private void ProcessSgrMouse(byte b, Action<InputRecord> onEvent)
    {
        if (b >= (byte)'0' && b <= (byte)'9')
        {
            _currentParam = _currentParam * 10 + (b - '0');
            _hasParam = true;
            return;
        }

        if (b == (byte)';')
        {
            PushParam();
            return;
        }

        if (b == (byte)'M' || b == (byte)'m')
        {
            PushParam();
            _state = State.Ground;
            DispatchSgrMouseEvent((char)b, onEvent);
        }
        else
        {
            // Нарушение протокола, сброс
            _state = State.Ground;
        }
    }

    private void ProcessSs3(byte b, Action<InputRecord> onEvent)
    {
        _state = State.Ground;
        ConsoleKey key = b switch
        {
            (byte)'P' => ConsoleKey.F1,
            (byte)'Q' => ConsoleKey.F2,
            (byte)'R' => ConsoleKey.F3,
            (byte)'S' => ConsoleKey.F4,
            _ => ConsoleKey.None
        };

        if (key != ConsoleKey.None)
        {
            onEvent(new InputRecord(new KeyEventRecord(key, '\0', ModifierKeys.None)));
        }
    }

    private void DispatchControlOrAscii(byte b, Action<InputRecord> onEvent)
    {
        if (b == 0x0D || b == 0x0A) // CR / LF
        {
            onEvent(new InputRecord(new KeyEventRecord(ConsoleKey.Enter, '\r', ModifierKeys.None)));
            return;
        }
        if (b == 0x09) // Tab
        {
            onEvent(new InputRecord(new KeyEventRecord(ConsoleKey.Tab, '\t', ModifierKeys.None)));
            return;
        }
        if (b == 0x7F || b == 0x08) // Backspace
        {
            onEvent(new InputRecord(new KeyEventRecord(ConsoleKey.Backspace, '\b', ModifierKeys.None)));
            return;
        }
        if (b >= 1 && b <= 26) // Ctrl+A ... Ctrl+Z
        {
            ConsoleKey key = ConsoleKey.A + (b - 1);
            onEvent(new InputRecord(new KeyEventRecord(key, (char)b, ModifierKeys.Control)));
            return;
        }

        char ch = (char)b;
        ConsoleKey consoleKey = (ch >= 'a' && ch <= 'z') ? (ConsoleKey)(ch - 32) : (ConsoleKey)ch;
        onEvent(new InputRecord(new KeyEventRecord(consoleKey, ch, ModifierKeys.None)));
    }

    private void DispatchCsiKey(char finalChar, Action<InputRecord> onEvent)
    {
        ModifierKeys modifiers = ModifierKeys.None;
        if (_paramCount >= 2)
        {
            modifiers = DecodeAnsiModifier(_params[1]);
        }

        if (finalChar == '~')
        {
            int code = _paramCount > 0 ? _params[0] : 0;
            ConsoleKey key = code switch
            {
                1 => ConsoleKey.Home,
                2 => ConsoleKey.Insert,
                3 => ConsoleKey.Delete,
                4 => ConsoleKey.End,
                5 => ConsoleKey.PageUp,
                6 => ConsoleKey.PageDown,
                15 => ConsoleKey.F5,
                17 => ConsoleKey.F6,
                18 => ConsoleKey.F7,
                19 => ConsoleKey.F8,
                20 => ConsoleKey.F9,
                21 => ConsoleKey.F10,
                23 => ConsoleKey.F11,
                24 => ConsoleKey.F12,
                _ => ConsoleKey.None
            };

            if (key != ConsoleKey.None)
                onEvent(new InputRecord(new KeyEventRecord(key, '\0', modifiers)));
            return;
        }

        ConsoleKey mappedKey = finalChar switch
        {
            'A' => ConsoleKey.UpArrow,
            'B' => ConsoleKey.DownArrow,
            'C' => ConsoleKey.RightArrow,
            'D' => ConsoleKey.LeftArrow,
            'H' => ConsoleKey.Home,
            'F' => ConsoleKey.End,
            'Z' => ConsoleKey.Tab, // Shift+Tab генерирует CSI Z
            _ => ConsoleKey.None
        };

        if (finalChar == 'Z') modifiers |= ModifierKeys.Shift;

        if (mappedKey != ConsoleKey.None)
        {
            onEvent(new InputRecord(new KeyEventRecord(mappedKey, '\0', modifiers)));
        }
    }

    private void DispatchSgrMouseEvent(char finalChar, Action<InputRecord> onEvent)
    {
        if (_paramCount < 3) return;

        int cb = _params[0];
        int x = Math.Max(0, _params[1] - 1); // Перевод из 1-based терминала в 0-based
        int y = Math.Max(0, _params[2] - 1);

        ModifierKeys modifiers = ModifierKeys.None;
        if ((cb & 4) != 0) modifiers |= ModifierKeys.Shift;
        if ((cb & 8) != 0) modifiers |= ModifierKeys.Alt;
        if ((cb & 16) != 0) modifiers |= ModifierKeys.Control;

        bool isRelease = (finalChar == 'm');
        bool isMotion = (cb & 32) != 0;
        bool isWheel = (cb & 64) != 0;

        MouseButton button = MouseButton.None;
        MouseAction action = MouseAction.Press;

        if (isWheel)
        {
            action = MouseAction.Wheel;
            button = (cb & 3) switch
            {
                0 => MouseButton.WheelUp,
                1 => MouseButton.WheelDown,
                2 => MouseButton.WheelLeft,
                3 => MouseButton.WheelRight,
                _ => MouseButton.None
            };
        }
        else if (isMotion)
        {
            action = MouseAction.Move;
            button = (cb & 3) switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                _ => MouseButton.None
            };
        }
        else if (isRelease)
        {
            action = MouseAction.Release;
            button = (cb & 3) switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                _ => MouseButton.None
            };
        }
        else
        {
            action = MouseAction.Press;
            button = (cb & 3) switch
            {
                0 => MouseButton.Left,
                1 => MouseButton.Middle,
                2 => MouseButton.Right,
                _ => MouseButton.None
            };
        }

        onEvent(new InputRecord(new MouseEventRecord(x, y, button, action, modifiers)));
    }

    private static ModifierKeys DecodeAnsiModifier(int mod) => (mod - 1) switch
    {
        1 => ModifierKeys.Shift,
        2 => ModifierKeys.Alt,
        3 => ModifierKeys.Shift | ModifierKeys.Alt,
        4 => ModifierKeys.Control,
        5 => ModifierKeys.Shift | ModifierKeys.Control,
        6 => ModifierKeys.Alt | ModifierKeys.Control,
        7 => ModifierKeys.Shift | ModifierKeys.Alt | ModifierKeys.Control,
        _ => ModifierKeys.None
    };
}