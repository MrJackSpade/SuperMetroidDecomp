namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Samus's counts (energy, reserves, ammunition and their maxima) run below zero and wrap (#1323, #1763). Crystal Flash
/// keeps draining an ammunition kind past zero, a missile, Super Missile or Power Bomb fired from a count below zero takes
/// it further below, and taking one from the smallest count gives the largest; the HUD shows a count drained below zero by the digits of the
/// wrapping count it stands for, so a count of -1 shows 65535. The game compares counts as ordinary numbers, so a count
/// below zero is less than every count of zero or more. The range is half the wrapping counter's either side of zero:
/// the counts of a <see cref="WrappingCounter"/> that read as below zero (<see cref="WrappingCounter.ReadsBelowZero"/>)
/// are the counts here below zero.
/// </summary>
/// <remarks>
/// No frame point of its own: consulted where a count is taken from (Crystal Flash's ammunition drain on each update it
/// drains, a missile, Super Missile or Power Bomb fired), where the HUD draws a count, and where a save is read, written
/// or kept, which accepts every count in the range and no other. Each save codec keeps a count in its own stored form;
/// this service knows none of them. A rule with no state of its own: each count is a value its owner holds. Evidence: the
/// original keeps each count as a word, takes one from it with no check, compares counts as below or above zero, and
/// draws the HUD's digits from the word as a count of 0 through 65535; the typed words check
/// (<c>VerifyTypedNativeWords</c>) covers both ends of the range and the decrement of zero and of the smallest count,
/// and the Crystal Flash check (<c>VerifySamusCrystalFlash</c>) drains ammunition.
/// </remarks>
[CompatibilityService("Samus's counts run below zero, taking one from the smallest gives the largest, and the HUD shows a count below zero by the digits of the wrapping count it stands for")]
public static class CountsRunBelowZero
{
    /// <summary>The largest count: the largest wrapping count that does not read as below zero.</summary>
    public const int MaximumValue = WrappingCounter.LargestCount / 2;

    /// <summary>The smallest count; taking one from it with <see cref="DecrementWrapping"/> gives <see cref="MaximumValue"/>.</summary>
    public const int MinimumValue = -MaximumValue - 1;

    /// <summary>A count, rejected when it is outside the range a count holds.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="count"/> is outside <c>[<see cref="MinimumValue"/>, <see cref="MaximumValue"/>]</c>.</exception>
    public static int Require(int count) => count is >= MinimumValue and <= MaximumValue
        ? count
        : throw new InvalidOperationException($"Samus count {count} is outside [{MinimumValue}, {MaximumValue}].");

    /// <summary>
    /// The number the HUD's ammunition digits show for <paramref name="count"/>: the count itself, or, for a count the
    /// game has drained below zero, the wrapping count it stands for, as far below the wrapping counter's top plus one as
    /// the count is below zero, so a count of -1 shows the digits of 65535.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="count"/> is outside the range a count holds.</exception>
    public static int HudShown(int count) => Require(count) < 0 ? WrappingCounter.SubtractCount(0, -count) : count;

    /// <summary>
    /// Takes one from a count as the game's unchecked decrement does: zero becomes -1 and the smallest
    /// count becomes the largest.
    /// </summary>
    internal static int DecrementWrapping(int count) =>
        Ensure.BetweenInclusive(MinimumValue, MaximumValue, count) == MinimumValue ? MaximumValue : count - 1;
}
