namespace SuperMetroid.Core.Progression;

/// <summary>
/// The range of a save field that saves only copy, never count or compare: the item-load counter, the checkpoint area
/// and load station, the four game-clock components and the two checksummed marker values. Saves carry these as
/// <see cref="int"/> in <c>[0, <see cref="MaximumValue"/>]</c>, so every value an imported save carries survives a round
/// trip exactly; the item-load counter and the game clock's components go from <see cref="MaximumValue"/> back to zero
/// when the game advances them. Each save codec keeps these values in its own stored form; this type knows none of them.
/// </summary>
public static class SaveFieldValue
{
    /// <summary>The largest value such a field keeps; advancing a counter past it gives zero.</summary>
    public const int MaximumValue = 65535;

    /// <summary>Whether <paramref name="value"/> is one such a field keeps.</summary>
    public static bool IsInRange(int value) => value is >= 0 and <= MaximumValue;

    /// <summary>A save field's value, rejected when the field cannot keep it.</summary>
    /// <param name="value">The field's value.</param>
    /// <param name="field">The field's name, reported when the value is out of range.</param>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is outside <c>[0, <see cref="MaximumValue"/>]</c>.</exception>
    public static int Require(int value, string field) => IsInRange(value)
        ? value
        : throw new InvalidOperationException($"Save field {field} value {value} is outside [0, {MaximumValue}].");
}
