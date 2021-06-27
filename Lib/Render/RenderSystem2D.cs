using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class RenderSystem2D : IEcsRunSystem, IEcsInitSystem
{
    //inject
    private readonly Game _game = null!;
    private readonly RenderInfo _renderInfo = null!;

    private EcsFilter<Position, QuadRenderer> _filterQuads = null!;

    //init
    private Renderer _renderer = null!;

    Stopwatch _sw = new();

    public void Init()
    {
        _renderer = new Renderer(_renderInfo);
        _game.Window.Resize += vector2D => GlWrapper.Gl.Viewport(vector2D);


        // Vbo = new BufferObject<float>(Vertices, BufferTargetARB.ArrayBuffer);
        // Vao = new VertexArrayObject<float, uint>(Vbo);


        GlWrapper.Gl.ClearColor(Color.SkyBlue);
    }


    public void Run(double delta)
    {
        _sw.Restart();
        Render();
        _sw.Stop();
        _renderInfo.CPUTime = _sw.Elapsed;
    }


    private void Render()
    {
        _renderer.Begin();
        _renderInfo.DrawCalls = 0;
        GlWrapper.Gl.Clear((uint) ClearBufferMask.ColorBufferBit);

        foreach (int i in _filterQuads)
        {
            ref EcsEntity ecsEntity = ref _filterQuads.GetEntity(i);

            ref Position pos = ref _filterQuads.Get1(i);
            Texture texture;
            // Color color;

            if (ecsEntity.Has<Texture>())
                texture = ecsEntity.Get<Texture>();
            else
                texture = Texture.Empty;


            // if (!ecsEntity.Has<Color>())
            // texture = Color.White;

            if (ecsEntity.Has<StaticDraw>()) ; //todo

            Matrix4x4 transform = Matrix4x4.Identity;

            if (ecsEntity.Has<Scale>())
                transform *= Matrix4x4.CreateScale(ecsEntity.Get<Scale>().Value);

            transform *= Matrix4x4.CreateTranslation((Vector3) pos);

            _renderer.RenderQuadDynamic(in transform, texture);
        }

        _renderer.End();
    }
}

}