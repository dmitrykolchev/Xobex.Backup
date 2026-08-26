namespace Xobex.Console;

public readonly struct InputToken
{
    public InputToken(InputTokenType tokenType, byte ch)
    {
        TokenType = tokenType;
        Ch = ch;
    }

    public InputTokenType TokenType { get; }

    public byte Ch { get; }

    public override string ToString()
    {
        return $"{TokenType}:{Ch:x2}";
    }
}
