using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;

namespace Game
{

    internal class Program : Lib.Game
    {
        public override void OnAfterLoad()
        {
            World.NewEntity()
                .Replace(new Position())
                .Replace(new Texture("silk.png"))
                .Replace(Mesh.Sprite);
        }

        private static void Main()
        {
            new Program().Run();
        }
    }
}