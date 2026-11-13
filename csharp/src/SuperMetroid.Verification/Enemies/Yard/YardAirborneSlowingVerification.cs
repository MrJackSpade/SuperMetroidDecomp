namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// Checks the Yard's airborne slowing (#1620) against the original airborne movement's addition: the slowing's two words
/// added with the fraction's carry, the fraction always stored and the whole word only when the sum is not zero.
/// </summary>
internal static class YardAirborneSlowingVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// The new fraction depends only on the fraction, and the new whole pixels only on the whole word and on whether the
    /// fraction's addition carries, which it does from 4096 up for a speed of nothing or more and from 61440 up for a
    /// leftward one. Every whole word is checked with a fraction on each side of both carries and at both ends, and every
    /// fraction with whole words either side of no speed and at both ends of the word, so every input class is covered.
    /// </summary>
    internal static void VerifyYardAirborneSlowing()
    {
        int[] fractions = [0x0000, 0x0fff, 0x1000, 0x1001, 0x8000, 0xefff, 0xf000, 0xf001, 0xffff];
        for (int whole = 0; whole < WordRange; whole++)
        foreach (int fraction in fractions)
            Check(whole, fraction);

        foreach (int whole in new[] { 0x0000, 0x0001, 0x0002, 0x7fff, 0x8000, 0xfffe, 0xffff })
        for (int fraction = 0; fraction < WordRange; fraction++)
            Check(whole, fraction);
        Console.WriteLine("  Yard airborne slowing: every whole word and every fraction slows as the original's airborne movement adds.");
    }

    /// <summary>Checks the slowing of the speed whose words are <paramref name="whole"/> and <paramref name="fraction"/>.</summary>
    private static void Check(int whole, int fraction)
    {
        bool leftward = (whole & 0x8000) != 0;
        int fractionSum = fraction + (leftward ? 0x1000 : 0xf000);
        int wholeSum = (whole + (leftward ? 0x0000 : 0xffff) + (fractionSum >> 16)) & 0xffff;
        var expected = SignedSixteenSixteen.FromParts(unchecked((short)(wholeSum != 0 ? wholeSum : whole)), fractionSum & 0xffff);
        var speed = SignedSixteenSixteen.FromParts(unchecked((short)whole), fraction);
        if (YardAirborneSlowing.Slowed(speed) != expected)
            throw new InvalidOperationException($"A Yard flying at {speed} slows to {YardAirborneSlowing.Slowed(speed)}, not {expected}.");
    }
}
