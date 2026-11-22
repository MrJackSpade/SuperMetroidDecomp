namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The creation of a species whose factory builds its enemies the same way everywhere (#2554), held as the factory
/// itself.
/// </summary>
/// <typeparam name="TSettings">The settings the creation takes.</typeparam>
/// <param name="factory">The species.</param>
internal sealed partial class FactorySpawnCreation<TSettings>(IEnemySpawnFactory<TSettings> factory) : SpeciesSpawnCreation<TSettings>
    where TSettings : EnemySpawnSettings
{
    /// <summary>The species.</summary>
    private readonly IEnemySpawnFactory<TSettings> _factory = Ensure.NotNull(factory);

    /// <inheritdoc/>
    internal override EnemyOccupant Create(RoomEnemyOccupant occupant, TSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        _factory.Create(occupant, settings, place, common);
}
