namespace Lib.Render
{
    public interface IUpdateInfo
    {
        public double Delta { get; }
    }

    public interface IRenderInfo
    {
        public double Delta { get; }
        public int DrawCalls { get; }
    }


    internal class UpdateInfo : IUpdateInfo
    {
        public double Delta { get; set; }
    }

    internal class RenderInfo : IRenderInfo
    {
        public double Delta { get; set; }
        public int DrawCalls { get; set; }
    }
}