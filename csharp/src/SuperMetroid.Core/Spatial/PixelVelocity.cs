namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional velocity, acceleration or running displacement: each axis is one
/// <see cref="SignedEightEight"/> value, added to a <see cref="FixedPosition{TSpace}"/> after
/// promotion to a displacement (<see cref="ToDisplacement"/>), or summed update by update with its fractions kept.
/// </summary>
/// <typeparam name="TSpace">The coordinate space the velocity is measured in.</typeparam>
public readonly partial record struct PixelVelocity<TSpace>(SignedEightEight X, SignedEightEight Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No velocity on either axis.</summary>
    public static PixelVelocity<TSpace> Zero => default;

    /// <summary>Replaces the X axis.</summary>
    public PixelVelocity<TSpace> WithX(SignedEightEight x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public PixelVelocity<TSpace> WithY(SignedEightEight y) => this with { Y = y };

    /// <summary>Negates the X speed, wrapping as a <see cref="SignedEightEight"/> does; Y is unchanged.</summary>
    public PixelVelocity<TSpace> ReversedX() => WithX(X.Negated());

    /// <summary>Negates the Y speed, wrapping as a <see cref="SignedEightEight"/> does; X is unchanged.</summary>
    public PixelVelocity<TSpace> ReversedY() => WithY(Y.Negated());

    /// <summary>Adds each axis's speeds, each sum wrapping as a <see cref="SignedEightEight"/> sum does.</summary>
    public static PixelVelocity<TSpace> operator +(PixelVelocity<TSpace> left, PixelVelocity<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>Scales each axis's speed by <paramref name="factor"/>, each product wrapping as a <see cref="SignedEightEight"/> does.</summary>
    public static PixelVelocity<TSpace> operator *(PixelVelocity<TSpace> vector, int factor) =>
        new(vector.X * factor, vector.Y * factor);

    /// <summary>Both axes' whole pixels, -128 through 127, each axis's fraction dropped toward negative infinity.</summary>
    public Offset16<TSpace> Whole => new(X.WholePixels, Y.WholePixels);

    /// <summary>The same velocity as the displacement positions move by.</summary>
    public FixedVector<TSpace> ToDisplacement() => new(X.ToSixteenSixteenDelta(), Y.ToSixteenSixteenDelta());
}
