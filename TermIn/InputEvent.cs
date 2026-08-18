using System;
using System.Collections.Generic;
using System.Text;

namespace TermIn;

public class InputEvent
{
}

public enum MouseEventType
{
    Unknown,
    MouseMove,
    MouseLeftClick,
    MouseRightClick,
    MouseMidleClick
}

public class MouseEvent : InputEvent
{
    public MouseEvent(MouseEventType eventType, bool release, int x, int y)
    {
        EventType = eventType;
        Release = release;
        X = x;
        Y = y;
    }

    public MouseEventType EventType { get; }

    public bool Release { get; }

    public int X { get; }

    public int Y { get; }
}

public class KeyboardEvent: InputEvent
{
    public KeyboardEvent(char ch)
    {
        Ch = ch;
    }

    public char Ch { get; }
}
