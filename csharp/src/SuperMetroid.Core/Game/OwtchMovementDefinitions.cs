namespace SuperMetroid.Core.Game;

/// <summary>Compiled patrol and underground-wait definitions for Owtch.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>$A2:A3DD, OwtchData.XDistanceRanges: eight patrol half-widths.</summary>
    /// <remarks>Issues #625 and #947 exact arithmetic progression: distance=16*(i+1), i=0..7.
    /// LookupTableResearch checks every word against NTSC J/U v1.0 ROM, pinned assembly,
    /// and this compiled table. Keep the eight-entry bound; runtime replacement is deferred.</remarks>
    private static ReadOnlySpan<ushort> TravelDistances =>
    [
        0x0010,
        0x0020,
        0x0030,
        0x0040,
        0x0050,
        0x0060,
        0x0070,
        0x0080,
    ];

    /// <summary>$A2:A3ED, OwtchData.undergroundTimers: six burial durations.</summary>
    /// <remarks>Issues #625 and #948 exact NTSC arithmetic progression: duration=32*(i+1), i=0..5.
    /// Pinned assembly expresses this directly using !FPS=1; do not apply it unchanged to PAL.
    /// All six ROM/assembly/compiled words are exhaustively checked by LookupTableResearch.
    /// This domain is shorter than the travel-distance domain; preserve its independent bound.</remarks>
    private static ReadOnlySpan<ushort> UndergroundTimers =>
    [
        0x0020,
        0x0040,
        0x0060,
        0x0080,
        0x00a0,
        0x00c0,
    ];

    internal static ushort TravelDistance(byte index)
    {
        if (index >= TravelDistances.Length)
        {
            throw new InvalidDataException(
                $"Owtch travel-distance index {index} is outside the eight authored values.");
        }

        return TravelDistances[index];
    }

    internal static ushort UndergroundTimer(byte index)
    {
        if (index >= UndergroundTimers.Length)
        {
            throw new InvalidDataException(
                $"Owtch underground-timer index {index} is outside the six authored values.");
        }

        return UndergroundTimers[index];
    }
}
