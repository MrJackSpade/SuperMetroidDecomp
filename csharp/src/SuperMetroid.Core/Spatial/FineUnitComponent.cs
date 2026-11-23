namespace SuperMetroid.Core.Spatial;

/// <summary>
/// One axis's share of a direction read from the game's unsigned half-wave (#2539,
/// <see cref="Trigonometry.UnsignedHalfWave"/>): the half-wave's size, from none to just short of the whole, with the side of
/// the axis the direction points to. Angle zero points right and a quarter turn on points up. A speed laid along the
/// direction moves along the axis at this share of itself, the fraction finer than the share's precision dropped.
/// </summary>
internal readonly partial record struct FineUnitComponent
{
    /// <summary>
    /// The component the half-wave at <paramref name="axisAngle"/> gives (#2569): its share of the whole
    /// (<see cref="Trigonometry.HalfWaveShare"/>), less than one whole, toward the axis's decreasing side where the angle
    /// lies in the turn's second half.
    /// </summary>
    private FineUnitComponent(TrigonometryAngle axisAngle) =>
        _share = axisAngle.WithSineSign(Trigonometry.HalfWaveShare(axisAngle.StepsIntoHalfTurn).AsDisplacement());

    /// <summary>
    /// The share of the whole, read once as the component is made: under one whole either way, negative toward
    /// the axis's decreasing side.
    /// </summary>
    private readonly SignedSixteenSixteen _share;

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
    private static FineUnitComponent SineOf(TrigonometryAngle angle) => new(angle);

    /// <summary>
    /// The speed along the axis of a point moving at <paramref name="wholePixels"/> whole pixels per update along the
    /// direction: the share times the speed, which keeps every fraction the share holds, so a speed toward the decreasing
    /// side is exactly the reverse of the same speed the other way.
    /// </summary>
    /// <param name="wholePixels">The speed along the direction in whole pixels per update, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="wholePixels"/> is negative.</exception>
    internal SignedSixteenSixteen SpeedAt(int wholePixels) => _share * Ensure.AtLeastZero(wholePixels);

    /// <inheritdoc />
    public override string ToString() => _share.ToString();
}
