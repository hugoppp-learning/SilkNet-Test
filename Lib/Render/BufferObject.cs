using System;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    internal readonly uint _handle;

    public unsafe BufferObject(ReadOnlySpan<TDataType> data, BufferTargetARB bufferType)
    {
        _handle = GlWrapper.Gl.CreateBuffer();
        fixed (void* d = data)
        {
            GlWrapper.Gl.NamedBufferStorage(_handle, (nuint) (data.Length * sizeof(TDataType)), d, BufferStorageMask.DynamicStorageBit);
        }
    }

    public unsafe BufferObject(int vertexCount, BufferTargetARB bufferType)
    {
        _handle = GlWrapper.Gl.CreateBuffer();
        GlWrapper.Gl.NamedBufferStorage(_handle, (nuint) (vertexCount * sizeof(TDataType)), null, BufferStorageMask.DynamicStorageBit);
    }

    public void Dispose()
    {
        GlWrapper.Gl.DeleteBuffer(_handle);
    }

    // public void Bind()
    // {
    // GlWrapper.Gl.BindBuffer(_bufferType, _handle);
    // }
}

}