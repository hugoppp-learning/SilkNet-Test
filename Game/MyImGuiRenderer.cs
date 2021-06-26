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
    private UpdateInfo _updateInfo = null!;
    private RenderInfo _renderInfo = null!;


    private const int buffer_size = 2 * 144;
    private float[] _frame_times = new float[buffer_size];
    private float[][] _GC = new float[3][];

    private int[] lastGC = new int[3];

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

        for (var i = 0; i < lastGC.Length; i++)
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
        for (var i = 0; i < _GC.Length; i++)
        {
            ImGui.PlotHistogram("", ref _GC[i][0], _GC[i].Length, 0, $"GC gen{i}: {_updateInfo.gc[i].ToString()}", 0, 1,
                new Vector2(250, 20));
        }

        ImGui.Text("DrawCalls: " + _renderInfo.DrawCalls);


        ImGui.End();

        ImGui.Begin("Entities");
        ImGui.Text("QuadRenderer Count: " + MyImGuiData.EntityCount);


        var s = Stopwatch.StartNew();
        Console.WriteLine(s.ElapsedTicks);

        int currentItem = 1;
        ImGui.ListBox("", ref currentItem, MyImGuiData.EntityBuffer, MyImGuiData.EntityCount);

        ImGui.End();
    }


    public class MyImGuiData : IEcsRunSystem, IEcsWorldDebugListener
    {
        public static int EntityCount => Math.Min(EntityBuffer.Length, Instance._quadRendererEntities.GetEntitiesCount());
        public static string[] EntityBuffer = new string[0];

        public static bool EntityBufferDirty = true;

        private MyImGuiData()
        {
        }

        public static MyImGuiData Instance = new();

        private EcsFilter<Position, QuadRenderer> _quadRendererEntities = null!;

        public void Run()
        {
            if (EntityBufferDirty)
            {
                UpdateEntityBuffer();
                EntityBufferDirty = false;
            }
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
    }
}

public struct Name
{
    public Name(string value) => Value = value;

    public string Value;
}

}