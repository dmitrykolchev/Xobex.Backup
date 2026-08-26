// <copyright file="ITerminalOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console.Abstractions;

public interface ITerminalOutputAdapter: IDisposable
{
    int Width { get; }
    int Height { get; }
    void Write(char ch);

    void Write(string text);

    void WriteLine();

    void WriteLine(string text);

    void Flush();

    void SetForeColor(Color color);

    void SetBackColor(Color color);

    void SaveCursorPosition();

    void RestoreCursorPosition();

    void HideCursor();

    void ShowCursor();

    void DisableWrap();

    void EnableWrap();

    void HomeCursor();

    void ResetColor();

    void SetCursorPosition(int x, int y);

    void MoveCursorUp(int rows);

    void MoveCursorDown(int rows);

    void MoveCursorRight(int cols);

    void MoveCursorLeft(int cols);

    void AlternateScreen(bool on);

    void SetTextStyle(TextStyle style);
}
