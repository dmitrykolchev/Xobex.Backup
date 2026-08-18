namespace TermIn;

internal class Program
{
    static void Main(string[] args)
    {
        LinuxInputAdapter ada = new();

        for(; ; )
        {
            if(!ada.GetInputEvent(out var _))
            {
                break;
            }
        }
        ada.Reset();
    }
}
