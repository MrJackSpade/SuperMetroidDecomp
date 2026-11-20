namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Which Tourian Sidehopper placement a dead Sidehopper is (#1519).</summary>
/// <param name="Layout">The placement.</param>
internal sealed record DeadSidehopperSpawnSettings(DeadSidehopperLayout Layout) : EnemySpawnSettings;
