namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>A moving shutter's speed and the direction it first moves in (#1471).</summary>
/// <param name="SpeedIndex">The common linear-speed record, 0 through 255, the shutter moves at.</param>
/// <param name="PrimaryDirection">The direction, 0 through 255, the shutter first moves in; its reaction direction derives from it.</param>
public sealed record ShutterSpawnSettings(int SpeedIndex, int PrimaryDirection) : EnemySpawnSettings
{
    /// <summary>The common linear-speed record, 0 through 255, the shutter moves at.</summary>
    public int SpeedIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, SpeedIndex);

    /// <summary>The direction, 0 through 255, the shutter first moves in.</summary>
    public int PrimaryDirection { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, PrimaryDirection);
}
