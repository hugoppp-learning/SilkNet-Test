#nullable enable
using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;

namespace Lib
{

public class MyImGuiRenderer : IEcsRunSystem
{
    private const int buffer_size = 2 * 144;
    private readonly float[] _frame_times = new float[buffer_size];
    private readonly float[][] _GC = new float[3][];
    private readonly RenderInfo _renderInfo = null!;
    private readonly UpdateInfo _updateInfo = null!;

    private readonly int[] lastGC = new int[3];

    public MyImGuiRenderer()
    {
        for (int i = 0; i < _GC.Length; i++)
            _GC[i] = new float[buffer_size];
    }

    public void Run()
    {
        float msDelta = (float) _renderInfo.Delta.TotalMilliseconds;

        Array.Copy(_frame_times, 0, _frame_times, 1, _frame_times.Length - 1);
        _frame_times[0] = msDelta;

        for (int i = 0; i < lastGC.Length; i++)
        {
            Array.Copy(_GC[i], 0, _GC[i], 1, _GC[i].Length - 1);
            int newVal = lastGC[i] == _updateInfo.gc[i] ? 0 : 1;
            _GC[i][0] = newVal;
            lastGC[i] = _updateInfo.gc[i];
        }

        float max = 0;
        for (int i = 0; i < _frame_times.Length; i++)
            max = Math.Max(max, _frame_times[i]);

        ImGui.Begin("Update Info");
        ImGui.Text($"Frame id: {_renderInfo.FrameId}");
        ImGui.Text($"Update CPU ms: {_updateInfo.CPUTime.Milliseconds}");
        ImGui.Text($"Render CPU ms: {_renderInfo.Delta.Milliseconds}");
        ImGui.Text($"FPS: {_renderInfo.Fps.ToString("F2")}");
        ImGui.Text("Frametimes:");
        ImGui.PlotLines("", ref _frame_times[0], _frame_times.Length, 0, max.ToString("F1"), 0, 4 * _renderInfo.FpsAsMs,
            new Vector2(250, 50));
        for (int i = 0; i < _GC.Length; i++)
            ImGui.PlotHistogram("", ref _GC[i][0], _GC[i].Length, 0, $"GC gen{i}: {_updateInfo.gc[i].ToString()}", 0, 1,
                new Vector2(250, 20));

        ImGui.Text("DrawCalls: " + _renderInfo.DrawCalls);


        ImGui.End();

        ImGui.Begin("Entities");
        ImGui.Text("QuadRenderer Count: " + MyImGuiData.EntityCount);


        int currentItem = 1;
        ImGui.ListBox("", ref currentItem, MyImGuiData.EntityBuffer, MyImGuiData.EntityCount);

        ImGui.End();
    }


    public class MyImGuiData : IEcsRunSystem
#if DEBUG
        , IEcsWorldDebugListener
#endif
    {
        public static int EntityCount => Math.Min(EntityBuffer.Length, Instance._quadRendererEntities.GetEntitiesCount());
        public static string[] EntityBuffer = new string[0];

        public static bool EntityBufferDirty = true;

    #if DEBUG
    #else
        private int _dirtCounter;
        private const int UpdateListEveryXWorldUpdates = 10;
    #endif

        private MyImGuiData()
        {
        }

        public static MyImGuiData Instance = new();

        private readonly EcsFilter<Position, QuadRenderer> _quadRendererEntities = null!;

        public void Run()
        {
            if (EntityBufferDirty)
            {
                var s = Stopwatch.StartNew();
                UpdateEntityBuffer();
                Console.WriteLine(s.Elapsed.TotalMilliseconds);
                EntityBufferDirty = false;
            }
        #if DEBUG
        #else
            _dirtCounter = (_dirtCounter + 1) % UpdateListEveryXWorldUpdates;
            EntityBufferDirty = _dirtCounter == 0;
        #endif
        }

        private void UpdateEntityBuffer()
        {
            int entitiesCount = _quadRendererEntities.GetEntitiesCount();
            if (entitiesCount > EntityBuffer.Length)
                EntityBuffer = new string[entitiesCount];
            for (int i = 0; i < entitiesCount; ++i)
            {
                EcsEntity ecsEntity = _quadRendererEntities.GetEntity(i);
                string name = ecsEntity.Has<Name>() ? ecsEntity.Get<Name>().Value : "Unnamed entity";
                EntityBuffer[i] = name;
            }
        }

    #if DEBUG
        public void OnEntityCreated(EcsEntity entity) => EntityBufferDirty = true;

        public void OnEntityDestroyed(EcsEntity entity) => EntityBufferDirty = true;

        public void OnFilterCreated(EcsFilter filter)
        {
        }

        public void OnComponentListChanged(EcsEntity entity)
        {
        }

        public void OnWorldDestroyed(EcsWorld world)
        {
        }
    #endif
    }
}

public struct Name
{
    public Name(string value)
    {
        Value = value;
    }

    public string Value;
}

}