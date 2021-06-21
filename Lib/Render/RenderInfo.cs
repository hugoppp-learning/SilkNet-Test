using System;

namespace Lib.Render
{

public class UpdateInfo
{
    public TimeSpan Delta { get; internal set; }
    public TimeSpan CPUTime { get; internal set; }
    public int GarbageGen0Count { get; internal set; }
    public int GarbageGen1Count { get; internal set; }
    public int GarbageGen2Count { get; internal set; }
}

public class RenderInfo
{
    public TimeSpan Delta { get; internal set; }
    public TimeSpan CPUTime { get; internal set; }
    public int DrawCalls { get; internal set; }
    public float FPS { get; internal set; }
}

}