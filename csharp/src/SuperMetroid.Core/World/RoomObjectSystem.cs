using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Areas.WreckedShip;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room's room objects: every family that spawns, sets up and runs them, the list of what the room holds and the
/// room-object update that advances each in the room's update order.
/// </summary>
/// <remarks>
/// A room object is not Samus's or an enemy's: it is the room's own, and it keeps running after whatever started it has
/// gone (a respawning grapple block grows back long after Samus has swung away). Which place each object holds, the order
/// the places update in and the loss of a spawn into a full room are the room's <see cref="Compatibility.RoomObjectPlaces"/>.
/// </remarks>
[RoomLifetime]
public sealed partial class RoomObjectSystem : IRoomObjects, ISamusRoomObjects
{
    /// <summary>Creates collision-triggered block room objects from Samus and projectile reactions.</summary>
    internal BlockReactionObjects BlockReactions { get; }

    /// <summary>Applies boss encounter mutations through the active room's room-object places.</summary>
    internal BossRoomMutationObjects BossRoomMutations { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    internal IStoryEvents Events { get; }

    /// <summary>The room's room objects at their places in the update order, shared by every room-object family.</summary>
    internal RoomObjectPlaceList Places { get; }

    /// <summary>Queues sound requests emitted by room object cues.</summary>
    internal RoomObjectSoundQueue Sounds { get; }

    /// <summary>Turns room objects' block draws into level changes and background picture updates.</summary>
    internal RoomObjectBlockDrawing Drawing { get; }

    /// <summary>Spawns room objects for breakable blocks struck by projectile impacts.</summary>
    internal ProjectileTriggerObjects ProjectileTriggers { get; }

    /// <summary>Handles enemy-triggered changes to breakable room terrain.</summary>
    internal EnemyBreakableTerrainObjects BreakableTerrain { get; }

    /// <summary>Runs quicksand room objects, which take the room's room-object places.</summary>
    internal QuicksandObjects Quicksand { get; }

    /// <summary>Coordinates Shaktool's tunnel digging and block replacement room objects.</summary>
    internal ShaktoolObjects Shaktool { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    internal SamusEaterObjects SamusEater { get; }

    /// <summary>Runs Tourian room object cues, including encounter scenery.</summary>
    internal TourianObjects Tourian { get; }

    /// <summary>Runs the Ceres escape's room objects: the elevator room's door seal (#2494).</summary>
    internal CeresEscapeObjects CeresEscape { get; }

    /// <summary>Controls escape animal room triggers and their persistent event.</summary>
    internal EscapeAnimalObjects EscapeAnimals { get; }

    /// <summary>Handles speed-booster escape tunnel block reactions.</summary>
    internal SpeedBoosterEscapeObjects SpeedBoosterEscape { get; }

    /// <summary>Connects Chozo statue activity with its room FX and room-object state.</summary>
    internal ChozoStatueObjects ChozoStatue { get; }

    /// <summary>The room's scroll triggers and extensions.</summary>
    internal ScrollObjects Scrolls { get; }

    /// <summary>
    /// The room's door contacts (#2545): the door Samus entered, until the door transition goes through it, and her stand at
    /// the elevator's door, which the elevator takes on the next update. They live with the room, as its room objects do.
    /// </summary>
    internal RoomDoorContacts DoorContacts { get; } = new();

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    internal DraygonCannonObjects DraygonCannons { get; }

    /// <summary>Runs the glass tube's scripted break and sound sequence.</summary>
    internal NoobTubeObjects NoobTube { get; }

    /// <summary>Spawns the Wrecked Ship entrance treadmills.</summary>
    internal WreckedShipTreadmillObjects WreckedShipTreadmill { get; }

    /// <summary>Spawns the object that starts a Maridia elevatube ride.</summary>
    internal MaridiaElevatubeObjects MaridiaElevatube { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    internal GateObjects Gates { get; }

    /// <summary>Builds a room's room-object population into its places, in spawn order.</summary>
    internal RoomObjectPopulationLoader PopulationLoader { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    internal StationObjects Stations { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    internal CollectibleObjects CollectibleObjects { get; }

    /// <summary>Runs eye door opening and projectile interaction cues.</summary>
    internal EyeDoorObjects EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    internal GreyDoorObjects GreyDoors { get; }

    /// <summary>Creates one room's empty room-object list and the room-object family owners that run in it.</summary>
    /// <param name="room">The room the objects live in.</param>
    /// <param name="services">The room's service owners; each room-object family receives the ones it uses.</param>
    /// <param name="reachedByEnemies">
    /// The room's room-object places and sounds and the room objects its enemies reach, built before the enemies.
    /// </param>
    /// <param name="visuals">The host's installed room-object artwork.</param>
    /// <param name="collectibleBlockPairs">
    /// The room's own block pairs its collectibles that show their own picture take in turn, created with the system by
    /// its builder so each room starts at the first pair.
    /// </param>
    /// <param name="shotCapClosing">Whether a blue cap a shot opened closes behind Samus, composed by the system's builder.</param>
    /// <param name="thrown">The room's enemy projectiles' spawners of the projectiles its room objects throw.</param>
    internal RoomObjectSystem(RoomObjectRoom room, RoomObjectServices services, RoomObjectsReachedByEnemies reachedByEnemies,
        RoomObjectVisuals visuals, CollectibleBlockPairs collectibleBlockPairs, ShotOpenedCapClosing<LockedDoor> shotCapClosing,
        RoomObjectThrownProjectileSpawners thrown)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(services);
        Ensure.NotNull(reachedByEnemies);
        Events = services.Events;
        Places = reachedByEnemies.Places;
        Sounds = reachedByEnemies.Sounds;
        Drawing = new RoomObjectBlockDrawing(visuals);
        ProjectileTriggers = new ProjectileTriggerObjects(Places);
        BreakableTerrain = reachedByEnemies.BreakableTerrain;
        Quicksand = new QuicksandObjects(Places);
        Scrolls = new ScrollObjects(Places);
        WreckedShipTreadmill = new WreckedShipTreadmillObjects(Places, Drawing);
        MaridiaElevatube = new MaridiaElevatubeObjects(Places);
        Shaktool = new ShaktoolObjects(services.Samus, services.Events, Places);
        SamusEater = new SamusEaterObjects(services.Samus, Places);
        EscapeAnimals = new EscapeAnimalObjects(services.Events, Places, Drawing);
        SpeedBoosterEscape = new SpeedBoosterEscapeObjects(services.Samus, services.Events, services.RoomFx, services.Earthquake);
        ChozoStatue = new ChozoStatueObjects(room.System, room.Area, services.RoomFx, Places);
        DraygonCannons = new DraygonCannonObjects(services.Enemies);
        NoobTube = new NoobTubeObjects(services.Samus, services.Events, services.RoomFx, services.Earthquake, services.Enemies);
        Gates = new GateObjects(services.Samus, Places, Sounds);
        Stations = new StationObjects(room.System, services.Samus, Drawing, Places, Sounds);
        CollectibleObjects = new CollectibleObjects(room.System, services.Samus, services.RoomFx, Drawing, Places, collectibleBlockPairs, Sounds);
        EyeDoors = new EyeDoorObjects(room.System, services.Samus, services.Enemies, Sounds);
        DoorObjects = new DoorObjects(room.System, Places, Sounds, shotCapClosing);
        GreyDoors = new GreyDoorObjects(room.System, room.Area, services.TourianStatue, Sounds);
        Tourian = new TourianObjects(services.Samus, services.Enemies, services.RoomFx, Places);
        CeresEscape = new CeresEscapeObjects(Places);
        PopulationLoader = new RoomObjectPopulationLoader(new RoomObjectPopulationLoaderDependencies { BombTorizoHand = reachedByEnemies.BombTorizoHand, CollectibleObjects = CollectibleObjects, EyeDoors = EyeDoors, GreyDoors = GreyDoors, Stations = Stations, Gates = Gates, MotherBrainGlass = reachedByEnemies.MotherBrainGlass, Places = Places, SpeedBoosterEscape = SpeedBoosterEscape, DraygonCannons = DraygonCannons, NoobTube = NoobTube, Scrolls = Scrolls }, thrown);
        BlockReactions = new BlockReactionObjects(EscapeAnimals, Places, Drawing);
        BossRoomMutations = new BossRoomMutationObjects(Places);
    }

    /// <summary>Door-object owner bound to this room's allocation and draw state.</summary>
    internal DoorObjects DoorObjects { get; }

    /// <summary>
    /// The objects as the door transition's step reaches them, freshly loaded and untouched by Samus
    /// (<see cref="UntouchedRoomObjects"/>).
    /// </summary>
    internal UntouchedRoomObjects Untouched => new(this);


    /// <summary>
    /// Steps the room objects with the buttons newly pressed at the frame boundary.
    /// Resident room objects such as the n00b tube consume edges, not held input.
    /// </summary>
    /// <returns>
    /// The step's own outcome (#2514): the items it collected, the stations' actions and the projectiles its room objects
    /// threw, for the update that ran it to act on.
    /// </returns>
    public RoomObjectStepOutcome Step(RoomLevelData level, RoomObjectFrameInputs frame)
    {
        Ensure.NotNull(level);
        var outcome = new RoomObjectStepOutcome();
        var context = new RoomObjectStepContext(this, level, frame, outcome);
        // Each typed room object advances by its own steps, at its place in the update order; a free place has none
        // to advance.
        foreach (RoomObjectPlace place in Places.InUpdateOrder)
        {
            if (place.Behaviour is { } behaviour)
                behaviour.Update(place, context);
        }
        return outcome;
    }

    /// <summary>The object at <paramref name="place"/> leaves the room, freeing its place.</summary>


    /// <inheritdoc/>
    BlockReactionObjects IRoomObjects.BlockReactions => BlockReactions;

    /// <inheritdoc/>
    IStoryEvents IRoomObjects.Events => Events;

    /// <inheritdoc/>
    RoomObjectSoundQueue IRoomObjects.Sounds => Sounds;

    /// <inheritdoc/>
    IRoomObjectDrawing IRoomObjects.Drawing => Drawing;

    /// <inheritdoc/>
    CollectibleObjects IRoomObjects.CollectibleObjects => CollectibleObjects;

    /// <inheritdoc/>
    StationObjects IRoomObjects.Stations => Stations;

    /// <inheritdoc/>
    DoorObjects IRoomObjects.DoorObjects => DoorObjects;

    /// <inheritdoc/>
    EyeDoorObjects IRoomObjects.EyeDoors => EyeDoors;

    /// <inheritdoc/>
    GreyDoorObjects IRoomObjects.GreyDoors => GreyDoors;

    /// <inheritdoc/>
    void IRoomObjects.DeleteObject(RoomObjectPlace place) => Ensure.NotNull(place).Delete();

    /// <inheritdoc/>
    BlockReactionObjects ISamusRoomObjects.BlockReactions => BlockReactions;

    /// <inheritdoc/>
    CollectibleObjects ISamusRoomObjects.CollectibleObjects => CollectibleObjects;

    /// <inheritdoc/>
    StationObjects ISamusRoomObjects.Stations => Stations;

    /// <inheritdoc/>
    DoorObjects ISamusRoomObjects.DoorObjects => DoorObjects;

    /// <inheritdoc/>
    GateObjects ISamusRoomObjects.Gates => Gates;

    /// <inheritdoc/>
    SamusEaterObjects ISamusRoomObjects.SamusEater => SamusEater;

    /// <inheritdoc/>
    ScrollObjects ISamusRoomObjects.Scrolls => Scrolls;

    /// <inheritdoc/>
    bool ISamusRoomObjects.EveryPlaceHeld => Places.IsFull;

    /// <inheritdoc/>
    ProjectileTriggerObjects ISamusRoomObjects.ProjectileTriggers => ProjectileTriggers;

    /// <inheritdoc/>
    QuicksandObjects ISamusRoomObjects.Quicksand => Quicksand;

    /// <inheritdoc/>
    IChozoStatueHand ISamusRoomObjects.ChozoStatueHand => ChozoStatue;

    /// <inheritdoc/>
    RoomDoorContacts ISamusRoomObjects.DoorContacts => DoorContacts;
}
