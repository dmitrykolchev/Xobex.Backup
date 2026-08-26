// <copyright file="ITerminalOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console.Abstractions;

public interface ITerminalOutputAdapter
{
    void Write(char ch);

    void Write(string text);

    void WriteLine();

    void WriteLine(string text);

    void Flush();

    IDisposable EnableMouseInput();

    void DisableMouseInput();
}
