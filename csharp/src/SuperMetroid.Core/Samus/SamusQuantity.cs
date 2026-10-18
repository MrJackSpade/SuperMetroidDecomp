namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's counts (energy, reserves, ammo and their maxima) are plain <see cref="int"/> values in the signed word
/// range <c>[<see cref="MinimumValue"/>, <see cref="MaximumValue"/>]</c> (#1323). Native code stores them as words
/// and compares them through signed differences, which equal ordinary comparisons of these values. The cartridge
/// itself reaches negative counts: Crystal Flash keeps draining an ammunition kind past zero. Words exist only at the
/// SRAM, JSON and snapshot boundaries, which convert through <see cref="Decode"/> and <see cref="Encode"/>.
/// </summary>
public static class SamusQuantity
{
    /// <summary>The smallest count: the word $8000 under the cartridge's signed comparisons.</summary>
    public const int MinimumValue = short.MinValue;

    /// <summary>The largest count: the word $7FFF under the cartridge's signed comparisons.</summary>
    public const int MaximumValue = short.MaxValue;

    /// <summary>A stored word as a count, reading it as the signed value the cartridge compares.</summary>
    public static int Decode(ushort word) => unchecked((short)word);

    /// <summary>A count as its stored word.</summary>
    public static ushort Encode(int value) => value is >= MinimumValue and <= MaximumValue
        ? unchecked((ushort)value)
        : throw new InvalidOperationException($"Samus count {value} is outside [{MinimumValue}, {MaximumValue}].");

    /// <summary>
    /// Removes one from a count as the cartridge's unchecked word <c>DEC</c> does: zero becomes -1 and the smallest
    /// count wraps to the largest.
    /// </summary>
    internal static int DecrementWord(int count) =>
        Ensure.BetweenInclusive(MinimumValue, MaximumValue, count) == MinimumValue ? MaximumValue : count - 1;

    /// <summary>
    /// Removes one from a count whose caller has established it is positive. Native code decrements the word without
    /// a check, so a violated guard fails here instead of wrapping.
    /// </summary>
    internal static int DecrementPositive(int count) => count > 0
        ? count - 1
        : throw new InvalidOperationException($"Decrementing Samus count {count} breaks its caller's positive-count guard.");
}
