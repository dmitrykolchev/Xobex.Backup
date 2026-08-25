using System;
using System.Collections.Generic;

namespace TVision
{
    public class TMenuItem
    {
        public string Name;
        public TKey ShortCut;
        public ushort Command;
        public ushort HelpCtx;
        public string Param;
        public TMenuItem Next;
        public TMenu SubMenu;
        public bool Disabled;

        public TMenuItem(string aName, ushort aCommand, TKey aShortCut,
            string aParam = null, TMenuItem aNext = null)
        {
            Name = aName;
            Command = aCommand;
            ShortCut = aShortCut;
            Param = aParam;
            Next = aNext;
            Disabled = !TView.CommandEnabled(aCommand);
        }

        public TMenuItem(string aName, TKey aShortCut, TMenu aSubMenu, TMenuItem aNext = null)
        {
            Name = aName;
            Command = 0;
            ShortCut = aShortCut;
            SubMenu = aSubMenu;
            Next = aNext;
        }
    }

    public class TSubMenu : TMenu
    {
        public static TSubMenu operator +(TSubMenu s, TMenuItem item)
        {
            if (s.Items == null) s.Items = item;
            else
            {
                var p = s.Items;
                while (p.Next != null) p = p.Next;
                p.Next = item;
            }
            return s;
        }

        public TSubMenu(string name, ushort key) : base(null)
        {
            Items = new TMenuItem(name, new TKey(key), null);
        }
    }

    public static class MenuOps
    {
        public static TMenu Add(this TMenu m, TMenuItem item)
        {
            if (m.Items == null) m.Items = item;
            else
            {
                var p = m.Items;
                while (p.Next != null) p = p.Next;
                p.Next = item;
            }
            return m;
        }
    }

    public class TMenu
    {
        public TMenuItem Items;
        public TMenuItem Deflt;

        public TMenu(TMenuItem aItems)
        {
            Items = aItems;
            Deflt = aItems;
        }
    }

    public class TMenuView : TView
    {
        protected const string CpMenuView = "\x02\x03\x04\x05\x06\x07";

        public TMenuItem Items;
        public TMenuItem Current;
        public TMenu Menu;
        public TMenuView ParentMenu;
        public bool PutClickEventOnExit = true;

        public TMenuView(TRect bounds) : base(bounds)
        {
            ParentMenu = null;
            Options |= Commands.OfPreProcess;
        }

        public override TPalette GetPalette()
        {
            return new TPalette(CpMenuView, 6);
        }

        protected static string StripHot(string name)
        {
            if (name == null) return string.Empty;
            var sb = new System.Text.StringBuilder();
            foreach (char c in name) if (c != '~') sb.Append(c);
            return sb.ToString();
        }

        protected static char HotKeyOf(string name)
        {
            if (name == null) return '\0';
            bool hot = false;
            foreach (char c in name)
            {
                if (c == '~') { hot = !hot; continue; }
                if (hot) return char.ToUpperInvariant(c);
            }
            return '\0';
        }

        private static readonly byte[] AltScan =
        {
            0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26,
            0x32, 0x31, 0x18, 0x19, 0x10, 0x13, 0x1F, 0x14, 0x16, 0x2F, 0x11, 0x2D,
            0x15, 0x2C
        };

        protected static ushort AltCode(char upper)
        {
            int i = char.ToUpperInvariant(upper) - 'A';
            if (i < 0 || i >= AltScan.Length) return 0;
            return (ushort)(AltScan[i] << 8);
        }

        internal void TrackMouse(TEvent e, ref bool mouseActive)
        {
            TPoint mouse = MakeLocal(e.Mouse.Where);
            for (var p = Menu.Items; p != null; p = p.Next)
            {
                TRect r = GetItemRect(p);
                if (r.Contains(mouse))
                {
                    Current = p;
                    mouseActive = true;
                    return;
                }
            }
        }

        internal void NextItem()
        {
            Current = Current.Next;
            if (Current == null) Current = Menu.Items;
        }

