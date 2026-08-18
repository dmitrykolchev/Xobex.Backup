namespace TermOut;

public struct ConsoleCell
{
    public ConsoleCell(char ch, Color fore, Color back, TextStyle st = TextStyle.None)
    {
        Ch = ch;
        Fg = fore;
        Bg = back;
        St = st;
    }
    public char Ch;
    public TextStyle St;
    public Color Fg;
    public Color Bg;
}
