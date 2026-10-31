using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Whether a Rinka is one of Mother Brain's room-wide Rinkas that take a free spawn point (#1472).</summary>
/// <param name="Special">Whether the Rinka takes a free spawn point instead of its own placement.</param>
internal sealed record RinkaSpawnSettings(bool Special) : EnemySpawnSettings;
