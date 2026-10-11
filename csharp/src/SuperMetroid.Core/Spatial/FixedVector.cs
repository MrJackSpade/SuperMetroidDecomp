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
    public FixedVector<TSpace> WithWholeX(short wholePixels) => WithX(X.WithWholePixels(wholePixels));

    /// <summary>Replaces only the Y whole word, as cartridge routines that store one half do.</summary>
    public FixedVector<TSpace> WithWholeY(short wholePixels) => WithY(Y.WithWholePixels(wholePixels));

    /// <summary>Replaces only the Y fraction word, as cartridge routines that store one half do.</summary>
    public FixedVector<TSpace> WithFractionY(ushort fraction) => WithY(Y.WithFraction(fraction));

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

    /// <summary>A whole-pixel position with zero subpixel words.</summary>
    public static FixedPosition<TSpace> FromWhole(Position16<TSpace> whole) =>
        new(new SnesFixedPosition(whole.X, 0), new SnesFixedPosition(whole.Y, 0));

    /// <summary>Replaces the X axis.</summary>
    public FixedPosition<TSpace> WithX(SnesFixedPosition x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedPosition<TSpace> WithY(SnesFixedPosition y) => this with { Y = y };

    /// <summary>Replaces both whole words, keeping both subpixel words.</summary>
    public FixedPosition<TSpace> WithWhole(Position16<TSpace> whole) =>
        new(X.WithWhole(whole.X), Y.WithWhole(whole.Y));

    /// <summary>Replaces only the X whole word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithWholeX(ushort whole) => WithX(X.WithWhole(whole));

    /// <summary>Replaces only the Y whole word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithWholeY(ushort whole) => WithY(Y.WithWhole(whole));

    /// <summary>Replaces only the X subpixel word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithFractionX(ushort fraction) => WithX(X.WithFraction(fraction));

    /// <summary>Replaces only the Y subpixel word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithFractionY(ushort fraction) => WithY(Y.WithFraction(fraction));

    /// <summary>Translates the whole words by a signed offset, wrapping at sixteen bits; subpixels are unchanged.</summary>
    public static FixedPosition<TSpace> operator +(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole + offset);

    /// <summary>Translates the whole words by the negated offset, wrapping at sixteen bits; subpixels are unchanged.</summary>
    public static FixedPosition<TSpace> operator -(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole - offset);

    /// <summary>Moves only the X axis by a signed 16.16 delta with the native 32-bit carry and wrap.</summary>
    public FixedPosition<TSpace> AdvanceX(SnesSignedSixteenSixteen delta) => WithX(delta.AddTo(X.Whole, X.Fraction));

    /// <summary>Moves only the Y axis by a signed 16.16 delta with the native 32-bit carry and wrap.</summary>
    public FixedPosition<TSpace> AdvanceY(SnesSignedSixteenSixteen delta) => WithY(delta.AddTo(Y.Whole, Y.Fraction));

    /// <summary>Applies a displacement with the native 32-bit carry and wrap on each axis.</summary>
    public FixedPosition<TSpace> Advance(FixedVector<TSpace> displacement) =>
        new(displacement.X.AddTo(X.Whole, X.Fraction), displacement.Y.AddTo(Y.Whole, Y.Fraction));
}
