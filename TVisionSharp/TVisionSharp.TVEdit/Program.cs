using System;
using TVision;

namespace TVEdit
{
    public class TEditorApp : TApplication
    {
        public TEditorApp(string[] args) : base()
        {
            var ts = new TCommandSet(
                Commands.CmSave, Commands.CmSaveAs,
                Commands.CmCut, Commands.CmCopy, Commands.CmPaste, Commands.CmClear,
                Commands.CmUndo,
                (ushort)Commands.CmFind, (ushort)Commands.CmReplace, (ushort)Commands.CmSearchAgain);
            DisableCommands(ts);

            TEditor.EditorDialogFunc = DoEditDialog;

            foreach (var arg in args)
                OpenEditor(arg, true);
            if (DeskTop != null)
                DeskTop.Cascade(DeskTop.GetExtent());
        }

        protected override void InitMenuBar()
        {
            var r = GetBounds();
            r.B.Y = r.A.Y + 1;

            var fileItems =
                Mi("~O~pen (F3)", Commands.CmOpen, KeyCodes.KbF3,
                Mi("~N~ew", Commands.CmNew, KeyCodes.KbCtrlN,
                Mi("~S~ave (F2)", Commands.CmSave, KeyCodes.KbF2,
                Mi("S~a~ve as...", Commands.CmSaveAs, 0,
                Mi("~C~hange dir...", Commands.CmChDir, 0,
                Mi("E~x~it", Commands.CmQuit, KeyCodes.KbCtrlQ))))));

            var editItems =
                Mi("~U~ndo", Commands.CmUndo, KeyCodes.KbCtrlU,
                Mi("Cu~t~", Commands.CmCut, KeyCodes.KbShiftDel,
                Mi("~C~opy", Commands.CmCopy, KeyCodes.KbCtrlIns,
                Mi("~P~aste", Commands.CmPaste, KeyCodes.KbShiftIns,
                Mi("~C~lear", Commands.CmClear, KeyCodes.KbCtrlDel)))));

            var searchItems =
                Mi("~F~ind...", (ushort)Commands.CmFind, 0,
                Mi("~R~eplace...", (ushort)Commands.CmReplace, 0,
                Mi("~S~earch again", (ushort)Commands.CmSearchAgain, 0)));

            var windowItems =
                Mi("~Z~oom (F5)", Commands.CmZoom, KeyCodes.KbF5,
                Mi("~T~ile", Commands.CmTile, 0,
                Mi("C~a~scade", Commands.CmCascade, 0,
                Mi("~N~ext (F6)", Commands.CmNext, KeyCodes.KbF6,
                Mi("~P~revious", Commands.CmPrev, KeyCodes.KbShiftF6,
                Mi("~C~lose", Commands.CmClose, KeyCodes.KbAltF3))))));

            var topItems =
                new TMenuItem("~F~ile", new TKey(KeyCodes.KbAltF), new TMenu(fileItems),
                new TMenuItem("~E~dit", new TKey(KeyCodes.KbAltE), new TMenu(editItems),
                new TMenuItem("~S~earch", new TKey(KeyCodes.KbAltS), new TMenu(searchItems),
                new TMenuItem("~W~indows", new TKey(KeyCodes.KbAltW), new TMenu(windowItems)))));

            MenuBar = new TMenuBar(r, new TMenu(topItems));
            MenuBar.GrowMode = Commands.GfGrowLoX;
            Insert(MenuBar);
        }

        private static TMenuItem Mi(string name, ushort cmd, ushort key, TMenuItem next = null)
        {
            return new TMenuItem(name, cmd, new TKey(key), null, next);
        }

        protected override void InitStatusLine()
        {
            var r = GetBounds();
            r.A.Y = r.B.Y - 1;
            var items =
                Hid(KeyCodes.KbShiftDel, Commands.CmCut,
                Hid(KeyCodes.KbCtrlIns, Commands.CmCopy,
                Hid(KeyCodes.KbShiftIns, Commands.CmPaste)));
            items = It("~F10~ Menu", KeyCodes.KbF10, Commands.CmMenu, items);
            items = It("~F6~ Next", KeyCodes.KbF6, Commands.CmNext, items);
            items = It("~F5~ Zoom", KeyCodes.KbF5, Commands.CmZoom, items);
            items = It("~Alt+F3~ Close", KeyCodes.KbAltF3, Commands.CmClose, items);
            items = It("~F3~ Open", KeyCodes.KbF3, Commands.CmOpen, items);
            items = It("~F2~ Save", KeyCodes.KbF2, Commands.CmSave, items);
            items = Hid(KeyCodes.KbAltX, Commands.CmQuit, items);
            StatusLine = new TStatusLine(r, new TStatusDef(0, 0xFFFF, items));
            StatusLine.GrowMode = Commands.GfGrowLoX | Commands.GfGrowHiY;
            Insert(StatusLine);
        }

        private static TStatusItem It(string text, ushort key, ushort cmd, TStatusItem next = null)
            => new TStatusItem(text, new TKey(key), cmd, next);

