using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Spatial;

/// <summary>
/// A two-dimensional signed native 16.16 displacement or per-frame velocity. Each axis is one
/// <see cref="SnesSignedSixteenSixteen"/>, so whole/fraction assembly, carry and 32-bit wrap live
/// in exactly one place.
/// </summary>
/// <typeparam name="TSpace">The coordinate space the displacement is measured in.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y fixed-point axes.")]
public readonly record struct FixedVector<TSpace>(SnesSignedSixteenSixteen X, SnesSignedSixteenSixteen Y)
    where TSpace : ISpatialSpace
{
    /// <summary>No displacement.</summary>
    public static FixedVector<TSpace> Zero => default;

    /// <summary>Assembles both axes from the native whole and fraction words.</summary>
    [SpatialComponents("Native codec: the four WRAM words of one displacement.")]
    public static FixedVector<TSpace> FromWords(ushort xWhole, ushort xFraction, ushort yWhole, ushort yFraction) =>
        new(SnesSignedSixteenSixteen.FromParts(unchecked((short)xWhole), xFraction),
            SnesSignedSixteenSixteen.FromParts(unchecked((short)yWhole), yFraction));

    /// <summary>Replaces the X axis.</summary>
    public FixedVector<TSpace> WithX(SnesSignedSixteenSixteen x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedVector<TSpace> WithY(SnesSignedSixteenSixteen y) => this with { Y = y };

    /// <summary>Adds two displacements, carrying fractions and wrapping each axis at 32 bits.</summary>
    public static FixedVector<TSpace> operator +(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>Subtracts two displacements, borrowing fractions and wrapping each axis at 32 bits.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X - right.X, left.Y - right.Y);
}

/// <summary>A two-dimensional unsigned native 16.16 position: a whole word and a subpixel word per axis.</summary>
/// <typeparam name="TSpace">The coordinate space of the position.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y fixed-point axes.")]
public readonly record struct FixedPosition<TSpace>(SnesFixedPosition X, SnesFixedPosition Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin with no subpixel.</summary>
    public static FixedPosition<TSpace> Zero => default;

    /// <summary>The whole-pixel position, discarding the subpixel words.</summary>
    public Position16<TSpace> Whole => new(X.Whole, Y.Whole);

    /// <summary>Replaces the X axis.</summary>
    public FixedPosition<TSpace> WithX(SnesFixedPosition x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedPosition<TSpace> WithY(SnesFixedPosition y) => this with { Y = y };

    /// <summary>Applies a displacement with the native 32-bit carry and wrap on each axis.</summary>
    public FixedPosition<TSpace> Advance(FixedVector<TSpace> displacement) =>
        new(displacement.X.AddTo(X.Whole, X.Fraction), displacement.Y.AddTo(Y.Whole, Y.Fraction));
}
