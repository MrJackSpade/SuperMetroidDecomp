namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// Checks the Yard's fall acceleration (#1620) against the original airborne movement's addition: an eighth of a pixel
/// added to the fraction word, its carry added to the whole word, the fraction always stored and the whole word only when
/// the sum less four is negative.
/// </summary>
internal static class YardFallAccelerationVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// The new fraction depends only on the fraction, and the new whole pixels only on the whole word and on whether the
    /// fraction's addition carries, which it does from 57344 up. Every whole word is checked with a fraction on each side
    /// of the carry and at both ends, and every fraction with whole words either side of the held four pixels, of no
    /// speed and at both ends of the word, so every input class is covered.
    /// </summary>
    internal static void VerifyYardFallAcceleration()
    {
        int[] fractions = [0x0000, 0x0001, 0x8000, 0xdfff, 0xe000, 0xe001, 0xffff];
        for (int whole = 0; whole < WordRange; whole++)
        foreach (int fraction in fractions)
            Check(whole, fraction);

        foreach (int whole in new[] { 0x0000, 0x0002, 0x0003, 0x0004, 0x0005, 0x0007, 0x7fff, 0x8000, 0x8003, 0x8004, 0xfffe, 0xffff })
        for (int fraction = 0; fraction < WordRange; fraction++)
            Check(whole, fraction);
        Console.WriteLine("  Yard fall acceleration: every whole word and every fraction gains speed as the original's airborne movement adds.");
    }

    /// <summary>Checks the gain on the speed whose words are <paramref name="whole"/> and <paramref name="fraction"/>.</summary>
    private static void Check(int whole, int fraction)
    {
        int fractionSum = fraction + 0x2000;
        int wholeSum = (whole + (fractionSum >> 16)) & 0xffff;
        bool storesWhole = ((wholeSum - 4) & 0x8000) != 0;
        var expected = SignedSixteenSixteen.FromParts(unchecked((short)(storesWhole ? wholeSum : whole)), fractionSum & 0xffff);
        var speed = SignedSixteenSixteen.FromParts(unchecked((short)whole), fraction);
        if (YardFallAcceleration.Accelerated(speed) != expected)
            throw new InvalidOperationException($"A Yard falling at {speed} gains to {YardFallAcceleration.Accelerated(speed)}, not {expected}.");
    }
}
