using System;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class Renderer
{
    // VertexArrayObject<SimpleTexturedVertex, uint> _vao;

    private const int BufferSize = 6000;

    //todo list of batches for use with multiple textures
    private readonly NonIndexedBatch<SimpleTexturedVertex> _dynamicBatch = new(BufferSize);
    private readonly RenderInfo _renderInfo;
    private readonly Shader _shader;


    public Renderer(RenderInfo renderInfo)
    {
        _renderInfo = renderInfo;
        _shader = new Shader("shader.vert", "shader.frag");
    }


    public void Begin(in Matrix4x4 viewProjection)
    {
        _shader.Use();
        _shader.SetUniform("uVP", viewProjection);
        _renderInfo.DrawCalls = 0;
        GlWrapper.Clear();
    }

    public void End()
    {
        _dynamicBatch.Render(_shader, Texture.Empty);
    }

    public void RenderQuadDynamic(in Matrix4x4 transform, Texture texture)
    {
        RenderQuadDynamic(transform, texture, Vector4.One);
    }

    public void RenderQuadDynamic(in Matrix4x4 transform, Vector4 color)
    {
        RenderQuadDynamic(transform, Texture.Empty, color);
    }

    public void RenderQuadDynamic(in Matrix4x4 transform, Texture texture, Vector4 color)
    {
        if (_dynamicBatch.VertexBuffer.Count + 6 > _dynamicBatch.VertexBuffer.Capacity)
        {
            _dynamicBatch.Render(_shader, texture);
            _renderInfo.DrawCalls++;
        }

        Span<SimpleTexturedVertex> map = _dynamicBatch.VertexBuffer.AddViaMap(6);
        map[0] = new SimpleTexturedVertex(Vector3.Transform(new(0.0f, 1.0f, 0.0f), transform), new(0.0f, 1.0f));
        map[1] = new SimpleTexturedVertex(Vector3.Transform(new(1.0f, 0.0f, 0.0f), transform), new(1.0f, 0.0f));
        map[2] = new SimpleTexturedVertex(Vector3.Transform(new(0.0f, 0.0f, 0.0f), transform), new(0.0f, 0.0f));
        map[3] = new SimpleTexturedVertex(Vector3.Transform(new(0.0f, 1.0f, 0.0f), transform), new(0.0f, 1.0f));
        map[4] = new SimpleTexturedVertex(Vector3.Transform(new(1.0f, 1.0f, 0.0f), transform), new(1.0f, 1.0f));
        map[5] = new SimpleTexturedVertex(Vector3.Transform(new(1.0f, 0.0f, 0.0f), transform), new(1.0f, 0.0f));
    }


    private readonly struct NonIndexedBatch<TVertex> where TVertex : unmanaged, IVertex
    {
        public readonly FixedSizeBuffer<TVertex> VertexBuffer;

        public VertexArrayObject<TVertex, uint> VAO { get; }
        private BufferObject<TVertex> VBO { get; }

        public NonIndexedBatch(int batchSize)
        {
            VertexBuffer = new(batchSize);
            VBO = new BufferObject<TVertex>(batchSize, BufferTargetARB.ArrayBuffer);
            VAO = new VertexArrayObject<TVertex, uint>(VBO);

            //todo replace attribs with DSA equivalent
            VAO.Bind();
            GlWrapper.Gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO._handle);
            VertexFormatStore.Set(typeof(TVertex), VAO);
        }


        public void Render(Shader shader, Texture tex)
        {
            unsafe
            {
                VAO.Bind();

                shader.Use();
                tex.Bind();

                shader.SetUniform("uModel", Matrix4x4.Identity);
                GlWrapper.Gl.NamedBufferSubData(VBO._handle, 0, (nuint) (VertexBuffer.Items.Length * sizeof(TVertex)), VertexBuffer.Items);
                GlWrapper.Gl.DrawArrays(PrimitiveType.Triangles, 0, (uint) VertexBuffer.Count);

                Clear();
            }
        }

        private void Clear()
        {
            VertexBuffer.Clear();
        }
    }
}

}