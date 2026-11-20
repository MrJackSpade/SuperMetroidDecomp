namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy spawned joining the encounter that spawns it (#2218, #2554): the spawner binds the entry to its species'
/// joining creation, the settings and the encounter, so the entity receives them as it is built.
/// </summary>
/// <typeparam name="TEncounter">The encounter the spawned enemy joins.</typeparam>
/// <typeparam name="TSettings">The settings the spawn gives the species.</typeparam>
internal sealed class JoiningEnemySpawn<TEncounter, TSettings> : EnemySpawn, IEnemyBuild
    where TEncounter : class where TSettings : EnemySpawnSettings
{
    /// <summary>The species.</summary>
    private readonly IEnemyJoiningFactory<TEncounter, TSettings> _factory;

    /// <summary>The settings the spawn gives the species.</summary>
    private readonly TSettings _settings;

    /// <summary>The encounter the spawned enemy joins.</summary>
    private readonly TEncounter _encounter;

    /// <summary>Binds <paramref name="placement"/>, whose settings are <paramref name="settings"/>, to <paramref name="factory"/> and <paramref name="encounter"/>.</summary>
    /// <param name="placement">The entry.</param>
    /// <param name="settings">The entry's settings, as the species takes them.</param>
    /// <param name="factory">The species.</param>
    /// <param name="encounter">The encounter the spawned enemy joins.</param>
    /// <exception cref="InvalidDataException">The entry lets its enemy respawn.</exception>
    internal JoiningEnemySpawn(RoomEnemyPlacement placement, TSettings settings, IEnemyJoiningFactory<TEncounter, TSettings> factory, TEncounter encounter)
        : base(NeverRespawning(placement, "spawned joining an encounter"))
    {
        _settings = Ensure.NotNull(settings);
        _factory = Ensure.NotNull(factory);
        _encounter = Ensure.NotNull(encounter);
    }

    /// <inheritdoc/>
    internal override IEnemyBuild? Respawn => null;

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        _factory.CreateJoining(occupant, _settings, place, common, _encounter);
}
