using System;
using System.Collections.Generic;
using System.Threading;

namespace TVision
{
    public static class PipeInput
    {
        private static Thread _readerThread;
        private static readonly object Lock = new object();
        private static bool _started;
        private static bool _eof;

        private static readonly Queue<TEvent> Events = new Queue<TEvent>();

        public static void EnsureStarted()
        {
            lock (Lock)
            {
                if (_started || _eof) return;
                _started = true;
            }
            _readerThread = new Thread(ReaderLoop) { IsBackground = true };
            _readerThread.Start();
        }

        private static void ReaderLoop()
        {
            var stream = Console.OpenStandardInput();
            var buf = new byte[4096];
            var pending = new List<byte>();
            while (true)
            {
                int n;
                try { n = stream.Read(buf, 0, buf.Length); }
                catch { break; }
                if (n <= 0) break;
                lock (Lock)
                {
                    foreach (byte b in buf)
                    {
                        pending.Add(b);
                        TryParse(pending);
                    }
                    Monitor.PulseAll(Lock);
                }
            }
            _eof = true;
        }

        private static void Enqueue(TEvent ev)
        {
            Events.Enqueue(ev);
        }

        private static void TryParse(List<byte> b)
        {
            while (b.Count > 0)
            {
                if (b[0] == 0x1B)
                {
                    if (b.Count == 1) return;

                    if (b[1] == '[')
                    {
                        int i = 2;
                        while (i < b.Count && ((b[i] >= '0' && b[i] <= '9') || b[i] == ';' || b[i] == '<' || b[i] == '?'))
                            i++;
                        if (i >= b.Count) return;
                        char cmd = (char)b[i];
                        string nums = System.Text.Encoding.ASCII.GetString(b.GetRange(2, i - 2).ToArray()).TrimStart('<');
                        b.RemoveRange(0, i + 1);
                        ParseCsi(nums, cmd);
                        continue;
                    }
                    if (b[1] == 'O')
                    {
                        if (b.Count < 3) return;
                        char c = (char)b[2];
                        b.RemoveRange(0, 3);
                        ushort code = c switch
                        {
                            'P' => KeyCodes.KbF1,
                            'Q' => KeyCodes.KbF2,
                            'R' => KeyCodes.KbF3,
                            'S' => KeyCodes.KbF4,
                            'H' => KeyCodes.KbHome,
                            'F' => KeyCodes.KbEnd,
                            _ => 0
                        };
                        if (code != 0) PushKey(code);
                        continue;
                    }
                    b.RemoveAt(0);
                    PushKey(KeyCodes.KbEsc);
                    continue;
                }

                byte cur = b[0];
                b.RemoveAt(0);
                switch (cur)
                {
                    case (byte)'\r': PushKey(KeyCodes.KbEnter); break;
                    case (byte)'\n': break;
                    case (byte)'\t': PushKey(KeyCodes.KbTab); break;
                    case (byte)8: PushKey(KeyCodes.KbBack); break;
                    default:
                        if (cur >= 32) PushKey(cur);
                        else if (cur >= 1 && cur <= 26) PushKey((ushort)cur);
                        break;
                }
            }
        }

        private static void ParseCsi(string nums, char cmd)
        {
            if (cmd == 'M' || cmd == 'm')
            {
                if (!nums.StartsWith("<")) return;
                var p = nums.Substring(1).Split(';');
                if (p.Length < 3) return;
                if (!int.TryParse(p[0], out int mb)) return;
                if (!int.TryParse(p[1], out int mx)) return;
                if (!int.TryParse(p[2], out int my)) return;

                int buttons = 0;
                if ((mb & 2) == 0) buttons |= EventCodes.MbLeftButton;
                if ((mb & 8) != 0) buttons |= EventCodes.MbRightButton;
                bool isMove = (mb & 32) != 0;
                bool release = (cmd == 'm');

                var ev = new TEvent();
                ev.Mouse.Where = new TPoint((short)(mx - 1), (short)(my - 1));
                ev.Mouse.Buttons = (byte)buttons;
                if (isMove)
                {
                    ev.What = EventCodes.EvMouseMove;
                }
                else if (release)
                {
                    ev.What = EventCodes.EvMouseUp;
                }
                else
                {
                    ev.What = EventCodes.EvMouseDown;
                }
                Enqueue(ev);
                return;
            }

            ushort keyCode = cmd switch
            {
                'A' => KeyCodes.KbUp,
                'B' => KeyCodes.KbDown,
                'C' => KeyCodes.KbRight,
                'D' => KeyCodes.KbLeft,
                'H' => KeyCodes.KbHome,
                'F' => KeyCodes.KbEnd,
                _ => 0
            };
            if (keyCode == 0 && nums.Length > 0 && int.TryParse(nums, out int vt))
            {
                keyCode = vt switch
                {
                    1 => KeyCodes.KbHome,
                    2 => KeyCodes.KbIns,
                    3 => KeyCodes.KbDel,
                    4 => KeyCodes.KbEnd,
                    5 => KeyCodes.KbPgUp,
                    6 => KeyCodes.KbPgDn,
                    11 => KeyCodes.KbF1, 12 => KeyCodes.KbF2, 13 => KeyCodes.KbF3,
                    14 => KeyCodes.KbF4, 15 => KeyCodes.KbF5, 17 => KeyCodes.KbF6,
                    18 => KeyCodes.KbF7, 19 => KeyCodes.KbF8, 20 => KeyCodes.KbF9,
                    21 => KeyCodes.KbF10, 23 => KeyCodes.KbF11, 24 => KeyCodes.KbF12,
                    _ => 0
                };
            }
            if (keyCode != 0) PushKey(keyCode);
        }

        private static void PushKey(ushort code)
        {
            var ev = new TEvent();
            ev.What = EventCodes.EvKeyDown;
            ev.KeyDown.KeyCode = code;
            if (code < 0x100 && code >= 32)
            {
                ev.KeyDown.Char0 = (char)code;
                ev.KeyDown.TextLength = 1;
            }
            Enqueue(ev);
        }

        public static bool GetEvent(ref TEvent ev, int timeoutMs)
        {
            EnsureStarted();
            lock (Lock)
            {
                if (Events.Count == 0)
                {
                    Monitor.Wait(Lock, Math.Max(1, timeoutMs));
                }
                if (Events.Count > 0)
                {
                    ev = Events.Dequeue();
                    return true;
                }
            }
            return false;
        }
    }
}
