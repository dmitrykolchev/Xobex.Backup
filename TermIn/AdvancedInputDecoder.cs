using System.Text;
using System.Text.RegularExpressions;

namespace TermIn;

public enum EventType { Key, Mouse }

[Flags]
public enum ControlKeyState : uint
{
    None = 0,
    RightAltPressed = 0x0001,
    LeftAltPressed = 0x0002,
    RightCtrlPressed = 0x0004,
    LeftCtrlPressed = 0x0008,
    ShiftPressed = 0x0010,
    NumLockOn = 0x0020,
    ScrollLockOn = 0x0040,
    CapsLockOn = 0x0080,
    EnhancedKey = 0x0100
}

public struct ConsoleEvent
{
    public EventType Type;
    public KeyEventRecord KeyEvent;
    public MouseEventRecord MouseEvent;
}

public struct KeyEventRecord
{
    public bool IsKeyDown;
    public char Char;
    public string KeyName;
    public ControlKeyState Modifiers;
    public byte[] RawBytes;
}

public struct MouseEventRecord
{
    public int X;
    public int Y;
    public bool LeftButton;
    public bool RightButton;
    public bool MiddleButton;
    public bool WheelUp;
    public bool WheelDown;
    public bool IsMove;
}

public static class AdvancedInputDecoder
{
    // Регулярные выражения для разбора динамических последовательностей клавиатуры XTerm
    private static readonly Regex XtermTildeRegex = new(@"^\x1b\[(\d+)(?:;(\d+))?~$", RegexOptions.Compiled);
    private static readonly Regex XtermLetterRegex = new(@"^\x1b\[1;(\d+)([A-HF-S])$", RegexOptions.Compiled);
    private static readonly Regex Ss3LetterRegex = new(@"^\x1bO(?:;(\d+))?([P-S])$", RegexOptions.Compiled);

    public static List<ConsoleEvent> ParseBuffer(ReadOnlySpan<byte> buffer)
    {
        List<ConsoleEvent> events = new();
        int i = 0;

        while (i < buffer.Length)
        {
            if (buffer[i] == 0x1b) // ESC символ
            {
                // 1. Проверяем расширенное SGR событие мыши: ESC [ <
                if (i + 2 < buffer.Length && buffer[i + 1] == 0x5b && buffer[i + 2] == 0x3c)
                {
                    int endIdx = -1;
                    byte typeByte = 0;

                    // Ищем завершающий маркер мыши 'm' или 'M'
                    for (int j = i + 3; j < buffer.Length; j++)
                    {
                        if (buffer[j] is ((byte)'M') or ((byte)'m'))
                        {
                            endIdx = j;
                            typeByte = buffer[j];
                            break;
                        }
                    }

                    if (endIdx != -1)
                    {
                        int start = i + 3;
                        int length = endIdx - start;
                        string mouseStr = Encoding.ASCII.GetString(buffer.Slice(start, length));

                        if (TryParseSgrMouse(mouseStr, typeByte, out MouseEventRecord mouseEvent))
                        {
                            events.Add(new ConsoleEvent { Type = EventType.Mouse, MouseEvent = mouseEvent });
                        }

                        i = endIdx + 1;
                        continue;
                    }
                }

                // 2. Если это не мышь, вычисляем длину escape-последовательности для клавиатуры
                int seqLen = 1;
                if (i + 1 < buffer.Length && (buffer[i + 1] == 0x5b || buffer[i + 1] == 0x4f)) // '[' или 'O'
                {
                    seqLen = 2;
                    while (i + seqLen < buffer.Length)
                    {
                        byte b = buffer[i + seqLen];
                        seqLen++;
                        if (b is >= ((byte)'A') and <= ((byte)'Z') or >= ((byte)'a') and <= ((byte)'z') or ((byte)'~'))
                            break;
                    }
                }

                var rawSeq = buffer.Slice(i, seqLen).ToArray();
                string asciiStr = Encoding.ASCII.GetString(rawSeq);

                // 3. Пытаемся разобрать сложную клавишу (Стрелки, F-кнопки с Shift/Ctrl/Alt)
                if (TryParseAdvancedKey(asciiStr, rawSeq, out KeyEventRecord keyEvent))
                {
                    events.Add(new ConsoleEvent { Type = EventType.Key, KeyEvent = keyEvent });
                    i += seqLen;
                    continue;
                }

                // 4. Обработка комбинаций Alt + Обычная Клавиша (например, Esc + 'a')
                if (seqLen == 1 && i + 1 < buffer.Length)
                {
                    byte nextByte = buffer[i + 1];
                    if (nextByte is >= 32 and <= 126) // Печатный символ
                    {
                        events.Add(new ConsoleEvent
                        {
                            Type = EventType.Key,
                            KeyEvent = new KeyEventRecord
                            {
                                IsKeyDown = true,
                                Char = (char)nextByte,
                                KeyName = $"Key '{(char)nextByte}'",
                                Modifiers = ControlKeyState.LeftAltPressed,
                                RawBytes = new byte[] { 0x1b, nextByte }
                            }
                        });
                        i += 2;
                        continue;
                    }
                }

                // Неизвестный ESC-код (пропускаем пачку, чтобы не зациклиться)
                events.Add(new ConsoleEvent
                {
                    Type = EventType.Key,
                    KeyEvent = new KeyEventRecord { IsKeyDown = true, KeyName = $"Unknown ESC ({BitConverter.ToString(rawSeq)})", RawBytes = rawSeq }
                });
                i += seqLen;
            }
            else
            {
                // 5. Обычный ASCII символ или базовая Ctrl+комбинация
                byte b = buffer[i];
                events.Add(new ConsoleEvent
                {
                    Type = EventType.Key,
                    KeyEvent = DecodeSingleByte(b)
                });
                i++;
            }
        }

        return events;
    }

