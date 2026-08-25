using System;
using System.Collections.Generic;
using System.IO;

namespace TVision
{
    public struct TSearchRec
    {
        public byte Attr;
        public int Time;
        public int Size;
        public string Name;
    }

    public class TFileInputLine : TInputLine
    {
        public TFileInputLine(TRect bounds, short aMaxLen) : base(bounds, aMaxLen) { }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }
    }

    public class TFileCollection : TSortedCollection
    {
        public TFileCollection(int aLimit, int aDelta) : base(aLimit, aDelta) { }

        public TSearchRec AtFile(int index) => (TSearchRec)At(index);
        public override int Insert(object item) => base.Insert(item);

        protected override int Compare(object key1, object key2)
        {
            return string.Compare(key1?.ToString(), key2?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    public class TSortedListBox : TListBox
    {
        public TSortedListBox(TRect bounds, ushort aNumCols, TScrollBar aScrollBar)
            : base(bounds, aNumCols, aScrollBar) { }

        public void NewList(TSortedCollection aList) => base.NewList(aList);

        public TSortedCollection SortedList() => _items as TSortedCollection;
    }

    public class TFileList : TSortedListBox
    {
        public TFileList(TRect bounds, TScrollBar aScrollBar)
            : base(bounds, 2, aScrollBar) { }

        public void NewList(TFileCollection aList) => base.NewList((TCollection)aList);

        public override void FocusItem(short item)
        {
            base.FocusItem(item);
            if (List() != null && item >= 0 && item < List().Count)
                Owner?.HandleEvent(Broadcast(Commands.CmFileFocused, List().At(item)));
        }

        public override void SelectItem(short item)
        {
            if (List() != null && item >= 0 && item < List().Count)
                Owner?.HandleEvent(Broadcast(Commands.CmFileDoubleClicked, List().At(item)));
        }

        private static TEvent Broadcast(ushort cmd, object infoPtr)
        {
            var e = new TEvent();
            e.What = EventCodes.EvBroadcast;
            e.Message.Command = cmd;
            e.Message.InfoPtr = infoPtr;
            return e;
        }

        public void ReadDirectory(string dir, string wildCard)
        {
            try
            {
                string root = string.IsNullOrEmpty(dir) ? "." : dir;
                var collection = new TFileCollection(64, 16);

                foreach (var d in System.IO.Directory.GetDirectories(root))
                    collection.Insert(new TSearchRec { Name = Path.GetFileName(d), Attr = 0x10 });

                foreach (var file in System.IO.Directory.GetFiles(root, wildCard))
                {
                    try
                    {
                        collection.Insert(new TSearchRec
                        {
                            Name = Path.GetFileName(file),
                            Size = (int)new FileInfo(file).Length,
                            Time = (int)(File.GetLastWriteTime(file).Ticks / TimeSpan.TicksPerSecond),
                            Attr = 0x20
                        });
                    }
                    catch { }
                }
                NewList(collection);
            }
            catch
            {
                NewList(new TFileCollection(4, 4));
            }
        }

        public void ReadDirectory(string wildCard)
        {
            ReadDirectory(Directory.GetCurrentDirectory(), wildCard);
        }

        public override string GetText(short item, short maxLen)
        {
            if (List() == null || item < 0 || item >= List().Count)
                return string.Empty;
            var rec = (TSearchRec)List().At(item);
            string s = rec.Name;
            if ((rec.Attr & 0x10) != 0)
                s += "\\";
            if (s.Length > maxLen) s = s.Substring(0, maxLen);
            return s;
        }

        public new TSearchRec AtFile(int index) => (TSearchRec)List().At(index);

        public override ushort DataSize() => 0;
    }

    public class TFileInfoPane : TView
    {
        public TSearchRec FileBlock;

        public TFileInfoPane(TRect bounds) : base(bounds) { }

        public override void Draw()
        {
            var color = GetColor(0x0101).Low;
            var b = new TDrawBuffer(Size.X);

            string path = "";
            if (Owner is TFileDialog dlg)
                path = Path.Combine(dlg.Directory ?? "", dlg.WildCard ?? "");

            b.MoveChar(0, '\u2502', color, 1);
            b.MoveStr(1, path, Math.Min(path.Length, Size.X - 2), color);
            b.MoveChar(Size.X - 1, '\u2502', color, 1);
            WriteLine(0, 0, (short)Size.X, 1, b);

            b.MoveChar(0, ' ', color, (short)Size.X);
            b.MoveStr(1, FileBlock.Name ?? string.Empty,
                Math.Min((FileBlock.Name ?? "").Length, Size.X - 1), color);

            if (!string.IsNullOrEmpty(FileBlock.Name))
            {
                b.MoveStr(Size.X - 38, FileBlock.Size.ToString(),
                    Math.Min(FileBlock.Size.ToString().Length, 10), color);

                try
                {
                    var dto = new DateTime(1980, 1, 1).AddSeconds(FileBlock.Time);
                    string mon = TFileDialog.Months[dto.Month];
                    b.MoveStr(Size.X - 22, mon, mon.Length, color);
                    b.MoveStr(Size.X - 18, dto.Day.ToString("00"), 2, color);
                    b.WriteChar(Size.X - 16, ',', color);
                    b.MoveStr(Size.X - 14, dto.Year.ToString(), 4, color);
                    b.MoveStr(Size.X - 9, dto.Hour.ToString("00"), 2, color);
                    b.WriteChar(Size.X - 7, ':', color);
                    b.MoveStr(Size.X - 6, dto.Minute.ToString("00"), 2, color);

                    if ((FileBlock.Attr & 0x10) != 0)
                        b.MoveStr(Size.X - 3, "DIR", 3, color);
                    else if ((FileBlock.Attr & 0x01) != 0)
                        b.MoveStr(Size.X - 3, "r/o", 3, color);
                    else
                        b.MoveStr(Size.X - 3, "   ", 3, color);
                }
                catch { }
            }
            WriteLine(0, 1, (short)Size.X, 1, b);

            for (short i = 2; i < Size.Y; i++)
            {
                b.MoveChar(0, '\u2502', color, 1);
                b.MoveChar(1, ' ', color, Size.X - 2);
                b.MoveChar(Size.X - 1, '\u2502', color, 1);
                WriteLine(0, i, (short)Size.X, 1, b);
            }
        }

        public override TPalette GetPalette() => new TPalette("\x1E", 1);

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvBroadcast &&
                ev.Message.Command == Commands.CmFileFocused &&
                ev.Message.InfoPtr is TSearchRec rec)
            {
                FileBlock = rec;
                DrawView();
                return;
            }
            base.HandleEvent(ev);
        }
    }

    public class TFileDialog : TDialog
    {
        public const ushort FdOpenButton = 0x0001;
        public const ushort FdOkButton = 0x0002;
        public const ushort FdReplaceButton = 0x0004;
        public const ushort FdClearButton = 0x0008;
        public const ushort FdHelpButton = 0x0010;
        public const ushort FdNoLoadDir = 0x0100;

        public static readonly string[] Months =
        {
            "", "Jan", "Feb", "Mar", "Apr", "May", "Jun",
            "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
        };

        public TFileInputLine FileName;
        public TFileList FileList;
        public string WildCard;
        public string Directory;

        private static readonly string OpenText = "~O~pen";
        private static readonly string OkText = "O~K~";

        public TFileDialog(string aWildCard, string aTitle, string inputName,
            ushort aOptions, byte histId)
            : base(new TRect(15, 1, 64, 20), aTitle)
        {
            Options |= Commands.OfCentered;
            Flags |= Commands.WfGrow;
            WildCard = string.IsNullOrEmpty(aWildCard) ? "*.*" : aWildCard;

            FileName = new TFileInputLine(new TRect(3, 3, 31, 4), 80);
            FileName.Data = WildCard;
            Insert(FileName);

            Insert(new TLabel(new TRect(2, 2, (short)(3 + (inputName ?? "~N~ame").Length), 3),
                inputName ?? "~N~ame", FileName));

            var sb = new TScrollBar(new TRect(3, 14, 34, 15));
            Insert(sb);
            FileList = new TFileList(new TRect(3, 6, 34, 14), sb);
            Insert(FileList);
            Insert(new TLabel(new TRect(2, 5, 8, 6), "~F~iles", FileList));

            ushort opt = TButton.BfDefault;
            var r = new TRect(35, 3, 46, 5);

            if ((aOptions & FdOpenButton) != 0)
            {
                Insert(new TButton(r, OpenText, Commands.CmFileOpen, opt));
                opt = TButton.BfNormal;
                r.A.Y += 3; r.B.Y += 3;
            }
            if ((aOptions & FdOkButton) != 0)
            {
                Insert(new TButton(r, OkText, Commands.CmFileOpen, opt));
                opt = TButton.BfNormal;
                r.A.Y += 3; r.B.Y += 3;
            }
            if ((aOptions & FdReplaceButton) != 0)
            {
                Insert(new TButton(r, "~R~eplace", Commands.CmFileReplace, opt));
                opt = TButton.BfNormal;
                r.A.Y += 3; r.B.Y += 3;
            }
            if ((aOptions & FdClearButton) != 0)
            {
                Insert(new TButton(r, "C~l~ear", Commands.CmFileClear, opt));
                opt = TButton.BfNormal;
                r.A.Y += 3; r.B.Y += 3;
            }

            Insert(new TButton(r, "Cancel", Commands.CmCancel, TButton.BfNormal));
            r.A.Y += 3; r.B.Y += 3;

            Insert(new TFileInfoPane(new TRect(1, 16, 48, 18)));

            SelectNext(false);

            if ((aOptions & FdNoLoadDir) == 0)
                ReadDirectory();
        }

        public void GetFileName(string[] s)
        {
            string buf = (FileName?.Data ?? string.Empty).Trim();
            if (buf.Length == 0) buf = WildCard;
            try
            {
                bool isWild = buf.Contains("*") || buf.Contains("?");
                if (!isWild && Directory != null && System.IO.Directory.Exists(
                    Path.IsPathRooted(buf) ? Path.GetDirectoryName(buf) : Directory))
                    s[0] = Path.GetFullPath(Path.IsPathRooted(buf) ? buf : Path.Combine(Directory, buf));
                else
                    s[0] = Path.GetFullPath(buf);
            }
            catch { s[0] = buf; }
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
            if (ev.What == EventCodes.EvCommand)
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmFileOpen:
                    case Commands.CmFileReplace:
                    case Commands.CmFileClear:
                        EndModal(ev.Message.Command);
                        ClearEvent(ev);
                        break;
                }
            }
            else if (ev.What == EventCodes.EvBroadcast &&
                     ev.Message.Command == Commands.CmFileDoubleClicked)
            {
                ev.What = EventCodes.EvCommand;
                ev.Message.Command = Commands.CmOK;
                PutEvent(ev);
                ClearEvent(ev);
            }
        }

        public void ReadDirectory()
        {
            Directory = System.IO.Directory.GetCurrentDirectory();
            FileList.ReadDirectory(Directory, WildCard);
        }

        public override void GetData(object rec)
        {
            if (rec is string[] arr && arr.Length > 0)
            {
                var box = new[] { string.Empty };
                GetFileName(box);
                arr[0] = box[0];
            }
        }

        public override void SetData(object rec)
        {
            if (rec is string s && s.Length > 0 && (s.Contains("*") || s.Contains("?")))
            {
                FileName.Data = s;
                Valid(Commands.CmFileInit);
                FileName.Select();
            }
        }

        public override bool Valid(ushort command)
        {
            if (command == 0)
                return true;

            if (command != Commands.CmCancel && command != Commands.CmFileClear)
            {
                var box = new[] { string.Empty };
                GetFileName(box);
                string fName = box[0];
                bool isWild = fName.Contains("*") || fName.Contains("?");
                bool isDir = !isWild && System.IO.Directory.Exists(fName);

                if (isWild || isDir)
                {
                    try
                    {
                        string dir = isDir ? fName : Path.GetDirectoryName(fName);
                        string wc = isDir ? WildCard : Path.GetFileName(fName);
                        if (string.IsNullOrEmpty(wc)) wc = "*.*";
                        if (string.IsNullOrEmpty(dir)) dir = System.IO.Directory.GetCurrentDirectory();
                        Directory = dir;
                        WildCard = wc;
                        FileList.ReadDirectory(dir, wc);
                        DrawView();
                    }
                    catch { }
                    return false;
                }

                if (command == Commands.CmFileInit)
                    return false;
            }
            return true;
        }

        public override void ShutDown()
        {
            FileList?.ShutDown();
            base.ShutDown();
        }
    }

    public class TDirEntry
    {
        public string DisplayText;
        public string Dir;

        public TDirEntry(string text, string dir)
        {
            DisplayText = text ?? string.Empty;
            Dir = dir ?? string.Empty;
        }
    }

    public class TDirCollection : TCollection
    {
        public TDirCollection(int aLimit, int aDelta) : base(aLimit, aDelta) { }
        public TDirEntry AtDir(int index) => (TDirEntry)At(index);
    }

    public class TDirListBox : TListBox
    {
        public TDirCollection List;

        public TDirListBox(TRect bounds, TScrollBar aScrollBar)
            : base(bounds, 1, aScrollBar) { }

        public void NewList(TDirCollection aList) { List = aList; }
    }

    public class TChDirDialog : TDialog
    {
        public TDirListBox DirList;
        public TInputLine DirInput;

        public TChDirDialog()
            : base(new TRect(15, 5, 64, 19), "Change Directory")
        {
        }

        public override void HandleEvent(TEvent ev) { base.HandleEvent(ev); }
    }

    public static class MsgBoxFlags
    {
        public const ushort MfWarning = 0x1;
        public const ushort MfError = 0x2;
        public const ushort MfInformation = 0x4;
        public const ushort MfConfirmation = 0x8;

        public const ushort MfYesButton = 0x10;
        public const ushort MfNoButton = 0x20;
        public const ushort MfOkButton = 0x40;
        public const ushort MfCancelButton = 0x80;

        public const ushort MfYesNoCancel = MfYesButton | MfNoButton | MfCancelButton;
        public const ushort MfOkCancel = MfOkButton | MfCancelButton;
        public const ushort MfYesNo = MfYesButton | MfNoButton;
    }

    public static class TVMessageBox
    {
        private static string Wrap(string msg, int width)
        {
            var lines = new List<string>();
            foreach (var rawLine in (msg ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                var words = rawLine.Split(' ');
                var cur = string.Empty;
                foreach (var word in words)
                {
                    if (cur.Length == 0)
                        cur = word;
                    else if (cur.Length + 1 + word.Length <= width)
                        cur += " " + word;
                    else
                    {
                        lines.Add(cur);
                        cur = word;
                    }
                }
                lines.Add(cur);
            }
            return string.Join("\n", lines);
        }

        public static ushort MessageBox(string msg, ushort options)
        {
            int btnCount = 0;
            bool hasYes = (options & MsgBoxFlags.MfYesButton) != 0;
            bool hasNo = (options & MsgBoxFlags.MfNoButton) != 0;
            bool hasOk = (options & MsgBoxFlags.MfOkButton) != 0;
            bool hasCancel = (options & MsgBoxFlags.MfCancelButton) != 0;
            if (hasYes) btnCount++;
            if (hasNo) btnCount++;
            if (hasOk) btnCount++;
            if (hasCancel) btnCount++;
            if (btnCount == 0) { hasOk = true; btnCount = 1; }

            int btnW = 12;
            int width = Math.Max(30, btnCount * (btnW + 2) + 4);
            var text = Wrap(msg, width - 4);
            int textLines = 1;
            foreach (char c in text) if (c == '\n') textLines++;

            int height = textLines + 5;
            int dw = width, dh = height;
            int dx = 0, dy = 0;
            var dt = TProgram.DeskTop;
            if (dt != null && dt.Size.X > dw && dt.Size.Y > dh)
            {
                dx = (dt.Size.X - dw) / 2;
                dy = (dt.Size.Y - dh) / 3;
            }

            var d = new TDialog(new TRect((short)dx, (short)dy, (short)(dx + dw), (short)(dy + dh)), null);
            d.Options |= Commands.OfCentered;

            d.Insert(new TStaticText(new TRect(2, 2, (short)(dw - 2), (short)(2 + textLines)), text));

            int bx = (dw - btnCount * btnW - (btnCount - 1)) / 2;
            int by = dh - 3;
            void AddBtn(string title, ushort cmd, bool def)
            {
                d.Insert(new TButton(new TRect((short)bx, (short)by, (short)(bx + btnW), (short)(by + 2)),
                    title, cmd, def ? TButton.BfDefault : TButton.BfNormal));
                bx += btnW + 1;
            }
            if (hasYes) AddBtn("~Y~es", Commands.CmYes, !hasOk);
            if (hasNo) AddBtn("~N~o", Commands.CmNo, false);
            if (hasOk) AddBtn("O~K~", Commands.CmOK, true);
            if (hasCancel) AddBtn("Cancel", Commands.CmCancel, false);

            d.Select();
            ushort result;
            TGroup dtg = (TGroup)TProgram.DeskTop ?? (TGroup)TProgram.App;
            if (dtg != null)
                result = dtg.ExecView(d);
            else
                result = d.Execute();

            TObject.Destroy(d);
            return result;
        }
    }

    public static class ColorSel
    {
    }
}