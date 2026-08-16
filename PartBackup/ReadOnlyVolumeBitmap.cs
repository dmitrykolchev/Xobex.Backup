using System.Runtime.InteropServices;
using Windows.Win32.System.Ioctl;

namespace PartBackup;

internal unsafe ref struct ReadOnlyVolumeBitmap
{
    private byte* _ptr;
    private byte* _bitmap;
    private readonly long _length;
    public ReadOnlyVolumeBitmap(long length)
    {
        _ptr = (byte*)NativeMemory.Alloc((nuint)(sizeof(VOLUME_BITMAP_BUFFER) + length));
        nint offset = Marshal.OffsetOf<VOLUME_BITMAP_BUFFER>(nameof(VOLUME_BITMAP_BUFFER.Buffer));
        _bitmap = _ptr + offset;
        _length = length;
    }

    public long BitmapSize => ((VOLUME_BITMAP_BUFFER*)_ptr)->BitmapSize;

    public Span<byte> AsSpan()
    {
        var length = (sizeof(VOLUME_BITMAP_BUFFER) + _length);
        if(length > int.MaxValue || length < 0)
        {
            throw new InvalidOperationException("buffer too long");
        }
        return new Span<byte>(_ptr, (int)length);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lcn">logical cluster number</param>
    /// <returns></returns>
    public bool IsAllocated(nuint lcn)
    {
        nuint offset = lcn / 8;
        byte mask = (byte)(1 << (int)(lcn % 8));
        return (*(_bitmap + offset) & mask) != 0;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lcn">logical cluster number</param>
    /// <returns></returns>
    public bool IsEmpty(nuint lcn)
    {
        nuint offset = lcn / 8;
        byte mask = (byte)(1 << (int)(lcn % 8));
        return (*(_bitmap + offset) & mask) == 0;
    }

    public void Dispose()
    {
        var ptr = _ptr;
        _ptr = null;
        _bitmap = null;
        if(ptr != null)
        {
            NativeMemory.Free(ptr);
        }
    }
}
