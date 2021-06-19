using System;
using System.Numerics;

namespace Lib.Components
{

public struct Position : IEquatable<Position>
{
    public Vector3 Value;
    public Position(Vector3 value) => Value = value;

    public override readonly string ToString() => Value.ToString();

    public override readonly bool Equals(object? obj) => obj is Position other && Equals(other);
    public override readonly int GetHashCode() => Value.GetHashCode();

    public readonly bool Equals(Position other) => Value == other.Value;
    public static bool operator ==(Position x, Position y) => x.Equals(y);
    public static bool operator !=(Position x, Position y) => !(x == y);

    public static explicit operator Vector3(Position x) => x.Value;
    public static explicit operator Position(Vector3 x) => new(x);
}

public struct Rotation : IEquatable<Rotation>
{
    public Quaternion Value;
    public Rotation(Quaternion value) => Value = value;

    public override readonly string ToString() => Value.ToString();
    public override readonly bool Equals(object? obj) => obj is Rotation other && Equals(other);
    public override readonly int GetHashCode() => Value.GetHashCode();

    public readonly bool Equals(Rotation other) => Value == other.Value;
    public static bool operator ==(Rotation x, Rotation y) => x.Equals(y);
    public static bool operator !=(Rotation x, Rotation y) => !(x == y);

    public static explicit operator Quaternion(Rotation x) => x.Value;
    public static explicit operator Rotation(Quaternion x) => new(x);
}

public struct Scale : IEquatable<Scale>
{
    public float Value;
    public Scale(float value) => Value = value;

    public const float Tolerance = 0.001f;

    public override readonly string ToString() => Value.ToString("F1");

    public override readonly bool Equals(object? obj) => obj is Scale other && Equals(other);
    public override readonly int GetHashCode() => Value.GetHashCode();

    public bool Equals(Scale other) => Math.Abs(Value - other.Value) < Tolerance;
    public static bool operator ==(Scale x, Scale y) => x.Equals(y);
    public static bool operator !=(Scale x, Scale y) => !(x == y);

    public static explicit operator float(Scale x) => x.Value;
    public static explicit operator Scale(float x) => new(x);
}

}