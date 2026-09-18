namespace SuperMetroid.Core.Game;

/// <summary>Compiled patrol and underground-wait definitions for Owtch.</summary>
internal static class OwtchMovementDefinitions
{
    /// <summary>$A2:A3DD, OwtchData.travelDistanceTable: eight patrol half-widths.</summary>
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

    /// <summary>$A2:A3ED, OwtchData.undergroundTimerTable: six burial durations.</summary>
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
