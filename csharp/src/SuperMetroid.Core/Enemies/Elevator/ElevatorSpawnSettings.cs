namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Which way an elevator travels and where it arrives (#1472).</summary>
/// <param name="DirectionIndex">Its travel direction, 0 through 32767.</param>
/// <param name="ArrivalY">The room row, 0 through 65535, it arrives at.</param>
internal sealed record ElevatorSpawnSettings(int DirectionIndex, int ArrivalY) : EnemySpawnSettings
{
    /// <summary>Its travel direction, 0 through 32767.</summary>
    public int DirectionIndex { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, DirectionIndex);

    /// <summary>The room row, 0 through 65535, it arrives at.</summary>
    public int ArrivalY { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ArrivalY);
}
