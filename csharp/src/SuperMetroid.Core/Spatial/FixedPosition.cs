namespace SuperMetroid.Core.Spatial;

/// <summary>A two-dimensional position: whole pixels and a fraction of a pixel on each axis, neither negative.</summary>
/// <typeparam name="TSpace">The coordinate space of the position.</typeparam>
/// <remarks>
/// Placing something at a whole-pixel point (<see cref="WithWhole"/>, <see cref="WithWholeX"/>,
/// <see cref="WithWholeY"/> and their wrapping forms) keeps the fractions of a pixel it already had, as the game's own
/// placements do (#2184). This is stated once here for every such placement: a Zoa sent back to the point it rose from
/// when its flight leaves the screen, or a Ninja Pirate set on its post's row when its dive lands, starts its next
/// movement from the fractions its last one left, and those fractions decide on which update that movement reaches each
/// next whole pixel. Only <see cref="FromWhole"/> and the fraction writes clear or set them.
/// </remarks>
[SpatialComponents("Spatial value type: owns its X and Y axes, each a whole pixel and its fraction.")]
public readonly record struct FixedPosition<TSpace>(FixedCoordinate X, FixedCoordinate Y)
    where TSpace : ISpatialSpace
{
    /// <summary>The origin, with no fraction of a pixel.</summary>
    public static FixedPosition<TSpace> Zero => default;

    /// <summary>The whole-pixel position, discarding the fractions of a pixel.</summary>
    public Position16<TSpace> Whole => new(X.Whole, Y.Whole);

    /// <summary>A whole-pixel position with no fraction of a pixel.</summary>
    public static FixedPosition<TSpace> FromWhole(Position16<TSpace> whole) =>
        new(new FixedCoordinate(whole.X, 0), new FixedCoordinate(whole.Y, 0));

    /// <summary>
    /// Replaces both whole-pixel coordinates and keeps both fractions of a pixel, as game code that
    /// sets only the whole-pixel position does.
    /// </summary>
    public FixedPosition<TSpace> WithWhole(Position16<TSpace> whole) =>
        new(X.WithWhole(whole.X), Y.WithWhole(whole.Y));

    /// <summary>Keeps both whole-pixel coordinates and takes both fractions of a pixel from <paramref name="source"/>.</summary>
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

    /// <summary>Replaces only the X fraction of a pixel, as game code that sets only the fraction does.</summary>
    public FixedPosition<TSpace> WithFractionX(int fraction) => WithX(X.WithFraction(fraction));

    /// <summary>Replaces only the Y fraction of a pixel, as game code that sets only the fraction does.</summary>
    public FixedPosition<TSpace> WithFractionY(int fraction) => WithY(Y.WithFraction(fraction));

    /// <summary>Translates the whole pixels by a signed offset (<see cref="CoordinateAxis.Wrap"/>); the fractions of a pixel are unchanged.</summary>
    public static FixedPosition<TSpace> operator +(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole + offset);

    /// <summary>Translates the whole pixels by the negated offset (<see cref="CoordinateAxis.Wrap"/>); the fractions of a pixel are unchanged.</summary>
    public static FixedPosition<TSpace> operator -(FixedPosition<TSpace> position, Offset16<TSpace> offset) =>
        position.WithWhole(position.Whole - offset);

    /// <summary>Moves only the X axis by <paramref name="delta"/>, the fraction's overflow passing on to the whole pixels, which wrap.</summary>
    public FixedPosition<TSpace> AdvanceX(SignedSixteenSixteen delta) => WithX(delta.AddTo(X.Whole, X.Fraction));

    /// <summary>Moves only the Y axis by <paramref name="delta"/>, the fraction's overflow passing on to the whole pixels, which wrap.</summary>
    public FixedPosition<TSpace> AdvanceY(SignedSixteenSixteen delta) => WithY(delta.AddTo(Y.Whole, Y.Fraction));

    /// <summary>Moves each axis by its velocity, the fraction's overflow passing on to the whole pixels, which wrap.</summary>
    public FixedPosition<TSpace> MovedBy(PixelVelocity<TSpace> velocity) =>
        WithX(X.MovedBy(velocity.X)).WithY(Y.MovedBy(velocity.Y));

    /// <summary>Applies a displacement on each axis, the fraction's overflow passing on to the whole pixels, which wrap.</summary>
    public FixedPosition<TSpace> Advance(FixedVector<TSpace> displacement) =>
        new(displacement.X.AddTo(X.Whole, X.Fraction), displacement.Y.AddTo(Y.Whole, Y.Fraction));
}
