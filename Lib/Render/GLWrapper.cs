using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Lib.Render
{

public static class GlWrapper
{
    private static GL? _gl;

    private static int _textureUnitCount = 1;
    private static readonly bool[] textureUnits = new bool[_textureUnitCount];
    public static GL Gl => _gl ?? throw new InvalidOperationException("GL not initialized");

    public static void Init(IWindow window)
    {
        _gl = GL.GetApi(window);

        Gl.Enable(GLEnum.DebugOutput);
        Gl.DebugMessageCallback(
            WriteDebug,
            ReadOnlySpan<byte>.Empty);


        _textureUnitCount = Gl.GetInteger((GLEnum) GetPName.MaxTextureImageUnits);

        // Gl.Enable(EnableCap.CullFace);
        // Gl.CullFace(CullFaceMode.Back);

        // Gl.Enable(EnableCap.DepthTest);
        // Gl.DepthFunc(DepthFunction.Less);

        // Gl.Enable(EnableCap.Blend);
        // Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    private static void WriteDebug(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        if (DebugId.IsUseless(id))
            return;

        string? msg = Marshal.PtrToStringAnsi(message);
        Console.WriteLine($"[GLDebug] [{severity}] {type}/{id}: {msg ?? "no message"}");
    }

    public static void Clear()
    {
        Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public static void ActivateTextureUnit(int slot)
    {
        if (!textureUnits[0])
        {
            Gl.ActiveTexture(TextureUnit.Texture0 + slot);
            textureUnits[0] = true;
        }
    }

    private static class DebugId
    {
        public const int BufferDetailedInfo = 131185;

        public static bool IsUseless(int id)
        {
            switch (id)
            {
                case 14:
                case BufferDetailedInfo:
                    return true;
                default:
                    return false;
            }
        }
    }
}

}