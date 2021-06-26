using System;
using System.IO;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lib.Render
{

public struct Texture : IDisposable
{
    private uint _handle;

    public unsafe Texture(string path)
    {
        _handle = 0;
        Image<Rgba32> img = (Image<Rgba32>) Image.Load(Path.Combine(Constants.ResFolder, path));
        img.Mutate(x => x.Flip(FlipMode.Vertical));

        fixed (void* data = &MemoryMarshal.GetReference(img.GetPixelRowSpan(0)))
        {
            Load(data, (uint) img.Width, (uint) img.Height);
        }

        img.Dispose();
    }

    public static readonly Texture Empty = White();

    private static Texture White()
    {
        unsafe
        {
            int whiteColor = int.MaxValue;
            return new Texture(new Span<byte>(&whiteColor, sizeof(int)), 1, 1);
        }
    }

    public unsafe Texture(Span<byte> data, uint width, uint height)
    {
        _handle = 0;
        fixed (void* d = &data[0])
        {
            Load(d, width, height);
        }
    }

    public void Dispose()
    {
        GlWrapper.Gl.DeleteTexture(_handle);
    }

    private unsafe void Load(void* data, uint width, uint height)
    {
        _handle = GlWrapper.Gl.GenTexture();
        Bind();

        GlWrapper.Gl.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba,
            PixelType.UnsignedByte, data);
        GlWrapper.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        GlWrapper.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        GlWrapper.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) GLEnum.Linear);
        GlWrapper.Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        GlWrapper.Gl.GenerateMipmap(TextureTarget.Texture2D);
    }


    public void Bind(int textureUnit = 0)
    {
        GlWrapper.ActivateTextureUnit(textureUnit);
        GlWrapper.Gl.BindTexture(TextureTarget.Texture2D, _handle);
    }
}

}