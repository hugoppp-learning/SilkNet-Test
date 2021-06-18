using System;
using System.Numerics;

namespace Lib.Render
{

public readonly struct SimpleTexturedVertex
{
    public readonly Vector3 Coord;
    public readonly Vector2 UvCoord;

    public SimpleTexturedVertex(Vector3 coord, Vector2 uvCoord)
    {
        (Coord, UvCoord) = (coord, uvCoord);
    }
}

}