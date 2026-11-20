using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Puts populations into places and runs their AI: place setup from a definition, initialization and main AI
/// through each place's entity, spawns into the first free place, and the places Samus can touch this frame.
/// </summary>
[RoomLifetime]
internal sealed class EnemyLifecycle : IEnemyLifecycle
{
    /// <summary>The room whose definitions spawns read.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Selects the places that interact with Samus this frame.</summary>
    private readonly EnemyProcessingWindow _processingWindow;

    /// <summary>Binds the room, places and processing window the lifecycle works in.</summary>
    internal EnemyLifecycle(EnemyRoomBinding room, EnemyPlaces places, EnemyProcessingWindow processingWindow)
    {
        _room = room;
        _places = places;
        _processingWindow = processingWindow;
    }

    /// <summary>
    /// Copies population and definition fields into a place, showing its sheet's standard colours, and records a spawn
    /// snapshot before initialization AI runs.
    /// </summary>
    /// <returns>The occupant installed, which its setup builds the enemy for.</returns>
    internal static RoomEnemyOccupant InitializePlaceFromDefinition(
        RoomEnemyPlace place,
        RoomEnemyPlacement population,
        RoomEnemyDefinition definition)
    {
        // The occupant carries its placement into the entity its species creates, so a species that places several actors
        // creates the one the placement names (#1953).
        RoomEnemyOccupant occupant = place.Bind(population, definition);
        place.Radius = new(definition.Radius.X, definition.Radius.Y);
        place.Health = definition.Health;
        place.Position = population.Placing.Place(place.Position);

        place.Contact = population.Contact;
        place.Processing = population.Processing;
        place.Presence = population.Presence;
        place.CollidesByFrameHitboxes = population.CollidesByFrameHitboxes;
        place.AiStates = EnemyAiStates.None;
        place.AnimationClock.Restart();
        place.FrameCounter = 0;
        place.HurtFlashFrames = definition.HurtFlashFrames;
        place.Shading = PictureShading.Standard;
        return occupant;
    }

    /// <summary>
    /// Builds the enemy for an occupant the room's population load has just placed, with its state: its species'
    /// initialization runs as it is built. A group's head takes its parts' places from <paramref name="listedAfter"/>.
    /// </summary>
    /// <returns>How each part of the group the enemy heads is built, in listing order; empty when it heads none.</returns>
    internal static IReadOnlyList<EnemyPartBuild> SetUpEnemy(RoomEnemyOccupant occupant, EnemyPopulationSurroundings surroundings,
        IReadOnlyList<RoomEnemyPlace> listedAfter) =>
        Ensure.NotNull(occupant).Place.SetUpListed(occupant, new EnemySetup.Loaded(Ensure.NotNull(surroundings)), listedAfter);

    /// <summary>
    /// Builds the enemy for an occupant spawned on another enemy's behalf, which has no population load's surroundings
    /// (#2455); its species' initialization runs as it is built.
    /// </summary>
    internal static void SetUpSpawnedEnemy(RoomEnemyOccupant occupant) =>
        Ensure.NotNull(occupant).Place.SetUp(occupant, new EnemySetup.Spawned());

    /// <summary>Exposes setting up a listed enemy from its population entry to the enemy loader.</summary>
    RoomEnemyOccupant IEnemyLifecycle.InitializePlaceFromDefinition(
        RoomEnemyPlace place, RoomEnemyPlacement population, RoomEnemyDefinition definition) =>
        InitializePlaceFromDefinition(place, population, definition);

    /// <summary>Lets the population loader run the place's species initialization.</summary>
    IReadOnlyList<EnemyPartBuild> IEnemyLifecycle.SetUpEnemy(RoomEnemyOccupant occupant, EnemyPopulationSurroundings surroundings,
        IReadOnlyList<RoomEnemyPlace> listedAfter) =>
        SetUpEnemy(occupant, surroundings, listedAfter);

    /// <inheritdoc/>
    void IEnemyLifecycle.SetUpPart(RoomEnemyOccupant occupant, EnemyPartBuild build, EnemyPopulationSurroundings surroundings) =>
        Ensure.NotNull(occupant).Place.SetUpAsPart(occupant, build, new EnemySetup.Loaded(Ensure.NotNull(surroundings)));

    /// <inheritdoc/>
    void IEnemyLifecycle.RespawnEnemy(RoomEnemyOccupant occupant) =>
        Ensure.NotNull(occupant).Place.SetUpAgain(occupant, new EnemySetup.Spawned());

    /// <summary>Lets a species run the initialization of an enemy it spawned on another enemy's behalf.</summary>
    void IEnemyLifecycle.SetUpSpawnedEnemy(RoomEnemyOccupant occupant) => SetUpSpawnedEnemy(occupant);

    /// <inheritdoc/>
    void IEnemyLifecycle.SetUpJoiningEnemy<TEncounter>(RoomEnemyOccupant occupant, TEncounter encounter) =>
        Ensure.NotNull(occupant).Place.SetUp(occupant, new EnemySetup.Joining<TEncounter>(Ensure.NotNull(encounter)));

    /// <inheritdoc/>
    RoomEnemyPlace? IEnemyLifecycle.SpawnEnemyJoining<TEncounter>(RoomEnemyPlacement population, TEncounter encounter) =>
        Spawn(population, new EnemySetup.Joining<TEncounter>(Ensure.NotNull(encounter)));

    /// <summary>
    /// Installs <paramref name="population"/> in the first free place, builds its enemy as <paramref name="setup"/> sets it
    /// up, which runs its initialization, starts its animation when it runs one and extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace? Spawn(RoomEnemyPlacement population, EnemySetup setup) =>
        _places.FindFreePlace() is { } place ? SpawnAt(place, population, setup) : null;

    /// <summary>
    /// Installs <paramref name="population"/> in <paramref name="place"/>, a free one, builds its enemy as
    /// <paramref name="setup"/> sets it up, which runs its initialization, starts its animation when it runs one and
    /// extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace SpawnAt(RoomEnemyPlace place, RoomEnemyPlacement population, EnemySetup setup)
    {
        RoomEnemyOccupant occupant = InitializePlaceFromDefinition(place, population, _room.Definitions.ReadEnemyDefinition(population.DefinitionId));
        place.SetUp(occupant, setup);
        // Spawning writes the empty frame after initialization, before the first animation tick.
        place.Frame = place.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.PointAtOrigin
            : null;
        _places.CoverSpawn(place);
        return place;
    }

    /// <summary>Routes a scheduled enemy update through its species' main AI.</summary>
    void IEnemyLifecycle.RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunMainAi(place, frame);

    /// <summary>Exposes the enemies selected for Samus collision checks this frame.</summary>
    IReadOnlyList<RoomEnemyOccupant> IEnemyLifecycle.InteractiveEnemies => _processingWindow.InteractiveEnemies;

    /// <summary>
    /// Runs the main AI of the enemy a loaded place holds through its shared main-AI contract, and nothing for one that has
    /// none, as a place awaiting its enemy's respawn has none: the enemy pass still counts its update.
    /// </summary>
    internal static void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        if (Ensure.NotNull(place).Entity is IEnemyAi entity)
            entity.RunMainAi(frame);
    }
}
