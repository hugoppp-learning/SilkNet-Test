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
        private IInputContext? _input;
        public EcsWorld World { get; } = new();

        public EcsSystems FixedUpdateSystems => _fixedUpdateSystems;
        private EcsSystems _fixedUpdateSystems;
        public EcsSystems FrameUpdateSystems => _frameUpdateSystems;
        private EcsSystems _frameUpdateSystems;


        public event Action<IKeyboard, Key, int>? KeyDown;


        RenderInfo _renderInfo = new();
        private UpdateInfo _updateInfo = new();

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

        public IInputContext Input => _input ?? throw new InvalidOperationException("Input not yet created");
        internal IWindow Window { get; set; }


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
            _frameUpdateSystems.Run();
        }

        private void OnClosing()
        {
            _fixedUpdateSystems.Destroy();
            _frameUpdateSystems.Destroy();
        }

        private void OnUpdate(double delta)
        {
            _updateInfo.Delta = delta;

            _fixedUpdateSystems.Run();
        }


        private void OnLoad()
        {
            _input = Window.CreateInput();
            for (int i = 0; i < _input.Keyboards.Count; i++)
                _input.Keyboards[i].KeyDown += (keyboard, key, arg3) => KeyDown?.Invoke(keyboard, key, arg3);


            GlWrapper.Init(Window);

            _fixedUpdateSystems = new EcsSystems(World, "Fixed Update")
                .Inject(_updateInfo)
                .Inject(this);

            _frameUpdateSystems = new EcsSystems(World, "Frame Update")
                .Add(new Renderer(), "Renderer")
                .Add(new FpsProcessor())
                .Inject(_renderInfo)
                .Inject(this);


            OnBeforeEcsSystemInit();
            _fixedUpdateSystems.Init();
            _frameUpdateSystems.Init();
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