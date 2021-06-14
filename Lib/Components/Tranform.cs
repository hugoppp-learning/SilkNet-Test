using System.Numerics;

namespace Lib.Components
{

public struct Position
{
    public Vector3 Value;

    public Position(Vector3 value)
    {
        Value = value;
    }

    public override readonly string ToString()
    {
        return Value.ToString();
    }

    public override readonly bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static implicit operator Vector3(Position p)
    {
        return p.Value;
    }
}

public struct Rotation
{
    public Quaternion Value;

    public override readonly string ToString()
    {
        return Value.ToString();
    }

    public override readonly bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

public struct Scale
{
    public float Value;

    public override readonly string ToString()
    {
        return Value.ToString("F1");
    }

    public override readonly bool Equals(object? obj)
    {
        return Value.Equals(obj);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }
}

}