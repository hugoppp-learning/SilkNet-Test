using System;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Lib.Render
{
    public struct Shader : IDisposable
    {
        private readonly uint _handle;

        public Shader(string vertexPath, string fragmentPath)
        {
            _handle = 0;
            uint vertex = LoadShader(ShaderType.VertexShader, Path.Combine(Constants.ResFolder, vertexPath));
            uint fragment = LoadShader(ShaderType.FragmentShader, Path.Combine(Constants.ResFolder, fragmentPath));

            _handle = GlWrapper.Gl.CreateProgram();
            GlWrapper.Gl.AttachShader(_handle, vertex);
            GlWrapper.Gl.AttachShader(_handle, fragment);
            GlWrapper.Gl.LinkProgram(_handle);
            GlWrapper.Gl.GetProgram(_handle, GLEnum.LinkStatus, out int status);

            if (status == 0) throw new Exception($"Program failed to link with error: {GlWrapper.Gl.GetProgramInfoLog(_handle)}");

            GlWrapper.Gl.DetachShader(_handle, vertex);
            GlWrapper.Gl.DetachShader(_handle, fragment);
            GlWrapper.Gl.DeleteShader(vertex);
            GlWrapper.Gl.DeleteShader(fragment);
        }

        public void Dispose()
        {
            GlWrapper.Gl.DeleteProgram(_handle);
        }

        public void Use()
        {
            GlWrapper.Gl.UseProgram(_handle);
        }

        public void SetUniform(string name, int value)
        {
            int location = GlWrapper.Gl.GetUniformLocation(_handle, name);
            if (location == -1) throw new ArgumentException($"{name} uniform not found on shader.", nameof(name));

            GlWrapper.Gl.Uniform1(location, value);
        }

        public unsafe void SetUniform(string name, Matrix4x4 value)
        {
            //A new overload has been created for setting a uniform so we can use the transform in our shader.
            int location = GlWrapper.Gl.GetUniformLocation(_handle, name);
            if (location == -1) throw new Exception($"{name} uniform not found on shader.");

            GlWrapper.Gl.UniformMatrix4(location, 1, false, (float*) &value);
        }

        public void SetUniform(string name, float value)
        {
            int location = GlWrapper.Gl.GetUniformLocation(_handle, name);
            if (location == -1) throw new ArgumentException($"{name} uniform not found on shader.", nameof(name));

            GlWrapper.Gl.Uniform1(location, value);
        }

        private uint LoadShader(ShaderType type, string path)
        {
            string src = File.ReadAllText(path);
            uint handle = GlWrapper.Gl.CreateShader(type);
            GlWrapper.Gl.ShaderSource(handle, src);
            GlWrapper.Gl.CompileShader(handle);
            string infoLog = GlWrapper.Gl.GetShaderInfoLog(handle);
            if (!string.IsNullOrWhiteSpace(infoLog))
                throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");

            return handle;
        }
    }
}