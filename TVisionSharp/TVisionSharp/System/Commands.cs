using System;
using System.Text;

namespace TVision
{
    public static class Commands
    {
        // Standard command codes
        public const ushort CmValid = 0;
        public const ushort CmQuit = 1;
        public const ushort CmError = 2;
        public const ushort CmMenu = 3;
        public const ushort CmClose = 4;
        public const ushort CmZoom = 5;
        public const ushort CmResize = 6;
        public const ushort CmNext = 7;
        public const ushort CmPrev = 8;
        public const ushort CmHelp = 9;

        // TDialog standard commands
        public const ushort CmOK = 10;
        public const ushort CmCancel = 11;
        public const ushort CmYes = 12;
        public const ushort CmNo = 13;
        public const ushort CmDefault = 14;

        // Standard application commands
        public const ushort CmNew = 30;
        public const ushort CmOpen = 31;
        public const ushort CmSave = 32;
        public const ushort CmSaveAs = 33;
        public const ushort CmSaveAll = 34;
        public const ushort CmChDir = 35;
        public const ushort CmDosShell = 36;
        public const ushort CmCloseAll = 37;

        // Application command codes
        public const ushort CmCut = 20;
        public const ushort CmCopy = 21;
        public const ushort CmPaste = 22;
        public const ushort CmUndo = 23;
        public const ushort CmClear = 24;
        public const ushort CmTile = 25;
        public const ushort CmCascade = 26;
        public const ushort CmRedo = 27;

        // Standard messages
        public const ushort CmReceivedFocus = 50;
        public const ushort CmReleasedFocus = 51;
        public const ushort CmCommandSetChanged = 52;
        public const ushort CmTimerExpired = 58;
        public const ushort CmScrollBarChanged = 53;
        public const ushort CmScrollBarClicked = 54;
        public const ushort CmSelectWindowNum = 55;
        public const ushort CmListItemSelected = 56;
        public const ushort CmScreenChanged = 57;

        // TView State masks
        public const ushort SfVisible = 0x001;
        public const ushort SfCursorVis = 0x002;
        public const ushort SfCursorIns = 0x004;
        public const ushort SfShadow = 0x008;
        public const ushort SfActive = 0x010;
        public const ushort SfSelected = 0x020;
        public const ushort SfFocused = 0x040;
        public const ushort SfDragging = 0x080;
        public const ushort SfDisabled = 0x100;
        public const ushort SfModal = 0x200;
        public const ushort SfDefault = 0x400;
        public const ushort SfExposed = 0x800;

        // TView Option masks
        public const ushort OfSelectable = 0x001;
        public const ushort OfTopSelect = 0x002;
        public const ushort OfFirstClick = 0x004;
        public const ushort OfFramed = 0x008;
        public const ushort OfPreProcess = 0x010;
        public const ushort OfPostProcess = 0x020;
        public const ushort OfBuffered = 0x040;
        public const ushort OfTileable = 0x080;
        public const ushort OfCenterX = 0x100;
        public const ushort OfCenterY = 0x200;
        public const ushort OfCentered = 0x300;
        public const ushort OfValidate = 0x400;
        public const ushort OfBottom = 0x1000;

        // TView GrowMode masks
        public const byte GfGrowLoX = 0x01;
        public const byte GfGrowLoY = 0x02;
        public const byte GfGrowHiX = 0x04;
        public const byte GfGrowHiY = 0x08;
        public const byte GfGrowAll = 0x0f;
        public const byte GfGrowRel = 0x10;
        public const byte GfFixed = 0x20;

        // TView DragMode masks
        public const byte DmDragMove = 0x01;
        public const byte DmDragGrow = 0x02;
        public const byte DmDragGrowLeft = 0x04;
        public const byte DmLimitLoX = 0x10;
        public const byte DmLimitLoY = 0x20;
        public const byte DmLimitHiX = 0x40;
        public const byte DmLimitHiY = 0x80;
        public const byte DmLimitAll = DmLimitLoX | DmLimitLoY | DmLimitHiX | DmLimitHiY;

        // TView inhibit flags
        public const ushort NoMenuBar = 0x0001;
        public const ushort NoDeskTop = 0x0002;
        public const ushort NoStatusLine = 0x0004;
        public const ushort NoBackground = 0x0008;
        public const ushort NoFrame = 0x0010;
        public const ushort NoViewer = 0x0020;
        public const ushort NoHistory = 0x0040;

        // TWindow Flags masks
        public const byte WfMove = 0x01;
        public const byte WfGrow = 0x02;
        public const byte WfClose = 0x04;
        public const byte WfZoom = 0x08;

        // TWindow number constants
        public const short WnNoNumber = 0;

        // TScrollBar part codes
        public const ushort SbLeftArrow = 0;
        public const ushort SbRightArrow = 1;
        public const ushort SbPageLeft = 2;
        public const ushort SbPageRight = 3;
        public const ushort SbUpArrow = 4;
        public const ushort SbDownArrow = 5;
        public const ushort SbPageUp = 6;
        public const ushort SbPageDown = 7;
        public const ushort SbIndicator = 8;

