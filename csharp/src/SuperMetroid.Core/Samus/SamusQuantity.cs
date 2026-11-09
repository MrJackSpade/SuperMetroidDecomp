namespace SuperMetroid.Core.Samus;

/// <summary>
/// The range of Samus's counts (energy, reserves, ammunition and their maxima) and the ways the game takes one from a
/// count (#1323). Counts are plain <see cref="int"/> values from <see cref="MinimumValue"/> through
/// <see cref="MaximumValue"/>, which the game compares as ordinary numbers. A count can fall below zero: Crystal Flash
/// keeps draining an ammunition kind past zero, and taking one from the smallest count gives the largest. Each save
/// codec keeps a count in its own stored form; this type knows none of them.
/// </summary>
public static class SamusQuantity
{
    /// <summary>The smallest count; taking one from it with <see cref="DecrementWrapping"/> gives <see cref="MaximumValue"/>.</summary>
    public const int MinimumValue = -32768;

    /// <summary>The largest count.</summary>
    public const int MaximumValue = 32767;

    /// <summary>How many counts the range holds; the HUD shows a count below zero as this much more.</summary>
    private const int CountsInRange = MaximumValue - MinimumValue + 1;

    /// <summary>A count, rejected when it is outside the range a count holds.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="count"/> is outside <c>[<see cref="MinimumValue"/>, <see cref="MaximumValue"/>]</c>.</exception>
    public static int Require(int count) => count is >= MinimumValue and <= MaximumValue
        ? count
        : throw new InvalidOperationException($"Samus count {count} is outside [{MinimumValue}, {MaximumValue}].");

    /// <summary>
    /// The number the HUD's ammunition digits show for <paramref name="count"/>: the count itself, or, for a count the
    /// game has drained below zero, the count plus the size of the range, so a count of -1 shows the digits of 65535.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="count"/> is outside the range a count holds.</exception>
    public static int HudShown(int count) => Require(count) < 0 ? count + CountsInRange : count;

    /// <summary>
    /// Takes one from a count as the game's unchecked decrement does: zero becomes -1 and the smallest
    /// count becomes the largest.
    /// </summary>
    internal static int DecrementWrapping(int count) =>
        Ensure.BetweenInclusive(MinimumValue, MaximumValue, count) == MinimumValue ? MaximumValue : count - 1;

    /// <summary>
    /// Takes one from a count whose caller has established it is positive. The game decrements without a check, so a
    /// violated guard fails here instead of wrapping.
    /// </summary>
    internal static int DecrementPositive(int count) => count > 0
        ? count - 1
        : throw new InvalidOperationException($"Decrementing Samus count {count} breaks its caller's positive-count guard.");
}