        internal void PrevItem()
        {
            var p = (Current == Menu.Items) ? null : Current;
            do { NextItem(); } while (Current.Next != p);
        }

        internal void TrackKey(bool findNext)
        {
            if (Current == null)
            {
                Current = Menu.Items;
                if (!findNext) PrevItem();
                if (Current.Name != null) return;
            }
            do
            {
                if (findNext) NextItem();
                else PrevItem();
            } while (Current.Name == null);
        }

        internal bool MouseInOwner(TEvent e)
        {
            if (ParentMenu == null) return false;
            TPoint mouse = ParentMenu.MakeLocal(e.Mouse.Where);
            TRect r = ParentMenu.GetItemRect(ParentMenu.Current);
            return r.Contains(mouse);
        }

        internal bool MouseInMenus(TEvent e)
        {
            var p = ParentMenu;
            while (p != null && !p.MouseInView(e.Mouse.Where))
                p = p.ParentMenu;
            return p != null;
        }

        internal TMenuView TopMenu()
        {
            var p = this;
            while (p.ParentMenu != null) p = p.ParentMenu;
            return p;
        }

        public virtual TRect GetItemRect(TMenuItem item)
        {
            return new TRect(0, 0, 0, 0);
        }

        public TMenuItem FindItem(char ch)
        {
            var p = Menu.Items;
            while (p != null)
            {
                char hk = HotKeyOf(p.Name);
                if (!p.Disabled && hk != '\0' &&
                    char.ToUpperInvariant(ch) == hk)
                    return p;
                p = p.Next;
            }
            return null;
        }

        public TMenuItem FindAltShortcut(TEvent e)
        {
            if (e.What != EventCodes.EvKeyDown) return null;
            ushort code = e.KeyDown.KeyCode;
            if ((code & 0xFF) == 0)
            {
                byte scan = (byte)(code >> 8);
                int i = Array.IndexOf(AltScan, scan);
                if (i >= 0)
                {
                    var p = TopMenu().Menu.Items;
                    while (p != null)
                    {
                        if (FindHotKeyIn(Menu.Items, (ushort)(AltScan[i] << 8)) != null)
                            return FindHotKeyIn(Menu.Items, (ushort)(AltScan[i] << 8));
                        p = p.Next;
                    }
                }
            }
            return null;
        }

        public TMenuItem HotKey(TKey key)
        {
            return FindHotKey(Menu.Items, key);
        }

        private TMenuItem FindHotKey(TMenuItem p, TKey key)
        {
            while (p != null)
            {
                if (p.Name != null)
                {
                    if (p.Command == 0)
                    {
                        var t = FindHotKey(p.SubMenu?.Items, key);
                        if (t != null) return t;
                    }
                    else if (!p.Disabled && p.ShortCut.Code != KeyCodes.KbNoKey && p.ShortCut == key)
                        return p;
                }
                p = p.Next;
            }
            return null;
        }

        private TMenuItem FindHotKeyIn(TMenuItem p, ushort altCode)
        {
            while (p != null)
            {
                char hk = HotKeyOf(p.Name);
                if (hk != '\0' && AltCode(hk) == altCode) return p;
                if (p.SubMenu != null)
                {
                    var t = FindHotKeyIn(p.SubMenu.Items, altCode);
                    if (t != null) return t;
                }
                p = p.Next;
            }
            return null;
        }

        internal TMenuItem FindTopLevelByAltItem(byte scan)
        {
            for (var p = Menu.Items; p != null; p = p.Next)
            {
                char hk = HotKeyOf(p.Name);
                if (hk != '\0' && AltCode(hk) == (ushort)(scan << 8))
                    return p;
            }
            return null;
        }

        protected virtual TMenuView NewSubView(TRect bounds, TMenu aMenu, TMenuView aParentMenu)
        {
            return new TMenuBox(bounds, aMenu, aParentMenu);
        }

        private enum MenuAction { DoNothing, DoSelect, DoReturn }

