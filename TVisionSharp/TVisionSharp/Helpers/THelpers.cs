namespace TVision
{
    public static class TVMemMgr
    {
        public static void ResizeSafetyPool(int size = Config.DefaultSafetyPoolSize) { }
        public static int SafetyPoolExhausted() => 0;
    }

    public static class Resources
    {
        public static void InitResources() { }
        public static void ShutdownResources() { }
    }
}
