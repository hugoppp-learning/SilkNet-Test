using System;
using System.Collections.Generic;
using System.Numerics;
using Leopotam.Ecs;
using Lib.Components;
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

        private EcsSystems _fixedUpdateSystems;
        private EcsSystems _frameUpdateSystems;

        RenderInfo _renderInfo = new();
        private UpdateInfo _updateInfo = new();

        public Game()
        {
            var options = WindowOptions.Default;
            options.Title = "Moin Moin";
            options.Size = new Vector2D<int>(1280, 720);
            options.API = GraphicsAPI.Default;

            options.UpdatesPerSecond = 30.0;
            options.FramesPerSecond = 300.0;

            options.ShouldSwapAutomatically = true;
            options.VSync = false;

            Window = Silk.NET.Windowing.Window.Create(options);
            Window.Load += OnLoad;
            Window.Update += OnUpdate;
            Window.Closing += OnClosing;
            Window.Render += WindowOnRender;
        }

        internal IInputContext Input => _input ?? throw new InvalidOperationException("Input not yet created");
        internal IWindow Window { get; set; }


        public virtual void OnAfterLoad()
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

            EcsEntity[] entities = null;
            World.GetAllEntities(ref entities);
            Move(entities[0]);
        }

        private void OnLoad()
        {
            _input = Window.CreateInput();
            for (int i = 0; i < _input.Keyboards.Count; i++)
                _input.Keyboards[i].KeyDown += KeyDown;

            GlWrapper.Init(Window);

            _fixedUpdateSystems = new EcsSystems(World, "Fixed Update")
                .Inject(_updateInfo);

            _frameUpdateSystems = new EcsSystems(World, "Frame Update")
                .Add(new Renderer(), "Renderer")
                .Add(new FpsProcessor())
                .Inject(_renderInfo)
                .Inject(this);


            _fixedUpdateSystems.Init();
            _frameUpdateSystems.Init();

            OnAfterLoad();
        }


        private void Move(EcsEntity e)
        {
            float rotationSpeed = 0.1f;

            ref Position pos = ref e.Get<Position>();

            var speed = new Vector3(0.01f);
            if (Input.Keyboards[0].IsKeyPressed(Key.W))
                pos.Value += Vector3.UnitY * speed;
            if (Input.Keyboards[0].IsKeyPressed(Key.S))
                pos.Value -= Vector3.UnitY * speed;
            if (Input.Keyboards[0].IsKeyPressed(Key.A))
                pos.Value -= Vector3.UnitX * speed;
            if (Input.Keyboards[0].IsKeyPressed(Key.D))
                pos.Value += Vector3.UnitX * speed;
            // if (Input.Keyboards[0].IsKeyPressed(Key.E))
            //     gameObject.Transform2D.Rotate(-rotationSpeed);
            // if (Input.Keyboards[0].IsKeyPressed(Key.Q))
            //     gameObject.Transform2D.Rotate(rotationSpeed);
        }

        private void KeyDown(IKeyboard arg1, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Escape:
                    Window.Close();
                    break;
            }
        }

        private class UpdateInfo : IUpdateInfo
        {
            public double Delta { get; set; }
        }

        private class RenderInfo : IRenderInfo
        {
            public double Delta { get; set; }
        }
    }
}