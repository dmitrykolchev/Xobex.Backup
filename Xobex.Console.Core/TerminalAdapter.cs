// <copyright file="TerminalAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using Xobex.Console.Abstractions;
using Xobex.Console.Linux;
using Xobex.Console.Windows;

namespace Xobex.Console;

public class TerminalAdapter : IDisposable
{
    public TerminalAdapter(ITerminalInputAdapter conIn, ITerminalOutputAdapter conOut)
    {
        In = conIn;
        Out = conOut;
    }

    public ITerminalInputAdapter In { get; }
    public ITerminalOutputAdapter Out { get; }

    public static TerminalAdapter Create()
    {
        if (OperatingSystem.IsWindows())
        {
            var conOut = WindowsOutputAdapter.Create();
            var conIn = WindowsInputAdapter.Create();
            return new TerminalAdapter(conIn, conOut);
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var conOut = LinuxOutputAdapter.Create();
            var conIn = LinuxInputAdapter.Create(conOut);
            return new TerminalAdapter(conIn, conOut);
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
    }

    public void Dispose()
    {
        In.Dispose();
        Out.Dispose();
    }
}
