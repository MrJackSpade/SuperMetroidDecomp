namespace SuperMetroid.Core.Spatial;

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

    /// <summary>A whole-pixel position with both subpixel words zero.</summary>
    public static FixedPosition<TSpace> FromWhole(Position16<TSpace> whole) =>
        new(new SnesFixedPosition(whole.X, 0), new SnesFixedPosition(whole.Y, 0));

    /// <summary>
    /// Replaces both whole words and keeps both subpixel words, as cartridge routines that
    /// store only the integer position words do.
    /// </summary>
    public FixedPosition<TSpace> WithWhole(Position16<TSpace> whole) =>
        new(X.WithWhole(whole.X), Y.WithWhole(whole.Y));

    /// <summary>Keeps both whole words and takes both subpixel words from <paramref name="source"/>.</summary>
    public FixedPosition<TSpace> WithFractionsOf(FixedPosition<TSpace> source) =>
        new(X.WithFraction(source.X.Fraction), Y.WithFraction(source.Y.Fraction));

    /// <summary>Replaces the X axis.</summary>
    public FixedPosition<TSpace> WithX(SnesFixedPosition x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedPosition<TSpace> WithY(SnesFixedPosition y) => this with { Y = y };

    /// <summary>Replaces only the X whole word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithWholeX(int whole) => WithX(X.WithWhole(whole));

    /// <summary>Replaces only the Y whole word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithWholeY(int whole) => WithY(Y.WithWhole(whole));

    /// <summary>Replaces only the X subpixel word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithFractionX(int fraction) => WithX(X.WithFraction(fraction));

    /// <summary>Replaces only the Y subpixel word, as cartridge routines that store one half do.</summary>
    public FixedPosition<TSpace> WithFractionY(int fraction) => WithY(Y.WithFraction(fraction));

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
