using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TVision
{
    public delegate ushort EditorDialog(int dialog, params object[] args);

    public static class EditorDefaults
    {
        public static ushort DefEditorDialog(int dialog, params object[] args) => Commands.CmCancel;
    }

    public static class EditDialogCodes
    {
        public const int EdOutOfMemory = 0;
        public const int EdReadError = 1;
        public const int EdWriteError = 2;
        public const int EdCreateError = 3;
        public const int EdSaveModify = 4;
        public const int EdSaveUntitled = 5;
        public const int EdSaveAs = 6;
        public const int EdFind = 7;
        public const int EdSearchFailed = 8;
        public const int EdReplace = 9;
        public const int EdReplacePrompt = 10;
    }

    public class TIndicator : TView
    {
        public TPoint Location;
        public bool Modified;

        public TIndicator(TRect bounds) : base(bounds)
        {
            GrowMode = Commands.GfGrowLoX;
        }

        public override void Draw()
        {
            var color = MapColor(1);
            if (color == default) color = new TColorAttr(0x17);

            var b = new TDrawBuffer(Size.X);
            b.MoveChar(0, ' ', color, Size.X);
            string text = $" {Location.Y + 1}:{Location.X + 1}{(Modified ? '*' : ' ')}";
            b.MoveStr(1, text, Math.Min(text.Length, Size.X - 1), color);
            WriteLine((short)0, (short)0, (short)Size.X, (short)1, b);
        }

        public override TPalette GetPalette() => new TPalette("\x06\x07", 2);

        public void SetValue(TPoint p, bool m)
        {
            Location = p;
            Modified = m;
            DrawView();
        }
    }

    public class TEditor : TView
    {
        public TScrollBar HScrollBar;
        public TScrollBar VScrollBar;
        public TIndicator Indicator;

        public char[] Buffer;
        public uint BufSize;
        public uint BufLen;
        public uint GapLen;
        public uint SelStart;
        public uint SelEnd;
        public uint CurPtr;
        public TPoint CurPos;
        public TPoint Delta;
        public TPoint Limit;
        public uint DelCount;
        public uint InsCount;
        public bool IsValid;
        public bool CanUndo;
        public bool Modified;
        public bool Selecting;
        public bool Overwrite;
        public bool AutoIndent;

        public static EditorDialog EditorDialogFunc = EditorDefaults.DefEditorDialog;
        public static string FindStr = string.Empty;
        public static string ReplaceStr = string.Empty;
        public static TEditor Clipboard;

        public const int EdOutOfMemory = 0;
        public const int EdReadError = 1;
        public const int EdWriteError = 2;
        public const int EdCreateError = 3;
        public const int EdSaveModify = 4;
        public const int EdSaveUntitled = 5;
        public const int EdSaveAs = 6;
        public const int EdFind = 7;
        public const int EdSearchFailed = 8;
        public const int EdReplace = 9;
        public const int EdReplacePrompt = 10;

        public TEditor(TRect bounds, TScrollBar aHScrollBar, TScrollBar aVScrollBar,
            TIndicator aIndicator, uint aBufSize) : base(bounds)
        {
            HScrollBar = aHScrollBar;
            VScrollBar = aVScrollBar;
            Indicator = aIndicator;
            BufSize = Math.Max(aBufSize, 4096);
            Buffer = new char[BufSize];
            BufLen = 0;
            GapLen = BufSize;
            SelStart = 0;
            SelEnd = 0;
            CurPtr = 0;
            CurPos = new TPoint(0, 0);
            Delta = new TPoint(0, 0);
            Limit = new TPoint(0, 1);
            Options |= Commands.OfSelectable | Commands.OfFirstClick;
            EventMask |= EventCodes.EvKeyboard | EventCodes.EvCommand | EventCodes.EvBroadcast;
            GrowMode = Commands.GfGrowLoX | Commands.GfGrowLoY;
        }

        public char BufChar(uint p)
        {
            uint bp = BufPtr(p);
            return (bp < Buffer.Length) ? Buffer[bp] : '\0';
        }

        public uint BufPtr(uint p) => p < CurPtr ? p : p + GapLen;

        private void MoveGapTo(uint ptr)
        {
            if (ptr == CurPtr) return;
            if (ptr < CurPtr)
            {
                uint count = CurPtr - ptr;
                Array.Copy(Buffer, ptr, Buffer, ptr + GapLen, count);
            }
            else
            {
                uint count = ptr - CurPtr;
                Array.Copy(Buffer, CurPtr + GapLen, Buffer, CurPtr, count);
            }
            CurPtr = ptr;
        }

        protected void EnsureCapacity(int extra)
        {
            if ((long)GapLen >= extra) return;
            uint needed = (uint)Math.Max((long)BufLen + extra + 256, (long)BufSize * 2);
            var nb = new char[needed];
            Array.Copy(Buffer, 0, nb, 0, CurPtr);
            uint tailSrc = CurPtr + GapLen;
            uint tailCount = BufLen - CurPtr;
            Array.Copy(Buffer, tailSrc, nb, needed - tailCount, tailCount);
            Buffer = nb;
            GapLen = needed - BufLen;
            BufSize = needed;
        }

        protected void InsertChars(char[] data, int offset, int count)
        {
            EnsureCapacity(count);
            MoveGapTo(CurPtr);
            Array.Copy(data, offset, Buffer, CurPtr, count);
            CurPtr += (uint)count;
            BufLen += (uint)count;
            GapLen -= (uint)count;
            Modified = true;
            CanUndo = true;
        }

        protected void DeleteChars(uint start, uint count)
        {
            if (start > BufLen) return;
            if (start + count > BufLen) count = BufLen - start;
            MoveGapTo(start);
            GapLen += count;
            BufLen -= count;
            Modified = true;
            CanUndo = true;
        }

        public uint LineStart(uint line)
        {
            uint pos = 0;
            for (uint i = 0; i < line && pos < BufLen; )
            {
                if (BufChar(pos) == '\n') i++;
                pos++;
            }
            return pos;
        }

        public uint LineCount()
        {
            uint count = 1;
            for (uint p = 0; p < BufLen; p++)
                if (BufChar(p) == '\n') count++;
            return count;
        }

        public uint LineLen(uint lineStart)
        {
            uint p = lineStart;
            while (p < BufLen && BufChar(p) != '\n') p++;
            return p - lineStart;
        }

        public void PosToXY(uint pos, out int x, out int y)
        {
            x = 0; y = 0;
            for (uint p = 0; p < pos && p < BufLen; p++)
            {
                if (BufChar(p) == '\n') { y++; x = 0; }
                else x++;
            }
        }

        public uint XYToPos(int x, int y)
        {
            uint pos = LineStart((uint)Math.Max(0, y));
            uint len = LineLen(pos);
            return pos + (uint)Math.Max(0, Math.Min(x, (int)len));
        }

        protected void SetCurPtr(uint ptr, bool selectExtend)
        {
            ptr = Math.Min(ptr, BufLen);
            if (!selectExtend)
            {
                SelStart = ptr;
                SelEnd = ptr;
            }
            else
            {
                if (ptr < SelStart) { SelStart = ptr; SelEnd = ptr < SelEnd ? SelEnd : ptr; }
                else { SelEnd = ptr; SelStart = SelEnd < SelStart ? SelStart : SelStart; }
            }
            MoveGapTo(ptr);
            PosToXY(ptr, out int cx, out int cy);
            CurPos = new TPoint(cx, cy);
            TrackCursor();
            Update(1);
        }

        public uint SelTextLength()
        {
            return SelEnd > SelStart ? SelEnd - SelStart : 0;
        }

        public string GetSelText()
        {
            if (SelEnd <= SelStart) return string.Empty;
            var sb = new StringBuilder((int)(SelEnd - SelStart));
            for (uint p = SelStart; p < SelEnd; p++)
                sb.Append(BufChar(p));
            return sb.ToString();
        }

        protected void DeleteSelection()
        {
            if (SelEnd > SelStart)
            {
                DeleteChars(SelStart, SelEnd - SelStart);
                SetCurPtr(SelStart, false);
            }
        }

        public bool InsertText(string text)
        {
            if (text.Length == 0) return true;
            DeleteSelection();
            EnsureCapacity(text.Length);
            var arr = text.ToCharArray();
            InsertChars(arr, 0, arr.Length);
            SetCurPtr(CurPtr, false);
            return true;
        }

        public virtual bool InsertFrom(TEditor editor)
        {
            if (editor == null || editor.SelEnd <= editor.SelStart) return false;
            var s = editor.GetSelText();
            return InsertText(s);
        }

        public void CopyToClipboard()
        {
            string s = SelEnd > SelStart ? GetSelText() : GetCurrentLineText();
            TClipboard.SetText(s);
        }

        private string GetCurrentLineText()
        {
            uint ls = LineStartAtPtr(CurPtr);
            uint len = LineLen(ls);
            var sb = new StringBuilder((int)len);
            for (uint p = ls; p < ls + len; p++) sb.Append(BufChar(p));
            return sb.ToString();
        }

        private uint LineStartAtPtr(uint ptr)
        {
            uint ls = ptr;
            while (ls > 0 && BufChar(ls - 1) != '\n') ls--;
            return ls;
        }

        public void CutToClipboard()
        {
            CopyToClipboard();
            if (SelEnd > SelStart) DeleteSelection();
        }

        public void PasteFromClipboard()
        {
            string s = TClipboard.GetText();
            if (!string.IsNullOrEmpty(s))
                InsertText(s.Replace("\r\n", "\n").Replace("\r", "\n"));
        }

        public void ClearCurrentOrSelection()
        {
            if (SelEnd > SelStart)
                DeleteSelection();
            else
            {
                uint ls = LineStartAtPtr(CurPtr);
                uint len = LineLen(ls);
                DeleteChars(ls, Math.Min(len + 1, BufLen - ls));
                SetCurPtr(ls, false);
            }
        }

        public void ScrollTo(int x, int y)
        {
            Delta = new TPoint(Math.Max(0, x), Math.Max(0, y));
            if (HScrollBar != null && HScrollBar.Value != Delta.X)
            {
                HScrollBar.SetParams(Delta.X, 0, Math.Max(0, Limit.X - 1), Size.X - 1, 1);
                HScrollBar.DrawView();
            }
            if (VScrollBar != null && VScrollBar.Value != Delta.Y)
            {
                VScrollBar.SetParams(Delta.Y, 0, Math.Max(0, Limit.Y - 1), Size.Y - 1, 1);
                VScrollBar.DrawView();
            }
            DrawView();
        }

        public void TrackCursor()
        {
            int nx = Delta.X, ny = Delta.Y;
            if (CurPos.X < Delta.X) nx = CurPos.X;
            if (CurPos.X > Delta.X + Size.X - 1) nx = CurPos.X - Size.X + 1;
            if (CurPos.Y < Delta.Y) ny = CurPos.Y;
            if (CurPos.Y > Delta.Y + Size.Y - 1) ny = CurPos.Y - Size.Y + 1;
            if (nx != Delta.X || ny != Delta.Y)
                ScrollTo(nx, ny);
            else
            {
                UpdateLimit();
                DrawView();
            }
            UpdateIndicator();
        }

        private void UpdateLimit()
        {
            uint maxLen = 1;
            for (uint line = 0; ; line++)
            {
                uint ls = LineStart(line);
                if (ls >= BufLen && line > 0) break;
                maxLen = Math.Max(maxLen, LineLen(ls));
                if (ls >= BufLen) break;
                if (line > 100000) break;
            }
            Limit = new TPoint(Math.Max((int)maxLen, Size.X), (int)LineCount());
        }

        protected void UpdateIndicator()
        {
            Indicator?.SetValue(CurPos, Modified);
        }

        public void Update(byte flags)
        {
            UpdateLimit();
            UpdateCommands();
            TrackCursor();
        }

        public void Lock() { }
        public void Unlock() { }

        public void UpdateCommands()
        {
            bool isFocused = GetState(Commands.SfFocused);
            if (!isFocused) return;
            SetCmdState(new TCommandSet(Commands.CmSave), Modified);
            SetCmdState(new TCommandSet(Commands.CmCut, Commands.CmCopy, Commands.CmClear),
                SelEnd > SelStart || BufLen > 0);
            SetCmdState(new TCommandSet(Commands.CmPaste),
                !string.IsNullOrEmpty(TClipboard.GetText()));
        }

        public override void Draw()
        {
            var normal = GetColor(0x0101).Low;
            var selected = GetColor(0x0202).Low;

            var b = new TDrawBuffer(Size.X);
            for (int row = 0; row < Size.Y; row++)
            {
                uint lineIdx = (uint)(Delta.Y + row);
                uint lineStart = LineStart(lineIdx);
                uint lineLen = (lineStart < BufLen || lineIdx == 0) ? LineLen(lineStart) : 0;
                if (lineStart > BufLen) lineLen = 0;

                b.MoveChar(0, ' ', normal, Size.X);
                for (int col = 0; col < Size.X; col++)
                {
                    uint srcCol = (uint)(Delta.X + col);
                    if (srcCol < lineLen)
                    {
                        char ch = BufChar(lineStart + srcCol);
                        if (ch == '\t') ch = ' ';
                        uint abs = lineStart + srcCol;
                        bool inSel = abs >= SelStart && abs < SelEnd;
                        b.WriteChar(col, ch, inSel ? selected : normal);
                    }
                }
                WriteLine((short)0, (short)row, (short)Size.X, (short)1, b);
            }

            if (GetState(Commands.SfFocused))
            {
                int cx = CurPos.X - Delta.X;
                int cy = CurPos.Y - Delta.Y;
                if (cx >= 0 && cx < Size.X && cy >= 0 && cy < Size.Y)
                    SetCursor((short)cx, (short)cy);
            }
        }

        public override TPalette GetPalette()
        {
            return new TPalette("\x06\x07", 2);
        }

        private bool ExtendMode;

        private void MoveCursor(int dx, int dy, bool extend)
        {
            int nx = Math.Max(0, CurPos.X + dx);
            int ny = Math.Max(0, CurPos.Y + dy);
            uint pos = XYToPos(nx, ny);
            SetCurPtr(pos, extend);
        }

        private void WordLeft(bool extend)
        {
            uint p = CurPtr;
            while (p > 0 && !char.IsLetterOrDigit(BufChar(p - 1))) p--;
            while (p > 0 && char.IsLetterOrDigit(BufChar(p - 1))) p--;
            SetCurPtr(p, extend);
        }

        private void WordRight(bool extend)
        {
            uint p = CurPtr;
            while (p < BufLen && !char.IsLetterOrDigit(BufChar(p))) p++;
            while (p < BufLen && char.IsLetterOrDigit(BufChar(p))) p++;
            SetCurPtr(p, extend);
        }

        private void DoEnter()
        {
            DeleteSelection();
            string indent = string.Empty;
            if (AutoIndent)
            {
                uint ls = LineStartAtPtr(CurPtr);
                int spaces = 0;
                for (uint p = ls; p < CurPtr && BufChar(p) == ' '; p++) spaces++;
                indent = new string(' ', spaces);
            }
            InsertText("\n" + indent);
        }

        private void DoChar(char ch)
        {
            DeleteSelection();
            if (ch == '\t')
            {
                int spaces = 4 - (CurPos.X % 4);
                InsertText(new string(' ', spaces));
            }
            else
                InsertText(ch.ToString());
        }

        public virtual void HandleKeyPress(ref TEvent ev)
        {
            ushort code = ev.KeyDown.KeyCode;
            bool shift = (ev.KeyDown.ControlKeyState & KeyCodes.KbShift) != 0;
            bool ctrl = (ev.KeyDown.ControlKeyState & KeyCodes.KbCtrlShift) != 0;
            ClearEvent(ev);

            switch (code)
            {
                case KeyCodes.KbLeft: MoveCursor(-1, 0, shift); break;
                case KeyCodes.KbRight: MoveCursor(1, 0, shift); break;
                case KeyCodes.KbUp: MoveCursor(0, -1, shift); break;
                case KeyCodes.KbDown: MoveCursor(0, 1, shift); break;
                case KeyCodes.KbHome: SetCurPtr(LineStartAtPtr(CurPtr), shift); break;
                case KeyCodes.KbEnd: SetCurPtr(LineStartAtPtr(CurPtr) + LineLen(LineStartAtPtr(CurPtr)), shift); break;
                case KeyCodes.KbPgUp: MoveCursor(0, -(Size.Y - 1), shift); break;
                case KeyCodes.KbPgDn: MoveCursor(0, Size.Y - 1, shift); break;
                case KeyCodes.KbCtrlLeft: WordLeft(shift); break;
                case KeyCodes.KbCtrlRight: WordRight(shift); break;
                case KeyCodes.KbCtrlHome: SetCurPtr(0, shift); break;
                case KeyCodes.KbCtrlEnd: SetCurPtr(BufLen, shift); break;
                case KeyCodes.KbBack:
                    if (SelEnd > SelStart) DeleteSelection();
                    else if (CurPtr > 0) { DeleteChars(CurPtr - 1, 1); SetCurPtr(CurPtr - 1, false); }
                    break;
                case KeyCodes.KbDel:
                    if (SelEnd > SelStart) DeleteSelection();
                    else if (CurPtr < BufLen) DeleteChars(CurPtr, 1);
                    break;
                case KeyCodes.KbEnter: DoEnter(); break;
                default:
                    if (code >= 32 && code < 0x100 && ev.KeyDown.TextLength > 0)
                        DoChar(ev.KeyDown.Char0);
                    break;
            }
        }

        public override bool PreProcessKeyEvent(ref TEvent ev)
        {
            return false;
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvMouseDown)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                uint pos = XYToPos(Delta.X + local.X, Delta.Y + local.Y);
                SetCurPtr(pos, (ev.Mouse.Buttons & EventCodes.MbLeftButton) != 0 && Selecting);
                ClearEvent(ev);
                return;
            }
            if (ev.What == EventCodes.EvMouseMove && (ev.Mouse.Buttons & EventCodes.MbLeftButton) != 0)
            {
                TPoint local = MakeLocal(ev.Mouse.Where);
                uint pos = XYToPos(Delta.X + local.X, Delta.Y + local.Y);
                SetCurPtr(pos, true);
                ClearEvent(ev);
                return;
            }
            if (ev.What == EventCodes.EvKeyDown && GetState(Commands.SfFocused))
            {
                HandleKeyPress(ref ev);
                return;
            }
            if (ev.What == EventCodes.EvCommand && GetState(Commands.SfFocused))
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmCut: CutToClipboard(); ClearEvent(ev); break;
                    case Commands.CmCopy: CopyToClipboard(); ClearEvent(ev); break;
                    case Commands.CmPaste: PasteFromClipboard(); ClearEvent(ev); break;
                    case Commands.CmClear: ClearCurrentOrSelection(); ClearEvent(ev); break;
                    case Commands.CmSave: if (Save()) ClearEvent(ev); break;
                    case Commands.CmSaveAs: if (SaveAs()) ClearEvent(ev); break;
                    case (ushort)Commands.CmFind:
                        {
                            var box = new[] { FindStr };
                            ushort r = EditorDialogFunc?.Invoke(EdFind, box) ?? Commands.CmCancel;
                            if (r != Commands.CmCancel && box[0].Length > 0)
                            {
                                FindStr = box[0];
                                if (!Search(FindStr, 0))
                                    EditorDialogFunc?.Invoke(EdSearchFailed);
                                ClearEvent(ev);
                            }
                            break;
                        }
                }
                return;
            }
            base.HandleEvent(ev);
        }

        protected virtual bool Save() => true;

        public virtual bool SaveAs() => true;

        public void SetSelText(string text)
        {
            DeleteSelection();
            InsertText(text);
        }

        public override void SetState(ushort aState, bool enable)
        {
            bool wasFocused = GetState(Commands.SfFocused);
            base.SetState(aState, enable);
            if (aState == Commands.SfFocused && wasFocused != enable)
            {
                if (enable) ShowCursor();
                else HideCursor();
                DrawView();
                UpdateCommands();
            }
        }

        public bool Search(string searchStr, ushort opts)
        {
            if (string.IsNullOrEmpty(searchStr)) return false;
            uint start = CurPtr < SelEnd ? SelEnd : CurPtr;
            for (uint p = start; p + (uint)searchStr.Length <= BufLen; p++)
            {
                bool match = true;
                for (int i = 0; i < searchStr.Length; i++)
                {
                    char a = BufChar(p + (uint)i);
                    char c = searchStr[i];
                    if (a != c)
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    SetCurPtr(p + (uint)searchStr.Length, false);
                    SelStart = p;
                    SelEnd = p + (uint)searchStr.Length;
                    DrawView();
                    return true;
                }
            }
            return false;
        }
    }

    public class TMemo : TEditor
    {
        public TMemo(TRect bounds, TScrollBar aHScrollBar, TScrollBar aVScrollBar,
            TIndicator aIndicator, ushort aBufSize)
            : base(bounds, aHScrollBar, aVScrollBar, aIndicator, aBufSize) { }

        public string Text
        {
            get
            {
                var sb = new StringBuilder((int)BufLen);
                for (uint p = 0; p < BufLen; p++) sb.Append(BufChar(p));
                return sb.ToString();
            }
            set
            {
                BufLen = 0; GapLen = BufSize; CurPtr = 0;
                Modified = true;
                if (!string.IsNullOrEmpty(value))
                {
                    var arr = value.ToCharArray();
                    InsertChars(arr, 0, arr.Length);
                }
                SetCurPtr(0, false);
                Update(1);
            }
        }
    }

    public class TFileEditor : TEditor
    {
        public string FileName;

        public TFileEditor(TRect bounds, TScrollBar aHScrollBar, TScrollBar aVScrollBar,
            TIndicator aIndicator, string aFileName)
            : base(bounds, aHScrollBar, aVScrollBar, aIndicator, 0xF000)
        {
            FileName = aFileName ?? string.Empty;
            IsValid = true;
            if (FileName.Length > 0)
                LoadFile();
        }

        public bool LoadFile()
        {
            try
            {
                BufLen = 0;
                GapLen = BufSize;
                CurPtr = 0;
                SelStart = SelEnd = 0;
                if (File.Exists(FileName))
                {
                    string text = File.ReadAllText(FileName)
                        .Replace("\r\n", "\n").Replace('\r', '\n');
                    var arr = text.ToCharArray();
                    EnsureCapacity(arr.Length);
                    InsertChars(arr, 0, arr.Length);
                }
                Modified = false;
                SetCurPtr(0, false);
                Update(1);
                IsValid = true;
                return true;
            }
            catch
            {
                EditorDialogFunc?.Invoke(EdReadError, FileName);
                IsValid = false;
                return false;
            }
        }

        public bool SaveFile()
        {
            try
            {
                var sb = new StringBuilder((int)BufLen + 16);
                for (uint p = 0; p < BufLen; p++)
                {
                    char ch = BufChar(p);
                    if (ch == '\n') sb.Append(Environment.NewLine);
                    else sb.Append(ch);
                }
                File.WriteAllText(FileName, sb.ToString());
                Modified = false;
                Update(1);
                return true;
            }
            catch
            {
                EditorDialogFunc?.Invoke(EdWriteError, FileName);
                return false;
            }
        }

        public override bool SaveAs()
        {
            var box = new[] { FileName.Length > 0 ? FileName : "*.*" };
            ushort r = EditorDialogFunc?.Invoke(EdSaveAs, box) ?? Commands.CmCancel;
            if (r != Commands.CmCancel && box[0].Length > 0)
            {
                FileName = box[0];
                return SaveFile();
            }
            return false;
        }

        public bool Save()
        {
            if (FileName.Length == 0)
                return SaveAs();
            return SaveFile();
        }

        public override bool Valid(ushort command)
        {
            if (!Modified) return true;
            if (command == Commands.CmCancel) return true;

            bool untitled = FileName.Length == 0;
            ushort r;
            if (untitled)
                r = EditorDialogFunc?.Invoke(EdSaveUntitled) ?? Commands.CmCancel;
            else
                r = EditorDialogFunc?.Invoke(EdSaveModify, FileName) ?? Commands.CmCancel;

            switch (r)
            {
                case Commands.CmYes:
                    if (untitled) return SaveAs();
                    return SaveFile();
                case Commands.CmNo:
                    Modified = false;
                    return true;
                default:
                    return false;
            }
        }

        public override void ShutDown()
        {
            Buffer = null;
            base.ShutDown();
        }
    }

    public class TEditWindow : TWindow
    {
        public TFileEditor Editor;

        public TEditWindow(TRect bounds, string aFileName, short aNumber)
            : base(bounds, null, aNumber)
        {
            Palette = (short)(aNumber != Commands.WnNoNumber ? aNumber : 0);
            Title = aFileName != null && aFileName.Length > 0 ? aFileName : "Untitled";

            var extent = GetExtent();
            int w = extent.Width, h = extent.Height;
            if (w < 12 || h < 5) return;

            var hScroll = new TScrollBar(new TRect(2, (short)(h - 2), (short)(w - 3), (short)(h - 1)));
            hScroll.GrowMode = Commands.GfGrowLoX | Commands.GfGrowHiY;
            var vScroll = new TScrollBar(new TRect((short)(w - 2), 1, (short)(w - 1), (short)(h - 2)));
            vScroll.GrowMode = Commands.GfGrowHiX | Commands.GfGrowLoY;
            var indicator = new TIndicator(new TRect(2, 1, (short)Math.Max(10, w - 20), 2));

            var edBounds = new TRect(1, 2, (short)(w - 2), (short)(h - 2));
            Editor = new TFileEditor(edBounds, hScroll, vScroll, indicator, aFileName);
            Editor.GrowMode = Commands.GfGrowLoX | Commands.GfGrowLoY;

            Insert(hScroll);
            Insert(vScroll);
            Insert(indicator);
            Insert(Editor);
            SetCurrent(Editor, SelectMode.NormalSelect);
        }

        public override void Close()
        {
            if (Editor == null || Editor.Valid(Commands.CmClose))
                base.Close();
        }

        public override string GetTitle(short maxSize)
        {
            return Title;
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public override void SizeLimits(out TPoint min, out TPoint max)
        {
            min = new TPoint(12, 5);
            max = new TPoint(1000, 1000);
        }

        public override TPalette GetPalette()
        {
            return new TPalette(CpCyanWindow, 8);
        }
    }

    public class TFindDialogRec
    {
        public string Find;
        public ushort Options;

        public TFindDialogRec(string str, ushort flags)
        {
            Find = str ?? string.Empty;
            Options = flags;
        }
    }

    public class TReplaceDialogRec
    {
        public string Find;
        public string Replace;
        public ushort Options;

        public TReplaceDialogRec(string str, string rep, ushort flags)
        {
            Find = str ?? string.Empty;
            Replace = rep ?? string.Empty;
            Options = flags;
        }
    }
}
