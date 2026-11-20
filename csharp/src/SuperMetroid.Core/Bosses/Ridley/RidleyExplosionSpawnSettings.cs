using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which piece of Ridley's body a breakup fragment is (#1472).</summary>
/// <param name="Part">The body piece.</param>
internal sealed record RidleyExplosionSpawnSettings(RidleyExplosionPart Part) : EnemySpawnSettings;
