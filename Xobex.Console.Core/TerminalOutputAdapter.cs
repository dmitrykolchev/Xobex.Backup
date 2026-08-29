// <copyright file="TerminalOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.CompilerServices;
using Xobex.Console.Abstractions;

namespace Xobex.Console;

public abstract class TerminalOutputAdapter : ITerminalOutputAdapter
{
    private bool _disposed;

    protected TerminalOutputAdapter(TextWriter writer)
    {
        Writer = writer;
        DisableWrap();
    }

    public int Width => System.Console.WindowWidth;

    public int Height => System.Console.WindowHeight;

    protected TextWriter Writer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
    }

    public virtual void Write(string text)
    {
        Writer.Write(text);
    }

    public virtual void Write(char ch)
    {
        Writer.Write(ch);
    }

    public virtual void WriteLine()
    {
        Writer.Write("\r\n");
    }

    public virtual void WriteLine(string text)
    {
        Write(text);
        WriteLine();
    }

    public virtual void Flush()
    {
        Writer.Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForeColor(Color color)
    {
        Write(Escapes.SetForeColor(color));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBackColor(Color color)
    {
        Write(Escapes.SetBackColor(color));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetColor(TerminalColor bg, TerminalColor fg)
    {
        Write(Escapes.SetColor(bg, fg));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetForeColor(TerminalColor fg)
    {
        Write(Escapes.SetForeColor(fg));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBackColor(TerminalColor bg)
    {
        Write(Escapes.SetBackColor(bg));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SaveCursorPosition()
    {
        Write(Escapes.SaveCursorPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RestoreCursorPosition()
    {
        Write(Escapes.RestoreCursorPosition);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void HideCursor()
    {
        Write(Escapes.HideCursor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ShowCursor()
    {
        Write(Escapes.ShowCursor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DisableWrap()
    {
        Write(Escapes.DisableWrap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnableWrap()
    {
        Write(Escapes.EnableWrap);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void HomeCursor()
    {
        Write(Escapes.HomeCursor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetColor()
    {
        Write(Escapes.ResetColor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCursorPosition(int x, int y)
    {
        Write(Escapes.SetCursorPosition(x, y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorUp(int rows)
    {
        Write(Escapes.MoveCursorUp(rows));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorDown(int rows)
    {
        Write(Escapes.MoveCursorDown(rows));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorRight(int cols)
    {
        Write(Escapes.MoveCursorRight(cols));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveCursorLeft(int cols)
    {
        Write(Escapes.MoveCursorLeft(cols));
    }

    public void AlternateScreen(bool on)
    {
        if (on)
        {
            Write(Escapes.AlternateScreenOn);
        }
        else
        {
            Write(Escapes.AlternateScreenOff);
        }
    }

    public void SetTextStyle(TextStyle style)
    {
        switch (style)
        {
            case TextStyle.None:
                ResetColor();
                break;
            case TextStyle.Bold:
                Write(Escapes.BoldText);
                break;
            case TextStyle.Dimmed:
                Write(Escapes.DimText);
                break;
            case TextStyle.Italic:
                Write(Escapes.ItalicText);
                break;
            case TextStyle.Underline:
                Write(Escapes.UnderlineText);
                break;
            case TextStyle.Invers:
                Write(Escapes.InversText);
                break;
            case TextStyle.Blink:
                Write(Escapes.BlinkText);
                break;
            case TextStyle.Hidden:
                Write(Escapes.HiddenText);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(style));
        }
    }

    protected virtual void Reset()
    {

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }
            Reset();
            _disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TerminalOutputAdapter()
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
