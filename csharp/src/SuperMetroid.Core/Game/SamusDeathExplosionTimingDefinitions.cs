namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing metadata for Samus's nine suit-explosion frames. Palette selectors
/// remain presentation data; this catalog owns only the fixed timer bytes interleaved
/// with those selectors by <c>HandleSamusDeathSequence</c> at <c>$9B:B823</c>.
/// </summary>
internal static class SamusDeathExplosionTimingDefinitions
{
    /// <summary>
    /// First timer byte in the nine two-byte timer/palette-index records at
    /// <c>$9B:B823-$9B:B834</c>.
    /// </summary>
    public const int NativeFirstTimerAddress = 0x9bb823;

    /// <summary>Number of authored suit-explosion timing records.</summary>
    public const ushort RecordCount = 9;

    /// <summary>Bytes occupied by each interleaved timer/palette-index record.</summary>
    public const ushort RecordByteCount = 2;

    private static ReadOnlySpan<byte> Durations =>
        [0x15, 0x06, 0x03, 0x04, 0x05, 0x05, 0x06, 0x06, 0x50];

    /// <summary>Returns the fixed duration for one authored explosion-frame index.</summary>
    public static ushort DurationForIndex(ushort index)
    {
        if (index >= RecordCount)
        {
            throw new InvalidDataException(
                $"Samus death-explosion timing index {index} is outside the " +
                $"{RecordCount}-record native sequence.");
        }

        return Durations[index];
    }
}
