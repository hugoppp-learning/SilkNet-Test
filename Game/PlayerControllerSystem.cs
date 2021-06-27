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
    public Vector3 MaximumSpeed = new(0.15f);

    public float AccelerationRate { get; set; } = 5.2f;
    public float DecelerationRate { get; set; } = 1.7f;

    public void Run(double delta)
    {
        for (int i = 0; i < _playerFilter.GetEntitiesCount(); i++)
        {
            Vector3 direction = GetNormalizedMoveDirection();

            ref var movementSpeed = ref _playerFilter.Get2(i).Value;

            if (direction.Length() == 0)
                movementSpeed += -movementSpeed * (DecelerationRate * (float) delta);
            else
                movementSpeed += (direction * MaximumSpeed - movementSpeed) * (AccelerationRate * (float) delta);

            ref Vector3 position = ref _playerFilter.Get1(i).Value;
            position += movementSpeed * (float) delta;
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