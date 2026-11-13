namespace SuperMetroid.Verification.Enemies;

/// <summary>Where the level-data place model of an enemy terrain lookup ends (#1628).</summary>
/// <param name="Place">The place of the block the lookup reads, or null when it finds no block.</param>
/// <param name="Failure">The exception the lookup fails with, or null when it ends on a block or finds none.</param>
internal sealed record EnemyTerrainResolvedPlace(int? Place, Type? Failure);
