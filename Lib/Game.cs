using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Leopotam.Ecs;
using Lib.Render;
using Lib.Systems;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Lib
{

public abstract class Game
{
    private readonly RenderInfo _renderInfo = new();
    private readonly UpdateInfo _updateInfo = new();

    private readonly Stopwatch FixedUpdateSw = new();
    private readonly Stopwatch RenderSw = new();
    private readonly Stopwatch UpdateSw = new();

    private EcsSystems[] _allEscSystems = new EcsSystems[3];

    private ImGuiController _imGuiController;
    private IInputContext? _input;

    public Game()
    {
        var options = WindowOptions.Default;
        options.Title = "Moin Moin";
        options.Size = new Vector2D<int>(1280, 720);
        options.API = GraphicsAPI.Default;

        options.UpdatesPerSecond = 10.0;
        options.FramesPerSecond = 300.0;

        options.ShouldSwapAutomatically = false;
        options.VSync = false;

        Window = Silk.NET.Windowing.Window.Create(options);
        Window.Load += OnLoad;
        Window.Update += OnUpdate;
        Window.Closing += OnClosing;
        Window.Render += WindowOnRender;
    }

    public EcsWorld World { get; } = new();

    public EcsSystems UpdateSystems { get; private set; } = null!;

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
        _renderInfo.FrameId++;

        UpdateSw.Start();
        UpdateSystems.Run(delta);
        UpdateSw.Stop();
        _updateInfo.UpdateCPUTime = UpdateSw.Elapsed;

        _imGuiController.Update((float) delta);

        RenderSw.Restart();
        RenderSystems.Run(delta);
        RenderSw.Stop();
        _renderInfo.CPUTime = RenderSw.Elapsed;

        _imGuiController.Render();

        Window.SwapBuffers();
    }

    private void OnClosing()
    {
        for (int i = 0; i < _allEscSystems.Length; i++)
            _allEscSystems[i].Destroy();

        _imGuiController.Dispose();
    }

    private void OnUpdate(double delta)
    {
        for (int i = 0; i < _updateInfo.gc.Length; i++)
            _updateInfo.gc[i] = GC.CollectionCount(i);

        FixedUpdateSw.Restart();
        UpdateSystems.RunFixed(delta);
        FixedUpdateSw.Stop();
        _updateInfo.FixedUpdateCPUTime = FixedUpdateSw.Elapsed;

        RenderSystems.RunFixed(delta);
    }

    private static void GbNotification(UpdateInfo updateInfo)
    {
        while (true)
        {
            GCNotificationStatus s = GC.WaitForFullGCApproach();
            if (s == GCNotificationStatus.Succeeded)
            {
                Console.WriteLine("GC approaching");
                updateInfo.FullGcApproaching = true;
            }
            else if (s == GCNotificationStatus.Canceled)
            {
                Console.WriteLine("GC Notification cancelled.");
            }
            else
            {
                Console.WriteLine("GC Notification not applicable.");
            }

            s = GC.WaitForFullGCComplete();
            if (s == GCNotificationStatus.Succeeded)
            {
                Console.WriteLine("Full GC complete");
                updateInfo.FullGc++;
                updateInfo.FullGcApproaching = false;
            }
            else if (s == GCNotificationStatus.Canceled)
            {
                Console.WriteLine("GC Notification cancelled.");
            }
            else
            {
                Console.WriteLine("GC Notification not applicable.");
            }
        }
    }


    private void OnLoad()
    {
        _input = Window.CreateInput();
        for (int i = 0; i < _input.Keyboards.Count; i++)
            _input.Keyboards[i].KeyDown += (keyboard, key, arg3) => KeyDown?.Invoke(keyboard, key, arg3);

        GC.RegisterForFullGCNotification(10, 10);
        Task.Run(() => GbNotification(_updateInfo));

        GlWrapper.Init(Window);


        UpdateSystems = new EcsSystems(World, "Variable Update System")
            .Inject(_updateInfo)
            .Inject(_renderInfo)
            .Inject(this);

        RenderSystems = new EcsSystems(World, "Render Systems")
            .Add(new RenderSystem2D(), "Renderer")
            .Add(new FpsProcessor())
            .Inject(_renderInfo)
            .Inject(_updateInfo)
            .Inject(this);

        _imGuiController = new ImGuiController(GlWrapper.Gl, Window, _input);

        OnBeforeEcsSystemInit();

        _allEscSystems = new[] {UpdateSystems, RenderSystems};
        for (int i = 0; i < _allEscSystems.Length; i++)
            _allEscSystems[i].Init();

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