// <copyright file="ITerminalParser.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

namespace Xobex.Console.Abstractions;

public interface ITerminalParser
{
    bool TryGetInputEvent(out InputEvent? ev);
}
