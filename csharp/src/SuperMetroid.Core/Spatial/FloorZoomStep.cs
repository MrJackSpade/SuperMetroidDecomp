namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far one step moves a scene's turning-floor zoom (<see cref="FloorZoom"/>): a whole number of 256ths of a floor pixel
/// per screen pixel, at least one. The scenes widen or narrow their zoom by a fixed step each update they zoom.
/// </summary>
public readonly partial record struct FloorZoomStep
{
    /// <summary>A step of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, at least one.</summary>
    internal FloorZoomStep(int twoHundredFiftySixths) =>
        TwoHundredFiftySixths = Ensure.BetweenInclusive(1, int.MaxValue, twoHundredFiftySixths);

    /// <summary>The step in 256ths of a floor pixel per screen pixel, at least one.</summary>
    [IntegerQuantity("turning-floor zoom step in 256ths of a floor pixel per screen pixel")]
    public int TwoHundredFiftySixths { get; }

    /// <summary>
    /// A step of <paramref name="numerator"/> <paramref name="denominator"/>ths of a floor pixel per screen pixel (#2555),
    /// more than none, the denominator a power of two from 1 through 256: a sixty-fourth of a floor pixel each update.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is not more than none, or the denominator is no power of two from 1 through 256.</exception>
    public static FloorZoomStep FromFloorPixelsPerScreenPixel(int numerator, int denominator) =>
        new(PowerOfTwoFraction.InFinestUnits(numerator, denominator, TwoHundredFiftySixthsPerFloorPixel));

    /// <summary>The 256ths one floor pixel per screen pixel counts.</summary>
    private const int TwoHundredFiftySixthsPerFloorPixel = 256;

    /// <inheritdoc />
    public override string ToString() => $"{TwoHundredFiftySixths}/256 floor pixels per screen pixel";
}
