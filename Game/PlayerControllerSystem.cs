using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Silk.NET.Input;

namespace Game
{
    public class PlayerControllerSystem : IEcsRunSystem
    {
        private EcsFilter<Position, PlayerFlag> _playerFilter = null!;
        private Lib.Game game = null!;
        Vector3 movementSpeed = new(0.01f);

        public void Run()
        {
            for (int i = 0; i < _playerFilter.GetEntitiesCount(); i++)
            {
                Move(ref _playerFilter.Get1(i).Value);
            }
        }

        private void Move(ref Vector3 pos)
        {
            if (game.Input.Keyboards[0].IsKeyPressed(Key.W))
                pos += Vector3.UnitY * movementSpeed;
            if (game.Input.Keyboards[0].IsKeyPressed(Key.S))
                pos -= Vector3.UnitY * movementSpeed;
            if (game.Input.Keyboards[0].IsKeyPressed(Key.A))
                pos -= Vector3.UnitX * movementSpeed;
            if (game.Input.Keyboards[0].IsKeyPressed(Key.D))
                pos += Vector3.UnitX * movementSpeed;
        }
    }
}