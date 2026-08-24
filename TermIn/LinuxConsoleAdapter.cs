namespace TermIn;

public class LinuxConsoleAdapter : ConsoleAdapter
{
    private readonly LinuxInputAdapter _inputAdapter;
    private readonly AnsiParser _parser;

    public LinuxConsoleAdapter() : base()
    {
        _inputAdapter = new LinuxInputAdapter(this);
        _parser = new AnsiParser(_inputAdapter, this);
    }

    protected override void Dispose(bool disposing)
    {
        if(disposing)
        {
            _inputAdapter.Dispose();
        }
        base.Dispose(disposing);
    }

    public bool HasInput()
    {
        return _inputAdapter.HasInput();
    }

    public bool GetEvent(out InputEvent ev)
    {
        if(_parser.TryParseEvent(out ev) ==  AnsiParser.ParseResult.Accepted)
        {
            return true;
        }
        ev = default;
        return false;
    }

    //private Queue<ConsoleEvent> _events = new ();

    //public bool GetInputEvent(out ConsoleEvent ev)
    //{
    //    if(_events.Count > 0 )
    //    {
    //        ev = _events.Dequeue();
    //        return true;
    //    }
    //    ev = default;
    //    byte* buffer = stackalloc byte[64];
    //    nint bytesRead = read(STDIN_FILENO, buffer, 64);
    //    if (bytesRead <= 0)
    //    {
    //        return false;
    //    }
    //    List<ConsoleEvent> evs = AdvancedInputDecoder.ParseBuffer(new ReadOnlySpan<byte>(buffer, (int)bytesRead));
    //    foreach (var item in evs)
    //    {
    //        _events.Enqueue(item);
    //    }
    //    ev = _events.Dequeue();
    //    return true;

    //    //ReadOnlySpan<byte> data = new(buffer, (int)bytesRead);
    //    //StringBuilder sb = new StringBuilder();
    //    //foreach (byte b in data)
    //    //{
    //    //    sb.Append(b < 32 ? $"\\x{b:X2}" : (char)b);
    //    //}

    //    //Writer.Write($"\'{sb.ToString()}\' ({BitConverter.ToString(data.ToArray())})\r\n");
    //    //Writer.Flush();
    //    //if (buffer[0] == (byte)'q')
    //    //{
    //    //    return false;
    //    //}
    //    return true;
    //}
}
