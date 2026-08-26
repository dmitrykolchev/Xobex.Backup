// <copyright file="InputEvent.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;
using System.Globalization;
using System.Text;

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
    None,
    Down,
    Up,
    DblClick,
    Move,
    WheelUp,
    WheelDown,
    WheelLeft,
    WheelRight
}

[Flags]
public enum MouseButton
{
    None = 0,
    Left = 1,
    Right = 2,
    Middle = 4
}

public class InputEvent
{
    private InputEvent(InputEventType eventType, ReadOnlySpan<byte> rawData)
    {
        EventType = eventType;
        Timestamp = Stopwatch.GetTimestamp();
        RawData = new byte[rawData.Length];
        rawData.CopyTo(RawData);
    }

    public InputEvent(InputEventType eventType, ConsoleKey key, ConsoleModifiers mod, char ch, bool keyDown, ReadOnlySpan<byte> rawData) : this(eventType, rawData)
    {
        Key = new KeyEvent { Key = key, Mod = mod, Ch = ch, KeyDown = keyDown };
    }

    public InputEvent(InputEventType eventType, MouseEvent mouse, ReadOnlySpan<byte> rawData) : this(eventType, rawData)
    {
        Mouse = mouse;
    }

    public InputEventType EventType { get; }

    public KeyEvent Key { get; }

    public MouseEvent Mouse { get; }

    public long Timestamp { get; }

    public byte[] RawData { get; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(EventType).Append(':');
        if (EventType == InputEventType.Key)
        {
            if(Key.KeyDown)
            {
                sb.Append("Down");
            }
            else
            {
                sb.Append("Up");
            }
            sb.Append(' ').Append(Key.Key);
            if (Key.Mod != ConsoleModifiers.None)
            {
                sb.Append('+').Append(Key.Mod);
            }
            sb.Append(" Ch=").Append(FormattableString.Invariant($"\\u{(ushort)Key.Ch:X4}"));
        }
        else if (EventType == InputEventType.Mouse)
        {
            sb.Append(' ').Append(Mouse.Button).Append(' ').Append(Mouse.Action);
            if (Mouse.Mod != ConsoleModifiers.None)
            {
                sb.Append('+').Append(Mouse.Mod);
            }
            sb.Append(" Pos=(").Append(Mouse.X).Append(',').Append(Mouse.Y).Append(')');
        }
        sb.Append(CultureInfo.InvariantCulture, $" TS={Timestamp}");
        sb.Append(" Raw=[").Append(Convert.ToHexString(RawData)).Append(']');
        return sb.ToString();
    }

    internal static InputEvent Create(ConsoleKey key, ConsoleModifiers mod, char ch, bool keyDown, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(InputEventType.Key, key, mod, ch, keyDown, rawData);
    }

    internal static InputEvent Create(MouseEvent mouse, ReadOnlySpan<byte> rawData)
    {
        return new InputEvent(InputEventType.Mouse, mouse, rawData);
    }

    public readonly struct KeyEvent
    {
        public char Ch { get; init; }
        public ConsoleKey Key { get; init; }
        public ConsoleModifiers Mod { get; init; }
        public bool KeyDown { get; init; }
    }

    public readonly struct MouseEvent
    {
        public MouseButton Button { get; init; }
        public MouseAction Action { get; init; }
        public ConsoleModifiers Mod { get; init; }
        public int X { get; init; }
        public int Y { get; init; }
    }
}
