namespace SuperMetroid.Core.Spatial;

/// <summary>A two-dimensional unsigned 1/65536-pixel position: whole pixels and subpixels on each axis.</summary>
/// <typeparam name="TSpace">The coordinate space of the position.</typeparam>
[SpatialComponents("Spatial value type: owns its X and Y fixed-point axes.")]
public readonly record struct FixedPosition<TSpace>(FixedCoordinate X, FixedCoordinate Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin with no subpixel.</summary>
    public static FixedPosition<TSpace> Zero => default;

    /// <summary>The whole-pixel position, discarding the subpixels.</summary>
    public Position16<TSpace> Whole => new(X.Whole, Y.Whole);

    /// <summary>A whole-pixel position with both subpixels zero.</summary>
    public static FixedPosition<TSpace> FromWhole(Position16<TSpace> whole) =>
        new(new FixedCoordinate(whole.X, 0), new FixedCoordinate(whole.Y, 0));

    /// <summary>
    /// Replaces both whole-pixel coordinates and keeps both subpixels, as game code that
    /// sets only the whole-pixel position does.
    /// </summary>
    public FixedPosition<TSpace> WithWhole(Position16<TSpace> whole) =>
        new(X.WithWhole(whole.X), Y.WithWhole(whole.Y));

    /// <summary>Keeps both whole-pixel coordinates and takes both subpixels from <paramref name="source"/>.</summary>
    public FixedPosition<TSpace> WithFractionsOf(FixedPosition<TSpace> source) =>
        new(X.WithFraction(source.X.Fraction), Y.WithFraction(source.Y.Fraction));

    /// <summary>Replaces the X axis.</summary>
    public FixedPosition<TSpace> WithX(FixedCoordinate x) => this with { X = x };

    /// <summary>Replaces the Y axis.</summary>
    public FixedPosition<TSpace> WithY(FixedCoordinate y) => this with { Y = y };

    /// <summary>Replaces only the X whole pixels, as game code that sets only the whole pixels does.</summary>
    public FixedPosition<TSpace> WithWholeX(int whole) => WithX(X.WithWhole(whole));

    /// <summary>Replaces only the Y whole pixels, as game code that sets only the whole pixels does.</summary>
    public FixedPosition<TSpace> WithWholeY(int whole) => WithY(Y.WithWhole(whole));

    /// <summary>
    /// Replaces only the X whole pixels with a computed coordinate, which wraps within the coordinate range
    /// (<see cref="CoordinateAxis.Wrap"/>), as game code that sets only the whole pixels of a moved position does.
    /// </summary>
    public FixedPosition<TSpace> WithWrappedWholeX(int whole) => WithX(X.WithWrappedWhole(whole));

    /// <summary>
    /// Replaces only the Y whole pixels with a computed coordinate, which wraps within the coordinate range
    /// (<see cref="CoordinateAxis.Wrap"/>), as game code that sets only the whole pixels of a moved position does.
    /// </summary>
    public FixedPosition<TSpace> WithWrappedWholeY(int whole) => WithY(Y.WithWrappedWhole(whole));

    /// <summary>Replaces only the X subpixels, as game code that sets only the subpixels does.</summary>
    public FixedPosition<TSpace> WithFractionX(int fraction) => WithX(X.WithFraction(fraction));

    /// <summary>Replaces only the Y subpixels, as game code that sets only the subpixels does.</summary>
    public FixedPosition<TSpace> WithFractionY(int fraction) => WithY(Y.WithFraction(fraction));

    /// <summary>Translates the whole pixels by a signed offset, wrapping within the coordinate range; subpixels are unchanged.</summary>
    public static FixedPosition<TSpace> operator +(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole + offset);

    /// <summary>Translates the whole pixels by the negated offset, wrapping within the coordinate range; subpixels are unchanged.</summary>
    public static FixedPosition<TSpace> operator -(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole - offset);

    /// <summary>Moves only the X axis by a signed 1/65536-pixel delta, the fraction's overflow passing on, wrapping in 1/65536 pixels.</summary>
    public FixedPosition<TSpace> AdvanceX(SignedSixteenSixteen delta) => WithX(delta.AddTo(X.Whole, X.Fraction));

    /// <summary>Moves only the Y axis by a signed 1/65536-pixel delta, the fraction's overflow passing on, wrapping in 1/65536 pixels.</summary>
    public FixedPosition<TSpace> AdvanceY(SignedSixteenSixteen delta) => WithY(delta.AddTo(Y.Whole, Y.Fraction));

    /// <summary>Applies a displacement on each axis, the fraction's overflow passing on, wrapping in 1/65536 pixels.</summary>
    public FixedPosition<TSpace> Advance(FixedVector<TSpace> displacement) =>
        new(displacement.X.AddTo(X.Whole, X.Fraction), displacement.Y.AddTo(Y.Whole, Y.Fraction));
}
