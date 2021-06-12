using System;
using System.Runtime.CompilerServices;
using Silk.NET.Maths;

namespace Lib.Render
{
    public struct NewVertex
    {
        public Vector3D<float> Coord;
        public Vector4D<float> Color;
        public Vector3D<float> UvCoord;
        public float TextId;

        public NewVertex(Vector3D<float> coord, Vector4D<float> color, Vector3D<float> uvCoord, float textId)
        {
            Coord = coord;
            Color = color;
            UvCoord = uvCoord;
            TextId = textId;
        }

        public static NewVertex Textured(Vector3D<float> coord, Vector3D<float> uvCoord, float textId)
        {
            return new(coord, new Vector4D<float>(1), uvCoord, textId);
        }

        public static NewVertex Colored(Vector3D<float> coord, Vector4D<float> color)
        {
            return new(coord, new Vector4D<float>(1), new Vector3D<float>(0), 0);
        }


        public unsafe Span<float> AsSpan()
        {
            void* valPtr = Unsafe.AsPointer(ref Unsafe.AsRef(this));
            return new Span<float>(valPtr, sizeof(NewVertex));
        }
    }
}