using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks the draining Shitroid's shift (#1620) against the original comparison's carry.
/// </summary>
internal static class DrainingShitroidFallShiftVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every vertical speed's whole pixels: the draining Shitroid sits one pixel right exactly when the original's
    /// unsigned comparison with four leaves its carry, slowed-past-zero speeds included.
    /// </summary>
    internal static void VerifyDrainingShitroidFallShift()
    {
        for (int speed = 0; speed < WordRange; speed++)
        {
            int carry = speed >= 4 ? 1 : 0;
            if (DrainingShitroidFallShift.PixelsRightOfShake(speed) != carry)
                throw new InvalidOperationException($"A drain at vertical speed {speed} shifts the Shitroid wrongly.");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DrainingShitroidFallShift.PixelsRightOfShake(WordRange), "a speed past the range is refused");
        Console.WriteLine("  Draining Shitroid shift: every vertical speed matches the comparison's carry.");
    }
}
