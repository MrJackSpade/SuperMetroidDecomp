using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// Checks the Yard's airborne reversal (#1620) against the original kick's and bounces' negation of each word on its own.
/// </summary>
internal static class YardAirborneReversalVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every whole word with each fraction class a Yard's flight reaches or bounds (none, the gravity and slowing steps'
    /// sixteenths and eighths, a kick's halves and the extremes), and every fraction with whole words either side of no
    /// speed and at the reach of a kick: the reversed speed equals both words negated on their own.
    /// </summary>
    internal static void VerifyYardAirborneReversal()
    {
        int[] fractions = [0x0000, 0x0001, 0x1000, 0x2000, 0x4000, 0x8000, 0xa000, 0xf000, 0xffff];
        for (int whole = 0; whole < WordRange; whole++)
        foreach (int fraction in fractions)
            Check(whole, fraction);

        foreach (int whole in new[] { 0x0000, 0x0001, 0x0002, 0x000f, 0x7fff, 0x8000, 0xfff8, 0xfffd, 0xffff })
        for (int fraction = 0; fraction < WordRange; fraction++)
            Check(whole, fraction);
        Console.WriteLine("  Yard airborne reversal: every whole word and every fraction reverses as the original's kick and bounces.");
    }

    /// <summary>Checks the reversal of the speed whose words are <paramref name="whole"/> and <paramref name="fraction"/>.</summary>
    private static void Check(int whole, int fraction)
    {
        var speed = SignedSixteenSixteen.FromParts(unchecked((short)whole), fraction);
        var expected = SignedSixteenSixteen.FromParts(
            unchecked((short)(((whole ^ ushort.MaxValue) + 1) & ushort.MaxValue)), ((fraction ^ ushort.MaxValue) + 1) & ushort.MaxValue);
        if (YardAirborneReversal.Reversed(speed) != expected)
            throw new InvalidOperationException($"A Yard reversing {speed} leaves at {YardAirborneReversal.Reversed(speed)}, not {expected}.");
    }
}
