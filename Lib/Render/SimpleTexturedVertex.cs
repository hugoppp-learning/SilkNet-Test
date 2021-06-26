using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace Lib.Render
{

/// <summary>
///     Provide an implementation for the static method, it will be invoked using reflection
/// </summary>
internal interface IVertex
{
    public static void SetLayout(IHasVertexAttribPointer v)
    {
        throw new NotImplementedException();
    }
}

public readonly struct SimpleTexturedVertex : IVertex
{
    public readonly Vector3 Coord;
    public readonly Vector2 UvCoord;

    public SimpleTexturedVertex(in Vector3 coord, in Vector2 uvCoord)
    {
        (Coord, UvCoord) = (coord, uvCoord);
    }

    public static void SetLayout(IHasVertexAttribPointer v)
    {
        uint stride;
        unsafe
        {
            stride = (uint) sizeof(SimpleTexturedVertex);
        }

        v.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, stride,
            (int) Marshal.OffsetOf(typeof(SimpleTexturedVertex), nameof(Coord)));
        v.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, stride,
            (int) Marshal.OffsetOf(typeof(SimpleTexturedVertex), nameof(UvCoord)));
    }
}

}