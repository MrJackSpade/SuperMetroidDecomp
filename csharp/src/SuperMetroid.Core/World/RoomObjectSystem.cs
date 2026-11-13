using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
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
public sealed class RoomObjectSystem : ISamusRoomObjects, IBossRoomObjects
{
    /// <summary>Creates collision-triggered block room objects from Samus and projectile reactions.</summary>
    internal BlockReactionObjects BlockReactions { get; }

    /// <summary>Applies boss encounter mutations to the active room's room-object slots.</summary>
    internal BossRoomMutationObjects BossRoomMutations { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    internal IGlobalEventFlags? Events { get; }

    /// <summary>The room's room objects at their places in the update order, shared by every room-object family.</summary>
    internal RoomObjectSlotTable SlotTable { get; }

    /// <summary>Queues sound requests emitted by room object instructions.</summary>
    internal RoomObjectSoundQueue Sounds { get; }

    /// <summary>Turns room-object draw-list commands into level changes and background picture updates.</summary>
    internal RoomObjectBlockDrawing Drawing { get; }

    /// <summary>Spawns room objects for breakable blocks struck by projectile impacts.</summary>
    internal ProjectileTriggerObjects ProjectileTriggers { get; }

    /// <summary>Handles enemy-triggered changes to breakable room terrain.</summary>
    internal EnemyBreakableTerrainObjects BreakableTerrain { get; }

    /// <summary>Runs quicksand room objects through the shared slot allocation.</summary>
    internal QuicksandObjects Quicksand { get; }

    /// <summary>Coordinates Shaktool's tunnel digging and block replacement room objects.</summary>
    internal ShaktoolObjects Shaktool { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    internal SamusEaterObjects SamusEater { get; }

    /// <summary>Runs Tourian room object instructions, including encounter scenery.</summary>
    internal TourianObjects Tourian { get; }

    /// <summary>Controls escape animal room triggers and their persistent event.</summary>
    internal EscapeAnimalObjects EscapeAnimals { get; }

    /// <summary>Handles speed-booster escape tunnel block reactions.</summary>
    internal SpeedBoosterEscapeObjects SpeedBoosterEscape { get; }

    /// <summary>Connects Chozo statue activity with its room FX and room-object state.</summary>
    internal ChozoStatueObjects ChozoStatue { get; }

    /// <summary>The room's scroll triggers and extensions.</summary>
    internal ScrollObjects Scrolls { get; }

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    internal DraygonCannonObjects DraygonCannons { get; }

    /// <summary>Runs Bomb Torizo's hand and statue room object sequences.</summary>
    internal BombTorizoHandObjects BombTorizoHand { get; }

    /// <summary>Runs the glass tube's scripted break and sound sequence.</summary>
    internal NoobTubeObjects NoobTube { get; }

    /// <summary>Advances Wrecked Ship treadmill animated tile objects.</summary>
    internal WreckedShipTreadmillObjects WreckedShipTreadmill { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    internal GateObjects Gates { get; }

    /// <summary>Controls the Mother Brain chamber glass barrier and shatter sequence.</summary>
    internal MotherBrainGlassObjects MotherBrainGlass { get; }

    /// <summary>Builds a room's room-object population into the slot table, in spawn order.</summary>
    internal RoomObjectPopulationLoader PopulationLoader { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    internal StationObjects Stations { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    internal CollectibleObjects CollectibleObjects { get; }

    /// <summary>Runs eye door opening and projectile interaction instructions.</summary>
    internal EyeDoorObjects EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    internal GreyDoorObjects GreyDoors { get; }

    /// <summary>Creates one room's empty room-object list and the room-object family owners that run in it.</summary>
    /// <param name="room">The room the objects live in.</param>
    /// <param name="services">The room's service owners; each room-object family receives the ones it uses.</param>
    /// <param name="visuals">The host's installed room-object artwork.</param>
    /// <param name="collectibleBlockPairs">
    /// The room's own block pairs its collectibles that show their own picture take in turn, created with the system by
    /// its builder so each room starts at the first pair.
    /// </param>
    /// <param name="places">The room's own room-object places, created with the system by its builder so each room starts with every place free.</param>
    internal RoomObjectSystem(RoomObjectRoom room, RoomObjectServices services, RoomObjectVisuals visuals,
        CollectibleBlockPairs collectibleBlockPairs, RoomObjectPlaces places)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(services);
        Events = services.Events;
        SlotTable = new RoomObjectSlotTable(places);
        Sounds = new RoomObjectSoundQueue();
        Drawing = new RoomObjectBlockDrawing(visuals);
        ProjectileTriggers = new ProjectileTriggerObjects(SlotTable);
        BreakableTerrain = new EnemyBreakableTerrainObjects(SlotTable);
        Quicksand = new QuicksandObjects(SlotTable);
        Scrolls = new ScrollObjects(SlotTable);
        WreckedShipTreadmill = new WreckedShipTreadmillObjects(SlotTable, Drawing);
        Shaktool = new ShaktoolObjects(services.Samus, services.Events, SlotTable);
        SamusEater = new SamusEaterObjects(services.Samus, SlotTable);
        EscapeAnimals = new EscapeAnimalObjects(services.Events, SlotTable, Drawing);
        SpeedBoosterEscape = new SpeedBoosterEscapeObjects(services.Samus, services.Events, services.RoomFx, services.Earthquake);
        ChozoStatue = new ChozoStatueObjects(room.System, room.AreaIndex, SlotTable);
        DraygonCannons = new DraygonCannonObjects(services.Enemies);
        BombTorizoHand = new BombTorizoHandObjects(services.Samus, Sounds);
        NoobTube = new NoobTubeObjects(services.Samus, services.Events, services.RoomFx, services.Earthquake, services.Enemies);
        Gates = new GateObjects(services.Samus, SlotTable, Sounds);
        MotherBrainGlass = new MotherBrainGlassObjects(services.Bosses, services.Events, Sounds);
        Stations = new StationObjects(room.System, services.Samus, Drawing, SlotTable, Sounds);
        CollectibleObjects = new CollectibleObjects(room.System, services.Samus, services.RoomFx, Drawing, SlotTable, collectibleBlockPairs, Sounds);
        EyeDoors = new EyeDoorObjects(room.System, services.Samus, services.Enemies, Sounds);
        DoorObjects = new DoorObjects(room.System, SlotTable, Sounds);
        GreyDoors = new GreyDoorObjects(room.System, room.AreaIndex, services.TourianStatue, Sounds);
        Tourian = new TourianObjects(services.Samus, services.Enemies, SlotTable);
        PopulationLoader = new RoomObjectPopulationLoader(new RoomObjectPopulationLoaderDependencies { BombTorizoHand = BombTorizoHand, CollectibleObjects = CollectibleObjects, EyeDoors = EyeDoors, Gates = Gates, MotherBrainGlass = MotherBrainGlass, SlotTable = SlotTable, SpeedBoosterEscape = SpeedBoosterEscape, NoobTube = NoobTube });
        BlockReactions = new BlockReactionObjects(room.AreaIndex, EscapeAnimals, SlotTable, Drawing);
        BossRoomMutations = new BossRoomMutationObjects(SlotTable);
    }

    /// <summary>Door-object instruction owner bound to this room's allocation and draw state.</summary>
    internal DoorObjects DoorObjects { get; }


    /// <summary>
    /// Executes the room-object handler with the frame boundary's newly pressed controller bits.
    /// Resident programs such as the n00b tube consume edges, not held input.
    /// </summary>
    public void Step(RoomLevelData level, RoomObjectFrameInputs frame)
    {
        Ensure.NotNull(level);
        Stations.BeginFrame();
        MotherBrainGlass.BeginMotherBrainGlassFrame();
        CollectibleObjects.BeginCollectibleFrame();
        BombTorizoHand.BeginBombTorizoHandFrame();

        var context = new RoomObjectStepContext(this, level, frame);
        // Each typed room object advances by its own steps, at its place in the update order; a place that holds no
        // typed object (one whose setup did all its work) stays idle until it is freed.
        foreach (RoomObjectSlot slot in SlotTable.InProcessingOrder)
        {
            if (slot.Active && slot.Behaviour is { } behaviour)
                behaviour.Update(slot, context);
        }

    }

    /// <summary>
    /// The object at <paramref name="slot"/> leaves the room, freeing its place; Mother Brain's glass first keeps its hit
    /// count.
    /// </summary>
    internal void DeleteObject(RoomObjectSlot slot)
    {
        MotherBrainGlass.OnObjectDeleted(slot);
        slot.Delete();
    }

    /// <inheritdoc/>
    BlockReactionObjects IRoomObjects.BlockReactions => BlockReactions;

    /// <inheritdoc/>
    IGlobalEventFlags? IRoomObjects.Events => Events;

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
    GateObjects IRoomObjects.Gates => Gates;

    /// <inheritdoc/>
    SamusEaterObjects IRoomObjects.SamusEater => SamusEater;

    /// <inheritdoc/>
    ScrollObjects IRoomObjects.Scrolls => Scrolls;

    /// <inheritdoc/>
    void IRoomObjects.DeleteObject(RoomObjectSlot slot) => DeleteObject(slot);

    /// <inheritdoc/>
    ProjectileTriggerObjects ISamusRoomObjects.ProjectileTriggers => ProjectileTriggers;

    /// <inheritdoc/>
    QuicksandObjects ISamusRoomObjects.Quicksand => Quicksand;

    /// <inheritdoc/>
    IChozoStatueHand ISamusRoomObjects.ChozoStatueHand => ChozoStatue;

    /// <inheritdoc/>
    DraygonCannonObjects IBossRoomObjects.DraygonCannons => DraygonCannons;

}
