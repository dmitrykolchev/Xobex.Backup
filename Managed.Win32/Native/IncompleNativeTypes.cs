using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Managed.Win32.Native;

[StructLayout(LayoutKind.Sequential)]
public struct _CONTEXT { }

[StructLayout(LayoutKind.Sequential)]
public struct _PROC_THREAD_ATTRIBUTE_LIST { }

[StructLayout(LayoutKind.Explicit)]
public unsafe struct _FILE_SEGMENT_ELEMENT
{
    [FieldOffset(0)]
    public void* Buffer;
    [FieldOffset(0)]
    public ulong Alignment;
}


[StructLayout(LayoutKind.Sequential)]
public struct _PROCESSOR_NUMBER
{
    public ushort Group;
    public byte Number;
    public byte Reserved;
}