// <copyright file="ITerminalInputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console.Abstractions;

public interface ITerminalInputAdapter : IDisposable
{
    ITerminalParser CreateParser();

    IDisposable EnableMouseInput();

    void DisableMouseInput();

    bool HasInput();

    bool HasInput(int timeoutMs);

    int Read(Span<byte> buffer);

    void Reset();
}
