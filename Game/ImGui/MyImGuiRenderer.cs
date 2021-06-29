#nullable enable
using System;
using System.Numerics;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Render;

namespace Lib
{

public partial class MyImGuiRenderer : IEcsRunSystem
{
    private const int PlotBufferSize = 2 * 144;
    private readonly float[] _frameTimes = new float[PlotBufferSize];
    private readonly RenderInfo _renderInfo = null!;
    private readonly UpdateInfo _updateInfo = null!;


    private bool _enablePlotGcInfo = true;

    private GcData _gc = GcData.Default;

    private Utf8Buffer frameId = new("Frame id: ", 32);
    private Utf8Buffer fixedCpuMs = new("Fixed Update CPU ms: ", 32);
    private Utf8Buffer updateCpuMs = new("Update CPU ms: ", 32);
    private Utf8Buffer renderCpuMs = new("Render CPU ms: ", 32);

    public ImGuiConfig Config { get; init; } = new();

    public class ImGuiConfig
    {
        public bool ShowEntities { get; set; } = true;
        public bool ShowUpdateInfo { get; set; } = true;
    }

    public static void Register(EcsSystems renderSystems)
    {
        renderSystems.Add(new MyImGuiRenderer());
        RegisterSubsystems(renderSystems);
    }

    private static void RegisterSubsystems(EcsSystems renderSystems)
    {
        renderSystems
            .Add(new ImGuiEntityList())
            ;
    }

    private MyImGuiRenderer()
    {
    }

    public void Run(double delta)
    {
        ImGui.Begin("Update Info");
        using (frameId.Put((int) _renderInfo.FrameId))
            ImGuiExtensions.Text(frameId);
        using (fixedCpuMs.Put(_updateInfo.FixedUpdateCPUTime.Milliseconds))
            ImGuiExtensions.Text(fixedCpuMs);
        using (updateCpuMs.Put(_updateInfo.UpdateCPUTime.Milliseconds))
            ImGuiExtensions.Text(updateCpuMs);
        using (renderCpuMs.Put(_renderInfo.Delta.Milliseconds))
            ImGuiExtensions.Text(renderCpuMs);

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
}

}