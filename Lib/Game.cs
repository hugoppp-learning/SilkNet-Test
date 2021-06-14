using System;
using Leopotam.Ecs;
using Lib.Render;
using Lib.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Lib
{

public abstract class Game
{
    private readonly RenderInfo _renderInfo = new();
    private readonly UpdateInfo _updateInfo = new();
    private IInputContext? _input;

    public Game()
    {
        var options = WindowOptions.Default;
        options.Title = "Moin Moin";
        options.Size = new Vector2D<int>(1280, 720);
        options.API = GraphicsAPI.Default;

        options.UpdatesPerSecond = 30.0;
        // options.FramesPerSecond = 300.0;

        options.ShouldSwapAutomatically = true;
        options.VSync = false;

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += OnLoad;
        Window.Update += OnUpdate;
        Window.Closing += OnClosing;
        Window.Render += WindowOnRender;
    }

    public EcsWorld World { get; } = new();

    public EcsSystems GameSystems { get; private set; } = null!;

    public EcsSystems RenderSystems { get; private set; } = null!;

    public IInputContext Input => _input ?? throw new InvalidOperationException("Input not yet created");
    internal IWindow Window { get; set; }


    public event Action<IKeyboard, Key, int>? KeyDown;


    public virtual void OnBeforeEcsSystemInit()
    {
    }

    public void Run()
    {
        Window.Run();
    }

    private void WindowOnRender(double delta)
    {
        _renderInfo.Delta = delta;
        RenderSystems.Run();
    }

    private void OnClosing()
    {
        GameSystems.Destroy();
        RenderSystems.Destroy();
    }

    private void OnUpdate(double delta)
    {
        _updateInfo.Delta = delta;

        GameSystems.Run();
    }


    private void OnLoad()
    {
        _input = Window.CreateInput();
        for (int i = 0; i < _input.Keyboards.Count; i++)
            _input.Keyboards[i].KeyDown += (keyboard, key, arg3) => KeyDown?.Invoke(keyboard, key, arg3);

        GlWrapper.Init(Window);

        GameSystems = new EcsSystems(World, "Fixed Update")
            .Inject(_updateInfo)
            .Inject(this);

        RenderSystems = new EcsSystems(World, "Frame Update")
            .Add(new Renderer(), "Renderer")
            .Add(new FpsProcessor())
            .Inject(_renderInfo)
            .Inject(this);


        OnBeforeEcsSystemInit();
        GameSystems.Init();
        RenderSystems.Init();
        OnAfterEcsSystemInit();
    }

    public virtual void OnAfterEcsSystemInit()
    {
    }

    public void Exit()
    {
        Window.Close();
    }
}

}