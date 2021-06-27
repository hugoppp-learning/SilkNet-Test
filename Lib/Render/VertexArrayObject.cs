using System;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class VertexArrayObject<TVertexType, TIndexType> : IHasVertexAttribPointer, IDisposable
    where TVertexType : unmanaged
    where TIndexType : unmanaged
{
    internal readonly uint _handle;


    private uint _currentlyInUse = uint.MaxValue;

    public VertexArrayObject(BufferObject<TVertexType> vbo, BufferObject<TIndexType>? ebo = null)
    {
        unsafe
        {
            _handle = GlWrapper.Gl.CreateVertexArray();
            // Bind();

            GlWrapper.Gl.VertexArrayVertexBuffer(_handle, 0, vbo._handle, 0, (uint) sizeof(TVertexType));
            if (ebo is not null)
                GlWrapper.Gl.VertexArrayElementBuffer(_handle, ebo._handle);
            // vbo.Bind();
            // ebo?.Bind();
        }
    }

    public bool InUse => _currentlyInUse == _handle;

    public void Dispose()
    {
        GlWrapper.Gl.DeleteVertexArray(_handle);
    }

    /// <param name="size">Number of components. 1 >= size >= 4</param>
    /// <param name="stride">Element offset to next vertex</param>
    /// <param name="offset">Element offset from first component to this component</param>
    public unsafe void VertexAttributePointer(uint index, int size, VertexAttribPointerType type, uint stride, int offset)
    {
        GlWrapper.Gl.VertexAttribPointer(index, size, type, false, stride, (void*) offset);
        GlWrapper.Gl.EnableVertexAttribArray(index);
    }

    public void Bind()
    {
        if (InUse)
            return;

        _currentlyInUse = _handle;
        GlWrapper.Gl.BindVertexArray(_handle);
    }
}

}