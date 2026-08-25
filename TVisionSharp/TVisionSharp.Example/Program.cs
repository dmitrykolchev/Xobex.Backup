using System;
using TVision;

namespace TVision.Example
{
    public class TMyApp : TApplication
    {
        public TMyApp() : base()
        {
        }

        protected override void InitMenuBar()
        {
            var r = GetBounds();
            r.B.Y = r.A.Y + 1;
            var items = new TMenu(
                new TMenuItem(
                    "~O~pen", Commands.CmOpen, new TKey(KeyCodes.KbF3),
                    null,
                    new TMenuItem(
                        "~S~ave", Commands.CmSave, new TKey(KeyCodes.KbF2),
                        null,
                        new TMenuItem(
                            "~E~xit", Commands.CmQuit, new TKey(KeyCodes.KbAltX)
                        )
                    )
                )
            );
            MenuBar = new TMenuBar(r, new TMenu(
                new TMenuItem("~F~ile", new TKey(KeyCodes.KbAltF), items)));
            MenuBar.GrowMode = Commands.GfGrowLoX;
            Insert(MenuBar);
        }

        protected override void InitStatusLine()
        {
            var r = GetBounds();
            r.A.Y = r.B.Y - 1;
            StatusLine = new TStatusLine(r,
                new TStatusDef(0, 0xFFFF,
                    new TStatusItem("~F1~ Help", new TKey(KeyCodes.KbF1), Commands.CmHelp,
                        new TStatusItem("~F10~ Menu", new TKey(KeyCodes.KbF10), Commands.CmMenu,
                            new TStatusItem("~Alt+X~ Exit", new TKey(KeyCodes.KbAltX), Commands.CmQuit)
                        )
                    )
                )
            );
            StatusLine.GrowMode = Commands.GfGrowLoX | Commands.GfGrowHiY;
            Insert(StatusLine);
        }

        public override void HandleEvent(TEvent ev)
        {
            base.HandleEvent(ev);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--selftest")
            {
                Environment.Exit(SelfTest());
            }
            Platform.Init();
            var app = new TMyApp();
            app.Run();
            Platform.Shutdown();
        }
        static int SelfTest()
        {
            Platform.Init();
            var app = new TMyApp();
            app.SetState(Commands.SfExposed, true);
            app.DrawView();
            TScreen.FlushScreen();

            var mb = TProgram.MenuBar;

            TEventQueue.SetPasteText("E");

            var altF = new TEvent();
            altF.What = EventCodes.EvKeyDown;
            altF.KeyDown.KeyCode = 0x2100;
            mb.PreProcessKeyEvent(ref altF);

            TEvent pe = new TEvent();
            app.GetEventRef(ref pe);

            bool ok = pe.What == EventCodes.EvCommand &&
                      pe.Message.Command == Commands.CmQuit;
            Platform.Shutdown();
            Console.Error.WriteLine(ok ? "SELFTEST OK" : $"SELFTEST FAIL what={pe.What} cmd={pe.Message.Command:X4}");
            return ok ? 7 : 3;
        }
    }
}
