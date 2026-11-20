namespace SuperMetroid.Core.Enemies;

/// <summary>An entry whose species only the population load builds, seeing the load's surroundings (#2554).</summary>
/// <typeparam name="TSettings">The settings the species' creation takes.</typeparam>
internal sealed class LoadedEnemySpawn<TSettings> : PopulationEnemySpawn where TSettings : EnemySpawnSettings
{
    /// <summary>The settings the entry gives its species.</summary>
    private readonly TSettings _settings;

    /// <summary>The species' creation.</summary>
    private readonly EnemyLoadedCreation<TSettings> _create;

    /// <summary>Binds <paramref name="placement"/>, whose settings are <paramref name="settings"/>, to <paramref name="create"/>.</summary>
    /// <param name="placement">The entry.</param>
    /// <param name="settings">The entry's settings, as the species takes them.</param>
    /// <param name="create">The species' creation.</param>
    /// <exception cref="InvalidDataException">The entry lets its enemy respawn.</exception>
    internal LoadedEnemySpawn(RoomEnemyPlacement placement, TSettings settings, EnemyLoadedCreation<TSettings> create)
        : base(NeverRespawning(placement, "placed only by its room's population load"))
    {
        _settings = Ensure.NotNull(settings);
        _create = Ensure.NotNull(create);
    }

    /// <inheritdoc/>
    internal override IEnemyBuild? Respawn => null;

    /// <inheritdoc/>
    internal override IEnemyBuild Loaded(EnemyPopulationSurroundings surroundings) => new InSurroundings(this, Ensure.NotNull(surroundings));

    /// <summary>The entry's build in the population load's surroundings.</summary>
    /// <param name="spawn">The entry.</param>
    /// <param name="surroundings">The load's surroundings.</param>
    private sealed class InSurroundings(LoadedEnemySpawn<TSettings> spawn, EnemyPopulationSurroundings surroundings) : IEnemyBuild
    {
        /// <inheritdoc/>
        public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
            spawn._create(occupant, spawn._settings, place, common, surroundings);
    }
}