        private static TStatusItem Hid(ushort key, ushort cmd, TStatusItem next = null)
            => new TStatusItem(null, new TKey(key), cmd, next);

        private static TEditWindow OpenEditor(string fileName, bool visible)
        {
            var dt = TProgram.DeskTop;
            if (dt == null) return null;
            var r = dt.GetExtent();
            var w = new TEditWindow(r, fileName, Commands.WnNoNumber);
            if (w.Editor == null || !w.Editor.IsValid)
            {
                TObject.Destroy(w);
                return null;
            }
            if (!visible) w.Hide();
            dt.Insert(w);
            dt.SetCurrent(w, SelectMode.NormalSelect);
            return w;
        }

        private void FileOpen()
        {
            var box = new[] { "*.*" };
            if (ExecDialog(new TFileDialog("*.*", "Open a File", "~N~ame",
                TFileDialog.FdOpenButton, 100), box) != Commands.CmCancel && box[0].Length > 0)
                OpenEditor(box[0], true);
        }

        private void FileNew() => OpenEditor(null, true);

        private static TEditor FocusedEditor()
        {
            var c = TProgram.DeskTop?.Current as TEditWindow;
            return c?.Editor;
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);

            if (ev.What == EventCodes.EvCommand)
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmOpen:
                        FileOpen();
                        ClearEvent(ev);
                        return;

                    case Commands.CmNew:
                        FileNew();
                        ClearEvent(ev);
                        return;

                    case Commands.CmChDir:
                        ExecDialog(new TChDirDialog(), null);
                        ClearEvent(ev);
                        return;

                    case Commands.CmQuit:
                        EndModal(Commands.CmQuit);
                        ClearEvent(ev);
                        return;

