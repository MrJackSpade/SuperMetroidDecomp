namespace SuperMetroid.Core.Spatial;

/// <summary>
/// One axis's share of a direction read from the game's unsigned half-wave (#2539,
/// <see cref="Trigonometry.UnsignedHalfWave"/>): the half-wave's size, from none to just short of the whole, with the side of
/// the axis the direction points to. Angle zero points right and a quarter turn on points up. A speed laid along the
/// direction moves along the axis at this share of itself, the fraction finer than the share's precision dropped.
/// </summary>
internal readonly record struct FineUnitComponent
{
    /// <summary>The share of the whole in each step of the half-wave: 65536 to the whole.</summary>
    private const int Whole = 65536;

    /// <summary>The component of <paramref name="share"/> steps of the whole, negative toward the axis's decreasing side.</summary>
    private FineUnitComponent(int share) => Share = Ensure.BetweenInclusive(-(Whole - 1), Whole - 1, share);

    /// <summary>The share, negative toward the axis's decreasing side; its size is the half-wave's.</summary>
    [IntegerQuantity("signed share of a direction in steps of the half-wave")]
    private int Share { get; }

    /// <summary>
    /// The share of <paramref name="angle"/> across, its cosine: the half-wave a quarter turn on, pointing left where that
    /// turned angle lies in the turn's second half.
    /// </summary>
    internal static FineUnitComponent AcrossOf(TrigonometryAngle angle) => SineOf(angle.TurnedBy(TrigonometryAngle.QuarterTurn));

    /// <summary>
    /// The share of <paramref name="angle"/> down, its sine reversed: the half-wave a half turn on, pointing up where that
    /// turned angle lies in the turn's second half.
    /// </summary>
    internal static FineUnitComponent DownOf(TrigonometryAngle angle) => SineOf(angle.TurnedBy(TrigonometryAngle.HalfTurn));

    /// <summary>The half-wave of <paramref name="angle"/>, reversed where the angle lies in the turn's second half.</summary>
    private static FineUnitComponent SineOf(TrigonometryAngle angle)
    {
        int size = Trigonometry.UnsignedHalfWave(angle.StepsIntoHalfTurn);
        return new(angle.IsInSecondHalf ? -size : size);
    }

    /// <summary>
    /// The speed along the axis of a point moving at <paramref name="wholePixels"/> whole pixels per update along the
    /// direction: the share's size times the speed, every finer fraction dropped, then given the component's side, so the
    /// speed rounds toward zero and a speed toward the decreasing side is exactly the reverse of the same speed the other way.
    /// </summary>
    /// <param name="wholePixels">The speed along the direction in whole pixels per update, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    internal SignedSixteenSixteen SpeedAt(int wholePixels)
    {
        SignedSixteenSixteen size = UnsignedSixteenSixteen.ShareOfWholePixels(wholePixels, Math.Abs(Share)).AsDisplacement();
        return Share < 0 ? -size : size;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Share}/{Whole}";
}
