using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Whether a Rinka is one of Mother Brain's room-wide Rinkas that take a free spawn point (#1472).</summary>
/// <param name="Placement">Where the Rinka spawns.</param>
internal sealed record RinkaSpawnSettings(RinkaPlacement Placement) : EnemySpawnSettings;
