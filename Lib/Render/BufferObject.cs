using System;
using Silk.NET.OpenGL;

namespace Lib.Render
{
    public class BufferObject<TDataType> : IDisposable
        where TDataType : unmanaged
    {
        private readonly BufferTargetARB _bufferType;
        private readonly uint _handle;

        public unsafe BufferObject(Span<TDataType> data, BufferTargetARB bufferType)
        {
            _bufferType = bufferType;

            _handle = GlWrapper.Gl.GenBuffer();
            Bind();
            fixed (void* d = data)
            {
                GlWrapper.Gl.BufferData(bufferType, (nuint) (data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
            }
        }

        public void Dispose()
        {
            GlWrapper.Gl.DeleteBuffer(_handle);
        }

        public void Bind()
        {
            GlWrapper.Gl.BindBuffer(_bufferType, _handle);
        }
    }
}