namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for before it grows (#1471).</summary>
/// <param name="WaitsForSamus">Whether it waits for Samus to come near rather than for its timer.</param>
public sealed record GrowingShutterSpawnSettings(bool WaitsForSamus) : EnemySpawnSettings;