    private static bool TryParseSgrMouse(string data, byte typeByte, out MouseEventRecord ev)
    {
        ev = default;
        var parts = data.Split(';');
        if (parts.Length != 3) return false;

        if (int.TryParse(parts[0], out int buttonCode) &&
            int.TryParse(parts[1], out int posX) &&
            int.TryParse(parts[2], out int posY))
        {
            bool isMouseDown = typeByte == (byte)'M'; // M = нажатие/перемещение, m = отпускание

            ev.X = posX - 1; // Переводим 1-based терминала в 0-based Windows-style
            ev.Y = posY - 1;
            ev.IsMove = (buttonCode & 32) != 0;

            // Проверяем 6-й бит: колесико мыши (код 64+)
            if ((buttonCode & 64) != 0)
            {
                if ((buttonCode & 1) == 0) ev.WheelUp = true;
                else ev.WheelDown = true;
            }
            else if (!ev.IsMove)
            {
                // Обычные клики кнопками
                int baseButton = buttonCode & 3;
                if (baseButton == 0) ev.LeftButton = isMouseDown;
                if (baseButton == 1) ev.MiddleButton = isMouseDown;
                if (baseButton == 2) ev.RightButton = isMouseDown;
            }
            else
            {
                // Движение мыши, когда какая-то кнопка зажата (Drag)
                int baseButton = buttonCode & 3;
                if (baseButton == 0) ev.LeftButton = true;
                if (baseButton == 1) ev.MiddleButton = true;
                if (baseButton == 2) ev.RightButton = true;
            }
            return true;
        }
        return false;
    }

