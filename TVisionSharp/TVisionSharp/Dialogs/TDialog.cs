using System;

namespace TVision
{
    public class TDialog : TWindow
    {
        public const string CpGrayDialog = "\x20\x21\x22\x23\x24\x25\x26\x27\x28\x29\x2A\x2B\x2C\x2D\x2E\x2F" + "\x30\x31\x32\x33\x34\x35\x36\x37\x38\x39\x3A\x3B\x3C\x3D\x3E\x3F";
        public const string CpDialog = CpGrayDialog;

        public TDialog(TRect bounds, string aTitle)
            : base(bounds, aTitle, Commands.WnNoNumber)
        {
            State |= Commands.SfModal;
            Options |= Commands.OfSelectable | Commands.OfCentered;
            Palette = 2;

            var dt = TProgram.DeskTop;
            if (dt != null && dt.Size.X > bounds.Width && dt.Size.Y > bounds.Height)
            {
                int dx = (dt.Size.X - bounds.Width) / 2;
                int dy = Math.Max(0, (dt.Size.Y - bounds.Height) / 3);
                MoveTo((short)(bounds.A.X + dx - Origin.X), (short)dy);
            }
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvKeyDown)
            {
                switch (ev.KeyDown.KeyCode)
                {
                    case KeyCodes.KbEsc:
                        EndModal(Commands.CmCancel);
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbTab:
                        ClearEvent(ev);
                        SelectNext(true);
                        break;
                    case KeyCodes.KbShiftTab:
                        ClearEvent(ev);
                        SelectNext(false);
                        break;
                    case KeyCodes.KbEnter:
                        if (GetState(Commands.SfFocused) && Current != null &&
                            (Current.Options & Commands.OfSelectable) != 0 &&
                            !(Current is TButton))
                        {
                            ushort r = Current.Execute();
                            if (r != 0)
                            {
                                EndModal(r);
                                ClearEvent(ev);
                                return;
                            }
                            ClearEvent(ev);
                        }
                        break;
                }
            }
            base.HandleEvent(ev);
        }

        public override TPalette GetPalette()
        {
            return new TPalette(CpGrayDialog, 32);
        }

