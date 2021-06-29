#nullable enable
using System;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Leopotam.Ecs;
using Lib.Components;
using Lib.Render;

namespace Lib
{

public class MyImGuiRenderer : IEcsRunSystem
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
    private Utf8Buffer quadRendererCount = new("QuadRenderer Count: ", 32);

    private int _entitiesCurrentSelectedIndex;

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


        ImGui.Begin("Entities");
        using (quadRendererCount.Put(ImGuiEntityList.EntityCount))
            ImGuiExtensions.Text(quadRendererCount);

        ImGui.ListBox("", ref _entitiesCurrentSelectedIndex, ImGuiEntityList.EntityBuffer, ImGuiEntityList.EntityCount);

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
                    FullGcBuffer = new float[PlotBufferSize],
                    LastGcVal = new int[3],
                    FullGcLastVal = 0
                };

                for (int i = 0; i < t.GcBuffer.Length; i++)
                    t.GcBuffer[i] = new float[PlotBufferSize];

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


    public class ImGuiEntityList : IEcsRunFixedSystem
#if DEBUG
        , IEcsWorldDebugListener
#endif
    {
        public static int EntityCount => Math.Min(EntityBuffer.Length, Instance._entities.GetEntitiesCount());
        public static string[] EntityBuffer = new string[0];

        public static bool EntityBufferDirty = true;

    #if DEBUG
    #else
        private int _dirtCounter;
        private const int UpdateListEveryXWorldUpdates = 10;
    #endif

        private ImGuiEntityList()
        {
        }

        public static ImGuiEntityList Instance = new();

        private readonly EcsFilter<Position> _entities = null!;

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
            int entitiesCount = _entities.GetEntitiesCount();
            if (entitiesCount > EntityBuffer.Length)
                EntityBuffer = new string[entitiesCount];
            for (int i = 0; i < entitiesCount; ++i)
            {
                EcsEntity ecsEntity = _entities.GetEntity(i);
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


public class Utf8Buffer
{
    private byte[] _data;
    private int len;
    private int prefixLen;
    public int BytesLeft => _data.Length - 1 - len;

    public ReadOnlySpan<byte> ToSpan() => _data;

    public void Put(string s)
    {
        Encoding.UTF8.GetBytes(s, GetAppendSpan(s.Length));
        len += s.Length;
    }

    public readonly struct ClearOnDispose : IDisposable
    {
        private readonly Utf8Buffer _buf;
        public ClearOnDispose(Utf8Buffer buf) => _buf = buf;

        public void Dispose()
        {
            _buf.Clear();
        }
    }

    public ClearOnDispose Put(int i)
    {
        len += itoa(i, GetAppendSpan());
        return new ClearOnDispose(this);
    }


    public ClearOnDispose Put(ReadOnlySpan<byte> bytes)
    {
        bytes.CopyTo(GetAppendSpan(bytes.Length));
        len += bytes.Length;
        return new ClearOnDispose(this);
    }

    public void Clear()
    {
        Array.Clear(_data, prefixLen, _data.Length - prefixLen);
        len = prefixLen;
    }

    public Utf8Buffer(string prefix, int size)
    {
        int utf8ByteCount = Encoding.UTF8.GetByteCount(prefix);
        if (utf8ByteCount > size - 1) throw new ArgumentException();

        prefixLen = prefix.Length;
        len = prefixLen;

        _data = new byte[size];

        Encoding.UTF8.GetBytes(prefix, 0, prefix.Length, _data, 0);
    }

    public static int itoa(int n, Span<byte> result, int radix = 10)
    {
        if (0 == n)
        {
            result[0] = 48;
            return 1;
        }

        int index = 10;
        Span<char> buffer = stackalloc char[10];
        string xlat = "0123456789abcdefghijklmnopqrstuvwxyz";

        for (int r = Math.Abs(n), q; r > 0; r = q)
        {
            q = Math.DivRem(r, radix, out r);
            buffer[--index] = xlat[r];
        }

        if (n < 0)
        {
            buffer[--index] = '-';
        }

        buffer = buffer.Slice(index);
        if (result.Length <= buffer.Length) throw new ArgumentException("write buffer to small");

        Encoding.UTF8.GetBytes(buffer, result);
        return index;
    }

    public override string ToString()
    {
        return Encoding.UTF8.GetString(_data.AsSpan(0, len));
    }

    private Span<byte> GetAppendSpan() => new(_data, len, _data.Length - len);

    private Span<byte> GetAppendSpan(int size)
    {
    #if debug
        if (BytesLeft < size)
        {
            throw new ArgumentException($"Requested {size} bytes, but there were only {BytesLeft} left");
        }
    #endif

        return new(_data, len, size);
    }
}

public static class ImGuiExtensions
{
    public static void Text(ReadOnlySpan<byte> utf8)
    {
        unsafe
        {
            fixed (byte* p = utf8)
                ImGuiNative.igText(p);
        }
    }

    public static void Text(Utf8Buffer buffer)
    {
        unsafe
        {
            fixed (byte* p = buffer.ToSpan())
                ImGuiNative.igText(p);
        }
    }
}

}