        // TScrollBar options
        public const ushort SbHorizontal = 0x000;
        public const ushort SbVertical = 0x001;
        public const ushort SbHandleKeyboard = 0x002;

        // File commands
        public const ushort CmFileOpen = 1001;
        public const ushort CmFileReplace = 1002;
        public const ushort CmFileClear = 1003;
        public const ushort CmFileInit = 1004;
        public const ushort CmChangeDir = 1005;
        public const ushort CmRevert = 1006;
        public const ushort CmFileFocused = 102;
        public const ushort CmFileDoubleClicked = 103;

        // Edit commands
        public const int CmFind = 82;
        public const int CmReplace = 83;
        public const int CmSearchAgain = 84;
        public const int CmCharLeft = 500;
        public const int CmCharRight = 501;
        public const int CmWordLeft = 502;
        public const int CmWordRight = 503;
        public const int CmLineStart = 504;
        public const int CmLineEnd = 505;
        public const int CmLineUp = 506;
        public const int CmLineDown = 507;
        public const int CmPageUp = 508;
        public const int CmPageDown = 509;
        public const int CmTextStart = 510;
        public const int CmTextEnd = 511;
        public const int CmNewLine = 512;
        public const int CmBackSpace = 513;
        public const int CmDelChar = 514;
        public const int CmDelWord = 515;
        public const int CmDelStart = 516;
        public const int CmDelEnd = 517;
        public const int CmDelLine = 518;
        public const int CmInsMode = 519;
        public const int CmStartSelect = 520;
        public const int CmHideSelect = 521;
        public const int CmIndentMode = 522;
        public const int CmUpdateTitle = 523;
        public const int CmSelectAll = 524;
        public const int CmDelWordLeft = 525;
        public const int CmEncoding = 526;

        // Event masks
        public const int PositionalEvents = 0x002f;
        public const int FocusedEvents = 0x0110;
    }

    public class TCommandSet
    {
        private byte[] _cmds = new byte[32];
        private static readonly int[] Masks = { 1, 2, 4, 8, 0x10, 0x20, 0x40, 0x80 };

        public TCommandSet() { }

        public TCommandSet(params ushort[] cmds)
        {
            if (cmds != null)
                foreach (ushort c in cmds)
                    EnableCmd(c);
        }

        public TCommandSet(TCommandSet other)
        {
            Array.Copy(other._cmds, _cmds, 32);
        }

        public bool Has(int cmd)
        {
            if (cmd < 0 || cmd >= 256) return false;
            return (_cmds[cmd / 8] & Masks[cmd & 7]) != 0;
        }

        public void EnableCmd(int cmd)
        {
            if (cmd >= 0 && cmd < 256)
                _cmds[cmd / 8] |= (byte)Masks[cmd & 7];
        }

        public void DisableCmd(int cmd)
        {
            if (cmd >= 0 && cmd < 256)
                _cmds[cmd / 8] &= (byte)~Masks[cmd & 7];
        }

        public void EnableCmd(TCommandSet cmds)
        {
            for (int i = 0; i < 32; i++)
                _cmds[i] |= cmds._cmds[i];
        }

        public void DisableCmd(TCommandSet cmds)
        {
            for (int i = 0; i < 32; i++)
                _cmds[i] &= (byte)~cmds._cmds[i];
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < 32; i++)
                if (_cmds[i] != 0) return false;
            return true;
        }

        public static TCommandSet operator |(TCommandSet a, TCommandSet b)
        {
            var result = new TCommandSet();
            for (int i = 0; i < 32; i++)
                result._cmds[i] = (byte)(a._cmds[i] | b._cmds[i]);
            return result;
        }

        public static TCommandSet operator &(TCommandSet a, TCommandSet b)
        {
            var result = new TCommandSet();
            for (int i = 0; i < 32; i++)
                result._cmds[i] = (byte)(a._cmds[i] & b._cmds[i]);
            return result;
        }

        public TCommandSet AndAssign(TCommandSet other)
        {
            for (int i = 0; i < 32; i++)
                _cmds[i] &= other._cmds[i];
            return this;
        }

        public TCommandSet OrAssign(TCommandSet other)
        {
            for (int i = 0; i < 32; i++)
                _cmds[i] |= other._cmds[i];
            return this;
        }

        public static bool operator ==(TCommandSet a, TCommandSet b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            for (int i = 0; i < 32; i++)
                if (a._cmds[i] != b._cmds[i]) return false;
            return true;
        }

        public static bool operator !=(TCommandSet a, TCommandSet b) => !(a == b);

        public static TCommandSet operator +(TCommandSet s, int cmd)
        {
            s.EnableCmd(cmd);
            return s;
        }

        public static TCommandSet operator -(TCommandSet s, int cmd)
        {
            s.DisableCmd(cmd);
            return s;
        }
    }
}
