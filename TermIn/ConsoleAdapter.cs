using System.Text;

namespace TermIn;

public abstract class ConsoleAdapter: IDisposable
{
    private bool _disposed;

    protected ConsoleAdapter()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Stream baseStream = Console.OpenStandardOutput(128 * 1024);
        Encoding noBomEncoding = new UTF8Encoding(false);
        Writer = new StreamWriter(baseStream, noBomEncoding, 128 * 1024);
    }

    protected TextWriter Writer { get; }

    public void Write(string text)
    {
        Writer.Write(text);
    }

    public int WindowWidth => Console.WindowWidth;

    public int WindowHeight => Console.WindowHeight;

    public void WriteLine(string text)
    {
        Writer.Write(text);
        Writer.Write("\r\n");
    }

    public void Flush()
    {
        Writer.Flush();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ConsoleAdapter()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
