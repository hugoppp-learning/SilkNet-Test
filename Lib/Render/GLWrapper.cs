using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Lib.Render
{
    public static class GlWrapper
    {
        private static GL? _gl;
        public static GL Gl => _gl ?? throw new InvalidOperationException("GL not initialized");

        public static void Init(IWindow window)
        {
            _gl = GL.GetApi(window);

            Gl.Enable(GLEnum.DebugOutput);
            Gl.DebugMessageCallback(
                (source, type, id, severity, length, message, userParam) =>
                    Console.WriteLine($"[GLDebug] [{severity}] {type}/{id}: {Marshal.PtrToStringAnsi(message)}"),
                ReadOnlySpan<byte>.Empty);

            // Gl.Enable(EnableCap.CullFace);
            // Gl.CullFace(CullFaceMode.Back);

            // Gl.Enable(EnableCap.DepthTest);
            // Gl.DepthFunc(DepthFunction.Less);

            // Gl.Enable(EnableCap.Blend);
            // Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void Clear()
        {
            Gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        }
    }
}