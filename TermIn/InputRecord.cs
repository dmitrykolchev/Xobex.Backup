using System;

namespace TermIn;

public enum InputEventType : byte
{
    None = 0,
    Key,
    Mouse
}

[Flags]
public enum ModifierKeys : byte
{
    None = 0,
    Shift = 1 << 0,
    Alt = 1 << 1,
    Control = 1 << 2
}

public enum MouseButton : byte
{
    None,
    Left,
    Middle,
    Right,
    WheelUp,
    WheelDown,
    WheelLeft,
    WheelRight
}

public enum MouseAction : byte
{
    Press,
    Release,
    Move,
    Wheel
}

public readonly struct KeyEventRecord
{
    public readonly ConsoleKey Key;
    public readonly char Char;
    public readonly ModifierKeys Modifiers;
    public readonly bool IsDown;

    public KeyEventRecord(ConsoleKey key, char ch, ModifierKeys modifiers, bool isDown = true)
    {
        Key = key;
        Char = ch;
        Modifiers = modifiers;
        IsDown = isDown;
    }

    public override string ToString() =>
        $"Key: {Key}, Char: '{(char.IsControl(Char) ? '?' : Char)}' (0x{(int)Char:X2}), Modifiers: {Modifiers}, Down: {IsDown}";
}

public readonly struct MouseEventRecord
{
    public readonly int X; // 0-based
    public readonly int Y; // 0-based
    public readonly MouseButton Button;
    public readonly MouseAction Action;
    public readonly ModifierKeys Modifiers;

    public MouseEventRecord(int x, int y, MouseButton button, MouseAction action, ModifierKeys modifiers)
    {
        X = x;
        Y = y;
        Button = button;
        Action = action;
        Modifiers = modifiers;
    }

    public override string ToString() =>
        $"Mouse: ({X}, {Y}), Button: {Button}, Action: {Action}, Modifiers: {Modifiers}";
}

public readonly struct InputRecord
{
    public readonly InputEventType EventType;
    public readonly KeyEventRecord KeyEvent;
    public readonly MouseEventRecord MouseEvent;

    public InputRecord(KeyEventRecord keyEvent)
    {
        EventType = InputEventType.Key;
        KeyEvent = keyEvent;
        MouseEvent = default;
    }

    public InputRecord(MouseEventRecord mouseEvent)
    {
        EventType = InputEventType.Mouse;
        MouseEvent = mouseEvent;
        KeyEvent = default;
    }

    public static InputRecord Empty => default;
}