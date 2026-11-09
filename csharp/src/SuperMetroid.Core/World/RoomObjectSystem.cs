using SuperMetroid.Core.Bosses;
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
/// Frame-steppable room-object owner translating movement-triggered breakable terrain and its
/// authored animation scripts.
/// </summary>
/// <remarks>
/// A room object is not a Samus animation. It is an independent room object which keeps running
/// after the rope disconnects, owns collision/BTS mutation, and requests a background redraw when
/// an instruction changes its level word. Keeping this state outside <c>SamusGrappleState</c>
/// is essential for the respawning block: Samus is long gone when its original word returns.
///
/// The retail allocation contains 40 slots and searches from
/// the highest slot downward. The C# array uses logical indices 0..39 and preserves that
/// search and handler order. The translated step surface is deliberately limited to the
/// collision-bomb, projectile-reaction bomb, and breakable-grapple lists. Encountering any
/// other program fails instead of silently inventing an effect for a still-untranslated room object
/// family.
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

    /// <summary>Forty word-indexed room-object slots shared by the room's object families.</summary>
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

    /// <summary>Creates one room's forty empty room-object slots and the room-object family owners that run in them.</summary>
    /// <param name="room">The room the objects live in.</param>
    /// <param name="services">The room's service owners; each room-object family receives the ones it uses.</param>
    /// <param name="visuals">The host's installed room-object artwork.</param>
    internal RoomObjectSystem(RoomObjectRoom room, RoomObjectServices services, RoomObjectVisuals visuals)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(services);
        Events = services.Events;
        SlotTable = new RoomObjectSlotTable();
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
        BombTorizoHand = new BombTorizoHandObjects(services.Samus, SlotTable);
        NoobTube = new NoobTubeObjects(services.Samus, services.Events, services.RoomFx, services.Earthquake, services.Enemies);
        Gates = new GateObjects(services.Samus, room.Level, SlotTable, Sounds);
        MotherBrainGlass = new MotherBrainGlassObjects(services.Bosses, services.Events, Sounds);
        Stations = new StationObjects(room.System, services.Samus, Drawing, SlotTable, Sounds);
        CollectibleObjects = new CollectibleObjects(room.System, services.Samus, services.RoomFx, Drawing, SlotTable);
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
        Sounds.Clear();
        Gates.PublishPendingDownwardGateSounds();
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
    /// Releases the slot. Lists and typed programs end a room object the same way.
    /// </summary>
    internal void DeleteObject(RoomObjectSlot slot)
    {
        slot.Delete();
        MotherBrainGlass.OnObjectDeleted(slot);
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
