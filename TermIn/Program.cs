using System.Text;

namespace TermIn;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            using LinuxConsoleAdapter ada = new();
            for (; ; )
            {
                if (ada.GetEvent(out InputEvent ev))
                {
                    ada.WriteLine(ev.ToString());
                    ada.Flush();
                    if (ev.RawData[0] == (byte)'q')
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

}
