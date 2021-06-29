using Silk.NET.OpenGL;

namespace Lib.Render
{

public interface INeedsFormat
{
    public void Format(uint index, int size, VertexAttribType type, uint offset);
}

}