using System;
using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lib.Render
{

public readonly struct Mesh : IEquatable<Mesh>
{
    public readonly ImmutableArray<SimpleTexturedVertex> Vertices;
    public readonly ImmutableArray<float>? Indices;
    public readonly int Id;
    private static int _lastId;

    public Mesh(SimpleTexturedVertex[] vertices, float[]? indices = null) :
        this(ImmutableArray.Create(vertices),
            ImmutableArray.Create(indices))
    {
    }

    public Mesh(ImmutableArray<SimpleTexturedVertex> vertices, ImmutableArray<float>? indices = null)
    {
        Id = _lastId++;
        Vertices = vertices;
        Indices = indices;
    }

    public Mesh(in Mesh mesh, in Matrix4x4 transform)
    {
        Id = _lastId++;
        var newVertecies = new SimpleTexturedVertex[mesh.Vertices.Length];
        for (int i = 0; i < mesh.Vertices.Length; i++)
            newVertecies[i] = new SimpleTexturedVertex(
                Vector3.Transform(mesh.Vertices[i].Coord, transform),
                mesh.Vertices[i].UvCoord);

        //saves Builder class alloc
        Vertices = Unsafe.As<SimpleTexturedVertex[], ImmutableArray<SimpleTexturedVertex>>(ref newVertecies);
        Indices = mesh.Indices;
    }


    public readonly ReadOnlySpan<float> AsSpan()
    {
        ReadOnlySpan<SimpleTexturedVertex> t = Vertices.AsSpan();
        return MemoryMarshal.Cast<SimpleTexturedVertex, float>(t);
    }

    public bool Equals(Mesh other) => other.Id == Id;
    public override bool Equals(object? obj) => obj is Mesh other && Equals(other);
    public override int GetHashCode() => Id;

    public static Mesh Sprite => Meshes.Sprite;

    //Can't be in Mesh struct, as it will cause a TypeException
    private static class Meshes
    {
        private static Mesh? _spriteMesh;

        public static Mesh Sprite => _spriteMesh ??= new Mesh(new[]
        {
            new SimpleTexturedVertex(new(0.0f, 1.0f, 0.0f), new(0.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 0.0f, 0.0f), new(1.0f, 0.0f)),
            new SimpleTexturedVertex(new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f)),
            new SimpleTexturedVertex(new(0.0f, 1.0f, 0.0f), new(0.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 1.0f, 0.0f), new(1.0f, 1.0f)),
            new SimpleTexturedVertex(new(1.0f, 0.0f, 0.0f), new(1.0f, 0.0f))
        });
    }
}

}