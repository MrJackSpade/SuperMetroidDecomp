namespace SuperMetroid.Core.Progression;

/// <summary>
/// The range of the counter a save copies unchanged: the item-load counter. Saves carry it as <see cref="int"/> in
/// <c>[0, <see cref="MaximumValue"/>]</c>, so every value a save holds survives a round trip exactly. The range is
/// load-bearing gameplay: the item-load counter goes from <see cref="MaximumValue"/> back to zero when the game advances
/// it. The game clock holds its own domain instead (<see cref="GameTimeState.IsInDomain"/>, #1692).
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
