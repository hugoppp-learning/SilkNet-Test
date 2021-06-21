using System;
using ImGuiNET;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Leopotam.Ecs;
using Lib.Render;
using Lib.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Extensions.ImGui;
using Timer = System.Timers.Timer;

namespace Lib
{

public abstract class Game
{
    private readonly RenderInfo _renderInfo = new();
    private readonly UpdateInfo _updateInfo = new();
    private IInputContext? _input;
    private ImGuiController _imGuiController;

    public Game()
    {
        var options = WindowOptions.Default;
        options.Title = "Moin Moin";
        options.Size = new Vector2D<int>(1280, 720);
        options.API = GraphicsAPI.Default;

        options.UpdatesPerSecond = 30.0;
        // options.FramesPerSecond = 300.0;

        options.ShouldSwapAutomatically = false;
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
        _renderInfo.Delta = TimeSpan.FromSeconds(delta);
        _imGuiController.Update((float) delta);
        RenderSystems.Run();
        _imGuiController.Render();
        Window.SwapBuffers();
    }

    private void OnClosing()
    {
        GameSystems.Destroy();
        RenderSystems.Destroy();
        _imGuiController.Dispose();
    }

    private Stopwatch sw = new();
    private void OnUpdate(double delta)
    {
        _updateInfo.Delta = TimeSpan.FromSeconds(delta);

        _updateInfo.GarbageGen0Count = GC.CollectionCount(0);
        _updateInfo.GarbageGen1Count = GC.CollectionCount(1);
        _updateInfo.GarbageGen2Count = GC.CollectionCount(2);

        sw.Restart();
        GameSystems.Run();
        sw.Stop();
        _updateInfo.CPUTime = sw.Elapsed;
    }

    private static void GbNotification()
    {
        while (true)
        {
            GCNotificationStatus s = GC.WaitForFullGCApproach();
            if (s == GCNotificationStatus.Succeeded)
                Console.WriteLine("GC approaching");
            else if (s == GCNotificationStatus.Canceled)
                Console.WriteLine("GC Notification cancelled.");
            else
                Console.WriteLine("GC Notification not applicable.");

            s = GC.WaitForFullGCComplete();
            if (s == GCNotificationStatus.Succeeded)
                Console.WriteLine("Full GC complete");
            else if (s == GCNotificationStatus.Canceled)
                Console.WriteLine("GC Notification cancelled.");
            else
                Console.WriteLine("GC Notification not applicable.");
        }
    }


    private void OnLoad()
    {
        _input = Window.CreateInput();
        for (int i = 0; i < _input.Keyboards.Count; i++)
            _input.Keyboards[i].KeyDown += (keyboard, key, arg3) => KeyDown?.Invoke(keyboard, key, arg3);

        GC.RegisterForFullGCNotification(10, 10);
        Task.Run(() => GbNotification());

        GlWrapper.Init(Window);

        GameSystems = new EcsSystems(World, "Fixed Update")
            .Inject(_updateInfo)
            .Inject(_renderInfo)
            .Inject(this);

        RenderSystems = new EcsSystems(World, "Frame Update")
            .Add(new Renderer(), "Renderer")
            .Add(new FpsProcessor())
            .Inject(_renderInfo)
            .Inject(_updateInfo)
            .Inject(this);

        _imGuiController = new ImGuiController(GlWrapper.Gl, Window, _input);

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