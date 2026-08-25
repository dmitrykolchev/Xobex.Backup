namespace TVision
{
    public static class EventCodes
    {
        public const int EvMouseDown = 0x0001;
        public const int EvMouseUp = 0x0002;
        public const int EvMouseMove = 0x0004;
        public const int EvMouseAuto = 0x0008;
        public const int EvMouseWheel = 0x0020;
        public const int EvKeyDown = 0x0010;
        public const int EvCommand = 0x0100;
        public const int EvBroadcast = 0x0200;
        public const int EvNothing = 0x0000;
        public const int EvMouse = 0x002f;
        public const int EvKeyboard = 0x0010;
        public const int EvMessage = 0xFF00;

        public const int MbLeftButton = 0x01;
        public const int MbRightButton = 0x02;
        public const int MbMiddleButton = 0x04;

        public const int MwUp = 0x01;
        public const int MwDown = 0x02;
        public const int MwLeft = 0x04;
        public const int MwRight = 0x08;

        public const int MeMouseMoved = 0x01;
        public const int MeDoubleClick = 0x02;
        public const int MeTripleClick = 0x04;
    }
}
