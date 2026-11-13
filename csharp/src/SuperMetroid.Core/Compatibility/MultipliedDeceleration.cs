namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How much horizontal speed Samus loses in one decelerating update while a deceleration multiplier is set (#1621): her
/// table's deceleration scaled by the multiplier in 256ths, its whole pixels and its fraction each scaled on their own.
/// The whole pixels' product keeps only whole pixels, its share of a pixel dropped instead of reaching the fraction, and
/// the fraction counts only its 256ths of a pixel before it is scaled. A deceleration of one and a half pixels at a
/// multiplier of 128 therefore removes a quarter of a pixel instead of three quarters: the whole pixel's half-pixel
/// product is dropped and only the half pixel's quarter is kept. Without a multiplier the table's deceleration applies
/// as it is.
/// </summary>
/// <remarks>
/// Consulted on every update Samus's horizontal base speed decelerates or turns, before the speed is tested for having
/// passed zero. Evidence: the original speed calculation multiplies the deceleration's whole pixels by the multiplier
/// keeping the product's upper byte as the whole pixels, and multiplies the upper byte of its fraction by the multiplier
/// keeping the whole product as the fraction, the two products never combined; the deceleration-multiplier check
/// compares the rule with those two products for every multiplier over boundary whole pixels and fractions.
/// </remarks>
[CompatibilityService("Samus's multiplied deceleration scales its whole pixels and its fraction by the multiplier in 256ths each on their own, dropping the whole pixels' remainder and the fraction's finest steps")]
public static class MultipliedDeceleration
{
    /// <summary>The steps the multiplier counts a whole deceleration in, and the fraction steps one scaled step of the fraction covers.</summary>
    private const int ScaleSteps = 256;

    /// <summary>
    /// The largest multiplier: one 256th short of a whole deceleration, as the multiplier counts 256ths of one below a whole
    /// and a whole would leave the table's deceleration as it is, which a zero multiplier already selects.
    /// </summary>
    public const int LargestMultiplier = ScaleSteps - 1;

    /// <summary>
    /// The speed one decelerating update removes for <paramref name="deceleration"/> at <paramref name="multiplier"/>,
    /// 0 through 255 256ths: the deceleration itself when the multiplier is zero.
    /// </summary>
    /// <param name="deceleration">The speed table's deceleration.</param>
    /// <param name="multiplier">The deceleration multiplier in 256ths, 0 through 255; zero sets none.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="multiplier"/> is outside 0 through 255.</exception>
    public static SignedSixteenSixteen Of(SignedSixteenSixteen deceleration, int multiplier)
    {
        if (Ensure.BetweenInclusive(0, LargestMultiplier, multiplier) == 0)
            return deceleration;

        int wholePixels = multiplier * deceleration.UnsignedWholePixels / ScaleSteps;
        int fraction = multiplier * (deceleration.Fraction / ScaleSteps);
        // The scaled whole pixels are stored back as a displacement's whole pixels, which read as backward from 32768 on.
        return new UnsignedSixteenSixteen(wholePixels, fraction).AsDisplacement();
    }
}
