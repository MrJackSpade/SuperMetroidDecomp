namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>What a growing shutter waits for before it grows (#1471).</summary>
/// <param name="Wait">What it waits for.</param>
public sealed record GrowingShutterSpawnSettings(GrowingShutterWait Wait) : EnemySpawnSettings;
