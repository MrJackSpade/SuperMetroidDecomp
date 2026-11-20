namespace SuperMetroid.Core.Enemies.Elevator;

/// <summary>Which way an elevator travels and where it arrives (#1472, #1519).</summary>
/// <param name="Direction">Its travel direction.</param>
/// <param name="ArrivalY">The room Y position, in pixels, it arrives at.</param>
internal sealed record ElevatorSpawnSettings(ElevatorTravelDirection Direction, int ArrivalY) : EnemySpawnSettings;
