namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus's counts (energy, reserves, ammo and their maxima) are plain <see cref="int"/> values in
/// <c>[0, <see cref="MaximumValue"/>]</c> (#1323). Native code stores them as words and relies on signed comparisons,
/// which equal ordinary comparisons inside that range. Words exist only at the SRAM, JSON and snapshot boundaries,
/// which convert through <see cref="Decode"/> and <see cref="Encode"/>.
/// </summary>
public static class SamusQuantity
{
    /// <summary>The largest count whose native word is still non-negative under a signed comparison.</summary>
    public const int MaximumValue = short.MaxValue;

    /// <summary>A stored word as a count; a word outside the count domain is corrupt.</summary>
    public static int Decode(ushort word) => word <= MaximumValue
        ? word
        : throw new InvalidDataException($"Samus count word ${word:X4} is outside [0, {MaximumValue}].");

    /// <summary>A count as its stored word.</summary>
    public static ushort Encode(int value) => value is >= 0 and <= MaximumValue
        ? (ushort)value
        : throw new InvalidOperationException($"Samus count {value} is outside [0, {MaximumValue}].");

    /// <summary>
    /// Removes one from a count whose caller has established it is positive. Native code decrements the word without
    /// a check, so a violated guard fails here instead of wrapping.
    /// </summary>
    internal static int DecrementPositive(int count) => count > 0
        ? count - 1
        : throw new InvalidOperationException($"Decrementing Samus count {count} breaks its caller's positive-count guard.");
}
