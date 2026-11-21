namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' creation of the enemies its bound entries place wherever they are set up (#2554), held as one object every
/// entry bound to it shares. The game's debugger state writes each entry's reference to it once it is written: the enemies
/// it builds reach those entries again, which a creation held by each entry as a delegate would reach before the delegate
/// itself is written.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
/// <param name="create">The species' creation.</param>
internal sealed class SpeciesSpawnCreation<TSettings>(EnemySpawnCreation<TSettings> create) where TSettings : EnemySpawnSettings
{
    /// <summary>The species' creation.</summary>
    private readonly EnemySpawnCreation<TSettings> _create = Ensure.NotNull(create);

    /// <summary>Builds <paramref name="occupant"/>'s enemy from <paramref name="settings"/> in <paramref name="place"/>.</summary>
    internal EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        _create(occupant, settings, place, common);
}
