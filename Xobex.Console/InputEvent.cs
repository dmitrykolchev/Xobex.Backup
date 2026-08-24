namespace Xobex.Console;

public enum InputEventType
{
    Unknown,
    Key,
    Mouse,
    Timer
}

public enum MouseAction
{
    Down,
    Up,
    Move
}

public enum MouseButton
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

public class InputEvent
{
    private readonly byte[] _rawData;

    public InputEvent(InputEventType eventType, ReadOnlySpan<byte> rawData)
    {
        EventType = eventType;
        _rawData = new byte[rawData.Length];
        rawData.CopyTo(_rawData);
    }

    public InputEvent(InputEventType eventType, ConsoleKey key, ConsoleModifiers mod, char ch, ReadOnlySpan<byte> rawData): this(eventType, rawData)
    {
        Key = new KeyEvent { Key = key, Mod = mod, Ch = ch };
    }

    public InputEvent(InputEventType eventType, MouseEvent mouse, ReadOnlySpan<byte> rawData) : this(eventType, rawData)
    {
        Mouse = mouse;
    }

    public InputEventType EventType { get; }

    public KeyEvent Key { get; }

    public MouseEvent Mouse { get; }

    public byte[] RawData => _rawData;

    internal static InputEvent Create(ConsoleKey key, ConsoleModifiers mod, char ch, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(InputEventType.Key, key, mod, ch, rawData);
    }

    internal static InputEvent Create(MouseEvent mouse, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(InputEventType.Mouse, mouse, rawData);
    }

    public struct KeyEvent
    {
        public char Ch { get; init; }
        public ConsoleKey Key { get; init; }
        public ConsoleModifiers Mod { get; init; }
    }

    public struct MouseEvent
    {
        public MouseButton Button { get; init; }
        public MouseAction Action { get; init; }
        public ConsoleModifiers Mod { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
    }
}
