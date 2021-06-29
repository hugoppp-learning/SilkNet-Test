using System;
using ImGuiNET;

namespace Lib
{

public static class ImGuiExtensions
{
    public static void Text(ReadOnlySpan<byte> utf8)
    {
        unsafe
        {
            fixed (byte* p = utf8)
                ImGuiNative.igText(p);
        }
    }

    public static void Text(Utf8Buffer buffer)
    {
        unsafe
        {
            fixed (byte* p = buffer.ToSpan())
                ImGuiNative.igText(p);
        }
    }
}

}