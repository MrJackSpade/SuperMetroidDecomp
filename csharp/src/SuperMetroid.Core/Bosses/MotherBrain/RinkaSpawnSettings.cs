using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Where a Rinka spawns (#1472, #2256): an ordinary Rinka at its own placement, or one of Mother Brain's room-wide Rinkas
/// at the spawn points they share, starting at one of them.
/// </summary>
internal abstract record RinkaSpawnSettings : EnemySpawnSettings
{
    /// <summary>Only the nested cases derive.</summary>
    private RinkaSpawnSettings()
    {
    }

    /// <inheritdoc/>
    internal override EnemyPlaced Place(EnemySpeciesRegistry species, EnemyPlacementRequest placing) => Ensure.NotNull(species).Place(this, placing);

    /// <inheritdoc/>
    internal override int PartsHeadedIn(EnemySpeciesRegistry species, EnemyDefinitionId definition) => Ensure.NotNull(species).PartsHeadedBy(this, definition);

    /// <summary>An ordinary Rinka, which spawns and respawns at its own placement.</summary>
    internal static RinkaSpawnSettings AtOwnPlacement { get; } = new OwnPlacement();

    /// <summary>
    /// One of Mother Brain's room-wide Rinkas, which share the room's spawn points and start at <paramref name="Start"/>;
    /// the import resolves it from the point whose centre the Rinka is placed at.
    /// </summary>
    /// <param name="Start">The spawn point the Rinka starts at.</param>
    internal sealed record SharingPoints(RinkaSpawnPoint Start) : RinkaSpawnSettings;

    /// <summary>An ordinary Rinka, which spawns and respawns at its own placement.</summary>
    private sealed record OwnPlacement : RinkaSpawnSettings;
}
