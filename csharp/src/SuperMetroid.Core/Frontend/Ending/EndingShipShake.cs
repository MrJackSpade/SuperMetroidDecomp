namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The gunship's horizontal shake while it emerges from the exploding planet (#1902): the authored displacements the
/// floor moves by, one per update, in the order the escape plays them. Each sequence repeats until its leg ends, and
/// each full pass moves the ship exactly two pixels to the right.
/// </summary>
public static partial class EndingShipShake
{
    /// <summary>Half a pixel to the right.</summary>
    private static readonly SignedSixteenSixteen HalfRight = SignedSixteenSixteen.FromPixelFraction(1, 2);

    /// <summary>Half a pixel to the left.</summary>
    private static readonly SignedSixteenSixteen HalfLeft = -HalfRight;

    /// <summary>One pixel to the right.</summary>
    private static readonly SignedSixteenSixteen OneRight = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>One pixel to the left.</summary>
    private static readonly SignedSixteenSixteen OneLeft = -OneRight;

    /// <summary>
    /// The fast spin's shake, half a pixel at a time, right and left in turn.
    /// </summary>
    public static IReadOnlyList<SignedSixteenSixteen> Fast { get; } =
    [
        HalfRight, HalfRight, HalfRight, HalfRight, HalfLeft, HalfLeft,
        HalfRight, HalfRight, HalfRight, HalfRight, HalfLeft, HalfLeft,
        HalfRight, HalfRight, HalfLeft, HalfLeft,
    ];

    /// <summary>The slow spin's shake, a pixel at a time, right and left in turn.</summary>
    public static IReadOnlyList<SignedSixteenSixteen> Slow { get; } =
    [
        OneRight, OneRight, OneRight, OneLeft, OneLeft, OneRight, OneRight, OneLeft,
    ];
}
