using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional signed native 8.8 velocity or acceleration: each axis is one
/// <see cref="SnesSignedEightEight"/> word, added to a <see cref="FixedPosition{TSpace}"/> after
/// promotion to 16.16 (<see cref="ToSixteenSixteen"/>).
/// </summary>
/// <typeparam name="TSpace">The coordinate space the velocity is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y 8.8 axes.")]
public readonly record struct EightEightVector<TSpace>(SnesSignedEightEight X, SnesSignedEightEight Y)
    where TSpace : ISpatialSpace
{
    /// <summary>Creates a vector from the signed raw 8.8 words of both axes.</summary>
    public EightEightVector(short x, short y)
        : this(SnesSignedEightEight.FromSignedRaw(x), SnesSignedEightEight.FromSignedRaw(y))
    {
    }

    /// <summary>Replaces the X axis.</summary>
    public EightEightVector<TSpace> WithX(SnesSignedEightEight x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public EightEightVector<TSpace> WithY(SnesSignedEightEight y) => this with { Y = y };

    /// <summary>Negates the raw X word (wrapping at sixteen bits); Y is unchanged.</summary>
    public EightEightVector<TSpace> ReversedX() => WithX(new SnesSignedEightEight(unchecked((ushort)-X.RawValue)));

    /// <summary>Negates the raw Y word (wrapping at sixteen bits); X is unchanged.</summary>
    public EightEightVector<TSpace> ReversedY() => WithY(new SnesSignedEightEight(unchecked((ushort)-Y.RawValue)));

    /// <summary>Adds the raw 8.8 words of each axis, wrapping at sixteen bits.</summary>
    public static EightEightVector<TSpace> operator +(EightEightVector<TSpace> left, EightEightVector<TSpace> right) =>
        new(new SnesSignedEightEight(unchecked((ushort)(left.X.RawValue + right.X.RawValue))),
            new SnesSignedEightEight(unchecked((ushort)(left.Y.RawValue + right.Y.RawValue))));

    /// <summary>Scales the signed raw 8.8 words of each axis, wrapping at sixteen bits.</summary>
    public static EightEightVector<TSpace> operator *(EightEightVector<TSpace> vector, int factor) =>
        new(unchecked((short)(unchecked((short)vector.X.RawValue) * factor)),
            unchecked((short)(unchecked((short)vector.Y.RawValue) * factor)));

    /// <summary>The same velocity as the native 16.16 displacement used by position adders.</summary>
    public FixedVector<TSpace> ToSixteenSixteen() => new(X.ToSixteenSixteenDelta(), Y.ToSixteenSixteenDelta());
}
