namespace SuperMetroid.Verification.Enemies;

/// <summary>What the level-data place model of an enemy projectile's edge scan finds (#1628).</summary>
/// <param name="Solid">Whether the scan meets a wall.</param>
/// <param name="Failure">The exception the scan fails with, or null when it completes.</param>
internal sealed record EnemyTerrainScanOutcome(bool Solid, Type? Failure);
