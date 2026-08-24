using System.Text;

namespace TermIn;


public enum InputEventType
{
    None = 0,
    Key,
    Mouse
}

[Flags]
public enum ControlKey
{
    None = 0,
    LeftAlt = 0x02,
    LeftCtrl = 0x08
}

[Flags]
public enum MouseWheel
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 4,
    Right = 8,
}

[Flags]
public enum MouseButton
{
    None = 0,
    LeftButton = 1,
    RightButton = 2,
    MiddleButton = 4
}

public struct InputEvent
{
    public struct MouseInputEvent
    {
        public int X;
        public int Y;
        public int State;
        public int Type;
    }

    public struct KeyInputEvent
    {
        public int Ch;
        public KeyCode KeyCode;

        public int ShiftState;

        public string KeyType;
    }

    private InputEvent(KeyInputEvent keyEvent, ReadOnlySpan<byte> rawData)
    {
        EventType = InputEventType.Key;
        KeyEvent = keyEvent;
        RawData = new byte[rawData.Length];
        rawData.CopyTo(RawData);
    }

    private InputEvent(MouseInputEvent mouseEvent, ReadOnlySpan<byte> rawData)
    {
        EventType = InputEventType.Mouse;
        MouseEvent = mouseEvent;
        RawData = new byte[rawData.Length];
        rawData.CopyTo(RawData);
    }

    public static InputEvent CreateKeyEvent(int ch, KeyCode keyCode, int shiftState, string keyType, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(new KeyInputEvent { Ch = ch, KeyCode = keyCode, ShiftState = shiftState, KeyType = keyType }, rawData);
    }

    public static InputEvent CreateMouseEvent(int x, int y, int state, int type, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(new MouseInputEvent { X = x, Y = y, State = state, Type = type }, rawData);
    }

    public MouseInputEvent MouseEvent { get; private set; }

    public KeyInputEvent KeyEvent { get; private set; }

    public InputEventType EventType { get; private set; }

    public byte[] RawData { get; private set; }

    public override string ToString()
    {
        if (EventType == InputEventType.Mouse)
        {
            return $"Mouse: ({MouseEvent.X}, {MouseEvent.Y}) {MouseEvent.State} {MouseEvent.Type} | {ToCString(RawData)} --- {BitConverter.ToString(RawData)}";
        }
        return $"Key: KeyCode: {KeyEvent.KeyCode}, Shift: {KeyEvent.ShiftState}, Type: {KeyEvent.KeyType} | {ToCString(RawData)}";
    }

    private static string ToCString(ReadOnlySpan<byte> data)
    {
        StringBuilder builder = new();
        foreach (byte b in data)
        {
            _ = builder.Append(b == 0x1B ? "^" : (b < 32 ? $"\\x{b:X2}" : (char)b));
        }
        return builder.ToString();
    }
}
