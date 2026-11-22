using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Puts populations into places and runs their AI: place setup from a definition, initialization and main AI
/// through each place's entity, spawns into the first free place, and the places Samus can touch this frame.
/// </summary>
[RoomLifetime]
internal sealed partial class EnemyLifecycle : IEnemyLifecycle
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
    /// <returns>The new occupant, whose species builds its entity as it is set up.</returns>
    internal static RoomEnemyOccupant InitializePlaceFromDefinition(
        RoomEnemyPlace place,
        EnemySpawn spawn,
        RoomEnemyDefinition definition)
    {
        // The occupant carries its placement into the entity its species creates, so a species that places several actors
        // creates the one the placement names (#1953).
        RoomEnemyOccupant occupant = place.Bind(spawn, definition);
        RoomEnemyPlacement population = spawn.Placement;
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
    /// Runs the setup of the enemy the room's population load has just placed: its own, or the one it runs from the load's
    /// <paramref name="surroundings"/>, which the load has; nothing for an enemy that has none.
    /// </summary>
    internal static void SetUpEnemy(RoomEnemyOccupant occupant, PopulationEnemySpawn spawn, EnemyPopulationSurroundings surroundings)
    {
        Ensure.NotNull(surroundings);
        EnemyOccupant built = Ensure.NotNull(occupant).Place.Build(occupant, Ensure.NotNull(spawn).Loaded(surroundings));
        built.Initialization?.Initialize();
        built.SurroundingsInitialization?.Initialize(surroundings);
    }

    /// <summary>
    /// Runs the setup of an enemy spawned on another enemy's behalf, or rebuilt by a respawn (#2455): its own, and nothing
    /// for an enemy that has none. Such a spawn has no population load's surroundings, so a setup that reads them is the
    /// population load's alone.
    /// </summary>
    internal static void SetUpSpawnedEnemy(RoomEnemyOccupant occupant, IEnemyBuild build) =>
        Ensure.NotNull(occupant).Place.Build(occupant, Ensure.NotNull(build)).Initialization?.Initialize();

    /// <summary>Exposes setting up a listed enemy from its population entry to the enemy loader.</summary>
    RoomEnemyOccupant IEnemyLifecycle.InitializePlaceFromDefinition(
        RoomEnemyPlace place, EnemySpawn spawn, RoomEnemyDefinition definition) =>
        InitializePlaceFromDefinition(place, spawn, definition);

    /// <summary>Lets the population loader run the place's species initialization.</summary>
    void IEnemyLifecycle.SetUpEnemy(RoomEnemyOccupant occupant, PopulationEnemySpawn spawn, EnemyPopulationSurroundings surroundings) =>
        SetUpEnemy(occupant, spawn, surroundings);

    /// <summary>Lets a species run the initialization of an enemy it spawned on another enemy's behalf.</summary>
    void IEnemyLifecycle.SetUpSpawnedEnemy(RoomEnemyOccupant occupant, IEnemyBuild build) => SetUpSpawnedEnemy(occupant, build);

    /// <inheritdoc/>
    RoomEnemyPlace? IEnemyLifecycle.SpawnEnemy<TSpawn>(TSpawn spawn) =>
        Spawn(Ensure.NotNull(spawn), occupant => SetUpSpawnedEnemy(occupant, spawn));


    /// <summary>
    /// Installs <paramref name="population"/> in the first free place, runs its setup through <paramref name="setUp"/>,
    /// starts its animation when it runs one and extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace? Spawn(EnemySpawn population, Action<RoomEnemyOccupant> setUp) =>
        _places.FindFreePlace() is { } place ? SpawnAt(place, population, setUp) : null;

    /// <summary>
    /// Installs <paramref name="population"/> in <paramref name="place"/>, a free one, runs its setup through
    /// <paramref name="setUp"/>, starts its animation when it runs one and extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace SpawnAt(RoomEnemyPlace place, EnemySpawn population, Action<RoomEnemyOccupant> setUp)
    {
        setUp(InitializePlaceFromDefinition(place, population, _room.Definitions.ReadEnemyDefinition(population.Placement.DefinitionId)));
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
    /// Runs the main AI the entity bound in a loaded place carries, and nothing for one that has none, as a place awaiting
    /// its enemy's respawn has none: the enemy pass still counts its update.
    /// </summary>
    internal static void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        Ensure.NotNull(place).BoundEntity?.MainAi?.RunMainAi(frame);
}
