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
    public int TwoHundredFiftySixths { get; }

    /// <summary>The zoom that shows the floor at its own size: one floor pixel to each screen pixel.</summary>
    public static FloorZoom OwnSize { get; } = new(TwoHundredFiftySixthsPerFloorPixel);

    /// <summary>A zoom of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative.</exception>
    public static FloorZoom FromTwoHundredFiftySixths(int twoHundredFiftySixths) => new(twoHundredFiftySixths);

    /// <summary>A zoom of <paramref name="floorPixels"/> whole floor pixels per screen pixel, zero or more.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is negative.</exception>
    public static FloorZoom FromFloorPixelsPerScreenPixel(int floorPixels) =>
        new(checked(Ensure.AtLeastZero(floorPixels) * TwoHundredFiftySixthsPerFloorPixel));

    /// <summary>The whole floor pixels one screen pixel spans, the fraction dropped.</summary>
    [IntegerQuantity("whole floor pixels per screen pixel of a turning-floor zoom")]
    public int WholeFloorPixelsPerScreenPixel => TwoHundredFiftySixths / TwoHundredFiftySixthsPerFloorPixel;

    /// <summary>How many floor pixels one screen pixel spans, fraction included.</summary>
    public double FloorPixelsPerScreenPixel => (double)TwoHundredFiftySixths / TwoHundredFiftySixthsPerFloorPixel;

    /// <summary>The zoom widened by <paramref name="twoHundredFiftySixths"/> 256ths: the floor shown smaller.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step would leave the zoom negative.</exception>
    public FloorZoom Widened(int twoHundredFiftySixths) => new(checked(TwoHundredFiftySixths + twoHundredFiftySixths));

    /// <summary>The zoom narrowed by <paramref name="twoHundredFiftySixths"/> 256ths: the floor shown larger.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step would leave the zoom negative.</exception>
    public FloorZoom Narrowed(int twoHundredFiftySixths) => new(checked(TwoHundredFiftySixths - twoHundredFiftySixths));

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