                    default:
                        if (ev.Message.Command == Commands.CmFind)
                        {
                            DoFind();
                            ClearEvent(ev);
                            return;
                        }
                        if (ev.Message.Command == Commands.CmReplace)
                        {
                            DoReplace();
                            ClearEvent(ev);
                            return;
                        }
                        if (ev.Message.Command == Commands.CmSearchAgain)
                        {
                            SearchAgain();
                            ClearEvent(ev);
                            return;
                        }
                        break;
                }
            }
        }

        internal static ushort ExecDialog(TDialog d, object data)
        {
            if (d == null) return Commands.CmCancel;
            var dt = TProgram.DeskTop ?? (TGroup)TProgram.App;
            if (data != null)
                d.SetData(data);
            d.SetState(Commands.SfExposed, true);
            dt.Insert(d);
            ushort result = dt.ExecView(d);
            dt.Remove(d);
            if (result != Commands.CmCancel && data != null)
                d.GetData(data);
            TObject.Destroy(d);
            return result;
        }

        internal static TDialog CreateFindDialog()
        {
            var d = new TFindDialog(new TRect(0, 0, 38, 10), "Find");
            var control = new TInputLine(new TRect(3, 3, 32, 4), 80);
            d.Insert(control);
            d.Insert(new TLabel(new TRect(2, 2, 15, 3), "~T~ext to find", control));
            d.Insert(new TButton(new TRect(14, 7, 24, 9), "O~K~", Commands.CmOK, TButton.BfDefault));
            d.Insert(new TButton(new TRect(26, 7, 36, 9), "Cancel", Commands.CmCancel, TButton.BfNormal));
            d.FindLine = control;
            d.SelectNext(false);
            return d;
        }

        internal static TDialog CreateReplaceDialog()
        {
            var d = new TReplaceDialog(new TRect(0, 0, 40, 13), "Replace");
            var find = new TInputLine(new TRect(3, 3, 34, 4), 80);
            d.Insert(find);
            d.Insert(new TLabel(new TRect(2, 2, 15, 3), "~T~ext to find", find));
            var repl = new TInputLine(new TRect(3, 6, 34, 7), 80);
            d.Insert(repl);
            d.Insert(new TLabel(new TRect(2, 5, 12, 6), "~N~ew text", repl));
            d.Insert(new TButton(new TRect(17, 9, 27, 11), "O~K~", Commands.CmOK, TButton.BfDefault));
            d.Insert(new TButton(new TRect(28, 9, 38, 11), "Cancel", Commands.CmCancel, TButton.BfNormal));
            d.FindLine = find;
            d.NewLine = repl;
            d.SelectNext(false);
            return d;
        }

        private static void DoFind()
        {
            var ed = FocusedEditor();
            if (ed == null) return;
            var rec = new TFindDialogRec(TEditor.FindStr, 0);
            rec.Find = string.IsNullOrEmpty(rec.Find) ? string.Empty : rec.Find;
            var box = new[] { rec.Find };
            if (ExecDialog(CreateFindDialog(), box) != Commands.CmCancel)
            {
                TEditor.FindStr = box[0];
                SearchAgain();
            }
        }

        private static void SearchAgain()
        {
            var ed = FocusedEditor();
            if (ed == null || string.IsNullOrEmpty(TEditor.FindStr)) return;
            if (!ed.Search(TEditor.FindStr, 0))
                TVMessageBox.MessageBox("Search string not found.", MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
        }

        private static void DoReplace()
        {
            var ed = FocusedEditor();
            if (ed == null) return;
            var find = new[] { TEditor.FindStr };
            var repl = new[] { TEditor.ReplaceStr };
            if (ExecDialog(CreateReplaceDialog(), new ReplaceData(find, repl)) == Commands.CmCancel)
                return;
            TEditor.FindStr = find[0];
            TEditor.ReplaceStr = repl[0];
            int count = 0;
            while (ed.Search(TEditor.FindStr, 0))
            {
                ed.SetSelText(TEditor.ReplaceStr);
                count++;
                if (count > 10000) break;
            }
            if (count > 0)
                TVMessageBox.MessageBox($"{count} replacement(s) made.",
                    MsgBoxFlags.MfInformation | MsgBoxFlags.MfOkButton);
            else
                TVMessageBox.MessageBox("Search string not found.",
                    MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
        }

        internal class ReplaceData
        {
            public readonly string[] Find;
            public readonly string[] Replace;
            public ReplaceData(string[] f, string[] r) { Find = f; Replace = r; }
        }

        internal class TFindDialog : TDialog
        {
            public TInputLine FindLine;

            public TFindDialog(TRect bounds, string title) : base(bounds, title) { }

            public override void SetData(object rec)
            {
                if (rec is string[] b && FindLine != null)
                {
                    FindLine.Data = b[0] ?? string.Empty;
                    FindLine.DrawView();
                }
            }

            public override void GetData(object rec)
            {
                if (rec is string[] b)
                    b[0] = FindLine?.Data ?? string.Empty;
            }
        }

        internal class TReplaceDialog : TDialog
        {
            public TInputLine FindLine;
            public TInputLine NewLine;

            public TReplaceDialog(TRect bounds, string title) : base(bounds, title) { }

            public override void SetData(object rec)
            {
                if (rec is ReplaceData rd)
                {
                    if (FindLine != null) { FindLine.Data = rd.Find[0] ?? string.Empty; FindLine.DrawView(); }
                    if (NewLine != null) { NewLine.Data = rd.Replace[0] ?? string.Empty; NewLine.DrawView(); }
                }
            }

            public override void GetData(object rec)
            {
                if (rec is ReplaceData rd)
                {
                    rd.Find[0] = FindLine?.Data ?? string.Empty;
                    rd.Replace[0] = NewLine?.Data ?? string.Empty;
                }
            }
        }

        internal static ushort DoEditDialog(int dialog, params object[] args)
        {
            switch (dialog)
            {
                case TEditor.EdOutOfMemory:
                    return TVMessageBox.MessageBox("Not enough memory for this operation",
                        MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
                case TEditor.EdReadError:
                    return TVMessageBox.MessageBox(
                        $"Error reading file {(args.Length > 0 ? args[0] : "?")}.",
                        MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
                case TEditor.EdWriteError:
                    return TVMessageBox.MessageBox(
                        $"Error writing file {(args.Length > 0 ? args[0] : "?")}.",
                        MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
                case TEditor.EdCreateError:
                    return TVMessageBox.MessageBox(
                        $"Error creating file {(args.Length > 0 ? args[0] : "?")}.",
                        MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
                case TEditor.EdSaveModify:
                    return TVMessageBox.MessageBox(
                        $"{(args.Length > 0 ? args[0] : "File")} has been modified. Save?",
                        MsgBoxFlags.MfInformation | MsgBoxFlags.MfYesNoCancel);
                case TEditor.EdSaveUntitled:
                    return TVMessageBox.MessageBox("Save untitled file?",
                        MsgBoxFlags.MfInformation | MsgBoxFlags.MfYesNoCancel);
                case TEditor.EdSaveAs:
                    {
                        var box = args.Length > 0 ? args[0] as string[] : null;
                        if (box == null) return Commands.CmCancel;
                        ushort r = ExecDialog(new TFileDialog("*.*", "Save file as",
                            "~N~ame", TFileDialog.FdOkButton, 101), box);
                        return r;
                    }
                case TEditor.EdFind:
                    {
                        var box = args.Length > 0 ? args[0] as string[] : null;
                        if (box == null) return Commands.CmCancel;
                        ushort r = ExecDialog(CreateFindDialog(), box);
                        return r;
                    }
                case TEditor.EdSearchFailed:
                    return TVMessageBox.MessageBox("Search string not found.",
                        MsgBoxFlags.MfError | MsgBoxFlags.MfOkButton);
                case TEditor.EdReplace:
                    return Commands.CmCancel;
            }
            return Commands.CmCancel;
        }

        public void ChangeDir()
        {
            ExecDialog(new TChDirDialog(), null);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Platform.Init();
            var app = new TEditorApp(args);
            app.Run();
            Platform.Shutdown();
        }
    }
}
