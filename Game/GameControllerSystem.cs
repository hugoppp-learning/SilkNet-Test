using Leopotam.Ecs;
using Silk.NET.Input;

namespace Game
{
    public class GameControllerSystem : IEcsInitSystem
    {
        private Lib.Game game = null!;

        public void Init()
        {
            game.KeyDown += (keyboard, key, arg3) =>
            {
                if (key == Key.Escape)
                    game.Exit();
            };
        }
    }
}