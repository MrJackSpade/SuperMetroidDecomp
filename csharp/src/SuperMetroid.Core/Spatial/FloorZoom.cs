namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far a scene has zoomed its turning floor: how many floor pixels one screen pixel spans, authored and stepped in
/// 256ths of a floor pixel. <see cref="OwnSize"/> shows the floor at its own size, a larger zoom shows it smaller and a
/// smaller zoom shows it larger. The scenes' thresholds, starting zooms and per-update steps are all whole 256ths, so the
/// zoom keeps that count exactly; it is never negative.
/// </summary>
public readonly record struct FloorZoom : IComparable<FloorZoom>
{
    /// <summary>The 256ths one floor pixel per screen pixel counts.</summary>
    private const int TwoHundredFiftySixthsPerFloorPixel = 256;

    /// <summary>A zoom of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, zero or more.</summary>
    private FloorZoom(int twoHundredFiftySixths) => TwoHundredFiftySixths = Ensure.AtLeastZero(twoHundredFiftySixths);

    /// <summary>The zoom in 256ths of a floor pixel per screen pixel, zero or more.</summary>
    [IntegerQuantity("turning-floor zoom in 256ths of a floor pixel per screen pixel")]
    private int TwoHundredFiftySixths { get; }

    /// <summary>The zoom that shows the floor at its own size: one floor pixel to each screen pixel.</summary>
    public static FloorZoom OwnSize { get; } = new(TwoHundredFiftySixthsPerFloorPixel);

    /// <summary>A zoom of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative.</exception>
    public static FloorZoom FromTwoHundredFiftySixths(int twoHundredFiftySixths) => new(twoHundredFiftySixths);

    /// <summary>A zoom of <paramref name="floorPixels"/> whole floor pixels per screen pixel, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative.</exception>
    public static FloorZoom FromFloorPixelsPerScreenPixel(int floorPixels) =>
        new(checked(Ensure.AtLeastZero(floorPixels) * TwoHundredFiftySixthsPerFloorPixel));

    /// <summary>
    /// A zoom of <paramref name="numerator"/> <paramref name="denominator"/>ths of a floor pixel per screen pixel (#2555),
    /// zero or more, the denominator a power of two from 1 through 256: one and a half floor pixels to each screen pixel.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative, or the denominator is no power of two from 1 through 256.</exception>
    public static FloorZoom FromFloorPixelsPerScreenPixel(int numerator, int denominator) =>
        new(PowerOfTwoFraction.InFinestUnits(numerator, denominator, TwoHundredFiftySixthsPerFloorPixel));

    /// <summary>The whole floor pixels one screen pixel spans, the fraction dropped.</summary>
    [IntegerQuantity("whole floor pixels per screen pixel of a turning-floor zoom")]
    public int WholeFloorPixelsPerScreenPixel => TwoHundredFiftySixths / TwoHundredFiftySixthsPerFloorPixel;

    /// <summary>How many floor pixels one screen pixel spans, fraction included.</summary>
    public double FloorPixelsPerScreenPixel => (double)TwoHundredFiftySixths / TwoHundredFiftySixthsPerFloorPixel;

    /// <summary>The zoom widened by one <paramref name="step"/>: the floor shown smaller.</summary>
    public FloorZoom Widened(FloorZoomStep step) => new(checked(TwoHundredFiftySixths + step.TwoHundredFiftySixths));

    /// <summary>The zoom narrowed by one <paramref name="step"/>: the floor shown larger.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step would leave the zoom negative.</exception>
    public FloorZoom Narrowed(FloorZoomStep step) => Narrowed(step, 1);

    /// <summary>The zoom narrowed by <paramref name="steps"/> of <paramref name="step"/> (#2555): the floor shown larger.</summary>
    /// <param name="step">How far each step narrows the zoom.</param>
    /// <param name="steps">How many steps, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The steps would leave the zoom negative.</exception>
    public FloorZoom Narrowed(FloorZoomStep step, int steps) =>
        new(checked(TwoHundredFiftySixths - Ensure.AtLeastZero(steps) * step.TwoHundredFiftySixths));

    /// <inheritdoc />
    public int CompareTo(FloorZoom other) => TwoHundredFiftySixths.CompareTo(other.TwoHundredFiftySixths);

    /// <summary>Whether <paramref name="left"/> shows the floor larger than <paramref name="right"/>.</summary>
    public static bool operator <(FloorZoom left, FloorZoom right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> shows the floor smaller than <paramref name="right"/>.</summary>
    public static bool operator >(FloorZoom left, FloorZoom right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> shows the floor no smaller than <paramref name="right"/>.</summary>
    public static bool operator <=(FloorZoom left, FloorZoom right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> shows the floor no larger than <paramref name="right"/>.</summary>
    public static bool operator >=(FloorZoom left, FloorZoom right) => left.CompareTo(right) >= 0;

    /// <inheritdoc />
    public override string ToString() => $"{TwoHundredFiftySixths}/256 floor pixels per screen pixel";
}
