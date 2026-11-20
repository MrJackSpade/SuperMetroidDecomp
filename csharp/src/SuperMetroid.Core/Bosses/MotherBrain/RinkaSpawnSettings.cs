using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where a Rinka spawns (#1472, #2256).</summary>
/// <param name="SharedSpawnPoint">
/// The spawn point of Mother Brain's room a room-wide Rinka starts at, one of the points those Rinkas share; the import
/// resolves it from the point whose centre the Rinka is placed at. Null for a Rinka that spawns at its own placement.
/// </param>
internal sealed record RinkaSpawnSettings(RinkaSpawnPoint? SharedSpawnPoint) : EnemySpawnSettings
{
    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacing placing) => Ensure.NotNull(species).Place(this, placing);
}
