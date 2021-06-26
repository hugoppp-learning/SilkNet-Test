using Leopotam.Ecs;
using Lib.Render;

namespace Lib.Systems
{

public class FpsProcessor : IEcsRunSystem
{
    private readonly RenderInfo _renderInfo = null!;
    private int _count;
    private float _fps;
    private int _sum;

    public int UpdateTime { get; set; } = 333;
    public bool PrintToConsole { get; set; } = true;

    public void Run()
    {
        _sum += _renderInfo.Delta.Milliseconds;
        _count++;

        if (_sum > UpdateTime)
        {
            _fps = (float) _count / _sum * 1000;
            _sum = 0;
            _count = 0;
            _renderInfo.FPS = _fps;
        }
    }
}

}