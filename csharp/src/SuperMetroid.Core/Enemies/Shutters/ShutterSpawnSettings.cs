namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>A moving shutter's speed, direction, trigger and rests (#1471, #1472).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the shutter moves at.</param>
/// <param name="PrimaryDirection">The end it first heads for; its reaction direction derives from it.</param>
/// <param name="TriggerMode">What starts it moving.</param>
/// <param name="TravelDistance">Pixels, 0 through 255, it travels.</param>
/// <param name="ProximityOrWaitTime">The distance Samus must come within, or the updates it waits, 0 through 65535.</param>
/// <param name="FirstRestUnits">Its rest after moving left or up, 0 through 255 sixteen-update units.</param>
/// <param name="SecondRestUnits">Its rest after moving right or down, 0 through 255 sixteen-update units.</param>
internal sealed record ShutterSpawnSettings(int SpeedIndex, ShutterHeading PrimaryDirection, ShutterTriggerMode TriggerMode, int TravelDistance, int ProximityOrWaitTime, int FirstRestUnits, int SecondRestUnits) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the shutter moves at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>Pixels, 0 through 255, it travels.</summary>
    public int TravelDistance { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, TravelDistance);

    /// <summary>The distance Samus must come within, or the updates it waits, 0 through 65535.</summary>
    public int ProximityOrWaitTime { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ProximityOrWaitTime);

    /// <summary>Its rest after moving left or up, 0 through 255 sixteen-update units.</summary>
    public int FirstRestUnits { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, FirstRestUnits);

    /// <summary>Its rest after moving right or down, 0 through 255 sixteen-update units.</summary>
    public int SecondRestUnits { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SecondRestUnits);
}
