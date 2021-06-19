using System;
using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Silk.NET.Input;

namespace Game
{

public class PlayerControllerSystem : IEcsRunSystem
{
    private EcsFilter<Position, Speed, PlayerFlag> _playerFilter = null!;
    private Lib.Game game = null!;

    public float AccelerationRate { get; set; } = 0.01f;
    public float Friction { get; set; } = 0.1f;

    public void Run()
    {
        for (int i = 0; i < _playerFilter.GetEntitiesCount(); i++)
        {
            Vector3 acceleration = GetNormalizedMoveDirection() * AccelerationRate;
            ref var movementSpeed = ref _playerFilter.Get2(i).Value;
            movementSpeed = (movementSpeed + acceleration) * new Vector3(1 - Friction) ;

            //position
            _playerFilter.Get1(i).Value += movementSpeed;
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