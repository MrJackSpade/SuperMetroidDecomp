namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Which way an elevator travels and where it arrives (#1472).</summary>
/// <param name="Direction">Its travel direction.</param>
/// <param name="ArrivalY">The room row, 0 through 65535, it arrives at.</param>
internal sealed record ElevatorSpawnSettings(ElevatorTravelDirection Direction, int ArrivalY) : EnemySpawnSettings
{
    /// <summary>The room row, 0 through 65535, it arrives at.</summary>
    public int ArrivalY { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, ArrivalY);
}
