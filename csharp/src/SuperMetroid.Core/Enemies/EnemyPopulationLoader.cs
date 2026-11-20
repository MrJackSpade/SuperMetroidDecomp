using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room enemy population loading: the population and kill quota, and resolution of enemy definitions and
/// populations from the room state.
/// </summary>
internal sealed class EnemyPopulationLoader
{
    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the boss identity declared by an initialized population entry.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Holds the room's enemy places and the extent the population fills.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Applies enemy definitions and runs each place's initialization AI.</summary>
    private readonly IEnemyLifecycle _lifecycle;

    /// <summary>Binds room artwork, status, place storage, and the enemy lifecycle for a population load.</summary>
    internal EnemyPopulationLoader(EnemyRoomBinding room, EnemyRoomStatus roomStatus, EnemyPlaces places, IEnemyLifecycle lifecycle)
    {
        _room = room;
        _roomStatus = roomStatus;
        _places = places;
        _lifecycle = lifecycle;
    }
    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches: the room population's own quota
    /// (<see cref="PlacedRoomEnemyPopulationDefinition.DeathQuota"/>) when the room places enemies; a room that places none
    /// keeps the quota of the last room that did.
    /// </summary>
    public int DeathQuota
    {
        get => _room.Session.DeathQuota;
        private set => _room.Session.DeathQuota = value;
    }

    /// <summary>Initializes each population place, runs its AI, and publishes count and kill quota for nonempty populations.</summary>
    internal void LoadPopulation(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyPopulationDefinition populationDefinition,
        EnemyPopulationSurroundings surroundings)
    {
        ReadOnlySpan<RoomEnemyPlacement> placements = populationDefinition.Placements.Span;
        for (int ordinal = 0; ordinal < placements.Length; ordinal++)
        {
            RoomEnemyPlacement population = placements[ordinal];
            EnemyDefinitionId enemy = population.DefinitionId;
            if (ordinal == EnemyPlaceOrder.Capacity)
            {
                throw new InvalidDataException(
                    $"Enemy population {populationDefinition.Name} has more placements than a room holds enemies.");
            }

            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(enemy);
            RoomEnemyPlace place = _places.InUpdateOrder[ordinal];
            RoomEnemyOccupant occupant = _lifecycle.InitializePlaceFromDefinition(place, population, definition);
            if (definition.Boss is { } boss)
                _roomStatus.Boss = boss;
            _lifecycle.SetUpEnemy(occupant, surroundings);

            // Loading replaces whatever frame the initialization AI showed: an animated enemy shows the empty frame until
            // its first animation tick replaces it, and one that collides by its frames' hitboxes collides at the point
            // at its origin meanwhile. An enemy without an animation shows no frame.
            place.Frame = place.Processing.HasAny(EnemyProcessing.Animates)
                ? EnemyFrameId.PointAtOrigin
                : null;

        }

        // An empty population has no quota: it leaves the enemy count at zero and keeps the previous room's
        // death quota. One that places enemies publishes both after every initialization has run.
        if (populationDefinition is not PlacedRoomEnemyPopulationDefinition placed)
            return;
        DeathQuota = placed.DeathQuota;
        _places.EnemyCount = placements.Length;
    }
}
