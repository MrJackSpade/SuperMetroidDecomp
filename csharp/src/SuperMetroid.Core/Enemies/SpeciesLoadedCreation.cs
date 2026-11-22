namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' creation of the enemies only its room's population load places (#2554), held as one object every entry bound
/// to it shares, as <see cref="SpeciesSpawnCreation{TSettings}"/> holds a creation built the same way everywhere. It binds
/// its definitions' entries itself.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
internal abstract partial class SpeciesLoadedCreation<TSettings> : IEnemyEntryBinding where TSettings : EnemySpawnSettings
{
    /// <summary>Builds <paramref name="occupant"/>'s enemy from <paramref name="settings"/> in <paramref name="place"/>, in the load's <paramref name="surroundings"/>.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and population entry.</param>
    /// <param name="settings">The settings the occupant's population entry gives its species.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    internal abstract EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings);

    /// <inheritdoc/>
    public PopulationEnemySpawn Bind(RoomEnemyPlacement placement) =>
        Ensure.NotNull(placement).SpawnSettings is TSettings settings
            ? new LoadedEnemySpawn<TSettings>(placement, settings, this)
            : throw IEnemyEntryBinding.Mismatch<TSettings>(placement);
}
