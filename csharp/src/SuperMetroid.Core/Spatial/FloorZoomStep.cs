namespace SuperMetroid.Core.Spatial;

/// <summary>
/// How far one step moves a scene's turning-floor zoom (<see cref="FloorZoom"/>): a whole number of 256ths of a floor pixel
/// per screen pixel, at least one. The scenes widen or narrow their zoom by a fixed step each update they zoom.
/// </summary>
public readonly record struct FloorZoomStep
{
    /// <summary>A step of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, at least one.</summary>
    private FloorZoomStep(int twoHundredFiftySixths) =>
        TwoHundredFiftySixths = Ensure.BetweenInclusive(1, int.MaxValue, twoHundredFiftySixths);

    /// <summary>The step in 256ths of a floor pixel per screen pixel, at least one.</summary>
    [IntegerQuantity("turning-floor zoom step in 256ths of a floor pixel per screen pixel")]
    public int TwoHundredFiftySixths { get; }

    /// <summary>A step of <paramref name="twoHundredFiftySixths"/> 256ths of a floor pixel per screen pixel, at least one.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The step is zero or negative.</exception>
    public static FloorZoomStep FromTwoHundredFiftySixths(int twoHundredFiftySixths) => new(twoHundredFiftySixths);

    /// <inheritdoc />
    public override string ToString() => $"{TwoHundredFiftySixths}/256 floor pixels per screen pixel";
}
