#nullable enable
using System;
using System.Numerics;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;

namespace Lib
{

public class MyImGuiRenderer : IEcsRunSystem
{
    private const int BufferSize = 2 * 144;
    private readonly float[] _frameTimes = new float[BufferSize];
    private readonly RenderInfo _renderInfo = null!;
    private readonly UpdateInfo _updateInfo = null!;


    private bool _enablePlotGcInfo = true;

    private GcData _gc = GcData.Default;


    public void Run(double delta)
    {
        ImGui.Begin("Update Info");
        ImGui.Text($"Frame id: {_renderInfo.FrameId}");
        ImGui.Text($"Update CPU ms: {_updateInfo.UpdateCPUTime.Milliseconds}");
        ImGui.Text($"Fixed Update CPU ms: {_updateInfo.FixedUpdateCPUTime.Milliseconds}");
        ImGui.Text($"Render CPU ms: {_renderInfo.Delta.Milliseconds}");
        ImGui.Text($"FPS: {_renderInfo.Fps.ToString("F2")}");
        PlotFrameTimes();

        ImGui.Checkbox("Garbage Collector Info", ref _enablePlotGcInfo);
        if (_enablePlotGcInfo)
        {
            PlotGcInfo();
            _gc.Update(_updateInfo);
        }
        else
        {
            _gc.ResetOnNextUpdate = true;
        }

        ImGui.Text("DrawCalls: " + _renderInfo.DrawCalls);

        ImGui.End();

        ImGui.Begin("Entities");
        ImGui.Text("QuadRenderer Count: " + MyImGuiData.EntityCount);


        int currentItem = 1;
        ImGui.ListBox("", ref currentItem, MyImGuiData.EntityBuffer, MyImGuiData.EntityCount);

        ImGui.End();
    }

    private void PlotFrameTimes()
    {
        float msDelta = (float) _renderInfo.Delta.TotalMilliseconds;
        UpdateBuffer(_frameTimes, msDelta);
        ImGui.Text("Frametimes:");
        float maxFrameTime = 0;
        for (int i = 0; i < _frameTimes.Length; i++)
            maxFrameTime = Math.Max(maxFrameTime, _frameTimes[i]);
        ImGui.PlotLines("", ref _frameTimes[0], _frameTimes.Length, 0, maxFrameTime.ToString("F1"), 0, 4 * _renderInfo.FpsAsMs,
            new Vector2(250, 50));
    }

    private void PlotGcInfo()
    {
        for (int i = 0; i < _gc.GcBuffer.Length; i++)
            ImGui.PlotHistogram("", ref _gc.GcBuffer[i][0], _gc.GcBuffer[i].Length, 0, $"GC gen{i}: {_updateInfo.gc[i].ToString()}", 0, 1,
                new Vector2(250, 20));
        ImGui.PlotHistogram("", ref _gc.FullGcBuffer[0], _gc.FullGcBuffer.Length, 0, $"Full GC: {_updateInfo.FullGc.ToString()}", 0, 1,
            new Vector2(250, 20));
    }

    private static void UpdateBuffer(float[] buffer, float newVal)
    {
        Array.Copy(buffer, 0, buffer, 1, buffer.Length - 1);
        buffer[0] = newVal;
    }


    private struct GcData
    {
        public float[][] GcBuffer;
        public int[] LastGcVal;

        public float[] FullGcBuffer;
        public int FullGcLastVal;

        public bool ResetOnNextUpdate;

        public static GcData Default
        {
            get
            {
                GcData t = new()
                {
                    GcBuffer = new float[3][],
                    FullGcBuffer = new float[BufferSize],
                    LastGcVal = new int[3],
                    FullGcLastVal = 0
                };

                for (int i = 0; i < t.GcBuffer.Length; i++)
                    t.GcBuffer[i] = new float[BufferSize];

                return t;
            }
        }

        private void Reset()
        {
            ResetOnNextUpdate = false;
            for (int i = 0; i < LastGcVal.Length; i++)
                Array.Clear(GcBuffer[i], 0, GcBuffer[i].Length);
            Array.Clear(FullGcBuffer, 0, FullGcBuffer.Length);
        }

        public void Update(UpdateInfo updateInfo)
        {
            if (ResetOnNextUpdate)
                Reset();

            for (int i = 0; i < LastGcVal.Length; i++)
            {
                float newVal = LastGcVal[i] == updateInfo.gc[i] ? 0 : 1;
                UpdateBuffer(GcBuffer[i], newVal);
                LastGcVal[i] = updateInfo.gc[i];
            }

            {
                float newVal = updateInfo.FullGcApproaching ? 0.1f :
                    FullGcLastVal == updateInfo.FullGc ? 0 : 1;
                UpdateBuffer(FullGcBuffer, newVal);
                FullGcLastVal = updateInfo.FullGc;
            }
        }
    }


    public class MyImGuiData : IEcsRunFixedSystem
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

        public void RunFixed(double delta)
        {
            if (EntityBufferDirty)
            {
                UpdateEntityBuffer();
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
        public void OnEntityCreated(EcsEntity entity)
        {
            EntityBufferDirty = true;
        }

        public void OnEntityDestroyed(EcsEntity entity)
        {
            EntityBufferDirty = true;
        }

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