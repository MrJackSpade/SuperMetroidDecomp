namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The creation of a species whose enemies only the population load places (#2554), held as the species' factory itself.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
internal sealed partial class FactoryLoadedCreation<TSettings> : SpeciesLoadedCreation<TSettings>
    where TSettings : EnemySpawnSettings
{
    /// <summary>The species.</summary>
    private readonly IEnemyLoadedFactory<TSettings> _factory;

    /// <summary>
    /// Holds the species. An ordinary constructor, not a primary one, so a restore of the game's state makes the creation
    /// before it reads the species: a state can reach the creation again from inside the species (#2554).
    /// </summary>
    /// <param name="factory">The species.</param>
    internal FactoryLoadedCreation(IEnemyLoadedFactory<TSettings> factory) => _factory = Ensure.NotNull(factory);

    /// <inheritdoc/>
    internal override EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common,
        EnemyPopulationSurroundings surroundings) =>
        _factory.CreateLoaded(occupant, settings, place, common, surroundings);
}