        public override ushort Execute()
        {
            bool autoSelect = false;
            bool firstEvent = true;
            var action = MenuAction.DoNothing;
            ushort result = 0;
            TMenuItem itemShown = null;
            TMenuItem lastTargetItem = null;
            bool mouseActive = false;

            Current = Menu.Deflt;

            do
            {
                action = MenuAction.DoNothing;
                TEvent e = new TEvent();
                if (Owner != null) Owner.GetEventRef(ref e);
                else TEventQueue.GetKeyEvent(ref e);

                switch (e.What)
                {
                    case EventCodes.EvMouseDown:
                        if (MouseInView(e.Mouse.Where) || MouseInOwner(e))
                        {
                            TrackMouse(e, ref mouseActive);
                            if (Size.Y == 1)
                                autoSelect = (Current == null || lastTargetItem != Current);
                            else if (!firstEvent && MouseInOwner(e))
                                action = MenuAction.DoReturn;
                        }
                        else
                        {
                            if (PutClickEventOnExit)
                                PutEvent(e);
                            action = MenuAction.DoReturn;
                        }
                        break;

                    case EventCodes.EvMouseUp:
                        TrackMouse(e, ref mouseActive);
                        if (MouseInOwner(e))
                            Current = Menu.Deflt;
                        else if (Current != null)
                        {
                            if (Current.Name != null)
                            {
                                if (Current != lastTargetItem)
                                    action = MenuAction.DoSelect;
                                else if (Size.Y == 1)
                                    action = MenuAction.DoReturn;
                                else
                                {
                                    action = MenuAction.DoNothing;
                                    lastTargetItem = null;
                                }
                            }
                        }
                        else if (mouseActive && !MouseInView(e.Mouse.Where))
                            action = MenuAction.DoReturn;
                        else if (Size.Y != 1)
                        {
                            Current = Menu.Deflt ?? Menu.Items;
                            action = MenuAction.DoNothing;
                        }
                        break;

                    case EventCodes.EvMouseMove:
                        if (e.Mouse.Buttons != 0)
                        {
                            TrackMouse(e, ref mouseActive);
                            if (!(MouseInView(e.Mouse.Where) || MouseInOwner(e)) && MouseInMenus(e))
                                action = MenuAction.DoReturn;
                            else if (Size.Y == 1 && mouseActive && Current != lastTargetItem)
                                autoSelect = true;
                        }
                        break;

                    case EventCodes.EvKeyDown:
                        {
                            ushort code = e.KeyDown.KeyCode;
                            switch (code)
                            {
                                case KeyCodes.KbUp:
                                case KeyCodes.KbDown:
                                    if (Size.Y != 1)
                                        TrackKey(code == KeyCodes.KbDown);
                                    else if (code == KeyCodes.KbDown)
                                        autoSelect = true;
                                    break;
                                case KeyCodes.KbLeft:
                                case KeyCodes.KbRight:
                                    if (Size.Y == 1)
                                        TrackKey(code == KeyCodes.KbRight);
                                    else if (ParentMenu != null)
                                        action = MenuAction.DoReturn;
                                    break;
                                case KeyCodes.KbHome:
                                case KeyCodes.KbEnd:
                                    if (Size.Y != 1)
                                    {
                                        Current = Menu.Items;
                                        if (code == KeyCodes.KbEnd)
                                            TrackKey(false);
                                    }
                                    break;
                                case KeyCodes.KbEnter:
                                    if (Size.Y == 1) autoSelect = true;
                                    action = MenuAction.DoSelect;
                                    break;
                                case KeyCodes.KbEsc:
                                    action = MenuAction.DoReturn;
                                    if (ParentMenu == null || ParentMenu.Size.Y != 1)
                                    {
                                        e.What = EventCodes.EvNothing;
                                    }
                                    break;
                                default:
                                    {
                                        char ch = (char)(code & 0xFF);
                                        TMenuItem p = null;
                                        bool isAlt = (code & 0xFF) == 0 && (code & 0xFF00) != 0;
                                        if (isAlt)
                                        {
                                            byte scan = (byte)(code >> 8);
                                            var tm = TopMenu();
                                            p = tm.FindTopLevelByAltItem(scan);
                                            if (p != null)
                                            {
                                                if (tm == this)
                                                {
                                                    if (Size.Y == 1) autoSelect = true;
                                                    action = MenuAction.DoSelect;
                                                    Current = p;
                                                }
                                                else
                                                    action = MenuAction.DoReturn;
                                                break;
                                            }
                                        }
                                        else if (code < 0x100 && code >= 32)
                                            p = FindItem(ch);

                                        if (p == null)
                                        {
                                            p = HotKey(e.KeyDown.ToKey());
                                            if (p != null && TView.CommandEnabled(p.Command))
                                            {
                                                result = p.Command;
                                                action = MenuAction.DoReturn;
                                            }
                                        }
                                        else
                                        {
                                            if (Size.Y == 1) autoSelect = true;
                                            action = MenuAction.DoSelect;
                                            Current = p;
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                    case EventCodes.EvCommand:
                        if (e.Message.Command == Commands.CmMenu)
                        {
                            autoSelect = false;
                            lastTargetItem = null;
                            if (ParentMenu != null)
                                action = MenuAction.DoReturn;
                        }
                        else
                            action = MenuAction.DoReturn;
                        break;
                }

                if (lastTargetItem != Current)
                    lastTargetItem = null;

                if (itemShown != Current)
                {
                    itemShown = Current;
                    DrawView();
                }

                if ((action == MenuAction.DoSelect || (action == MenuAction.DoNothing && autoSelect)) &&
                    Current != null && Current.Name != null)
                {
                    if (Current.Command == 0 && !Current.Disabled)
                    {
                        if ((e.What & (EventCodes.EvMouseDown | EventCodes.EvMouseMove)) != 0)
                            PutEvent(e);
                        TRect r = GetItemRect(Current);
                        r.A.X += Origin.X;
                        r.A.Y = r.B.Y + Origin.Y;
                        r.B.X = Owner.Size.X;
                        r.B.Y = Owner.Size.Y;
                        if (Size.Y == 1) r.A.X--;
                        var target = NewSubView(r, Current.SubMenu, this);
                        target.SetState(Commands.SfVisible, true);
                        target.SetState(Commands.SfExposed, true);
                        Owner.Insert(target);
                        result = Owner.ExecView(target);
                        Owner.Remove(target);
                        TObject.Destroy(target);
                        lastTargetItem = Current;
                        Menu.Deflt = Current;
                    }
                    else if (action == MenuAction.DoSelect)
                        result = Current.Command;
                }

                if (result != 0 && TView.CommandEnabled(result))
                {
                    action = MenuAction.DoReturn;
                    e.What = EventCodes.EvNothing;
                }
                else
                    result = 0;

                firstEvent = false;
            } while (action != MenuAction.DoReturn);

            if (Current != null)
            {
                Menu.Deflt = Current;
                Current = null;
                DrawView();
            }
            return result;
        }

        protected void DoASelect(TEvent ev)
        {
            PutEvent(ev);
            ushort cmd = Owner != null ? Owner.ExecView(this) : Execute();
            if (cmd != 0 && TView.CommandEnabled(cmd))
            {
                var ce = new TEvent();
                ce.What = EventCodes.EvCommand;
                ce.Message.Command = cmd;
                PutEvent(ce);
            }
        }
    }

    public class TMenuBar : TMenuView
    {
        public TMenuBar(TRect bounds, TMenu aMenu) : base(bounds)
        {
            Menu = aMenu;
            Items = aMenu?.Items;
            GrowMode = Commands.GfGrowHiX;
        }

        public override void Draw()
        {
            TAttrPair cNormal = GetColor(0x0301);
            TAttrPair cSelect = GetColor(0x0604);
            TAttrPair cNormDisabled = GetColor(0x0202);
            TAttrPair cSelDisabled = GetColor(0x0505);

            var b = new TDrawBuffer(Size.X);
            b.MoveChar(0, ' ', cNormal, Size.X);

            if (Menu != null)
            {
                short x = 1;
                for (var p = Menu.Items; p != null; p = p.Next)
                {
                    if (p.Name != null)
                    {
                        int l = StripHot(p.Name).Length;
                        if (x + l < Size.X)
                        {
                            TAttrPair color;
                            if (p.Disabled)
                                color = (p == Current) ? cSelDisabled : cNormDisabled;
                            else
                                color = (p == Current) ? cSelect : cNormal;

                            b.MoveChar(x, ' ', color, 1);
                            b.MoveCStr(x + 1, p.Name, color);
                            b.MoveChar(x + l + 1, ' ', color, 1);
                        }
                        x += (short)(l + 2);
                    }
                }
            }
            WriteLine((short)0, (short)0, (short)Size.X, (short)1, b);
        }

        public override TRect GetItemRect(TMenuItem item)
        {
            var r = new TRect(1, 0, 1, 1);
            var p = Menu.Items;
            while (true)
            {
                r.A.X = r.B.X;
                if (p.Name != null)
                    r.B.X += StripHot(p.Name).Length + 2;
                if (p == item)
                    return r;
                p = p.Next;
            }
        }

        public override void HandleEvent(TEvent ev)
        {
            if (Menu == null) return;

            if (ev.What == EventCodes.EvMouseDown)
            {
                DoASelect(ev);
            }
            else if (ev.What == EventCodes.EvCommand && ev.Message.Command == Commands.CmMenu)
            {
                DoASelect(ev);
                ClearEvent(ev);
            }
        }

        public override bool PreProcessKeyEvent(ref TEvent ev)
        {
            if (ev.What != EventCodes.EvKeyDown) return false;
            ushort code = ev.KeyDown.KeyCode;

            if ((code & 0xFF) == 0 && (code & 0xFF00) != 0)
            {
                byte scan = (byte)(code >> 8);
                if (FindTopLevelByAltItem(scan) != null)
                {
                    var altEv = new TEvent();
                    altEv.What = EventCodes.EvKeyDown;
                    altEv.KeyDown.KeyCode = code;
                    ev.What = EventCodes.EvNothing;
                    DoASelect(altEv);
                    return true;
                }
            }
            return false;
        }
    }

    public class TMenuBox : TMenuView
    {
        public TMenuBox(TRect bounds, TMenu aMenu, TMenuView parentMenu)
            : base(GetRect(bounds, aMenu))
        {
            Menu = aMenu;
            Items = aMenu?.Items;
            ParentMenu = parentMenu;
        }

        public static TRect GetRect(TRect bounds, TMenu aMenu)
        {
            short w = 0, h = 2;
            if (aMenu != null)
            {
                for (var p = aMenu.Items; p != null; p = p.Next)
                {
                    if (p.Name != null)
                    {
                        short l = (short)(StripHot(p.Name).Length + 6);
                        if (p.Command == 0)
                            l += 3;
                        else if (p.Param != null)
                            l += (short)(StripHot(p.Param).Length + 2);
                        w = Math.Max(l, w);
                    }
                    h++;
                }
            }

            var r = bounds;
            if (r.A.X + w < r.B.X)
                r.B.X = r.A.X + w;
            else
                r.A.X = r.B.X - w;

            if (r.A.Y + h < r.B.Y)
                r.B.Y = r.A.Y + h;
            else
                r.A.Y = r.B.Y - h;

            return r;
        }

        private void FrameLine(TDrawBuffer b, int n, TAttrPair cN, TAttrPair col)
        {
            char[] fc =
            {
                '\u250C', '\u2500', '\u2500', '\u2510',
                '\u2502', '\u2500', '\u2500', '\u2510',
                '\u251C', '\u2500', '\u2500', '\u2524',
                '\u2514', '\u2500', '\u2500', '\u2518'
            };
            switch (n)
            {
                case 0:
                    b.MoveChar(0, '\u250C', cN, 1);
                    b.MoveChar(1, '\u2500', col, Size.X - 2);
                    b.MoveChar(Size.X - 1, '\u2510', cN, 1);
                    break;
                case 5:
                    b.MoveChar(0, '\u2514', cN, 1);
                    b.MoveChar(1, '\u2500', col, Size.X - 2);
                    b.MoveChar(Size.X - 1, '\u2518', cN, 1);
                    break;
                case 10:
                    b.MoveChar(0, '\u2502', cN, 1);
                    b.MoveChar(1, ' ', col, Size.X - 2);
                    b.MoveChar(Size.X - 1, '\u2502', cN, 1);
                    break;
                case 15:
                    b.MoveChar(0, '\u251C', cN, 1);
                    b.MoveChar(1, '\u2500', col, Size.X - 2);
                    b.MoveChar(Size.X - 1, '\u2524', cN, 1);
                    break;
            }
        }

        public override void Draw()
        {
            TAttrPair cNormal = GetColor(0x0301);
            TAttrPair cSelect = GetColor(0x0604);
            TAttrPair cNormDisabled = GetColor(0x0202);
            TAttrPair cSelDisabled = GetColor(0x0505);

            var b = new TDrawBuffer(Size.X);
            short y = 0;
            FrameLine(b, 0, cNormal, cNormal);
            WriteLine((short)0, y++, (short)Size.X, (short)1, b);

            if (Menu != null)
            {
                for (var p = Menu.Items; p != null; p = p.Next)
                {
                    TAttrPair color = cNormal;
                    if (p.Name == null)
                        FrameLine(b, 15, cNormal, cNormal);
                    else
                    {
                        if (p.Disabled)
                            color = (p == Current) ? cSelDisabled : cNormDisabled;
                        else if (p == Current)
                            color = cSelect;
                        FrameLine(b, 10, cNormal, color);
                        b.MoveCStr(3, p.Name, color);
                        if (p.Command == 0)
                            b.WriteChar(Size.X - 4, '\u25BA', color);
                        else if (p.Param != null)
                            b.MoveCStr(Size.X - 3 - StripHot(p.Param).Length, p.Param, color);
                    }
                    WriteLine((short)0, y++, (short)Size.X, (short)1, b);
                }
            }
            FrameLine(b, 5, cNormal, cNormal);
            WriteLine((short)0, y, (short)Size.X, (short)1, b);
        }

        public override TRect GetItemRect(TMenuItem item)
        {
            short yPos = 1;
            var p = Menu.Items;
            while (p != item && p != null)
            {
                yPos++;
                p = p.Next;
            }
            return new TRect(2, yPos, (short)(Size.X - 2), (short)(yPos + 1));
        }
    }

    public class TMenuPopup : TMenuBox
    {
        public TMenuPopup(TRect bounds, TMenu aMenu)
            : base(bounds, aMenu, null) { }
    }

    public class TStatusItem
    {
        public string Text;
        public TKey KeyCode;
        public ushort Command;
        public TStatusItem Next;

        public TStatusItem(string aText, TKey aKeyCode, ushort aCommand, TStatusItem aNext = null)
        {
            Text = aText;
            KeyCode = aKeyCode;
            Command = aCommand;
            Next = aNext;
        }
    }

    public class TStatusDef
    {
        public ushort Min;
        public ushort Max;
        public TStatusItem Items;
        public TStatusDef Next;

        public TStatusDef(ushort aMin, ushort aMax, TStatusItem aItems, TStatusDef aNext = null)
        {
            Min = aMin;
            Max = aMax;
            Items = aItems;
            Next = aNext;
        }
    }

    public class TStatusLine : TView
    {
        private const string CpStatusLine = "\x02\x03\x04\x05\x06\x07";
        private const string HintSeparator = "\xB3 ";

        public TStatusDef Defs;
        public TStatusDef CurDefs;
        public TStatusItem Items;
        public ushort HelpCtx;

        public TStatusLine(TRect bounds, TStatusDef aDefs) : base(bounds)
        {
            Defs = aDefs;
            Options |= Commands.OfPreProcess;
            EventMask |= EventCodes.EvBroadcast;
            GrowMode = Commands.GfGrowLoY | Commands.GfGrowHiX | Commands.GfGrowHiY;
            FindItems();
        }

        public override TPalette GetPalette()
        {
            return new TPalette(CpStatusLine, 6);
        }

        private void FindItems()
        {
            var p = Defs;
            while (p != null && (HelpCtx < p.Min || HelpCtx > p.Max))
                p = p.Next;
            Items = p?.Items;
        }

        private void DrawSelect(TStatusItem selected)
        {
            TAttrPair cNormal = GetColor(0x0301);
            TAttrPair cSelect = GetColor(0x0604);
            TAttrPair cNormDisabled = GetColor(0x0202);
            TAttrPair cSelDisabled = GetColor(0x0505);

            var b = new TDrawBuffer(Size.X);
            b.MoveChar(0, ' ', cNormal, Size.X);
            var t = Items;
            int i = 0;

            while (t != null)
            {
                if (t.Text != null)
                {
                    int l = t.Text.Length;
                    if (i + l < Size.X)
                    {
                        TAttrPair color;
                        if (TView.CommandEnabled(t.Command))
                            color = (t == selected) ? cSelect : cNormal;
                        else
                            color = (t == selected) ? cSelDisabled : cNormDisabled;

                        b.MoveChar(i, ' ', color, 1);
                        b.MoveCStr(i + 1, t.Text, color);
                        b.MoveChar(i + l + 1, ' ', color, 1);
                    }
                    i += l + 2;
                }
                t = t.Next;
            }

            WriteLine((short)0, (short)0, (short)Size.X, (short)1, b);
        }

        public override void Draw()
        {
            DrawSelect(null);
        }

        public TStatusItem FindStatusItem(TKey key)
        {
            for (var t = Items; t != null; t = t.Next)
                if (t.KeyCode == key)
                    return t;
            return null;
        }

        private TStatusItem ItemMouseIsIn(TPoint mouse)
        {
            if (mouse.Y != 0) return null;
            int i = 0;
            for (var t = Items; t != null; t = t.Next)
            {
                if (t.Text != null)
                {
                    int k = i + t.Text.Length + 2;
                    if (mouse.X >= i && mouse.X < k)
                        return t;
                    i = k;
                }
            }
            return null;
        }

        public override void HandleEvent(TEvent ev)
        {
            switch (ev.What)
            {
                case EventCodes.EvMouseDown:
                    {
                        TStatusItem t = null;
                        do
                        {
                            TPoint mouse = MakeLocal(ev.Mouse.Where);
                            var nt = ItemMouseIsIn(mouse);
                            if (t != nt)
                            {
                                t = nt;
                                DrawSelect(t);
                            }
                            ev.What = EventCodes.EvNothing;
                            TEvent me = new TEvent();
                            GetEventRef(ref me);
                            if (me.What != EventCodes.EvMouseMove)
                            {
                                if (me.What != EventCodes.EvNothing)
                                    PutEvent(me);
                                break;
                            }
                            ev = me;
                        } while (true);

                        if (t != null && TView.CommandEnabled(t.Command))
                        {
                            var ce = new TEvent();
                            ce.What = EventCodes.EvCommand;
                            ce.Message.Command = t.Command;
                            PutEvent(ce);
                        }
                        DrawView();
                        break;
                    }

                case EventCodes.EvBroadcast:
                    if (ev.Message.Command == Commands.CmCommandSetChanged)
                    {
                        FindItems();
                        DrawView();
                    }
                    break;
            }
        }

        public override bool PreProcessKeyEvent(ref TEvent ev)
        {
            if (ev.What != EventCodes.EvKeyDown) return false;
            if (ev.KeyDown.KeyCode != KeyCodes.KbNoKey)
            {
                var key = ev.KeyDown.ToKey();
                for (var t = Items; t != null; t = t.Next)
                {
                    if (t.KeyCode == key && TView.CommandEnabled(t.Command))
                    {
                        ev.What = EventCodes.EvCommand;
                        ev.Message.Command = t.Command;
                        ev.Message.InfoPtr = null;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
