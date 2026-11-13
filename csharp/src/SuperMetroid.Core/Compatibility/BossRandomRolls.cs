namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The coin-flip rolls bosses make from a random sample by its sign (#1620): the game reads the sample, or the sample less
/// a threshold, as a signed sixteen-bit number and branches on whether it is negative. A roll therefore passes for the
/// half of the sample range that starts at the threshold and fails for the rest, the lowest samples below the threshold
/// and the highest samples past that half alike, so a roll with a threshold passes for fewer low samples than an even
/// split would and for some high samples an ordinary comparison would refuse.
/// </summary>
/// <remarks>
/// Consulted at each roll, as the boss's update draws its sample: which way Mother Brain's breath, the Shitroid's remorse
/// cry, Ceres Ridley's fireball jitter, Norfair Ridley's breakup fragments and Crocomire's dust and step go depends on
/// these halves exactly. The samples come from the shared random sequence (<see cref="SharedRandomState"/>), 0 through
/// 65535. Evidence: each roll's original routine subtracts its threshold from the random word (or compares it with
/// zero) and takes the branch on the result's sign bit, never on an unsigned comparison; the SMV replays compare the
/// random value and these bosses' outcomes on every update.
/// </remarks>
[CompatibilityService("a boss's coin-flip roll passes for the half of the random range that starts at its threshold, read by the sign of the sample less the threshold")]
public static class BossRandomRolls
{
    /// <summary>The samples in half the random range: a roll passes for this many consecutive samples.</summary>
    private const int HalfRange = 32768;

    /// <summary>Whether a sample lies in the lower half of the random range, the half whose sign reads as not negative.</summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sample"/> is outside the random range.</exception>
    public static bool InLowerHalf(int sample) => InHalfFrom(sample, threshold: 0);

    /// <summary>
    /// Whether a sample lies in the half of the random range that starts at <paramref name="threshold"/>: the sample less
    /// the threshold reads as not negative, so samples from the threshold through 32767 past it pass.
    /// </summary>
    /// <param name="sample">The random sample, 0 through 65535.</param>
    /// <param name="threshold">The first sample that passes, 0 through 32767.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="sample"/> or <paramref name="threshold"/> is outside its range.</exception>
    public static bool InHalfFrom(int sample, int threshold)
    {
        int above = Ensure.BetweenInclusive(0, ushort.MaxValue, sample) - Ensure.BetweenInclusive(0, HalfRange - 1, threshold);
        return above is >= 0 and < HalfRange;
    }
}
