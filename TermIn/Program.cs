namespace TermIn;

internal class Program
{
    static void Main(string[] args)
    {
        LinuxInputAdapter ada = new();
        try
        {
            PosixInputReader reader = new PosixInputReader();
            bool done = false;
            while (!done)
            {
                reader.ReadEvents((e) =>
                {
                    ada.Writer.Write($"{e}\r\n");
                    if (e.EventType == InputEventType.Key)
                    {
                        if (e.KeyEvent.Key == ConsoleKey.Q)
                        {
                            done = true;
                        }
                    }
                });
            }
        }
        finally
        {
            ada.Reset();
        }
    }
}
