// <copyright file="InputBuffer.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Text;

namespace TermIn;

public class InputBuffer
{
    private readonly LinuxInputAdapter _inputAdapter;
    private readonly LinuxConsoleAdapter _con;
    private readonly Queue<byte> _inputBuffer = new(1024);
    private readonly Stack<byte> _ungetBuffer = new(1024);
    private int _lastChar = -1;
    private readonly byte[] _rawBuffer = new byte[256];
    private int _rawBufferIndex;

    public InputBuffer(LinuxInputAdapter inputAdapter, LinuxConsoleAdapter con)
    {
        _inputAdapter = inputAdapter;
        _con = con;
    }

    public int GetChar()
    {
        if (_ungetBuffer.Count == 0 && _inputBuffer.Count == 0)
        {
            Span<byte> data = stackalloc byte[64];
            var read = _inputAdapter.Read(data);
            StringBuilder builder = new();
            for (var i = 0; i < read; ++i)
            {
                if (data[i] == 0)
                {
                    throw new InvalidOperationException("EOS");
                }
                _inputBuffer.Enqueue(data[i]);
                builder.Append(data[i] == 0x1B ? "^" : (data[i] < 32 ? $"\\x{data[i]:X2}" : (char)data[i]));
            }
            _con.WriteLine($"{builder}");
            _con.Flush();
        }
        _lastChar = -1;
        if (_ungetBuffer.Count != 0)
        {
            _lastChar = _ungetBuffer.Pop();
        }
        else if (_inputBuffer.Count != 0)
        {
            _lastChar = _inputBuffer.Dequeue();
        }
        if (_lastChar >= 0)
        {
            _rawBuffer[_rawBufferIndex++] = (byte)_lastChar;
        }
        return _lastChar;
    }

    public ReadOnlySpan<byte> GetRawBuffer()
    {
        var result = new ReadOnlySpan<byte>(_rawBuffer, 0, _rawBufferIndex);
        _rawBufferIndex = 0;
        return result;
    }

    public void Unget(byte data)
    {
        _ungetBuffer.Push(data);
    }

    public void Unget()
    {
        if (_lastChar >= 0)
        {
            _ungetBuffer.Push((byte)_lastChar);
            _lastChar = -1;
            _rawBufferIndex--;
            return;
        }
        throw new InvalidOperationException();
    }
}
