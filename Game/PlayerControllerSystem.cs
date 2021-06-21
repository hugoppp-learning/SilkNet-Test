using System;
using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Silk.NET.Input;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace Game
{

public class PlayerControllerSystem : IEcsRunSystem
{
    private EcsFilter<Position, Speed, PlayerFlag> _playerFilter = null!;
    private Lib.Game game = null!;

    public float AccelerationRate { get; set; } = 0.2f;
    public float DecelerationRate { get; set; } = 0.7f;
    public Vector3 MaximumSpeed = new(0.1f);

    public void Run()
    {
        for (int i = 0; i < _playerFilter.GetEntitiesCount(); i++)
        {
            Vector3 direction = GetNormalizedMoveDirection();

            ref var movementSpeed = ref _playerFilter.Get2(i).Value;

            if (direction.Length() == 0)
                movementSpeed += -movementSpeed * DecelerationRate;
            else
                movementSpeed += (direction * MaximumSpeed - movementSpeed) * AccelerationRate;


            ref Vector3 position = ref _playerFilter.Get1(i).Value;
            position += movementSpeed;
        }
    }

    private Vector3 GetNormalizedMoveDirection()
    {
        Vector3 dir = Vector3.Zero;

        if (game.Input.Keyboards[0].IsKeyPressed(Key.W))
            dir.Y += 1;
        if (game.Input.Keyboards[0].IsKeyPressed(Key.S))
            dir.Y -= 1;
        if (game.Input.Keyboards[0].IsKeyPressed(Key.A))
            dir.X -= 1;
        if (game.Input.Keyboards[0].IsKeyPressed(Key.D))
            dir.X += 1;
        return dir.Length() > 0 ? Vector3.Normalize(dir) : dir;
    }
}

}