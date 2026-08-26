using System;
using System.Collections.Generic;

namespace TVision
{
    public class TDeskTop : TGroup
    {
        public TBackground Background;

        public TDeskTop(TRect bounds) : base(bounds)
        {
            Options = 0;
            State = Commands.SfVisible;
            GrowMode = Commands.GfGrowLoX | Commands.GfGrowLoY;
            InitBackground();
        }

        protected virtual void InitBackground()
        {
            Background = new TBackground(GetExtent(), "\u2591");
            Insert(Background);
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvCommand)
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmTile:
                        Tile(GetExtent());
                        ClearEvent(ev);
                        return;
                    case Commands.CmCascade:
                        Cascade(GetExtent());
                        ClearEvent(ev);
                        return;
                    case Commands.CmNext:
                        SelectNext(true);
                        ClearEvent(ev);
                        return;
                    case Commands.CmPrev:
                        SelectNext(false);
                        ClearEvent(ev);
                        return;
                }
            }
            base.HandleEvent(ev);
        }

        public override void SetState(ushort aState, bool enable)
        {
            base.SetState(aState, enable);
        }

        public void Cascade(TRect r)
        {
            var windows = new List<TWindow>();
            var v = First();
            while (v != null)
            {
                if (v is TWindow w && v.GetState(Commands.SfVisible))
                    windows.Add(w);
                v = v.Next;
                if (v == First()) break;
            }
            if (windows.Count == 0) return;

            int w2 = Math.Max(2, r.Width / 2);
            int h2 = Math.Max(1, r.Height / 2);
            int dx = 2;
            int dy = 1;

            for (int i = 0; i < windows.Count; i++)
            {
                var wr = new TRect(
                    r.A.X + i * dx,
                    r.A.Y + i * dy,
                    r.A.X + i * dx + w2,
                    r.A.Y + i * dy + h2
                );
                wr = wr.Intersect(r);
                windows[i].ChangeBounds(wr);
            }
        }

        public void Tile(TRect r)
        {
            var windows = new List<TWindow>();
            var v = First();
            while (v != null)
            {
                if (v is TWindow w && v.GetState(Commands.SfVisible))
                    windows.Add(w);
                v = v.Next;
                if (v == First()) break;
            }
            if (windows.Count == 0) return;

            int cols = (int)Math.Ceiling(Math.Sqrt(windows.Count));
            int rows = (int)Math.Ceiling((double)windows.Count / cols);
            int cellW = r.Width / cols;
            int cellH = r.Height / rows;

            for (int i = 0; i < windows.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                var wr = new TRect(
                    r.A.X + col * cellW,
                    r.A.Y + row * cellH,
                    r.A.X + (col + 1) * cellW,
                    r.A.Y + (row + 1) * cellH
                );
                windows[i].ChangeBounds(wr);
            }
        }
    }

    public class TProgram : TGroup
    {
        public const string CpAppColor =
            "\x71\x70\x78\x74\x20\x28\x24\x17\x1F\x1A\x31\x31\x1E\x71\x1F" +
            "\x37\x3F\x3A\x13\x13\x3E\x21\x3F\x70\x7F\x7A\x13\x13\x70\x7F\x7E" +
            "\x70\x7F\x7A\x13\x13\x70\x70\x7F\x7E\x20\x2B\x2F\x78\x2E\x70\x30" +
            "\x3F\x3E\x1F\x2F\x1A\x20\x72\x31\x31\x30\x2F\x3E\x31\x13\x38\x00" +
            "\x17\x1F\x1A\x71\x71\x1E\x17\x1F\x1E\x20\x2B\x2F\x78\x2E\x10\x30" +
            "\x3F\x3E\x70\x2F\x7A\x20\x12\x31\x31\x30\x2F\x3E\x31\x13\x38\x00";

        public override TPalette GetPalette()
        {
            return new TPalette(CpAppColor, 96);
        }

        public static TStatusLine StatusLine;
        public static TMenuBar MenuBar;
        public static TDeskTop DeskTop;
        public static TApplication App;

        public static uint TickCount;

        private static TEvent _pendingEvent;
        private static bool _hasPendingEvent;        public TProgram() : base(GetDesktopBounds())
        {
            App = this as TApplication;
            StatusLine = null;
            MenuBar = null;
            DeskTop = null;
        }

        private static TRect GetDesktopBounds()
        {
            ushort w, h;
            try { w = (ushort)Console.WindowWidth; h = (ushort)Console.WindowHeight; }
            catch { w = 80; h = 25; }
            return new TRect(0, 0, w, h);
        }

        public override void HandleEvent(TEvent ev)
        {
            if (ev.What == EventCodes.EvCommand)
            {
                switch (ev.Message.Command)
                {
                    case Commands.CmQuit:
                        EndModal(Commands.CmQuit);
                        ClearEvent(ev);
                        break;
                }
            }
            base.HandleEvent(ev);
        }

        public virtual void Idle()
        {
            if (TView.CommandSetChanged)
            {
                TView.CommandSetChanged = false;
                var tv = TopView();
                if (tv != null)
                {
                    var ce = new TEvent();
                    ce.What = EventCodes.EvBroadcast;
                    ce.Message.Command = Commands.CmCommandSetChanged;
                    tv.HandleEvent(ce);
                }
            }
        }

        public static uint GetTickCount() => (uint)(Environment.TickCount & 0xFFFFFFFF);

        public static uint GetTickCountMs() => (uint)(Environment.TickCount & 0xFFFFFFFF);

        public override void GetEventRef(ref TEvent ev)
        {
            if (_hasPendingEvent)
            {
                ev.What = _pendingEvent.What;
                ev.Mouse = _pendingEvent.Mouse;
                ev.KeyDown = _pendingEvent.KeyDown;
                ev.Message = _pendingEvent.Message;
                _hasPendingEvent = false;
                return;
            }
            TEventQueue.GetKeyEvent(ref ev);
            if (ev.What == EventCodes.EvNothing)
                TEventQueue.GetMouseEvent(ref ev);
        }

        public override void PutEvent(TEvent ev)
        {
            _pendingEvent = ev;
            _hasPendingEvent = true;
        }

        public override void ShutDown()
        {
            DeskTop?.ShutDown();
            MenuBar?.ShutDown();
            StatusLine?.ShutDown();
            base.ShutDown();
        }
    }

    public class TApplication : TProgram
    {
        public TApplication() : base()
        {
            InitScreen();
            InitDesktop();
            InitMenuBar();
            InitStatusLine();
        }

        protected virtual void InitScreen()
        {
            Platform.Init();
            TScreen.SetCrtData();
            int w = Math.Max(1, Size.X);
            int h = Math.Max(1, Size.Y);
            TScreen.ScreenWidth = (ushort)w;
            TScreen.ScreenHeight = (ushort)h;
            TScreen.ScreenBuffer = new TScreenCell[w * h];
            var initAttr = new TColorAttr(0x07);
            for (int i = 0; i < TScreen.ScreenBuffer.Length; i++)
                TScreen.ScreenBuffer[i] = new TScreenCell(' ', initAttr);
            Buffer = TScreen.ScreenBuffer;
        }

        protected virtual void InitDesktop()
        {
            var bounds = GetBounds();
            bounds.A.Y += 1;
            bounds.B.Y -= 1;
            DeskTop = new TDeskTop(bounds);
            Insert(DeskTop);
            SetCurrent(DeskTop, SelectMode.EnterSelect);
        }

        protected virtual void InitMenuBar()
        {
        }

        protected virtual void InitStatusLine()
        {
        }

        public void Run()
        {
            SetState(Commands.SfExposed, true);
            DrawView();
            TScreen.FlushScreen();

            while (true)
            {
                TEvent ev = new TEvent();
                GetEventRef(ref ev);

                if (ev.What == EventCodes.EvNothing)
                {
                    Idle();
                    if (CheckForResize())
                        continue;
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                HandleEvent(ev);
                if (ev.What != EventCodes.EvNothing)
                    EventError(ev);

                DrawView();
                TScreen.FlushScreen();

                if (EndState == Commands.CmQuit)
                    break;
            }
        }

        private bool CheckForResize()
        {
            try
            {
                var adapter = Platform.GetAdapter();
                int w = adapter.GetCols();
                int h = adapter.GetRows();
                if (w == TScreen.ScreenWidth && h == TScreen.ScreenHeight)
                    return false;
                if (w <= 0 || h <= 0)
                    return false;
                ResizeScreen(w, h);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ResizeScreen(int newWidth, int newHeight)
        {
            var adapter = Platform.GetAdapter();
            adapter.Invalidate();

            TScreen.ScreenWidth = (ushort)Math.Max(1, newWidth);
            TScreen.ScreenHeight = (ushort)Math.Max(1, newHeight);
            int w = TScreen.ScreenWidth;
            int h = TScreen.ScreenHeight;
            TScreen.ScreenBuffer = new TScreenCell[w * h];
            var initAttr = new TColorAttr(0x07);
            for (int i = 0; i < TScreen.ScreenBuffer.Length; i++)
                TScreen.ScreenBuffer[i] = new TScreenCell(' ', initAttr);

            Buffer = TScreen.ScreenBuffer;

            Locate(new TRect(0, 0, w, h));
            DrawView();
            TScreen.FlushScreen();
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }

        public override void ShutDown()
        {
            Platform.Shutdown();
            base.ShutDown();
        }
    }
}
