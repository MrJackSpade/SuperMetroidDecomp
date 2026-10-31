namespace SuperMetroid.Core.Escape;

/// <summary>
/// Checks' view of escape timer times: a countdown from its minutes, seconds and centiseconds, and the cartridge's
/// packed-decimal byte for one field of a readout, which native oracles draw from.
/// </summary>
internal static class EscapeTimerClock
{
    /// <summary>The countdown, in centiseconds, of <paramref name="minutes"/>:<paramref name="seconds"/>.<paramref name="centiseconds"/>.</summary>
    internal static int Time(int minutes, int seconds, int centiseconds) =>
        minutes * EscapeTimerDefinitions.CentisecondsPerMinute +
        seconds * EscapeTimerCadenceDefinitions.CentisecondsPerSecond + centiseconds;

    /// <summary>The cartridge's packed-decimal byte for a two-digit value: the tens digit in the high nibble.</summary>
    internal static byte PackedDecimal(int value) => (uint)value <= 99
        ? (byte)(value / 10 * 16 + value % 10)
        : throw new ArgumentOutOfRangeException(nameof(value), value, "A packed-decimal byte holds 0 through 99.");
}
