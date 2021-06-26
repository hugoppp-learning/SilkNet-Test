using Silk.NET.OpenGL;

namespace Lib.Render
{

public interface IHasVertexAttribPointer
{
    public void VertexAttributePointer(uint index, int size, VertexAttribPointerType type, uint stride, int offset);
}

}