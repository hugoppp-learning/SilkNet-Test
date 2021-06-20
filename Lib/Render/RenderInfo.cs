namespace Lib.Render
{

internal class UpdateInfo
{
    public double Delta { get; internal set; }
    public double CPUTime { get; internal set; }
    public int GarbageGen0Count { get; internal set; }
    public int GarbageGen1Count { get; internal set; }
    public int GarbageGen2Count { get; internal set; }
}

internal class RenderInfo
{
    public double Delta { get; internal set; }
    public double CPUTime { get; internal set; }
    public int DrawCalls { get; internal set; }
}

}