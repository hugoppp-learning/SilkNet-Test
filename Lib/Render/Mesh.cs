using System;
using System.Runtime.InteropServices;

namespace Lib.Render
{
    public struct Mesh : IEquatable<Mesh>
    {
        public readonly SimpleTexturedVertex[] Vertices;
        public readonly float[]? Indices;

        public static SimpleTexturedVertex[]? SpriteMesh;

        public Mesh(SimpleTexturedVertex[] vertices, float[]? indices = null) => (Vertices, Indices) = (vertices, indices);

        public static Mesh Sprite => new(SpriteMesh ??= new[]
        {
            new SimpleTexturedVertex(new(0.0f, 1.0f, 0.0f), new(0.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 0.0f, 0.0f), new(1.0f, 0.0f)),
            new SimpleTexturedVertex(new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f)),
            new SimpleTexturedVertex(new(0.0f, 1.0f, 0.0f), new(0.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 1.0f, 0.0f), new(1.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 0.0f, 0.0f), new(1.0f, 0.0f))
        });

        public Span<float> AsSpan()
        {
            Span<SimpleTexturedVertex> t = Vertices.AsSpan();
            return MemoryMarshal.Cast<SimpleTexturedVertex, float>(t);
        }

        public bool Equals(Mesh other)
        {
            return Vertices.Equals(other.Vertices);
        }

        public override bool Equals(object? obj)
        {
            return obj is Mesh other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Vertices.GetHashCode();
        }
    }
}