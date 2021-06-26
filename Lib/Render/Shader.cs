using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Silk.NET.OpenGL;

namespace Lib.Render
{

public class Shader : IDisposable
{
    private static uint _currentlyInUse = uint.MaxValue;

    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocationCache;

    public Shader(string vertexPath, string fragmentPath)
    {
        _handle = 0;
        _uniformLocationCache = new Dictionary<string, int>(3);

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

    public bool InUse => _currentlyInUse == _handle;

    public void Dispose()
    {
        GlWrapper.Gl.DeleteProgram(_handle);
    }

    public void Use()
    {
        if (InUse)
            return;

        _currentlyInUse = _handle;
        GlWrapper.Gl.UseProgram(_handle);
    }

    public void SetUniform(string name, int value)
    {
        int location = GetUniformLocation(name);
        if (location == -1) throw new ArgumentException($"{name} uniform not found on shader.", nameof(name));

        GlWrapper.Gl.Uniform1(location, value);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = GetUniformLocation(name);
        GlWrapper.Gl.UniformMatrix4(location, 1, false, (float*) &value);
    }


    public void SetUniform(string name, float value)
    {
        int location = GetUniformLocation(name);
        if (location == -1) throw new ArgumentException($"{name} uniform not found on shader.", nameof(name));

        GlWrapper.Gl.Uniform1(location, value);
    }

    private int GetUniformLocation(string name)
    {
        int location = _uniformLocationCache.ContainsKey(name)
            ? _uniformLocationCache[name]
            : _uniformLocationCache[name] = GlWrapper.Gl.GetUniformLocation(_handle, name);
        if (location == -1) throw new Exception($"{name} uniform not found on shader.");

        return location;
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