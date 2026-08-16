// <copyright file="NativeTypeNameAttribute.cs" company="Dmitry Kolchev">
// Copyright (c) 2026 Dmitry Kolchev. All rights reserved.
// See LICENSE in the project root for license information
// </copyright>

using System.Runtime.InteropServices;

namespace Managed.Win32.Native;

[AttributeUsage(AttributeTargets.All)]
public sealed class NativeTypedefAttribute : Attribute
{
    public NativeTypedefAttribute() { }
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class InvalidHandleValueAttribute : Attribute
{
    public InvalidHandleValueAttribute([In] long Value)
    {
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct LUID
{
    public uint LowPart;

    public int HighPart;
}

[AttributeUsage(AttributeTargets.All)]
public class NativeTypeNameAttribute : Attribute
{
    public NativeTypeNameAttribute(string typeName) { }
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class NativeInheritanceAttribute : Attribute
{
    public NativeInheritanceAttribute(string name) { }
}
