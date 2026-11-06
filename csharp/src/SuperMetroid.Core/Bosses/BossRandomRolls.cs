namespace SuperMetroid.Core.Bosses;

/// <summary>
/// The coin-flip rolls bosses make from a random sample by its sign: the game reads the sample, or the sample less a
/// threshold, as a signed number and branches on whether it is negative. A roll therefore passes for the half of the
/// sample range that starts at the threshold and fails for the rest, the lowest samples below the threshold and the
/// highest samples past that half alike.
/// </summary>
/// <remarks>
/// Gameplay-observable: which way Mother Brain's breath, the Shitroid's remorse cry, Ceres Ridley's fireball jitter,
/// Norfair Ridley's breakup fragments and Crocomire's dust go depends on these halves exactly. The samples come from the
/// shared random sequence (<see cref="SharedRandomState"/>), 0 through 65535.
/// </remarks>
internal static class BossRandomRolls
{
    /// <summary>The samples in half the random range: a roll passes for this many consecutive samples.</summary>
    private const int HalfRange = 32768;

    /// <summary>Whether a sample lies in the lower half of the random range, the half whose sign reads as not negative.</summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    internal static bool InLowerHalf(int sample) => InHalfFrom(sample, threshold: 0);

    /// <summary>
    /// Whether a sample lies in the half of the random range that starts at <paramref name="threshold"/>: the sample less
    /// the threshold reads as not negative, so samples from the threshold through 32767 past it pass.
    /// </summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    /// <param name="threshold">The first sample that passes, 0 through 32767.</param>
    internal static bool InHalfFrom(int sample, int threshold)
    {
        int above = Ensure.BetweenInclusive(0, ushort.MaxValue, sample) - Ensure.BetweenInclusive(0, HalfRange - 1, threshold);
        return above is >= 0 and < HalfRange;
    }
}