    private static bool TryParseAdvancedKey(string seq, byte[] rawBytes, out KeyEventRecord ev)
    {
        ev = new KeyEventRecord { IsKeyDown = true, RawBytes = rawBytes };

        var strictMatch = seq switch
        {
            "\x1b[A" => "ArrowUp",
            "\x1b[B" => "ArrowDown",
            "\x1b[C" => "ArrowRight",
            "\x1b[D" => "ArrowLeft",
            "\x1b[H" => "Home",
            "\x1b[F" => "End",
            "\x1b[E" => "NumPad5",
            "\x1bOP" => "F1",
            "\x1bOQ" => "F2",
            "\x1bOR" => "F3",
            "\x1bOS" => "F4",
            _ => null
        };

        if (strictMatch != null)
        {
            ev.KeyName = strictMatch;
            return true;
        }

        // Шаблон 1: С тильдой на конце (\x1b[<код>;<мод>~)
        var matchTilde = XtermTildeRegex.Match(seq);
        if (matchTilde.Success)
        {
            string keyId = matchTilde.Groups[1].Value;
            string modId = matchTilde.Groups[2].Value;

            ev.KeyName = keyId switch
            {
                "2" => "Insert",
                "3" => "Delete",
                "5" => "PageUp",
                "6" => "PageDown",
                "1" or "7" => "Home",
                "4" or "8" => "End",
                "11" => "F1",
                "12" => "F2",
                "13" => "F3",
                "14" => "F4",
                "15" => "F5",
                "17" => "F6",
                "18" => "F7",
                "19" => "F8",
                "20" => "F9",
                "21" => "F10",
                "23" => "F11",
                "24" => "F12",
                _ => $"Key-{keyId}"
            };
            ev.Modifiers = ParseModifierCode(modId);
            return true;
        }

        // Шаблон 2: Стрелки/Home/End с модификаторами (\x1b[1;<мод><буква>)
        var matchLetter = XtermLetterRegex.Match(seq);
        if (matchLetter.Success)
        {
            string modId = matchLetter.Groups[1].Value;
            string letter = matchLetter.Groups[2].Value;
            ev.KeyName = letter switch
            {
                "A" => "ArrowUp",
                "B" => "ArrowDown",
                "C" => "ArrowRight",
                "D" => "ArrowLeft",
                "H" => "Home",
                "F" => "End",
                _ => $"Key-{letter}"
            };
            ev.Modifiers = ParseModifierCode(modId);
            return true;
        }
        // Шаблон 3: F1-F4 в режиме SS3 (\x1bO;<мод><буква>)
        var matchSs3 = Ss3LetterRegex.Match(seq);
        if (matchSs3.Success)
        {
            string modId = matchSs3.Groups[1].Value;
            string letter = matchSs3.Groups[2].Value;
            ev.KeyName = letter switch
            {
                "P" => "F1",
                "Q" => "F2",
                "R" => "F3",
                "S" => "F4",
                _ => $"F-{letter}"
            };
            ev.Modifiers = ParseModifierCode(modId);
            return true;
        }
        return false;
    }

    private static ControlKeyState ParseModifierCode(string codeStr)
    {
        if (string.IsNullOrEmpty(codeStr) || !int.TryParse(codeStr, out int code))
            return ControlKeyState.None;
        ControlKeyState state = ControlKeyState.None;
        int internalCode = code - 1;
        if ((internalCode & 1) != 0)
            state |= ControlKeyState.ShiftPressed;
        if ((internalCode & 2) != 0)
            state |= ControlKeyState.LeftAltPressed;
        if ((internalCode & 4) != 0) state |= ControlKeyState.LeftCtrlPressed;
        return state;
    }

    private static KeyEventRecord DecodeSingleByte(byte b)
    {
        KeyEventRecord ev = new() { IsKeyDown = true, RawBytes = new byte[] { b } };
        if (b is >= 0x01 and <= 0x1a and not 0x09 and not 0x0a and not 0x0d)
        {
            ev.Char = (char)(b + 96);
            ev.KeyName = $"Key '{ev.Char}'";
            ev.Modifiers = ControlKeyState.LeftCtrlPressed;
            return ev;
        }
        switch (b)
        {
            case 0x09:
                ev.KeyName = "Tab";
                ev.Char = '\t';
                break;
            case 0x0d:
            case 0x0a:
                ev.KeyName = "Enter";
                ev.Char = '\r';
                break;
            case 0x1b:
                ev.KeyName = "Escape";
                break;
            case 0x20:
                ev.KeyName = "Space";
                ev.Char = ' ';
                break;
            case 0x7f:
                ev.KeyName = "Backspace";
                break;
            default:
                ev.Char = (char)b;
                ev.KeyName = $"Key '{(char)b}'";
                break;
        }
        return ev;
    }
}