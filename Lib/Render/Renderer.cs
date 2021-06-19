using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class Renderer : IEcsRunSystem, IEcsInitSystem
{
    //Injected
    private EcsWorld World = null!;
    private readonly Game _game = null!;
    private readonly RenderInfo _renderInfo = null!;
    private EcsFilter<Texture, Position, Mesh> _filter = null!;

    private readonly Dictionary<Mesh, VertexArrayObject<float, uint>> VAOs = new();
    private Shader? _shader;


    // private VertexArrayObject<float, uint> Vao;

    private Shader Shader => _shader ?? throw new InvalidOperationException($"{nameof(Shader)} was not initialized");

    public void Init()
    {
        _game.Window.Resize += vector2D => GlWrapper.Gl.Viewport(vector2D);

        // Vbo = new BufferObject<float>(Vertices, BufferTargetARB.ArrayBuffer);
        // Vao = new VertexArrayObject<float, uint>(Vbo);


        _shader = new Shader("shader.vert", "shader.frag");

        GlWrapper.Gl.ClearColor(Color.SkyBlue);
    }

    Stopwatch sw = new();

    public void Run()
    {
        sw.Restart();
        Render();
        sw.Stop();
        _renderInfo.CPUTime = sw.Elapsed.TotalSeconds;
    }

    private VertexArrayObject<float, uint> GetVAO(Mesh mesh)
    {
        if (VAOs.TryGetValue(mesh, out var val))
            return val;

        var vbo = new BufferObject<float>(mesh.AsSpan(), BufferTargetARB.ArrayBuffer);
        var vao = new VertexArrayObject<float, uint>(vbo);

        vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
        vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

        return VAOs[mesh] = vao;
    }

    public void OnClose()
    {
        Shader.Dispose();
    }

    public void Render(Mesh mesh, in Matrix4x4 transform, Texture texture)
    {
        Render(mesh, transform, texture, Vector4.One);
    }

    public void Render(Mesh mesh, in Matrix4x4 transform, Vector4 color)
    {
        Render(mesh, transform, Texture.Empty, color);
    }

    public void Render(Mesh mesh, in Matrix4x4 transform, Texture texture, Vector4 color)
    {
        GetVAO(mesh).Bind();
        texture.Bind();
        Shader.SetUniform("uModel", Matrix4x4.Identity * transform);
    }

    private void Render()
    {
        _renderInfo.DrawCalls = 0;
        GlWrapper.Gl.Clear((uint) ClearBufferMask.ColorBufferBit);

        Shader.Use();

        foreach (int i in _filter)
        {
            ref Texture tex = ref _filter.Get1(i);
            ref Position pos = ref _filter.Get2(i);
            ref Mesh mesh = ref _filter.Get3(i);

            GetVAO(mesh).Bind();

            tex.Bind();
            Shader.SetUniform("uModel", Matrix4x4.Identity * Matrix4x4.CreateTranslation(pos.Value));
            GlWrapper.Gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            _renderInfo.DrawCalls++;
        }
    }
}

}