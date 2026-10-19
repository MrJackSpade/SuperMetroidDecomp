namespace SuperMetroid.Core.Saves;

/// <summary>
/// The range of a save field that saves only copy, never count, compare or wrap: the item-load counter, the checkpoint
/// area and load station, the startup dispatcher route, the four game-clock components and the two checksummed marker
/// values. Saves carry these as <see cref="int"/> in <c>[0, <see cref="MaximumValue"/>]</c>, the range one SRAM slot
/// field holds, so every value an imported save carries survives a round trip exactly. The SRAM and JSON codecs, and the
/// runtime owners that still keep these values in sixteen bits, convert through <see cref="Encode"/>, which rejects a
/// value outside the range instead of wrapping it.
/// </summary>
public static class SaveFieldValue
{
    /// <summary>The largest value one SRAM slot field holds.</summary>
    public const int MaximumValue = ushort.MaxValue;

    /// <summary>Whether <paramref name="value"/> is one an SRAM slot field can hold.</summary>
    public static bool IsInRange(int value) => value is >= 0 and <= MaximumValue;

    /// <summary>A save field's value as the sixteen-bit form the SRAM slot and the remaining runtime owners store.</summary>
    /// <param name="value">The field's value.</param>
    /// <param name="field">The field's name, reported when the value is out of range.</param>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is outside <c>[0, <see cref="MaximumValue"/>]</c>.</exception>
    public static ushort Encode(int value, string field) => IsInRange(value)
        ? (ushort)value
        : throw new InvalidOperationException($"Save field {field} value {value} is outside [0, {MaximumValue}].");
}
