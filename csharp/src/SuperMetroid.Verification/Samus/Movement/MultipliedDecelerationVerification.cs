using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks the deceleration Samus loses under a deceleration multiplier (#1621).</summary>
internal static class MultipliedDecelerationVerification
{
    private static readonly int[] Wholes = [0, 1, 2, 3, 0x7f, 0x80, 0xff, 0x100, 0x7fff, 0x8000, 0xffff];
    private static readonly int[] Fractions = [0, 1, 0xff, 0x100, 0x1234, 0x7fff, 0x8000, 0xc0ff, 0xff00, 0xffff];

    /// <summary>
    /// Confirms <see cref="MultipliedDeceleration"/>: no multiplier keeps the table's deceleration, the whole pixels' share
    /// of a pixel is dropped, the fraction counts only its 256ths, and a large whole product reads as backward; then
    /// compares every multiplier, over boundary whole pixels and fractions, with the two separate products it replaced,
    /// and the rule at its site.
    /// </summary>
    internal static void VerifyMultipliedDeceleration()
    {
        SignedSixteenSixteen oneAndAHalf = SignedSixteenSixteen.FromParts(1, 0x8000);
        AssertEqual(oneAndAHalf, MultipliedDeceleration.Of(oneAndAHalf, 0), "no multiplier keeps the table's deceleration");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0x4000), MultipliedDeceleration.Of(oneAndAHalf, 128),
            "half of one and a half pixels removes a quarter pixel: the whole pixel's half-pixel product is dropped");
        AssertEqual(SignedSixteenSixteen.FromParts(1, 0), MultipliedDeceleration.Of(SignedSixteenSixteen.FromParts(3, 0), 128),
            "half of three pixels removes one whole pixel");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0), MultipliedDeceleration.Of(SignedSixteenSixteen.FromParts(0, 0x00ff), 255),
            "a fraction finer than a 256th of a pixel removes nothing");
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0xfe01), MultipliedDeceleration.Of(SignedSixteenSixteen.FromParts(0, 0xffff), 255),
            "the largest fraction at the largest multiplier stays within the fraction");
        AssertTrue(MultipliedDeceleration.Of(SignedSixteenSixteen.FromParts(-1, 0), 255).IsNegative,
            "a whole product of 32768 pixels or more reads as backward");
        AssertThrows<ArgumentOutOfRangeException>(() => MultipliedDeceleration.Of(oneAndAHalf, 256), "a multiplier past 255 is refused");

        int compared = 0;
        for (int multiplier = 0; multiplier <= 255; multiplier++)
        foreach (int whole in Wholes)
        foreach (int fraction in Fractions)
        {
            SignedSixteenSixteen deceleration = SixteenSixteenWords.ToDisplacement(unchecked((int)((uint)whole << 16 | (uint)fraction)));
            AssertEqual(Replaced(deceleration, multiplier), MultipliedDeceleration.Of(deceleration, multiplier),
                $"deceleration {whole:X4}.{fraction:X4} at multiplier {multiplier}");
            compared++;
        }

        // At its site: a turning base speed of two pixels with the running deceleration of half a pixel at a multiplier
        // of 64 loses an eighth of a pixel.
        var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir)
        {
            BaseSpeed = new UnsignedSixteenSixteen(2, 0),
            AccelerationMode = SamusAccelerationMode.Turning,
            DecelerationMultiplier = 64,
        };
        speed.CalculateBaseSpeed(new SamusHorizontalSpeeds(SignedSixteenSixteen.FromParts(0, 0x3000), SignedSixteenSixteen.FromParts(2, 0xc000), SignedSixteenSixteen.FromParts(0, 0x8000)));
        AssertEqual(new UnsignedSixteenSixteen(1, 0xe000), speed.BaseSpeed, "the multiplied deceleration slows a turning speed");

        Console.WriteLine($"Multiplied deceleration: dropped remainders and the fraction's 256ths hold; {compared} decelerations agree with the replaced products.");
    }

    /// <summary>The replaced products: the whole pixels times the multiplier keeping the product's upper byte, the fraction's upper byte times the multiplier.</summary>
    private static SignedSixteenSixteen Replaced(SignedSixteenSixteen deceleration, int multiplier)
    {
        if (multiplier == 0)
            return deceleration;
        int wholeProduct = multiplier * unchecked((ushort)(deceleration.RawValue >> 16));
        int fractionProduct = multiplier * (unchecked((ushort)deceleration.RawValue) >> 8);
        return SignedSixteenSixteen.FromParts(unchecked((short)(wholeProduct >> 8)), fractionProduct);
    }
}