        public override ushort Execute()
        {
            DrawAndFlush();
            while (GetState(Commands.SfModal))
            {
                TEvent ev = new TEvent();
                GetEventRef(ref ev);
                if (ev.What == EventCodes.EvNothing)
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }
                HandleEvent(ev);
                if (ev.What != EventCodes.EvNothing)
                    EventError(ev);
                DrawAndFlush();
            }
            return EndState;
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
        }
    }

    public class TInputLine : TView
    {
        public string Data;
        public int MaxLen;
        public int CurPos;
        public int FirstPos;
        public int SelStart;
        public int SelEnd;

        public TInputLine(TRect bounds, short aMaxLen) : base(bounds)
        {
            MaxLen = aMaxLen;
            Data = string.Empty;
            CurPos = 0;
            FirstPos = 0;
            SelStart = 0;
            SelEnd = 0;
            Options |= Commands.OfSelectable | Commands.OfFirstClick;
            EventMask |= EventCodes.EvKeyboard;
        }

        public override ushort DataSize() => (ushort)(MaxLen + 1);

        public override void GetData(object rec)
        {
            if (rec is string s)
                Data = s;
        }

        public override void SetData(object rec)
        {
            if (rec is string s)
                Data = s ?? string.Empty;
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();
            TColorAttr color = GetColor(0x0001).Low;

            string text = Data ?? string.Empty;
            int w = Size.X;
            int len = text.Length;
            int pos = FirstPos;
            int n = Math.Min(len - pos, w);
            if (n > 0)
                buf.MoveStr(0, text.Substring(pos, n), n, color);
            if (n < w)
                buf.MoveChar(n, ' ', color, w - n);

            if (GetState(Commands.SfFocused))
            {
                int cursorX = CurPos - FirstPos;
                if (cursorX >= 0 && cursorX < w)
                    buf.PutAttribute(cursorX, GetColor(0x0002).Low);
            }

            WriteBuf(0, 0, (short)w, 1, buf);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x13\x13\x14\x15", 4);
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvKeyboard)
            {
                switch (ev.KeyDown.KeyCode)
                {
                    case KeyCodes.KbLeft:
                        if (CurPos > 0) CurPos--;
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbRight:
                        if (CurPos < Data.Length) CurPos++;
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbHome:
                        CurPos = 0;
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbEnd:
                        CurPos = Data.Length;
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbBack:
                        if (CurPos > 0)
                        {
                            Data = Data.Remove(CurPos - 1, 1);
                            CurPos--;
                        }
                        ClearEvent(ev);
                        break;
                    case KeyCodes.KbDel:
                        if (CurPos < Data.Length)
                            Data = Data.Remove(CurPos, 1);
                        ClearEvent(ev);
                        break;
                    default:
                        if (ev.KeyDown.TextLength > 0 && ev.KeyDown.KeyCode >= 0x20)
                        {
                            string ch = ev.KeyDown.GetText();
                            if (Data.Length < MaxLen)
                            {
                                Data = Data.Insert(CurPos, ch);
                                CurPos += ch.Length;
                            }
                            ClearEvent(ev);
                        }
                        break;
                }
            }
            base.HandleEvent(ev);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if (aState == Commands.SfFocused)
                DrawView();
        }
    }

    public class TButton : TView
    {
        public string Title;
        public ushort Command;
        public byte Flags;
        public bool AmDefault;

        public const byte BfLeftGrab = 0x01;
        public const byte BfRightGrab = 0x02;
        public const byte BfGrabDefault = 0x04;

        public const ushort BfNormal = 0x00;
        public const ushort BfDefault = 0x01;
        public const ushort BfLeftJust = 0x02;

        public TButton(TRect bounds, string aTitle, ushort aCommand, ushort aFlags)
            : base(bounds)
        {
            Title = aTitle ?? string.Empty;
            Command = aCommand;
            Flags = (byte)aFlags;
            AmDefault = false;
            Options |= Commands.OfSelectable | Commands.OfFirstClick;
            EventMask |= EventCodes.EvKeyboard;
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            TAttrPair pair;
            if (GetState(Commands.SfDisabled))
                pair = GetColor(0x0004);
            else if (GetState(Commands.SfFocused))
                pair = AmDefault ? GetColor(0x0703) : GetColor(0x0602);
            else if (AmDefault)
                pair = GetColor(0x0703);
            else
                pair = GetColor(0x0501);

            var normalColor = pair.Low;
            var color = normalColor;

            int w = Size.X;
            string title = Title ?? string.Empty;
            string plainTitle = title.Replace("~", "");
            int titleLen = Math.Min(plainTitle.Length, w - 4);
            int pos = Math.Max(1, (w - titleLen - 2) / 2);

            buf.MoveChar(0, ' ', color, w);
            buf.WriteChar(0, '[', color);
            if (titleLen > 0)
                buf.MoveCStr(pos, Title, pair);
            buf.WriteChar(w - 1, ']', color);

            WriteBuf(0, 0, (short)w, 1, buf);

            byte shadowBios = MapColor(8).ToBios();
            if (shadowBios == 0) shadowBios = 0x08;
            var shadowAttr = new TColorAttr(shadowBios);
            if (Size.Y >= 2)
            {
                var sbuf = new TDrawBuffer(w);
                sbuf.MoveChar(0, ' ', color, w);
                sbuf.WriteChar(1, '\u2584', shadowAttr, Math.Max(0, w - 1));
                WriteLine(0, 1, (short)w, 1, sbuf);
            }
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x0A\x0B\x0C\x0D\x0E\x0E\x0E\x0F", 8);
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvMouseDown)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                if (local.X >= 0 && local.X < Size.X && local.Y >= 0 && local.Y < Size.Y)
                {
                    if (!GetState(Commands.SfFocused))
                        Select();
                    _pressed = true;
                    ClearEvent(ev);
                    DrawView();
                }
            }
            else if (ev.What == EventCodes.EvMouseUp && _pressed)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                _pressed = false;
                ClearEvent(ev);
                if (local.X >= 0 && local.X < Size.X && local.Y >= 0 && local.Y < Size.Y)
                    Press();
                else
                    DrawView();
            }
            else if (ev.What == EventCodes.EvKeyDown && GetState(Commands.SfFocused))
            {
                ushort code = ev.KeyDown.KeyCode;
                if (code == (ushort)' ' || code == KeyCodes.KbEnter)
                {
                    Press();
                    ClearEvent(ev);
                }
            }
            base.HandleEvent(ev);
        }

        private bool _pressed;

        public void MakeDefault(bool enable)
        {
            AmDefault = enable;
        }

        public void Press()
        {
            if ((State & Commands.SfDisabled) == 0)
            {
                if ((Flags & BfDefault) != 0)
                    EndModal(Command);
                else
                {
                    var ev = new TEvent();
                    ev.What = EventCodes.EvCommand;
                    ev.Message.Command = Command;
                    ev.Message.InfoPtr = this;
                    PutEvent(ev);
                }
            }
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
        }
    }

    public class TCluster : TView
    {
        public string[] Strings;
        public uint Sel;
        public ushort Value;

        public TCluster(TRect bounds, string[] aStrings) : base(bounds)
        {
            Strings = aStrings;
            Sel = 0;
            Value = 0;
            Options |= Commands.OfSelectable | Commands.OfFirstClick;
            EventMask |= EventCodes.EvKeyboard;
        }

        public override ushort DataSize() => sizeof(uint);

        public override void GetData(object rec)
        {
            if (rec is uint[] arr && arr.Length > 0) arr[0] = Value;
        }

        public override void SetData(object rec)
        {
            if (rec is uint[] arr && arr.Length > 0) Value = (ushort)arr[0];
        }

        public virtual void Press(int item) { }

        public override TPalette GetPalette()
        {
            return new TPalette("\x10\x11\x12\x12\x1F", 5);
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            var pair = GetState(Commands.SfSelected) ? GetColor(0x0202) : GetColor(0x0101);
            var normalColor = pair.Low;
            var selectedColor = pair.High;

            int w = Size.X;
            if (Strings == null) { WriteBuf(0, 0, (short)w, (short)Size.Y, buf); return; }

            for (int i = 0; i < Strings.Length && i < Size.Y; i++)
            {
                bool checked_ = (Value & (1 << i)) != 0;
                string marker = checked_ ? "(X) " : "( ) ";
                buf.MoveStr(0, marker, marker.Length, normalColor);
                string itemText = Strings[i] ?? string.Empty;
                int textLen = Math.Min(itemText.Length, w - marker.Length);
                if (textLen > 0)
                    buf.MoveStr(marker.Length, itemText, textLen, normalColor);
            }

            WriteBuf(0, 0, (short)w, (short)Size.Y, buf);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
        }
    }

    public class TRadioButtons : TCluster
    {
        public TRadioButtons(TRect bounds, string[] aStrings) : base(bounds, aStrings) { }

        public override void Press(int item)
        {
            Value = (ushort)(1 << item);
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            TColorAttr normalColor = GetColor(0x0101).Low;

            int w = Size.X;
            if (Strings == null) { WriteBuf(0, 0, (short)w, (short)Size.Y, buf); return; }

            for (int i = 0; i < Strings.Length && i < Size.Y; i++)
            {
                bool selected = (Value & (1 << i)) != 0;
                string marker = selected ? "(*) " : "( ) ";
                buf.MoveStr(0, marker, marker.Length, normalColor);
                string itemText = Strings[i] ?? string.Empty;
                int textLen = Math.Min(itemText.Length, w - marker.Length);
                if (textLen > 0)
                    buf.MoveStr(marker.Length, itemText, textLen, normalColor);
            }

            WriteBuf(0, 0, (short)w, (short)Size.Y, buf);
        }
    }

    public class TCheckBoxes : TCluster
    {
        public TCheckBoxes(TRect bounds, string[] aStrings) : base(bounds, aStrings) { }

        public override void Press(int item)
        {
            Value ^= (ushort)(1 << item);
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            TColorAttr normalColor = GetColor(0x0101).Low;

            int w = Size.X;
            if (Strings == null) { WriteBuf(0, 0, (short)w, (short)Size.Y, buf); return; }

            for (int i = 0; i < Strings.Length && i < Size.Y; i++)
            {
                bool checked_ = (Value & (1 << i)) != 0;
                string marker = checked_ ? "[X] " : "[ ] ";
                buf.MoveStr(0, marker, marker.Length, normalColor);
                string itemText = Strings[i] ?? string.Empty;
                int textLen = Math.Min(itemText.Length, w - marker.Length);
                if (textLen > 0)
                    buf.MoveStr(marker.Length, itemText, textLen, normalColor);
            }

            WriteBuf(0, 0, (short)w, (short)Size.Y, buf);
        }
    }

    public class TMultiCheckBoxes : TCluster
    {
        public ushort Mask;
        public ushort BoxFrame;
        public byte BoxMarker;

        public TMultiCheckBoxes(TRect bounds, string[] aStrings, ushort aMask,
            ushort aFrame, byte aMarker) : base(bounds, aStrings)
        {
            Mask = aMask;
            BoxFrame = aFrame;
            BoxMarker = aMarker;
        }

        public override ushort DataSize() => sizeof(uint);

        public override void GetData(object rec)
        {
            if (rec is uint[] arr && arr.Length > 0)
                arr[0] = Value;
        }

        public override void SetData(object rec)
        {
            if (rec is uint[] arr && arr.Length > 0)
                Value = (ushort)arr[0];
        }
    }

    public class TStaticText : TView
    {
        public string Text;

        public TStaticText(TRect bounds, string aText) : base(bounds)
        {
            Text = aText ?? string.Empty;
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();
            TColorAttr color = GetColor(0x0101).Low;

            string text = Text ?? string.Empty;
            int w = Size.X;
            int h = Size.Y;
            int pos = 0;

            for (int row = 0; row < h; row++)
            {
                int col = 0;
                while (col < w && pos < text.Length)
                {
                    char ch = text[pos];
                    if (ch == '\n') { pos++; break; }
                    if (ch == '\r') { pos++; continue; }
                    buf.MoveChar(col, ch, color, 1);
                    col++;
                    pos++;
                }
            }

            WriteBuf(0, 0, (short)w, (short)h, buf);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x06", 1);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }
    }

    public class TLabel : TView
    {
        public string Text;
        public TView Link;

        public TLabel(TRect bounds, string aText, TView aLink) : base(bounds)
        {
            Text = aText ?? string.Empty;
            Link = aLink;
            Options |= Commands.OfSelectable;
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            var labelPair = GetState(Commands.SfFocused) ? GetColor(0x0202) : GetColor(0x0101);

            buf.MoveCStr(0, Text, labelPair);
            WriteBuf(0, 0, (short)Size.X, 1, buf);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x07\x08\x09\x09", 4);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
        }
    }

    public class TListBox : TListViewer
    {
        protected TCollection _items;

        public TListBox(TRect bounds, ushort aNumCols, TScrollBar aScrollBar)
            : base(bounds, aNumCols, null, aScrollBar) { }

        public TCollection List() => _items;

        public virtual void NewList(TCollection aList)
        {
            _items = aList;
            SetRange((short)(aList?.Count ?? 0));
        }

        public override string GetText(short item, short maxLen)
        {
            if (_items == null || item < 0 || item >= _items.Count)
                return string.Empty;
            var s = _items.At(item)?.ToString() ?? string.Empty;
            return s.Length > maxLen ? s.Substring(0, maxLen) : s;
        }
    }

    public class THistory : TView
    {
        public THistory(TRect bounds, TInputLine aLink, byte aHistoryId)
            : base(bounds) { }
    }
}
