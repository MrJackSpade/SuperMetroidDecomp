namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' creation of the enemies only its room's population load places (#2554), held as one object every entry bound
/// to it shares, as <see cref="SpeciesSpawnCreation{TSettings}"/> holds a creation built the same way everywhere.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
/// <param name="create">The species' creation.</param>
internal sealed class SpeciesLoadedCreation<TSettings>(EnemyLoadedCreation<TSettings> create) where TSettings : EnemySpawnSettings
{
    /// <summary>The species' creation.</summary>
    private readonly EnemyLoadedCreation<TSettings> _create = Ensure.NotNull(create);

    /// <summary>Builds <paramref name="occupant"/>'s enemy from <paramref name="settings"/> in <paramref name="place"/>, in the load's <paramref name="surroundings"/>.</summary>
    internal EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) =>
        _create(occupant, settings, place, common, surroundings);
}
