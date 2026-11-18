namespace SuperMetroid.Core.Spatial;

/// <summary>
/// One axis's share of a unit direction (#2110): the game's sine or cosine of a direction, minus one through one in
/// 256ths of the whole, exactly as <see cref="Trigonometry.SignedSine"/> samples it. A length laid along the direction
/// reaches this share of itself along the axis; a turn carries a point along the axis at this share of its speed.
/// </summary>
public readonly record struct UnitComponent
{
    /// <summary>The 256ths in the whole of a direction, as a power of two.</summary>
    private const int WholeExponent = 8;

    /// <summary>The 256ths in the whole of a direction.</summary>
    private const int Whole = 1 << WholeExponent;

    /// <summary>The component of <paramref name="twoFiftySixths"/> 256ths of the whole, minus one through one.</summary>
    private UnitComponent(int twoFiftySixths) => TwoFiftySixths = Ensure.BetweenInclusive(-Whole, Whole, twoFiftySixths);

    /// <summary>The share in 256ths of the whole, -256 through 256: negative toward the axis's decreasing side.</summary>
    [IntegerQuantity("signed share of a unit direction in 256ths")]
    private int TwoFiftySixths { get; }

    /// <summary>The game's sine of <paramref name="angle"/>: the share of its direction along the axis a quarter turn on from zero.</summary>
    public static UnitComponent SineOf(TrigonometryAngle angle) => new(Trigonometry.SignedSine(angle));

    /// <summary>The 256ths of the whole in each step of the fine sine's coarse reading, which counts 128ths of it.</summary>
    private const int TwoFiftySixthsPerCoarseStep = 2;

    /// <summary>
    /// The fine sine (<see cref="Trigonometry.FineSignedSine"/>) of <paramref name="angle"/> read coarsely (#2453): its size
    /// as <see cref="Trigonometry.CoarseSizeOfFineSine"/> reads it, 128ths of the whole, with the fine sine's sign.
    /// </summary>
    public static UnitComponent CoarseFineSineOf(TrigonometryAngle angle)
    {
        int size = Trigonometry.CoarseSizeOfFineSine(angle) * TwoFiftySixthsPerCoarseStep;
        return new(Trigonometry.FineSignedSine(angle) < 0 ? -size : size);
    }

    /// <summary>The component pointing the other way along the axis.</summary>
    public UnitComponent Reversed => new(-TwoFiftySixths);

    /// <summary>Whether the component points toward the axis's decreasing side.</summary>
    public bool IsNegative => TwoFiftySixths < 0;

    /// <summary>
    /// How far along the axis a length of <paramref name="length"/> whole pixels laid along the direction reaches: the
    /// length times the component's size with its 256ths dropped, then the component's sign restored, so the reach
    /// rounds toward zero and a whole component reaches the full length.
    /// </summary>
    /// <param name="length">The length in whole pixels, zero or more.</param>
    public int ReachOf(int length)
    {
        int reach = Math.Abs(TwoFiftySixths) * Ensure.BetweenInclusive(0, int.MaxValue, length) / Whole;
        return IsNegative ? -reach : reach;
    }

    /// <summary>
    /// How far along the axis a length of <paramref name="length"/> whole pixels laid along the direction reaches, in
    /// whole pixels rounded toward negative infinity: a reach toward the axis's decreasing side that falls short of a
    /// whole pixel lies a whole pixel further out, unlike <see cref="ReachOf"/>, which rounds toward zero.
    /// </summary>
    /// <param name="length">The length in whole pixels, negative for a length laid the other way along the direction.</param>
    public int ReachRoundedDownOf(int length) => TwoFiftySixths * length >> WholeExponent;

    /// <summary>The pixels in each pair a reach in whole pairs (<see cref="ReachRoundedDownToPixelPairsOf"/>) counts, as a power of two.</summary>
    private const int PairExponent = 1;

    /// <summary>
    /// How far along the axis a length of <paramref name="length"/> whole pixels laid along the direction reaches, in whole
    /// pairs of pixels rounded toward negative infinity (#2453): the reach halved and rounded down, then doubled, so it is
    /// always an even count of pixels and a reach toward the decreasing side lies a pair further out than the pairs it
    /// fills.
    /// </summary>
    /// <param name="length">The length in whole pixels, negative for a length laid the other way along the direction.</param>
    public int ReachRoundedDownToPixelPairsOf(int length) =>
        TwoFiftySixths * length >> (WholeExponent + PairExponent) << PairExponent;

    /// <summary>
    /// How far along the axis a length of <paramref name="length"/> whole pixels laid along the direction reaches, every
    /// 256th of a pixel kept, so reaches added one after another lose nothing.
    /// </summary>
    /// <param name="length">The length in whole pixels.</param>
    public SignedSixteenSixteen ExactReachOf(int length) =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(TwoFiftySixths * length);

    /// <summary>
    /// The speed, in pixels an update, at which a turn at <paramref name="speed"/> carries a point along the axis when
    /// each step of turn counts as a pixel: the component's size times the speed's, with every fraction kept.
    /// </summary>
    /// <exception cref="OverflowException">The product is faster than an unsigned speed holds.</exception>
    public UnsignedSixteenSixteen SizeTimes(AngularSpeed speed) =>
        UnsignedSixteenSixteen.ProductOfTwoFiftySixths(Math.Abs(TwoFiftySixths), speed.Magnitude.FractionsOfStep);

    /// <inheritdoc />
    public override string ToString() => $"{TwoFiftySixths}/256";
}
