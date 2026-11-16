using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>
/// Checks Ceres Ridley's fireball target (#1620) against the original's chained addition.
/// </summary>
internal static class CeresRidleyFireballTargetVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// Every base X with every jitter, at bases near the room's top and inside it: the target is the base moved by the
    /// jitter as the original adds it, the X addition's carry passing into Y; a base whose X could pass either end of the
    /// coordinate space is refused.
    /// </summary>
    internal static void VerifyCeresRidleyFireballTarget()
    {
        foreach (int baseY in new[] { 0, 80, 127, 65535 })
        for (int baseX = 0; baseX < WordRange; baseX++)
        for (int jitter = -CeresRidleyFireballTarget.LargestJitter; jitter <= CeresRidleyFireballTarget.LargestJitter; jitter++)
        {
            var basePosition = new Position16<RoomPixels>(baseX, baseY);
            int jitterWord = CoordinateAxis.Wrap(jitter);
            int x = baseX + jitterWord;
            int carry = x > ushort.MaxValue ? 1 : 0;
            var expected = new Position16<RoomPixels>(x & ushort.MaxValue, (baseY + jitterWord + carry) & ushort.MaxValue);
            bool reachable = baseX is >= CeresRidleyFireballTarget.LargestJitter and <= ushort.MaxValue - CeresRidleyFireballTarget.LargestJitter;
            if (!reachable)
            {
                AssertThrows<ArgumentOutOfRangeException>(() => CeresRidleyFireballTarget.Around(basePosition, jitter),
                    $"a base at X {baseX} that could pass the coordinate space's end is refused");
                continue;
            }
            Position16<RoomPixels> actual = CeresRidleyFireballTarget.Around(basePosition, jitter);
            if (actual != expected)
                throw new InvalidOperationException($"Fireball target from {basePosition} by {jitter} is {actual}, not {expected}.");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CeresRidleyFireballTarget.Around(new(100, 100), 8), "a jitter past seven is refused");
        AssertEqual(new Position16<RoomPixels>(97, 98), CeresRidleyFireballTarget.Around(new(100, 100), -3), "three pixels left lifts two");
        Console.WriteLine("  Ceres Ridley fireball target: every base and jitter matches the chained addition.");
    }
}
