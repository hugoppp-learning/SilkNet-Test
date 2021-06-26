using System;
using System.Numerics;

namespace Lib.Components
{

public struct Position : IEquatable<Position>
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
        return obj is Position other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public readonly bool Equals(Position other)
    {
        return Value == other.Value;
    }

    public static bool operator ==(Position x, Position y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(Position x, Position y)
    {
        return !(x == y);
    }

    public static explicit operator Vector3(Position x)
    {
        return x.Value;
    }

    public static explicit operator Position(Vector3 x)
    {
        return new(x);
    }
}

public struct Rotation : IEquatable<Rotation>
{
    public Quaternion Value;

    public Rotation(Quaternion value)
    {
        Value = value;
    }

    public override readonly string ToString()
    {
        return Value.ToString();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Rotation other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public readonly bool Equals(Rotation other)
    {
        return Value == other.Value;
    }

    public static bool operator ==(Rotation x, Rotation y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(Rotation x, Rotation y)
    {
        return !(x == y);
    }

    public static explicit operator Quaternion(Rotation x)
    {
        return x.Value;
    }

    public static explicit operator Rotation(Quaternion x)
    {
        return new(x);
    }
}

public struct Scale : IEquatable<Scale>
{
    public float Value;

    public Scale(float value)
    {
        Value = value;
    }

    public const float Tolerance = 0.001f;

    public override readonly string ToString()
    {
        return Value.ToString("F1");
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Scale other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public bool Equals(Scale other)
    {
        return Math.Abs(Value - other.Value) < Tolerance;
    }

    public static bool operator ==(Scale x, Scale y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(Scale x, Scale y)
    {
        return !(x == y);
    }

    public static explicit operator float(Scale x)
    {
        return x.Value;
    }

    public static explicit operator Scale(float x)
    {
        return new(x);
    }
}

}