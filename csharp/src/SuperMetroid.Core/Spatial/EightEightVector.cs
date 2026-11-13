namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional signed 8.8 velocity or acceleration: each axis is one
/// <see cref="SignedEightEight"/> value, added to a <see cref="FixedPosition{TSpace}"/> after
/// promotion to 16.16 (<see cref="ToSixteenSixteen"/>).
/// </summary>
/// <typeparam name="TSpace">The coordinate space the velocity is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y 8.8 axes.")]
public readonly record struct EightEightVector<TSpace>(SignedEightEight X, SignedEightEight Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No velocity on either axis.</summary>
    public static EightEightVector<TSpace> Zero => default;

    /// <summary>
    /// The velocity of <paramref name="x"/> and <paramref name="y"/> 256ths of a pixel an update, each -32768 through 32767.
    /// </summary>
    public static EightEightVector<TSpace> FromTwoHundredFiftySixthsOfPixel(int x, int y) =>
        new(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(x), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(y));

    /// <summary>Replaces the X axis.</summary>
    public EightEightVector<TSpace> WithX(SignedEightEight x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public EightEightVector<TSpace> WithY(SignedEightEight y) => this with { Y = y };

    /// <summary>Negates the raw X value (wrapping within its signed 8.8 range); Y is unchanged.</summary>
    public EightEightVector<TSpace> ReversedX() => WithX(X.Negated());

    /// <summary>Negates the raw Y value (wrapping within its signed 8.8 range); X is unchanged.</summary>
    public EightEightVector<TSpace> ReversedY() => WithY(Y.Negated());

    /// <summary>Adds the raw 8.8 values of each axis, wrapping within the signed 8.8 range.</summary>
    public static EightEightVector<TSpace> operator +(EightEightVector<TSpace> left, EightEightVector<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>Scales the signed raw 8.8 values of each axis, wrapping within the signed 8.8 range.</summary>
    public static EightEightVector<TSpace> operator *(EightEightVector<TSpace> vector, int factor) =>
        new(SignedEightEight.Wrapped(vector.X.InTwoHundredFiftySixthsOfPixel * factor),
            SignedEightEight.Wrapped(vector.Y.InTwoHundredFiftySixthsOfPixel * factor));

    /// <summary>The same velocity as the 16.16 displacement used by position adders.</summary>
    public FixedVector<TSpace> ToSixteenSixteen() => new(X.ToSixteenSixteenDelta(), Y.ToSixteenSixteenDelta());
}
