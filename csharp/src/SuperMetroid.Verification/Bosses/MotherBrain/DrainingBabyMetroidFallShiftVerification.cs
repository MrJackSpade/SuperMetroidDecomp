namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>
/// Checks the draining Baby Metroid's shift (#1620) against the original comparison's carry.
/// </summary>
internal static class DrainingBabyMetroidFallShiftVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every vertical speed's whole pixels: the draining Baby Metroid sits one pixel right exactly when the original's
    /// unsigned comparison with four leaves its carry, slowed-past-zero speeds included.
    /// </summary>
    internal static void VerifyDrainingBabyMetroidFallShift()
    {
        for (int speed = 0; speed < WordRange; speed++)
        {
            int carry = speed >= 4 ? 1 : 0;
            if (DrainingBabyMetroidFallShift.PixelsRightOfShake(speed) != carry)
                throw new InvalidOperationException($"A drain at vertical speed {speed} shifts the Shitroid wrongly.");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => DrainingBabyMetroidFallShift.PixelsRightOfShake(WordRange), "a speed past the range is refused");
        Console.WriteLine("  Draining Shitroid shift: every vertical speed matches the comparison's carry.");
    }
}
