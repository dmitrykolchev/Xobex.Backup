namespace TermOut;

public struct ConsoleCell
{
    public ConsoleCell(char ch, Color fore, Color back)
    {
        Ch = ch;
        Fore = fore;
        Back = back;
    }
    public char Ch;
    public Color Fore;
    public Color Back;
}
