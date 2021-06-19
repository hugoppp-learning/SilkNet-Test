using System;
using System.Numerics;

namespace Game
{

public struct Speed : IEquatable<Speed>
{
    public Vector3 Value;
    public Speed(Vector3 value) => Value = value;

    public override readonly string ToString() => Value.ToString();

    public override readonly bool Equals(object? obj) => obj is Speed other && Equals(other);
    public override readonly int GetHashCode() => Value.GetHashCode();

    public readonly bool Equals(Speed other) => Value == other.Value;
    public static bool operator ==(Speed x, Speed y) => x.Equals(y);
    public static bool operator !=(Speed x, Speed y) => !(x == y);

    public static explicit operator Vector3(Speed x) => x.Value;
    public static explicit operator Speed(Vector3 x) => new(x);
}

}