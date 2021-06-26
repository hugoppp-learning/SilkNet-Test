using System;

namespace Lib.Render
{

public class UpdateInfo
{
    public TimeSpan Delta { get; internal set; }
    public TimeSpan CPUTime { get; internal set; }
    public int[] gc = new int[3];
}

public class RenderInfo
{
    public TimeSpan Delta { get; internal set; }
    public TimeSpan CPUTime { get; internal set; }
    public int DrawCalls { get; internal set; }
    public float Fps { get; internal set; }
    public float FpsAsMs => 1 / Fps * 1000;
    public uint FrameId { get; internal set; }
}

}