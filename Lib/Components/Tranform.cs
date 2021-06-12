using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using Lib.Render;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Lib.Components
{
    public struct Position
    {
        public Vector3 Value;

        public override readonly string ToString() => Value.ToString();

        public override readonly bool Equals(object? obj) => Value.Equals(obj);

        public override readonly int GetHashCode() => Value.GetHashCode();

        public static implicit operator Vector3(Position p) => p.Value;
    }

    public struct Rotation
    {
        public Quaternion Value;

        public override readonly string ToString() => Value.ToString();

        public override readonly bool Equals(object? obj) => Value.Equals(obj);

        public override readonly int GetHashCode() => Value.GetHashCode();
    }

    public struct Scale
    {
        public float Value;

        public override readonly string ToString() => Value.ToString("F1");

        public override readonly bool Equals(object? obj) => Value.Equals(obj);

        public override readonly int GetHashCode() => Value.GetHashCode();
    }
}