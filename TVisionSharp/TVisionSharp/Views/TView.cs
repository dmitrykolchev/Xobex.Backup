using System;
using System.Text;

namespace TVision
{
    public class TPalette
    {
        public TColorAttr[] Data;

        public TPalette(string palette, ushort size)
        {
            Data = new TColorAttr[size + 1];
            Data[0] = size;
            for (int i = 0; i < palette.Length && i < size; i++)
                Data[i + 1] = (byte)palette[i];
        }

        public TPalette(TColorAttr[] array, ushort size)
        {
            Data = new TColorAttr[size + 1];
            for (int i = 0; i < array.Length && i < size; i++)
                Data[i + 1] = array[i];
        }

        public TPalette(TPalette other)
        {
            if (other?.Data != null)
            {
                Data = new TColorAttr[other.Data.Length];
                Array.Copy(other.Data, Data, other.Data.Length);
            }
            else
                Data = Array.Empty<TColorAttr>();
        }

        public TColorAttr this[int index]
        {
            get => (index >= 0 && index < Data.Length) ? Data[index] : default;
            set { if (index >= 0 && index < Data.Length) Data[index] = value; }
        }
    }

    public class TView : TObject
    {
        public enum PhaseType { PhFocused, PhPreProcess, PhPostProcess }
        public enum SelectMode { NormalSelect, EnterSelect, LeaveSelect }

        public TPoint Size;
        public ushort Options;
        public ushort EventMask;
        public ushort State;
        public TPoint Origin;
        public TPoint Cursor;
        public byte GrowMode;
        public byte DragMode;
        public ushort HelpCtx;
        public TGroup Owner;
        public TView Next;
        public ushort EndState;

        public static bool CommandSetChanged;
        public static TCommandSet CurCommandSet = new TCommandSet();
        public static bool ShowMarkers;
        public static TColorAttr ErrorAttr;

        private TPoint _resizeBalance;

        static TView()
        {
            for (ushort c = 0; c < 256; c++)
                CurCommandSet.EnableCmd(c);
        }

        public TView(TRect bounds)
        {
            Size = new TPoint(bounds.Width, bounds.Height);
            Origin = bounds.A;
            Options = 0;
            EventMask = EventCodes.EvKeyboard | EventCodes.EvCommand;
            State = Commands.SfVisible;
            Owner = null;
            Next = null;
        }

        public override void ShutDown() { }

        public virtual void SizeLimits(out TPoint min, out TPoint max)
        {
            min = new TPoint(0, 0);
            max = new TPoint(1000, 1000);
        }

        public TRect GetBounds() => new TRect(Origin.X, Origin.Y, Origin.X + Size.X, Origin.Y + Size.Y);

        public TRect GetExtent() => new TRect(0, 0, Size.X, Size.Y);

        public TRect GetClipRect()
        {
            if (Owner == null) return GetExtent();
            var clip = Owner.Clip;
            clip.A.X -= Origin.X;
            clip.A.Y -= Origin.Y;
            clip.B.X -= Origin.X;
            clip.B.Y -= Origin.Y;
            var ext = GetExtent();
            return clip.Intersect(ext);
        }

        public bool MouseInView(TPoint mouse)
        {
            var ext = GetExtent();
            return ext.Contains(mouse);
        }

        public bool ContainsMouse(TEvent ev)
        {
            if (Owner == null || !GetState(Commands.SfVisible)) return false;
            return MouseInView(ev.Mouse.Where);
        }

        public virtual void Locate(TRect bounds)
        {
            ChangeBounds(bounds);
        }

        public virtual void ChangeBounds(TRect bounds)
        {
            SetBounds(bounds);
            DrawView();
        }

        public void SetBounds(TRect bounds)
        {
            Size = new TPoint(bounds.Width, bounds.Height);
            Origin = bounds.A;
        }

        public void GrowTo(short x, short y)
        {
            TPoint min, max;
            SizeLimits(out min, out max);
            x = (short)Math.Max(min.X, Math.Min(max.X, x));
            y = (short)Math.Max(min.Y, Math.Min(max.Y, y));
            var bounds = GetBounds();
            bounds.B.X = bounds.A.X + x;
            bounds.B.Y = bounds.A.Y + y;
            ChangeBounds(bounds);
        }

        public void MoveTo(short x, short y)
        {
            var bounds = GetBounds();
            bounds.A = new TPoint(x, y);
            bounds.B = new TPoint(x + Size.X, y + Size.Y);
            ChangeBounds(bounds);
        }

        public virtual void Draw() { }

        public void DrawView()
        {
            if ((State & Commands.SfExposed) != 0)
                Draw();
        }

        public bool Exposed()
        {
            return Owner != null && (State & Commands.SfExposed) != 0;
        }

        public bool Focus()
        {
            bool result = true;
            if ((State & (Commands.SfSelected | Commands.SfModal)) == 0)
            {
                if (Owner != null)
                {
                    result = Owner.Focus();
                    if (result && Owner.Current != this)
                        Select();
                }
            }
            return result;
        }

        public void Hide()
        {
            if (GetState(Commands.SfVisible))
            {
                SetState(Commands.SfVisible, false);
                DrawHide(null);
            }
        }

        public void Show()
        {
            if (!GetState(Commands.SfVisible))
            {
                SetState(Commands.SfVisible, true);
                DrawShow(null);
            }
        }

        public virtual void HideCursor() => SetState(Commands.SfCursorVis, false);
        public void ShowCursor() => SetState(Commands.SfCursorVis, true);
        public void BlockCursor() => SetState(Commands.SfCursorIns, true);
        public void NormalCursor() => SetState(Commands.SfCursorIns, false);

        public virtual void ResetCursor() { }

        public void SetCursor(int x, int y)
        {
            Cursor = new TPoint(x, y);
            DrawCursor();
        }

        public void DrawCursor()
        {
            if (GetState(Commands.SfFocused | Commands.SfCursorVis))
                ResetCursor();
        }

        public void DrawHide(TView lastView)
        {
            if ((State & Commands.SfVisible) == 0 && Owner != null)
            {
                var r = GetBounds();
                Owner.Redraw();
            }
        }

