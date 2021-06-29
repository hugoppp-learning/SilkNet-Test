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
#if DEBUG
    public string Name { get; init; }

#endif

    public unsafe Texture(string path)
    {
    #if DEBUG
        Name = path;
    #endif

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
            return new Texture(new Span<byte>(&whiteColor, sizeof(int)), 1, 1)
            #if DEBUG
                {Name = "White"};
            #endif
        }
    }

    public unsafe Texture(Span<byte> data, uint width, uint height)
    {
    #if DEBUG
        Name = "Custom";
    #endif
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
        GlWrapper.Gl.CreateTextures(TextureTarget.Texture2D, 1, out _handle);
        GlWrapper.Gl.TextureStorage2D(_handle, 1, SizedInternalFormat.Rgba8, width, height);
        GlWrapper.Gl.TextureSubImage2D(_handle, 0, 0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, data);

        GlWrapper.Gl.TextureParameter(_handle, TextureParameterName.TextureWrapS, (int) GLEnum.ClampToEdge);
        GlWrapper.Gl.TextureParameter(_handle, TextureParameterName.TextureWrapT, (int) GLEnum.ClampToEdge);
        GlWrapper.Gl.TextureParameter(_handle, TextureParameterName.TextureMinFilter, (int) GLEnum.Linear);
        GlWrapper.Gl.TextureParameter(_handle, TextureParameterName.TextureMagFilter, (int) GLEnum.Linear);
        GlWrapper.Gl.GenerateTextureMipmap(_handle);
    }


    public void Bind(int textureUnit = 0)
    {
        GlWrapper.ActivateTextureUnit(textureUnit);
        GlWrapper.Gl.BindTexture(TextureTarget.Texture2D, _handle);
    }
}

}