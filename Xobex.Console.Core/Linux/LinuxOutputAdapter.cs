// <copyright file="LinuxOutputAdapter.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text;

namespace Xobex.Console.Linux;

public class LinuxOutputAdapter : TerminalOutputAdapter
{
    public LinuxOutputAdapter(TextWriter writer) : base(writer)
    {
    }

    public static LinuxOutputAdapter Create(int bufferSize = 128 * 1024)
    {
        System.Console.OutputEncoding = Encoding.UTF8;
        var baseStream = System.Console.OpenStandardOutput(bufferSize);
        Encoding noBomEncoding = new UTF8Encoding(false);
        var writer = new StreamWriter(baseStream, noBomEncoding, bufferSize);
        return new LinuxOutputAdapter(writer);
    }
}
