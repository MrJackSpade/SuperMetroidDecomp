namespace SuperMetroid.Core.Spatial;

/// <summary>
/// One axis's share of a unit direction (#2110): the game's sine or cosine of a direction, minus one through one in
/// 256ths of the whole, exactly as <see cref="Trigonometry.SignedSine"/> samples it. A length laid along the direction
/// reaches this share of itself along the axis; a turn carries a point along the axis at this share of its speed.
/// </summary>
public readonly record struct UnitComponent
{
    /// <summary>The 256ths in the whole of a direction.</summary>
    private const int Whole = 256;

    /// <summary>The component of <paramref name="twoFiftySixths"/> 256ths of the whole, minus one through one.</summary>
    private UnitComponent(int twoFiftySixths) => TwoFiftySixths = Ensure.BetweenInclusive(-Whole, Whole, twoFiftySixths);

    /// <summary>The share in 256ths of the whole, -256 through 256: negative toward the axis's decreasing side.</summary>
    [IntegerQuantity("signed share of a unit direction in 256ths")]
    private int TwoFiftySixths { get; }

    /// <summary>The game's sine of <paramref name="angle"/>: the share of its direction along the axis a quarter turn on from zero.</summary>
    public static UnitComponent SineOf(TrigonometryAngle angle) => new(Trigonometry.SignedSine(angle));

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
        int reach = EightEight.Multiply(Math.Abs(TwoFiftySixths), Ensure.BetweenInclusive(0, int.MaxValue, length));
        return IsNegative ? -reach : reach;
    }

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
