namespace Xobex.Console;

public enum InputEventType
{
    Unknown,
    Key,
    Mouse,
    Timer
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

    public InputEventType EventType { get; }

    public KeyEvent Key { get; }

    public byte[] RawData => _rawData;

    internal static InputEvent Create(ConsoleKey key, ConsoleModifiers mod, char ch, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(InputEventType.Key, key, mod, ch, rawData);
    }

    public struct KeyEvent
    {
        public char Ch { get; init; }
        public ConsoleKey Key { get; init; }
        public ConsoleModifiers Mod { get; init; }
    }
}
