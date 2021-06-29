using System;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class VertexArrayObject<TVertexType, TIndexType> : INeedsFormat, IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    internal readonly uint _handle;


    private static uint CurrentlyInUse = uint.MaxValue;

    public VertexArrayObject(BufferObject<TVertexType> vbo, BufferObject<TIndexType>? ebo = null)
    {
        unsafe
        {
            _handle = GlWrapper.Gl.CreateVertexArray();

            GlWrapper.Gl.VertexArrayVertexBuffer(_handle, 0, vbo._handle, 0, (uint) sizeof(TVertexType));
            if (ebo is not null)
                GlWrapper.Gl.VertexArrayElementBuffer(_handle, ebo._handle);
        }
    }

    public unsafe void Format(uint index, int size, VertexAttribType type, uint offset)
    {
        GlWrapper.Gl.EnableVertexArrayAttrib(_handle, index);
        GlWrapper.Gl.VertexArrayAttribFormat(_handle, index, size, type, false,  offset);
        GlWrapper.Gl.VertexArrayAttribBinding(_handle, index, 0);
    }

    public bool InUse => CurrentlyInUse == _handle;

    public void Dispose()
    {
        GlWrapper.Gl.DeleteVertexArray(_handle);
    }

    public void Bind()
    {
        if (InUse)
            return;

        CurrentlyInUse = _handle;
        GlWrapper.Gl.BindVertexArray(_handle);
    }
}

}