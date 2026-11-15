namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The gunship's horizontal shake while it emerges from the exploding planet (#1902): the authored displacements the
/// floor moves by, one per update, in the order the escape plays them. Each sequence repeats until its leg ends, and
/// each full pass moves the ship exactly two pixels to the right.
/// </summary>
public static class EndingShipShake
{
    /// <summary>Half a pixel to the right.</summary>
    private static readonly SignedSixteenSixteen HalfRight = SignedSixteenSixteen.FromSixteenthsOfPixel(8);

    /// <summary>Half a pixel to the left.</summary>
    private static readonly SignedSixteenSixteen HalfLeft = -HalfRight;

    /// <summary>One pixel to the right.</summary>
    private static readonly SignedSixteenSixteen OneRight = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>One pixel to the left.</summary>
    private static readonly SignedSixteenSixteen OneLeft = -OneRight;

    /// <summary>
    /// The fast spin's shake, half a pixel at a time: four steps right and two left, twice, then two right and two left.
    /// </summary>
    public static IReadOnlyList<SignedSixteenSixteen> Fast { get; } =
    [
        HalfRight, HalfRight, HalfRight, HalfRight, HalfLeft, HalfLeft,
        HalfRight, HalfRight, HalfRight, HalfRight, HalfLeft, HalfLeft,
        HalfRight, HalfRight, HalfLeft, HalfLeft,
    ];

    /// <summary>The slow spin's shake, a pixel at a time: three steps right, two left, two right and one left.</summary>
    public static IReadOnlyList<SignedSixteenSixteen> Slow { get; } =
    [
        OneRight, OneRight, OneRight, OneLeft, OneLeft, OneRight, OneRight, OneLeft,
    ];
}