        public void DrawShow(TView lastView)
        {
            State |= Commands.SfExposed;
            if ((State & Commands.SfVisible) != 0)
            {
                DrawView();
                if ((Options & Commands.OfSelectable) != 0)
                {
                    SetState(Commands.SfSelected, Owner?.Current == this);
                }
            }
        }

        public void DrawUnderRect(TRect r, TView lastView)
        {
            if (Owner != null) Owner.Redraw();
        }

        public void DrawUnderView(bool doShadow, TView lastView)
        {
            if (Owner != null) Owner.Redraw();
        }

        public void DrawAndFlush()
        {
            var root = this;
            while (root.Owner != null) root = root.Owner;
            root.DrawView();
            TScreen.FlushScreen();
        }

        public virtual ushort DataSize() => 0;
        public virtual void GetData(object rec) { }
        public virtual void SetData(object rec) { }
        public virtual void Awaken() { }

        public virtual ushort GetHelpCtx() => HelpCtx;

        public virtual bool Valid(ushort command) => true;

        public virtual void GetEvent(TEvent ev)
        {
            Owner?.GetEvent(ev);
        }

        public virtual void GetEventRef(ref TEvent ev)
        {
            if (Owner != null)
                Owner.GetEventRef(ref ev);
            else
                TEventQueue.GetKeyEvent(ref ev);
        }

