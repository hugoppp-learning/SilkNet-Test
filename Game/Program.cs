using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;
using Silk.NET.Maths;

namespace Game
{

public struct PlayerFlag : IEcsIgnoreInFilter
{
}


internal class Program : Lib.Game
{
    public override void OnBeforeEcsSystemInit()
    {
        World.NewEntity()
            .Replace(new Position())
            .Replace(new PlayerFlag())
            .Replace(new Texture("silk.png"))
            .Replace(Mesh.Sprite);

        GameSystems
            .Add(new PlayerControllerSystem(), "Player Controller")
            .Add(new GameControllerSystem())
            ;
    }

    public override void OnAfterEcsSystemInit()
    {
        LoadLevel();
    }

    private static void Main()
    {
        new Program().Run();
    }

    private void LoadLevel()
    {
        var texture = new Texture("silk.png");

        Vector2D<int> max = new(50, 50);
        Vector2D<int> min = new(-50, -50);

        for (int x = min.X; x < max.X; x++)
        for (int y = min.Y; y < max.Y; y++)
        {
            World.NewEntity()
                .Replace(new Position(new(0.05f * x, 0.05f * y, 0)))
                .Replace(texture)
                .Replace(Mesh.Sprite.Transform(Matrix4x4.CreateScale(0.04f)))
                ;
        }
    }
}

}