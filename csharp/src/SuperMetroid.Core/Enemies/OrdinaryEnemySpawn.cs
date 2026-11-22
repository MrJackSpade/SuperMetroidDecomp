namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An entry whose species builds its enemy the same way wherever it is set up (#2554): placed by the population load,
/// rebuilt by a respawn, or spawned on another enemy's behalf.
/// </summary>
/// <typeparam name="TSettings">The settings the species' creation takes.</typeparam>
internal sealed partial class OrdinaryEnemySpawn<TSettings> : PopulationEnemySpawn, IEnemyBuild where TSettings : EnemySpawnSettings
{
    /// <summary>The settings the entry gives its species.</summary>
    private readonly TSettings _settings;

    /// <summary>The species' creation, which every entry bound to it shares.</summary>
    private readonly SpeciesSpawnCreation<TSettings> _create;

    /// <summary>Binds <paramref name="placement"/>, whose settings are <paramref name="settings"/>, to <paramref name="create"/>.</summary>
    /// <param name="placement">The entry.</param>
    /// <param name="settings">The entry's settings, as the species takes them.</param>
    /// <param name="create">The species' creation, which every entry bound to it shares.</param>
    internal OrdinaryEnemySpawn(RoomEnemyPlacement placement, TSettings settings, SpeciesSpawnCreation<TSettings> create)
        : base(placement)
    {
        _settings = Ensure.NotNull(settings);
        _create = Ensure.NotNull(create);
    }

    /// <summary>Binds <paramref name="placement"/>, the one entry <paramref name="create"/> builds, to it.</summary>
    /// <param name="placement">The entry.</param>
    /// <param name="settings">The entry's settings, as the species takes them.</param>
    /// <param name="create">The creation of the entry's enemy, which no other entry shares.</param>
    internal OrdinaryEnemySpawn(RoomEnemyPlacement placement, TSettings settings, EnemySpawnCreation<TSettings> create)
        : this(placement, settings, new SpeciesSpawnCreation<TSettings>(create))
    {
    }

    /// <summary>Binds <paramref name="placement"/>, whose settings are <paramref name="settings"/>, to <paramref name="factory"/>.</summary>
    /// <param name="placement">The entry.</param>
    /// <param name="settings">The entry's settings, as the species takes them.</param>
    /// <param name="factory">The species.</param>
    internal OrdinaryEnemySpawn(RoomEnemyPlacement placement, TSettings settings, IEnemySpawnFactory<TSettings> factory)
        : this(placement, settings, Ensure.NotNull(factory).Create)
    {
    }

    /// <inheritdoc/>
    internal override IEnemyBuild Respawn => this;

    /// <inheritdoc/>
    internal override IEnemyBuild Loaded(EnemyPopulationSurroundings surroundings) => this;

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        _create.Create(occupant, _settings, place, common);
}
