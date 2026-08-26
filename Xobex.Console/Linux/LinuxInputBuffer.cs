// <copyright file="InputBuffer.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Diagnostics;

namespace Xobex.Console.Linux;

internal class LinuxInputBuffer
{
    private readonly LinuxInputAdapter _conIn;
    private readonly Queue<InputToken> _queue = new();
    private long _lastReadTime = Stopwatch.GetTimestamp();
    private bool _separatorWritten = true;

    public LinuxInputBuffer(LinuxInputAdapter conIn)
    {
        _conIn = conIn;
    }

    public InputToken NextToken()
    {
        if (_queue.Count > 0)
        {
            return _queue.Dequeue();
        }
        ReadRawInput();
        return _queue.Dequeue();
    }

    private void ReadRawInput()
    {
        var spinWait = new SpinWait();
        Span<byte> data = stackalloc byte[256];
        for (; ; )
        {
            if (!_conIn.HasInput())
            {
                var ts = Stopwatch.GetElapsedTime(_lastReadTime);
                if (ts.TotalMilliseconds > 25 && !_separatorWritten)
                {
                    _queue.Enqueue(new InputToken(InputTokenType.Separator, 0));
                    _separatorWritten = true;
                    break;
                }
                spinWait.SpinOnce();
            }
            else
            {
                var readCount = _conIn.Read(data);
                _separatorWritten = false;
                _lastReadTime = Stopwatch.GetTimestamp();
                for (var i = 0; i < readCount; ++i)
                {
                    var text = data[i] switch
                    {
                        0x01 => InputTokenType.SOH,
                        0x02 => InputTokenType.STX,
                        0x03 => InputTokenType.ETX,
                        0x04 => InputTokenType.EOT,
                        0x05 => InputTokenType.ENQ,
                        0x06 => InputTokenType.ACK,
                        0x07 => InputTokenType.BEL,
                        0x08 => InputTokenType.BS,
                        0x09 => InputTokenType.HT,
                        0x0A => InputTokenType.LF,
                        0x0B => InputTokenType.VT,
                        0x0C => InputTokenType.FF,
                        0x0D => InputTokenType.CR,
                        0x0E => InputTokenType.SO,
                        0x0F => InputTokenType.SI,
                        0x10 => InputTokenType.DLE,
                        0x11 => InputTokenType.DC1,
                        0x12 => InputTokenType.DC2,
                        0x13 => InputTokenType.DC3,
                        0x14 => InputTokenType.DC4,
                        0x15 => InputTokenType.NAK,
                        0x16 => InputTokenType.SYN,
                        0x17 => InputTokenType.ETB,
                        0x18 => InputTokenType.CAN,
                        0x19 => InputTokenType.EM,
                        0x1A => InputTokenType.SUB,
                        0x1B => InputTokenType.ESC,
                        0x1C => InputTokenType.IS4,
                        0x1D => InputTokenType.IS3,
                        0x1E => InputTokenType.IS2,
                        0x1F => InputTokenType.IS1,
                        0x20 => InputTokenType.SP,
                        >= (byte)'0' and <= (byte)'9' => InputTokenType.Digit,
                        >= (byte)'A' and <= (byte)'Z' => InputTokenType.UpperCase,
                        >= (byte)'a' and <= (byte)'z' => InputTokenType.LowerCase,
                        < 0x7F => InputTokenType.Symbol,
                        0x7F => InputTokenType.DEL,
                        > 0x7F => InputTokenType.Char8Bit
                    };
                    _queue.Enqueue(new InputToken(text, data[i]));
                }
                break;
            }
        }
    }
}
