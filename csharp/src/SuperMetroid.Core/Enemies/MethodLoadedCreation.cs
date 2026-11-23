namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A creation, of an enemy only the population load places, that one named method makes (#2554): a species' own method,
/// or one of the object that carries what a part joins, so a copy of the game's state names the method and holds its
/// target.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
internal sealed partial class MethodLoadedCreation<TSettings> : SpeciesLoadedCreation<TSettings>
    where TSettings : EnemySpawnSettings
{
    /// <summary>The named method.</summary>
    private readonly EnemyLoadedCreation<TSettings> _create;

    /// <summary>
    /// Holds the named method. An ordinary constructor, not a primary one, so a restore of the game's state makes the creation
    /// before it reads its method's target: a state can reach the creation again from inside its method's target (#2554).
    /// </summary>
    /// <param name="create">The named method.</param>
    internal MethodLoadedCreation(EnemyLoadedCreation<TSettings> create) => _create = Ensure.NotNull(create);

    /// <inheritdoc/>
    internal override EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) =>
        _create(occupant, settings, place, common, surroundings);
}
