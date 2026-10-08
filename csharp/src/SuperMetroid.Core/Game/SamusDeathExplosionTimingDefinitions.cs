namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing metadata for Samus's nine suit-explosion frames. Palette selectors
/// remain presentation data; this catalog owns only the fixed timer bytes interleaved
/// with those selectors by <c>HandleSamusDeathSequence</c> at <c>$9B:B823</c>.
/// </summary>
/// <remarks>Issue1165 retains the nine countdown choices under the authored
/// animation/nonsense exception. Native9BB5B4 loads the entry countdown and
/// immediately steps it;9BB75B decrements before advancing the drawing index.
/// 92EDBE selects one of nine fixed right/left drawings by that index at the
/// same captured position. The initial21 countdown delays whiteout,the middle
/// 6/3/4/5/5/6/6 countdowns choose the drawing cadence,and the final80 delays
/// completion. There is no distance,velocity or measured brightness input
/// whose units determine those dwell times. A numeric case list or fitted
/// curve would only recite these chosen animation delays; replacing them with
/// uniform or physically generated timing changes the cinematic. This is not
/// a size,complexity,performance or missing-provenance justification. The odd
/// palette selectors are separately calculated; this exception covers only
/// these nine even-byte durations and does not exempt adjacent control data.</remarks>
internal static class SamusDeathExplosionTimingDefinitions
{

    /// <summary>Number of authored suit-explosion timing records.</summary>
    public const ushort RecordCount = 9;

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
