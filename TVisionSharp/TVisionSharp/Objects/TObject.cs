namespace TVision
{
    public class TObject
    {
        public virtual void ShutDown() { }

        public static void Destroy(TObject o)
        {
            o?.ShutDown();
        }
    }
}
