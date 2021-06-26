using System.Numerics;
using Leopotam.Ecs;
using Lib;
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
            .Replace(new Speed())
            .Replace(new PlayerFlag())
            .Replace(new Texture("silk.png"))
            .Replace(new Name("Player"))
            .Replace(new QuadRenderer());

        RenderSystems.Add(new MyImGui());

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
                .Replace(new Position(new(0.015f * x, 0.015f * y, 0)))
                .Replace(texture)
                .Replace(new Scale(0.01f))
                .Replace(new QuadRenderer())
                .Replace(new Name("small block"))
                .Replace(new PlayerFlag())
                .Replace(new Speed())
                ;
        }
    }
}

}