        public virtual void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvMouseDown)
            {
                if (!GetState(Commands.SfSelected | Commands.SfDisabled) &&
                    (Options & Commands.OfSelectable) != 0 &&
                    !Focus())
                    ClearEvent(ev);
            }
        }

        public virtual bool PreProcessKeyEvent(ref TEvent ev)
        {
            return false;
        }

        public virtual void PutEvent(TEvent ev)
        {
            Owner?.PutEvent(ev);
        }

        public void GetEvent(TEvent ev, int timeoutMs) { }

        public void KeyEvent(TEvent ev) { }

        public bool MouseEvent(TEvent ev, ushort mask)
        {
            return false;
        }

        public void ClearEvent(TEvent ev)
        {
            ev.What = EventCodes.EvNothing;
            ev.Message.Command = 0;
            ev.Message.InfoPtr = null;
        }

        public bool EventAvail()
        {
            var ev = new TEvent();
            GetEvent(ev);
            if (ev.What == EventCodes.EvNothing) return true;
            PutEvent(ev);
            return false;
        }

        public static bool CommandEnabled(ushort command)
        {
            return CurCommandSet.Has(command);
        }

        public static void EnableCommand(ushort command)
        {
            CurCommandSet.EnableCmd(command);
            CommandSetChanged = true;
        }

        public static void DisableCommand(ushort command)
        {
            CurCommandSet.DisableCmd(command);
            CommandSetChanged = true;
        }

        public static void EnableCommands(TCommandSet commands)
        {
            CurCommandSet.EnableCmd(commands);
            CommandSetChanged = true;
        }

        public static void DisableCommands(TCommandSet commands)
        {
            CurCommandSet.DisableCmd(commands);
            CommandSetChanged = true;
        }

        public static void GetCommands(TCommandSet commands)
        {
            commands.OrAssign(CurCommandSet);
        }

        public static void SetCommands(TCommandSet commands)
        {
            CurCommandSet.OrAssign(commands);
            CommandSetChanged = true;
        }

        public static void SetCmdState(TCommandSet commands, bool enable)
        {
            if (enable) CurCommandSet.EnableCmd(commands);
            else CurCommandSet.DisableCmd(commands);
            CommandSetChanged = true;
        }

        public virtual void EndModal(ushort command) { }

        public virtual ushort Execute() => 0;

        public TAttrPair GetColor(ushort color)
        {
            uint pair = 0;
            if ((color & 0xFF00) != 0)
                pair = MapColorChain((uint)(color >> 8)) << 8;
            pair |= MapColorChain((uint)(color & 0xFF));
            return new TAttrPair(
                new TColorAttr((byte)(pair & 0xFF)),
                new TColorAttr((byte)((pair >> 8) & 0xFF)));
        }

        public virtual TPalette GetPalette()
        {
            return null;
        }

        private uint MapColorChain(uint value)
        {
            var pal = GetPalette();
            uint color;
            if (pal != null && pal.Data != null && pal.Data.Length > 1)
            {
                int len = pal.Data[0].ToBios();
                if (value > 0 && value <= len)
                    color = pal.Data[value].ToBios();
                else
                    return ErrorAttr.ToBios() == 0 ? 0x07u : ErrorAttr.ToBios();
            }
            else
                color = value;
            if (color == 0)
                color = ErrorAttr.ToBios() == 0 ? 0x07u : ErrorAttr.ToBios();
            if (Owner != null)
                return Owner.MapColorChain(color);
            return color;
        }

        public virtual TColorAttr MapColor(byte color)
        {
            return new TColorAttr((byte)(MapColorChain(color) & 0xFF));
        }

        public bool GetState(ushort aState) => (State & aState) == aState;

        public virtual void SetState(ushort aState, bool enable)
        {
            if (enable) State |= aState;
            else State &= (ushort)~aState;
        }

        public void Select()
        {
            if ((Options & Commands.OfSelectable) != 0 && Owner != null)
                Owner.SetCurrent(this, SelectMode.NormalSelect);
        }

        public void MakeGlobal(TPoint source, out TPoint dest)
        {
            dest = source;
            var view = this;
            while (view != null)
            {
                dest.X += view.Origin.X;
                dest.Y += view.Origin.Y;
                view = view.Owner;
            }
        }

        public TPoint MakeGlobal(TPoint source)
        {
            MakeGlobal(source, out var dest);
            return dest;
        }

        public TPoint MakeLocal(TPoint source)
        {
            TPoint dest = source;
            var view = this;
            while (view != null)
            {
                dest.X -= view.Origin.X;
                dest.Y -= view.Origin.Y;
                view = view.Owner;
            }
            return dest;
        }

        public TView NextView()
        {
            return Next;
        }

        public TView Prev()
        {
            if (Owner == null) return null;
            var v = Owner.First();
            while (v != null && v.Next != this)
                v = v.Next;
            return v;
        }

        public void MakeFirst()
        {
            Owner?.InsertBefore(this, Owner.First());
        }

        public void PutInFrontOf(TView target)
        {
            Owner?.InsertBefore(this, target);
        }

        public TView TopView()
        {
            var view = this;
            while (view?.Owner != null)
                view = view.Owner;
            return view;
        }

        public void WriteBuf(short x, short y, short w, short h, TScreenCell[] b)
        {
            if (Owner == null || b == null || w <= 0 || h <= 0) return;
            Owner.GetBuffer();
            if (Owner.Buffer == null) return;

            var clip = GetClipRect();
            clip.A.X += Origin.X;
            clip.A.Y += Origin.Y;
            clip.B.X += Origin.X;
            clip.B.Y += Origin.Y;
            int ownerW = Owner.Size.X;

            for (int row = 0; row < h; row++)
            {
                int destY = Origin.Y + y + row;
                if (destY < clip.A.Y || destY >= clip.B.Y) continue;

                for (int col = 0; col < w; col++)
                {
                    int destX = Origin.X + x + col;
                    if (destX < clip.A.X || destX >= clip.B.X) continue;

                    int srcIdx = row * w + col;
                    if (srcIdx < 0 || srcIdx >= b.Length) continue;
                    int destIdx = destY * ownerW + destX;
                    if (destIdx < 0 || destIdx >= Owner.Buffer.Length) continue;

                    Owner.Buffer[destIdx] = b[srcIdx];
                }
            }
        }

        public void WriteBuf(short x, short y, short w, short h, TDrawBuffer b)
        {
            WriteBuf(x, y, w, h, b.Data);
        }

        public void WriteChar(short x, short y, char c, byte color, short count)
        {
            if (Owner == null || count <= 0) return;
            Owner.GetBuffer();
            if (Owner.Buffer == null) return;

            var attr = new TColorAttr(color);
            var clip = GetClipRect();
            clip.A.X += Origin.X;
            clip.A.Y += Origin.Y;
            clip.B.X += Origin.X;
            clip.B.Y += Origin.Y;
            int ownerW = Owner.Size.X;

            for (int col = 0; col < count; col++)
            {
                int destX = Origin.X + x + col;
                int destY = Origin.Y + y;
                if (destX < clip.A.X || destX >= clip.B.X) continue;
                if (destY < clip.A.Y || destY >= clip.B.Y) continue;
                int destIdx = destY * ownerW + destX;
                if (destIdx >= 0 && destIdx < Owner.Buffer.Length)
                    Owner.Buffer[destIdx] = new TScreenCell(c, attr);
            }
        }

        public void WriteLine(short x, short y, short w, short h, TDrawBuffer b)
        {
            WriteBuf(x, y, w, h, b.Data);
        }

        public void WriteLine(short x, short y, short w, short h, TScreenCell[] b)
        {
            WriteBuf(x, y, w, h, b);
        }

        public void WriteStr(short x, short y, string str, byte color)
        {
            if (Owner == null || str == null) return;
            Owner.GetBuffer();
            if (Owner.Buffer == null) return;

            var attr = new TColorAttr(color);
            var clip = GetClipRect();
            clip.A.X += Origin.X;
            clip.A.Y += Origin.Y;
            clip.B.X += Origin.X;
            clip.B.Y += Origin.Y;
            int ownerW = Owner.Size.X;
            int destY = Origin.Y + y;

            if (destY < clip.A.Y || destY >= clip.B.Y) return;

            for (int i = 0; i < str.Length; i++)
            {
                int destX = Origin.X + x + i;
                if (destX < clip.A.X || destX >= clip.B.X) continue;
                int destIdx = destY * ownerW + destX;
                if (destIdx >= 0 && destIdx < Owner.Buffer.Length)
                    Owner.Buffer[destIdx] = new TScreenCell(str[i], attr);
            }
        }

        public void WriteView(short x, short y, short count, TScreenCell[] b)
        {
            if (Owner == null || b == null || count <= 0) return;
            Owner.GetBuffer();
            if (Owner.Buffer == null) return;

            var clip = GetClipRect();
            clip.A.X += Origin.X;
            clip.A.Y += Origin.Y;
            clip.B.X += Origin.X;
            clip.B.Y += Origin.Y;
            int ownerW = Owner.Size.X;

            for (int i = 0; i < count; i++)
            {
                int destX = Origin.X + x + i;
                int destY = Origin.Y + y;
                if (destX < clip.A.X || destX >= clip.B.X) continue;
                if (destY < clip.A.Y || destY >= clip.B.Y) continue;
                int destIdx = destY * ownerW + destX;
                if (i < b.Length && destIdx >= 0 && destIdx < Owner.Buffer.Length)
                    Owner.Buffer[destIdx] = b[i];
            }
        }

        public virtual TTimerId SetTimer(uint timeoutMs, int periodMs = -1) => null;
        public virtual void KillTimer(TTimerId id) { }
    }

    public class TGroup : TView
    {
        public TView Last;
        public TView Current;
        public TRect Clip;
        public PhaseType Phase;
        public TScreenCell[] Buffer;
        public byte LockFlag;

        public TGroup(TRect bounds) : base(bounds)
        {
            Clip = GetExtent();
            Buffer = null;
            Last = null;
            Phase = PhaseType.PhFocused;
        }

        public override void ShutDown()
        {
            var v = First();
            while (v != null)
            {
                var next = v.Next;
                TObject.Destroy(v);
                v = next;
            }
            FreeBuffer();
        }

        public TView First() => Last?.Next;

        public TView At(short index)
        {
            var v = First();
            for (short i = 0; v != null && i < index; i++)
                v = v.Next;
            return v;
        }

        public short IndexOf(TView p)
        {
            short i = 0;
            var v = First();
            while (v != null)
            {
                if (v == p) return i;
                v = v.Next;
                i++;
            }
            return -1;
        }

        public void Insert(TView p)
        {
            InsertView(p, null);
        }

        public void InsertBefore(TView p, TView Target)
        {
            InsertView(p, Target);
        }

        public void InsertView(TView p, TView target)
        {
            if (p == null) return;
            p.Owner = this;
            if (target != null)
            {
                var prev = target.Prev();
                if (prev != null) prev.Next = p;
                else Last.Next = p;
                p.Next = target;
            }
            else
            {
                if (Last != null)
                {
                    p.Next = Last.Next;
                    Last.Next = p;
                }
                else
                {
                    p.Next = p;
                    Last = p;
                }
            }
        }

        public void Remove(TView p)
        {
            if (p == null || p.Owner != this) return;
            var prev = p.Prev();
            if (prev != null) prev.Next = p.Next;
            else Last.Next = p.Next;
            if (Last == p) Last = prev;
            if (Last == p) Last = null;
            p.Owner = null;
            p.Next = null;
        }

        public void RemoveView(TView p)
        {
            Remove(p);
        }

        public override ushort Execute()
        {
            return 0;
        }

        public ushort ExecView(TView p)
        {
            if (p == null) return Commands.CmCancel;
            var saveCurrent = Current;
            p.SetState(Commands.SfModal, true);
            SetCurrent(p, SelectMode.EnterSelect);
            ushort result;
            if (p is TMenuBox directBox)
                result = directBox.Execute();
            else
                result = p.Execute();
            SetCurrent(saveCurrent, SelectMode.LeaveSelect);
            p.SetState(Commands.SfModal, false);
            return result;
        }

        public void SetCurrent(TView p, SelectMode mode)
        {
            if (p == Current)
            {
                if (mode != SelectMode.NormalSelect && p != null)
                    p.SetState(Commands.SfSelected, true);
                return;
            }
            if (Current != null)
            {
                Current.SetState(Commands.SfSelected, false);
                Current.SetState(Commands.SfFocused, false);
            }
            Current = p;
            if (p != null)
            {
                p.SetState(Commands.SfSelected, true);
                p.SetState(Commands.SfFocused, true);
            }
        }

        public void SelectNext(bool forwards)
        {
            var v = Current;
            if (v == null) v = First();
            if (v != null)
            {
                var start = v;
                do
                {
                    v = forwards ? v.Next : v.Prev();
                    if (v == null) v = forwards ? First() : Last;
                    if (v != null && (v.Options & Commands.OfSelectable) != 0)
                    {
                        SetCurrent(v, SelectMode.NormalSelect);
                        return;
                    }
                } while (v != start);
            }
        }

        public bool FocusNext(bool forwards)
        {
            return false;
        }

        public override void GetEvent(TEvent ev)
        {
            if (Phase == PhaseType.PhFocused && Current != null)
                Current.GetEvent(ev);
            else
                base.GetEvent(ev);
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvKeyDown)
            {
                TEvent local = ev;
                TView p = Last;
                while (p != null)
                {
                    if (p.GetState(Commands.SfVisible) && p.PreProcessKeyEvent(ref local))
                    {
                        ev = local;
                        break;
                    }
                    p = p.Next;
                    if (p == Last) break;
                }

                if (ev.What == EventCodes.EvCommand)
                {
                    if (Current != null && Current.GetState(Commands.SfVisible))
                        Current.HandleEvent(ev);
                    return;
                }
                if (ev.What != EventCodes.EvKeyDown)
                {
                    return;
                }

                if (Current != null && Current.GetState(Commands.SfVisible))
                    Current.HandleEvent(ev);
                return;
            }
            if (ev.What == EventCodes.EvCommand)
            {
                if (Current != null && Current.GetState(Commands.SfVisible))
                    Current.HandleEvent(ev);
                return;
            }

            TView q = Last;
            while (q != null)
            {
                if (q.GetState(Commands.SfVisible))
                {
                    q.HandleEvent(ev);
                }
                q = q.Next;
                if (q == Last) break;
            }
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if (aState == Commands.SfFocused)
            {
                Current?.SetState(Commands.SfFocused, enable);
            }
            if (aState == Commands.SfExposed)
            {
                var v = First();
                while (v != null)
                {
                    v.SetState(aState, enable);
                    v = v.Next;
                    if (v == First()) break;
                }
            }
        }

        public TView FirstThat(Func<TView, bool> test)
        {
            var v = First();
            while (v != null)
            {
                if (test(v)) return v;
                v = v.Next;
                if (v == First()) break;
            }
            return null;
        }

        public TView FirstMatch(ushort aState, ushort aOptions)
        {
            return FirstThat(v => v.GetState(aState) && (v.Options & aOptions) == aOptions);
        }

        public override void Draw()
        {
            var p = Last;
            while (p != null)
            {
                if (p.GetState(Commands.SfVisible))
                    p.DrawView();
                p = p.Next;
                if (p == Last) break;
            }

            if (Owner != null && Buffer != null && Size.X > 0 && Size.Y > 0)
                WriteBuf((short)0, (short)0, (short)Size.X, (short)Size.Y, Buffer);
        }

        public void Redraw()
        {
            DrawView();
        }

        public void Lock()
        {
            LockFlag++;
        }

        public void Unlock()
        {
            if (LockFlag > 0) LockFlag--;
            if (LockFlag == 0) Redraw();
        }

        public void DrawSubViews(TView p, TView bottom)
        {
            while (p != null && p != bottom)
            {
                if (p.GetState(Commands.SfVisible))
                    p.DrawView();
                p = p.Next;
            }
        }

        public override void ChangeBounds(TRect bounds)
        {
            var oldBounds = GetBounds();
            Clip = new TRect(0, 0, bounds.Width, bounds.Height);
            int dxb = bounds.B.X - oldBounds.B.X;
            int dyb = bounds.B.Y - oldBounds.B.Y;
            SetBounds(bounds);
            GetBuffer();

            var v = First();
            while (v != null)
            {
                byte gm = v.GrowMode;
                if (gm != 0)
                {
                    var b = v.GetBounds();
                    if ((gm & Commands.GfGrowLoX) != 0) b.B.X += dxb;
                    else if ((gm & Commands.GfGrowHiX) != 0) { b.A.X += dxb; b.B.X += dxb; }
                    if ((gm & Commands.GfGrowLoY) != 0) b.B.Y += dyb;
                    else if ((gm & Commands.GfGrowHiY) != 0) { b.A.Y += dyb; b.B.Y += dyb; }
                    v.Locate(b);
                }
                v = v.Next;
                if (v == First()) break;
            }
        }

        public override ushort DataSize()
        {
            ushort total = 0;
            var v = First();
            while (v != null)
            {
                total += v.DataSize();
                v = v.Next;
                if (v == First()) break;
            }
            return total;
        }

        public override void GetData(object rec)
        {
        }

        public override void SetData(object rec)
        {
        }

        public override void ResetCursor()
        {
            if (Current != null) Current.ResetCursor();
        }

        public override void EndModal(ushort command)
        {
            EndState = command;
            SetState(Commands.SfModal, false);
        }

        public void EventError(TEvent ev)
        {
            Owner?.EventError(ev);
        }

        public override ushort GetHelpCtx()
        {
            return Current?.GetHelpCtx() ?? 0;
        }

        public override bool Valid(ushort command)
        {
            if (command == 0)
            {
                var v = First();
                while (v != null)
                {
                    if (!v.Valid(0)) return false;
                    v = v.Next;
                    if (v == First()) break;
                }
                return true;
            }
            return true;
        }

        public void FreeBuffer()
        {
            Buffer = null;
        }

        public void GetBuffer()
        {
            int count = Math.Max(0, Size.X) * Math.Max(0, Size.Y);
            if (Buffer == null || Buffer.Length != count)
                Buffer = count > 0 ? new TScreenCell[count] : null;
        }

        public void ForEach(Action<TView> action)
        {
            var v = First();
            while (v != null)
            {
                var next = v.Next;
                action(v);
                v = next;
                if (v == First()) break;
            }
        }
    }

    public class TWindowInit
    {
        public Func<TRect, TFrame> CreateFrame;

        public TWindowInit(Func<TRect, TFrame> cFrame)
        {
            CreateFrame = cFrame;
        }
    }

    public class TWindow : TGroup
    {
        public byte Flags;
        public TRect ZoomRect;
        public short Number;
        public short Palette;
        public TFrame Frame;
        public string Title;

        public const string CpBlueWindow = "\x17\x1F\x1E\x70\x78\x17\x1F\x1B";
        public const string CpCyanWindow = "\x1F\x17\x1E\x70\x78\x1F\x1F\x1A";
        public const string CpGrayWindow = "\x70\x78\x7F\x0F\x78\x70\x70\x70";

        public TWindow(TRect bounds, string aTitle, short aNumber)
            : base(bounds)
        {
            Title = aTitle ?? string.Empty;
            Number = aNumber;
            Palette = 0;
            Flags = Commands.WfMove | Commands.WfClose | Commands.WfZoom;
            Frame = null;

            var frameBounds = GetExtent();
            Frame = new TFrame(frameBounds);
            Frame.Owner = this;
            Frame.Next = First();
            if (Last != null) Last.Next = Frame;
            else { Frame.Next = Frame; Last = Frame; }
        }

        public static TFrame InitFrame(TRect r) => new TFrame(r);

        public virtual void Close()
        {
            if ((State & Commands.SfModal) != 0)
                EndModal(Commands.CmClose);
            else
                Hide();
        }

        public virtual string GetTitle(short maxSize) => Title;

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvCommand)
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmClose:
                        Close();
                        ClearEvent(ev);
                        break;
                    case Commands.CmZoom:
                        Zoom();
                        ClearEvent(ev);
                        break;
                    case Commands.CmResize:
                        break;
                }
            }
            base.HandleEvent(ev);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if (aState == Commands.SfFocused || aState == Commands.SfExposed)
                Frame?.SetState(aState, enable);
        }

        public override void SizeLimits(out TPoint min, out TPoint max)
        {
            min = new TPoint(2, 1);
            max = new TPoint(1000, 1000);
        }

        public TScrollBar StandardScrollBar(ushort aOptions)
        {
            var extent = GetExtent();
            TRect r;
            if ((aOptions & Commands.SbVertical) != 0)
            {
                r = new TRect(extent.B.X - 1, 0, extent.B.X, extent.B.Y);
            }
            else
            {
                r = new TRect(0, extent.B.Y - 1, extent.B.X, extent.B.Y);
            }
            var sb = new TScrollBar(r);
            Insert(sb);
            return sb;
        }

        public virtual void Zoom()
        {
        }

        public override void ShutDown()
        {
            Frame?.ShutDown();
            base.ShutDown();
        }
    }

    public class TFrame : TView
    {
        private const string CpFrame = "\x01\x01\x02\x02\x03";

        public static readonly byte[] InitFrame =
        {
            0xDA, 0xC4, 0xBF, 0xB3, 0x20, 0xB3, 0xC0, 0xC4, 0xD9,
            0xC9, 0xCD, 0xBB, 0xBA, 0x20, 0xBA, 0xC8, 0xCD, 0xBC,
            0xC7, 0xB4, 0xB6, 0xD1, 0xCF, 0xD1, 0xB6, 0xC7, 0xCF
        };

        public static readonly string CloseIcon = "[~\u25A0~]";
        public static readonly string ZoomIcon = "[~\u25B2~]";
        public static readonly string UnZoomIcon = "[~\u25BC~]";
        public static readonly string DragIcon = "~\u2500\u2518~";
        public static readonly string DragLeftIcon = "~\u2514\u2500~";

        private byte[] _frameMask;

        public TFrame(TRect bounds) : base(bounds)
        {
            GrowMode = Commands.GfGrowHiX | Commands.GfGrowHiY;
            EventMask |= EventCodes.EvBroadcast | EventCodes.EvMouseUp;
        }

        public override TPalette GetPalette()
        {
            return new TPalette(CpFrame, 5);
        }

        private static char Cp437(byte b)
        {
            return b switch
            {
                0xC4 => '\u2500', 0xB3 => '\u2502', 0xDA => '\u250C', 0xBF => '\u2510',
                0xC0 => '\u2514', 0xD9 => '\u2518', 0xC3 => '\u251C', 0xB4 => '\u2524',
                0xC2 => '\u252C', 0xC1 => '\u2534', 0xC5 => '\u253C',
                0xCD => '\u2550', 0xBA => '\u2551', 0xC9 => '\u2554', 0xBB => '\u2557',
                0xC8 => '\u255A', 0xBC => '\u255D', 0xCC => '\u255E', 0xB9 => '\u2563',
                0xCB => '\u2564', 0xCA => '\u2567', 0xCE => '\u256C',
                _ => (char)b
            };
        }

        private void FrameLine(TDrawBuffer b, short y, short n, TAttrPair color)
        {
            if (_frameMask == null || _frameMask.Length != Size.X)
                _frameMask = new byte[Math.Max(0, Size.X)];
            if (Size.X <= 0) return;

            _frameMask[0] = InitFrame[n];
            for (int x = 1; x < Size.X - 1; ++x)
                _frameMask[x] = InitFrame[n + 1];
            _frameMask[Size.X - 1] = InitFrame[n + 2];

            if (Owner != null && Owner.Last != null && Owner.Last.Next != null)
            {
                for (var v = Owner.Last.Next; v != this && v != null; v = v.Next)
                {
                    if ((v.Options & Commands.OfFramed) != 0 && v.GetState(Commands.SfVisible))
                    {
                        ushort mask = 0;
                        if (y < v.Origin.Y)
                        {
                            if (y == v.Origin.Y - 1)
                                mask = 0x0A06;
                        }
                        else if (y < v.Origin.Y + v.Size.Y)
                            mask = 0x0005;
                        else if (y == v.Origin.Y + v.Size.Y)
                            mask = 0x0A03;

                        if (mask != 0)
                        {
                            int start = Math.Max(v.Origin.X, 1);
                            int end = Math.Min(v.Origin.X + v.Size.X, Size.X - 1);
                            if (start < end)
                            {
                                byte maskLow = (byte)(mask & 0x00FF);
                                byte maskHigh = (byte)((mask & 0xFF00) >> 8);
                                _frameMask[start - 1] |= maskLow;
                                _frameMask[end] |= (byte)(maskLow ^ maskHigh);
                                if (maskLow != 0)
                                    for (int x = start; x < end; x++)
                                        _frameMask[x] |= maskHigh;
                            }
                        }
                    }
                    if (v.Next == Owner.Last.Next) break;
                }
            }

            for (int x = 0; x < Size.X; x++)
            {
                b.WriteChar((short)x, Cp437(_frameMask[x]), color);
            }
        }

        public override void Draw()
        {
            TAttrPair cFrame, cTitle;
            short f;

            if (GetState(Commands.SfDragging))
            {
                cFrame = GetColor(0x0505);
                cTitle = GetColor(0x0005);
                f = 0;
            }
            else if (!GetState(Commands.SfActive))
            {
                cFrame = GetColor(0x0101);
                cTitle = GetColor(0x0002);
                f = 0;
            }
            else
            {
                cFrame = GetColor(0x0503);
                cTitle = GetColor(0x0004);
                f = 9;
            }

            int width = Size.X;
            int l = width - 10;
            var win = Owner as TWindow;
            if (win != null && (win.Flags & (Commands.WfClose | Commands.WfZoom)) != 0)
                l -= 6;

            var b = new TDrawBuffer(width);
            FrameLine(b, 0, f, cFrame);

            if (win != null)
            {
                string title = win.GetTitle((short)Math.Max(0, l));
                if (title != null)
                {
                    int tl = Math.Min(title.Length, width - 10);
                    tl = Math.Max(tl, 0);
                    int i = (width - tl) >> 1;
                    b.MoveChar((short)(i - 1), ' ', cTitle, 1);
                    b.MoveStr((short)i, title, tl, cTitle);
                    b.MoveChar((short)(i + tl), ' ', cTitle, 1);
                }

                if (GetState(Commands.SfActive))
                {
                    if ((win.Flags & Commands.WfClose) != 0)
                        b.MoveCStr(2, CloseIcon, cFrame);
                    if ((win.Flags & Commands.WfZoom) != 0)
                    {
                        TPoint minSize, maxSize;
                        win.SizeLimits(out minSize, out maxSize);
                        if (win.Size == maxSize)
                            b.MoveCStr(width - 5, UnZoomIcon, cFrame);
                        else
                            b.MoveCStr(width - 5, ZoomIcon, cFrame);
                    }
                }
            }

            WriteLine(0, 0, (short)width, 1, b);

            for (short i = 1; i <= Size.Y - 2; i++)
            {
                FrameLine(b, i, (short)(f + 3), cFrame);
                WriteLine((short)0, i, (short)width, (short)1, b);
            }

            FrameLine(b, (short)(Size.Y - 1), (short)(f + 6), cFrame);
            if (GetState(Commands.SfActive) && win != null && (win.Flags & Commands.WfGrow) != 0)
            {
                b.MoveCStr(0, DragLeftIcon, cFrame);
                b.MoveCStr((short)(width - 2), DragIcon, cFrame);
            }
            WriteLine((short)0, (short)(Size.Y - 1), (short)width, (short)1, b);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
            if (ev.What != EventCodes.EvMouseDown) return;
            var win = Owner as TWindow;
            if (win == null) return;

            TPoint mouse = MakeLocal(ev.Mouse.Where);
            if (mouse.Y == 0)
            {
                if ((win.Flags & Commands.WfClose) != 0 &&
                    GetState(Commands.SfActive) &&
                    mouse.X >= 2 && mouse.X <= 4)
                {
                    var ce = new TEvent();
                    ce.What = EventCodes.EvCommand;
                    ce.Message.Command = Commands.CmClose;
                    ce.Message.InfoPtr = win;
                    PutEvent(ce);
                    ClearEvent(ev);
                }
                else if ((win.Flags & Commands.WfZoom) != 0 &&
                         GetState(Commands.SfActive) &&
                         ((mouse.X >= Size.X - 5 && mouse.X <= Size.X - 3) ||
                          (ev.Mouse.EventFlags & EventCodes.MeDoubleClick) != 0))
                {
                    var ce = new TEvent();
                    ce.What = EventCodes.EvCommand;
                    ce.Message.Command = Commands.CmZoom;
                    ce.Message.InfoPtr = win;
                    PutEvent(ce);
                    ClearEvent(ev);
                }
            }
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if ((aState & (Commands.SfActive | Commands.SfDragging)) != 0)
                DrawView();
        }
    }

    public class TScrollBar : TView
    {
        public int Value;
        public char[] Chars;
        public int MinVal;
        public int MaxVal;
        public int PgStep;
        public int ArStep;

        private static readonly char[] VChars = { '\u2191', '\u2193', '\u25B2', '\u25BC', '\u25CF' };
        private static readonly char[] HChars = { '\u2190', '\u2192', '\u25C4', '\u25BA', '\u25CF' };

        public TScrollBar(TRect bounds) : base(bounds)
        {
            Chars = new char[5];
            Array.Copy(VChars, Chars, 5);
            MinVal = 0;
            MaxVal = 100;
            Value = 0;
            PgStep = 10;
            ArStep = 1;
        }

        public override void Draw()
        {
            var buf = new TDrawBuffer(Size.X);
            buf.Clear();

            TColorAttr normalColor = MapColor(1);
            if (normalColor == default) normalColor = new TColorAttr(0x07);
            TColorAttr arrowColor = MapColor(2);
            if (arrowColor == default) arrowColor = new TColorAttr(0x07);
            TColorAttr thumbColor = MapColor(3);
            if (thumbColor == default) thumbColor = new TColorAttr(0x0F);

            bool vertical = Size.X == 1 || Size.Y > Size.X;
            int len = vertical ? Size.Y : Size.X;

            if (len < 3) { WriteBuf(0, 0, (short)Size.X, (short)Size.Y, buf); return; }

            if (vertical)
            {
                buf.MoveChar(0, Chars[0], arrowColor, 1);
                buf.MoveChar(len - 1, Chars[1], arrowColor, 1);

                int range = MaxVal - MinVal;
                int thumbPos = 0;
                if (range > 0)
                    thumbPos = Math.Max(1, Math.Min(len - 2, (int)((long)Value * (len - 2) / range)));
                buf.MoveChar(thumbPos, Chars[4], thumbColor, 1);
            }
            else
            {
                buf.MoveChar(0, Chars[0], arrowColor, 1);
                buf.MoveChar(len - 1, Chars[1], arrowColor, 1);

                int range = MaxVal - MinVal;
                int thumbPos = 0;
                if (range > 0)
                    thumbPos = Math.Max(1, Math.Min(len - 2, (int)((long)Value * (len - 2) / range)));
                buf.MoveChar(thumbPos, Chars[4], thumbColor, 1);
            }

            WriteBuf(0, 0, (short)Size.X, (short)Size.Y, buf);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x04\x05\x05", 3);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public virtual void ScrollDraw() { }

        public virtual int ScrollStep(int part)
        {
            switch (part)
            {
                case Commands.SbLeftArrow:
                case Commands.SbUpArrow: return -ArStep;
                case Commands.SbRightArrow:
                case Commands.SbDownArrow: return ArStep;
                case Commands.SbPageLeft:
                case Commands.SbPageUp: return -PgStep;
                case Commands.SbPageRight:
                case Commands.SbPageDown: return PgStep;
                default: return 0;
            }
        }

        public void SetParams(int aValue, int aMin, int aMax, int aPgStep, int aArStep)
        {
            Value = Math.Max(aMin, Math.Min(aMax, aValue));
            MinVal = aMin;
            MaxVal = aMax;
            PgStep = aPgStep;
            ArStep = aArStep;
        }

        public void SetRange(int aMin, int aMax)
        {
            MinVal = aMin;
            MaxVal = aMax;
            Value = Math.Max(aMin, Math.Min(aMax, Value));
        }

        public void SetStep(int aPgStep, int aArStep)
        {
            PgStep = aPgStep;
            ArStep = aArStep;
        }

        public void SetValue(int aValue)
        {
            Value = Math.Max(MinVal, Math.Min(MaxVal, aValue));
        }

        public int GetPos() => Value;
        public int GetSize() => MaxVal - MinVal + PgStep;
    }

    public class TScroller : TView
    {
        public TPoint Delta;
        protected TPoint Limit;
        protected TScrollBar HScrollBar;
        protected TScrollBar VScrollBar;
        protected byte DrawLock;
        protected bool DrawFlag;

        public TScroller(TRect bounds, TScrollBar aHScrollBar, TScrollBar aVScrollBar)
            : base(bounds)
        {
            HScrollBar = aHScrollBar;
            VScrollBar = aVScrollBar;
        }

        public override void ChangeBounds(TRect bounds)
        {
            SetBounds(bounds);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x06\x07", 2);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public virtual void ScrollDraw() { }

        public void ScrollTo(int x, int y)
        {
            Delta = new TPoint(Math.Min(Limit.X, Math.Max(0, x)),
                               Math.Min(Limit.Y, Math.Max(0, y)));
            ScrollDraw();
        }

        public void SetLimit(int x, int y)
        {
            Limit = new TPoint(x, y);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if (aState == Commands.SfActive)
            {
                if (HScrollBar != null) HScrollBar.Show();
                if (VScrollBar != null) VScrollBar.Show();
            }
        }

        public override void ShutDown()
        {
            base.ShutDown();
        }
    }

    public class TListViewer : TView
    {
        public TScrollBar HScrollBar;
        public TScrollBar VScrollBar;
        public short NumCols;
        public short TopItem;
        public short Focused;
        public short Range;

        public TListViewer(TRect bounds, ushort aNumCols, TScrollBar aHScrollBar, TScrollBar aVScrollBar)
            : base(bounds)
        {
            NumCols = (short)aNumCols;
            HScrollBar = aHScrollBar;
            VScrollBar = aVScrollBar;
            TopItem = 0;
            Focused = 0;
            Range = 0;
            Options |= Commands.OfSelectable | Commands.OfFirstClick;
            EventMask |= EventCodes.EvKeyboard | EventCodes.EvBroadcast;
        }

        public override void ChangeBounds(TRect bounds)
        {
            SetBounds(bounds);
        }

        protected virtual short GetNumRows() => (short)(Size.Y - (HScrollBar != null ? 1 : 0));

        public override void Draw()
        {
            var normal = MapColor(1);
            if (normal == default) normal = new TColorAttr(0x17);
            var focusedAttr = MapColor(2);
            if (focusedAttr == default) focusedAttr = new TColorAttr(0x70);
            var selectedAttr = MapColor(3);
            if (selectedAttr == default) selectedAttr = new TColorAttr(0x1F);

            int rows = GetNumRows();
            int colW = Math.Max(1, (int)Size.X / Math.Max(1, (int)NumCols));

            var b = new TDrawBuffer(Size.X);
            for (int row = 0; row < rows; row++)
            {
                b.MoveChar(0, ' ', normal, Size.X);
                for (int col = 0; col < NumCols; col++)
                {
                    short item = (short)(TopItem + col * rows + row);
                    if (item >= Range) break;
                    string text = GetText(item, (short)(colW - 2));
                    if (text == null) text = string.Empty;
                    if (text.Length > colW - 2) text = text.Substring(0, colW - 2);

                    TColorAttr a = item == Focused && GetState(Commands.SfFocused)
                        ? focusedAttr
                        : (IsSelected(item) ? selectedAttr : normal);
                    int x0 = col * colW;
                    b.MoveChar(x0, ' ', a, 1);
                    b.MoveStr(x0 + 1, text, colW - 2, a);
                    b.MoveChar(x0 + colW - 1, ' ', a, 1);

                    if (col < NumCols - 1)
                        b.WriteChar(x0 + colW - 1, '\xB3', normal);
                }
                WriteLine((short)0, (short)row, (short)Size.X, (short)1, b);
            }
        }

        public virtual void FocusItem(short item)
        {
            Focused = item;
            DrawView();
        }

        public void FocusItemNum(short item)
        {
            if (item < 0) item = 0;
            if (item >= Range) item = (short)Math.Max(0, Range - 1);
            if (Range > 0)
                FocusItem(item);
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x1A\x1A\x1B\x1C\x1D", 5);
        }

        public virtual string GetText(short item, short maxLen)
        {
            return string.Empty;
        }

        public virtual bool IsSelected(short item) => item == Focused;

        protected bool _mouseDownSelect;

        public override void HandleEvent(TEvent ev)
        {
            if (!GetState(Commands.SfSelected) && ev.What == EventCodes.EvMouseDown)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                if (local.X >= 0 && local.X < Size.X && local.Y >= 0 && local.Y < Size.Y)
                    Select();
            }

            if (ev.What == EventCodes.EvKeyDown && GetState(Commands.SfFocused))
            {
                int rows = GetNumRows();
                switch (ev.KeyDown.KeyCode)
                {
                    case KeyCodes.KbUp:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused - 1));
                        return;
                    case KeyCodes.KbDown:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused + 1));
                        return;
                    case KeyCodes.KbLeft:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused - rows));
                        return;
                    case KeyCodes.KbRight:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused + rows));
                        return;
                    case KeyCodes.KbPgUp:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused - rows * NumCols));
                        return;
                    case KeyCodes.KbPgDn:
                        ClearEvent(ev);
                        FocusItemNum((short)(Focused + rows * NumCols));
                        return;
                    case KeyCodes.KbHome:
                        ClearEvent(ev);
                        FocusItemNum(0);
                        return;
                    case KeyCodes.KbEnd:
                        ClearEvent(ev);
                        FocusItemNum((short)Math.Max(0, Range - 1));
                        return;
                    case KeyCodes.KbEnter:
                        ClearEvent(ev);
                        if (Focused >= 0 && Focused < Range)
                        {
                            SelectItem(Focused);
                            var ce = new TEvent();
                            ce.What = EventCodes.EvBroadcast;
                            ce.Message.Command = Commands.CmListItemSelected;
                            ce.Message.InfoPtr = this;
                            PutEvent(ce);
                        }
                        return;
                }
            }

            if (ev.What == EventCodes.EvMouseDown)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                if (local.X >= 0 && local.X < Size.X && local.Y >= 0 && local.Y < Size.Y)
                {
                    ClearEvent(ev);
                    int rows = GetNumRows();
                    int colW = Math.Max(1, (int)Size.X / Math.Max(1, (int)NumCols));
                    int col = Math.Min(local.X / colW, NumCols - 1);
                    short item = (short)(TopItem + col * rows + local.Y);
                    if (item >= 0 && item < Range)
                    {
                        FocusItem(item);
                        SelectItem(item);
                        _mouseDownSelect = true;
                    }
                }
                else
                    _mouseDownSelect = false;
                return;
            }

            if (ev.What == EventCodes.EvMouseWheel)
            {
                ClearEvent(ev);
                int dir = (ev.Mouse.Wheel & EventCodes.MwDown) != 0 ? 1 : -1;
                FocusItemNum((short)(Focused + dir * 3));
                return;
            }

            base.HandleEvent(ev);
        }

        public virtual void SelectItem(short item) { }

        public void SetRange(short aRange)
        {
            Range = aRange;
            if (VScrollBar != null) VScrollBar.SetRange(0, (int)Math.Max(0, (int)Range - GetNumRows()));
            if (Focused >= Range) Focused = (short)Math.Max(0, Range - 1);
            DrawView();
        }

        public void SetTopItem(short item)
        {
            TopItem = item;
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
            if (aState == Commands.SfSelected || aState == Commands.SfFocused)
                DrawView();
        }

        public override void ShutDown()
        {
            base.ShutDown();
        }
    }
}
