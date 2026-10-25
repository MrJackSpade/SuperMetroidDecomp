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

    /// <summary>Assembles both axes from the native whole and fraction words, each 0 through 65535.</summary>
    [SpatialComponents("Native codec: the four WRAM words of one displacement.")]
    public static FixedVector<TSpace> FromWords(int xWhole, int xFraction, int yWhole, int yFraction) =>
        new(SnesSignedSixteenSixteen.FromParts(unchecked((short)Ensure.BetweenInclusive(0, ushort.MaxValue, xWhole)), xFraction),
            SnesSignedSixteenSixteen.FromParts(unchecked((short)Ensure.BetweenInclusive(0, ushort.MaxValue, yWhole)), yFraction));

    /// <summary>A whole-pixel displacement with zero fraction words.</summary>
    public static FixedVector<TSpace> FromWholePixels(Offset16<TSpace> offset) =>
        new(SnesSignedSixteenSixteen.FromParts(offset.X, 0), SnesSignedSixteenSixteen.FromParts(offset.Y, 0));

    /// <summary>Replaces the X axis.</summary>
    public FixedVector<TSpace> WithX(SnesSignedSixteenSixteen x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedVector<TSpace> WithY(SnesSignedSixteenSixteen y) => this with { Y = y };

    /// <summary>Replaces both whole words, keeping both fraction words.</summary>
    public FixedVector<TSpace> WithWhole(Offset16<TSpace> whole) =>
        new(X.WithWholePixels(whole.X), Y.WithWholePixels(whole.Y));

    /// <summary>Replaces only the X whole word, as cartridge routines that store one half do.</summary>
    public FixedVector<TSpace> WithWholeX(int wholePixels) => WithX(X.WithWholePixels(wholePixels));

    /// <summary>Replaces only the Y whole word, as cartridge routines that store one half do.</summary>
    public FixedVector<TSpace> WithWholeY(int wholePixels) => WithY(Y.WithWholePixels(wholePixels));

    /// <summary>Replaces only the Y fraction word, as cartridge routines that store one half do.</summary>
    public FixedVector<TSpace> WithFractionY(int fraction) => WithY(Y.WithFraction(fraction));

    /// <summary>Adds two displacements, carrying fractions and wrapping each axis at 32 bits.</summary>
    public static FixedVector<TSpace> operator +(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X + right.X, left.Y + right.Y);

    /// <summary>Adds whole pixels to each axis, wrapping each whole word at sixteen bits; fractions are unchanged.</summary>
    public static FixedVector<TSpace> operator +(FixedVector<TSpace> vector, Offset16<TSpace> wholePixels) =>
        vector + FromWholePixels(wholePixels);

    /// <summary>Subtracts whole pixels from each axis, wrapping each whole word at sixteen bits; fractions are unchanged.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> vector, Offset16<TSpace> wholePixels) =>
        vector - FromWholePixels(wholePixels);

    /// <summary>Subtracts two displacements, borrowing fractions and wrapping each axis at 32 bits.</summary>
    public static FixedVector<TSpace> operator -(FixedVector<TSpace> left, FixedVector<TSpace> right) =>
        new(left.X - right.X, left.Y - right.Y);
}
