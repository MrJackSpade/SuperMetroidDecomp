using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Frame-steppable room PLM owner translating movement-triggered breakable terrain and its
/// ROM-authored bank-$84 instruction lists.
/// </summary>
/// <remarks>
/// A PLM is not a Samus animation. It is an independent room object which keeps running
/// after the rope disconnects, owns collision/BTS mutation, and requests a BG1 redraw when
/// an instruction changes its level word. Keeping this state outside <c>SamusGrappleState</c>
/// is essential for the respawning block: Samus is long gone when its original word returns.
///
/// The retail allocation contains 40 word-indexed slots ($00 through $4E) and searches from
/// the highest slot downward. The C# array uses logical indices 0..39 but preserves that
/// search and handler order. The translated opcode surface is deliberately limited to the
/// collision-bomb, projectile-reaction bomb, and breakable-grapple lists. Encountering any
/// other pointer fails instead of silently inventing an effect for a still-untranslated PLM
/// family.
/// </remarks>
public sealed class RoomPlmSystem
{
    /// <summary>Creates collision-triggered block PLMs from Samus and projectile reactions.</summary>
    internal BlockReactionPlms BlockReactions { get; }

    /// <summary>Applies boss encounter mutations to the active room's PLM slots.</summary>
    internal BossRoomMutationPlms BossRoomMutations { get; }

    /// <summary>Connects room events, enemy actors, and memory to the PLM family handlers.</summary>
    internal PlmRoomBindings Bindings { get; }

    /// <summary>Forty native word-indexed PLM slots shared by the room's object families.</summary>
    internal PlmSlotTable SlotTable { get; }

    /// <summary>Queues sound requests emitted by room object instructions.</summary>
    internal PlmSoundQueue Sounds { get; }

    /// <summary>Turns PLM draw-list commands into level changes and BG tilemap updates.</summary>
    internal PlmBlockDrawing Drawing { get; }

    /// <summary>Spawns PLMs for breakable blocks struck by projectile impacts.</summary>
    internal ProjectileTriggerPlms ProjectileTriggers { get; }

    /// <summary>Handles enemy-triggered changes to breakable room terrain.</summary>
    internal EnemyBreakableTerrainPlms BreakableTerrain { get; }

    /// <summary>Runs quicksand room objects through the shared slot allocation.</summary>
    internal QuicksandPlms Quicksand { get; }

    /// <summary>Coordinates Shaktool's tunnel digging and block replacement PLMs.</summary>
    internal ShaktoolPlms Shaktool { get; }

    /// <summary>Runs Samus Eater plant damage, release, and room object state.</summary>
    internal SamusEaterPlms SamusEater { get; }

    /// <summary>Runs Tourian room object instructions, including encounter scenery.</summary>
    internal TourianPlms Tourian { get; }

    /// <summary>Controls escape animal room triggers and their persistent event.</summary>
    internal EscapeAnimalPlms EscapeAnimals { get; }

    /// <summary>Handles speed-booster escape tunnel block reactions.</summary>
    internal SpeedBoosterEscapePlms SpeedBoosterEscape { get; }

    /// <summary>Connects Chozo statue activity with its room FX and PLM state.</summary>
    internal ChozoStatuePlms ChozoStatue { get; }

    /// <summary>Applies PLM-driven room scroll grid changes.</summary>
    internal ScrollPlms Scrolls { get; }

    /// <summary>Handles Draygon arena cannon room objects and their block changes.</summary>
    internal DraygonCannonPlms DraygonCannons { get; }

    /// <summary>Runs Bomb Torizo's hand and statue room object sequences.</summary>
    internal BombTorizoHandPlms BombTorizoHand { get; }

    /// <summary>Runs the glass tube's scripted break and sound sequence.</summary>
    internal NoobTubePlms NoobTube { get; }

    /// <summary>Advances Wrecked Ship treadmill animated tile objects.</summary>
    internal WreckedShipTreadmillPlms WreckedShipTreadmill { get; }

    /// <summary>Opens and closes colored gates after their matching triggers.</summary>
    internal GatePlms Gates { get; }

    /// <summary>Controls the Mother Brain chamber glass barrier and shatter sequence.</summary>
    internal MotherBrainGlassPlms MotherBrainGlass { get; }

    /// <summary>Builds a room's PLM population into native-order physical slots.</summary>
    internal PlmPopulationLoader PopulationLoader { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    internal StationPlms Stations { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    internal CollectiblePlms CollectiblePlms { get; }

    /// <summary>Runs eye door opening and projectile interaction instructions.</summary>
    internal EyeDoorPlms EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    internal GreyDoorPlms GreyDoors { get; }

    /// <summary>Creates the forty empty PLM slots and the PLM family owners that run in them.</summary>
    public RoomPlmSystem()
    {
        SlotTable = new PlmSlotTable();
        Sounds = new PlmSoundQueue();
        Drawing = new PlmBlockDrawing();
        ProjectileTriggers = new ProjectileTriggerPlms(SlotTable);
        BreakableTerrain = new EnemyBreakableTerrainPlms(SlotTable);
        Quicksand = new QuicksandPlms(SlotTable);
        Scrolls = new ScrollPlms(SlotTable);
        WreckedShipTreadmill = new WreckedShipTreadmillPlms(SlotTable);
        Bindings = new PlmRoomBindings();
        Shaktool = new ShaktoolPlms(Bindings, SlotTable);
        SamusEater = new SamusEaterPlms(Bindings, SlotTable);
        EscapeAnimals = new EscapeAnimalPlms(Bindings, SlotTable);
        SpeedBoosterEscape = new SpeedBoosterEscapePlms(Bindings);
        ChozoStatue = new ChozoStatuePlms(Bindings, SlotTable);
        DraygonCannons = new DraygonCannonPlms(Bindings);
        BombTorizoHand = new BombTorizoHandPlms(Bindings, SlotTable);
        NoobTube = new NoobTubePlms(Bindings, Sounds);
        Gates = new GatePlms(Bindings, SlotTable, Sounds);
        MotherBrainGlass = new MotherBrainGlassPlms(Bindings, SlotTable, Sounds);
        Stations = new StationPlms(Bindings, Drawing, SlotTable, Sounds);
        CollectiblePlms = new CollectiblePlms(Bindings, Drawing, SlotTable, Sounds);
        EyeDoors = new EyeDoorPlms(Bindings, Sounds);
        DoorPlms = new DoorPlms(Bindings, Drawing, SlotTable, Sounds);
        GreyDoors = new GreyDoorPlms(Bindings, Drawing, Sounds, DoorPlms);
        Tourian = new TourianPlms(Bindings, SlotTable);
        PopulationLoader = new PlmPopulationLoader(new PlmPopulationLoaderDependencies { Bindings = Bindings, BombTorizoHand = BombTorizoHand, CollectiblePlms = CollectiblePlms, EyeDoors = EyeDoors, Gates = Gates, MotherBrainGlass = MotherBrainGlass, SlotTable = SlotTable, SpeedBoosterEscape = SpeedBoosterEscape });
        BlockReactions = new BlockReactionPlms(Bindings, EscapeAnimals, SlotTable);
        BossRoomMutations = new BossRoomMutationPlms(SlotTable);
        foreach (IPlmFamilySource source in (IPlmFamilySource[])[Scrolls, WreckedShipTreadmill, Stations, CollectiblePlms, DoorPlms, GreyDoors, Gates, EyeDoors, DraygonCannons])
            source.RegisterPlmFamilies(_families);
        _families.RequireEveryFamily();
    }

    /// <summary>Each PLM family's step, run before a family slot's shared pass.</summary>
    private readonly PlmFamilyTable _families = new();

    /// <summary>Door-object instruction owner bound to this room's allocation and draw state.</summary>
    internal DoorPlms DoorPlms { get; }

    /// <summary>
    /// Clears the room-owned allocation during <c>$82:E3C0</c>'s destination-room setup.
    /// PLMs never survive a door transition; retaining one would let its old block index
    /// mutate unrelated level data in the newly decompressed room.
    /// </summary>
    public void Reset()
    {
        ChozoStatue._chozoEnemies = null;
        ChozoStatue._chozoRoomFx = null;
        foreach (PlmSlot slot in SlotTable._slots)
            slot.Clear();
        Sounds._soundRequests.Clear();
        Gates._pendingDownwardGateSounds.Clear();
        Gates._downwardGateProjectileRequests.Clear();
        Bindings.Clear();
        Drawing._tilemapUpdates.Clear();
        Stations._stationActivationEvents.Clear();
        Stations._saveStationLockedOut = false;
        // The progression owner belongs to the room population just discarded. Holding
        // it past Reset would let an accidentally reused PLM slot persist a hit into the
        // previous runtime/system-state instance.
        MotherBrainGlass.ResetMotherBrainGlassState();
        CollectiblePlms.ResetCollectibleState();
        BombTorizoHand.ResetBombTorizoHandState();
    }

    /// <summary>
    /// Executes the PLM handler with the accepted NMI's newly pressed controller bits.
    /// Resident programs such as the n00b tube consume edges, not held input.
    /// </summary>
    public IReadOnlyList<PlmTilemapUpdate> Step(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmView view, PlmFrameInputs frame)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(streamer);
        Sounds._soundRequests.Clear();
        Gates.PublishPendingDownwardGateSounds();
        Drawing._tilemapUpdates.Clear();
        Stations._stationActivationEvents.Clear();
        MotherBrainGlass.BeginMotherBrainGlassFrame();
        CollectiblePlms.BeginCollectibleFrame();
        BombTorizoHand.BeginBombTorizoHandFrame();

        var context = new PlmStepContext(this, level, streamer, view, frame);
        for (int slotIndex = SlotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = SlotTable._slots[slotIndex];
            if (!slot.Active)
                continue;

            // A family that consumes the pass skips the pre-instruction, the shared timer and the interpreter.
            if (slot.Family is { } family && _families.Step(slot, family, context) == PlmStepOutcome.Consumed)
                continue;

            // Native JSR ($1CD7,x): the slot's one pre-instruction. A routine it installs runs from the next pass.
            slot.PreInstruction.Run(slot, context);
            if (!slot.Active)
                continue;

            slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
            if (!slot.InstructionTimer.IsZero)
                continue;

            (slot.Animation ?? throw new InvalidOperationException(
                $"PLM slot {slotIndex} ({slot.HeaderPointer}) runs neither a typed program nor a family step.")).Advance(slot, context);
        }

        return Drawing._tilemapUpdates;
    }

    /// <summary>
    /// <c>$84:86BC Instruction_PLM_Delete</c>: releases the slot. Native lists and typed programs end a PLM the same way.
    /// </summary>
    internal void DeletePlm(PlmSlot slot)
    {
        slot.Active = false;
        BombTorizoHandPlms.MarkBombTorizoHandDeleted(slot);
        MotherBrainGlass.OnPlmDeleted(slot);
        // Semantic family references are discriminators, not retained debug
        // history. A later allocation can reuse this physical native slot and
        // must never inherit door-only pre-handler behavior.
        slot.Family = null;
        slot.Family = null;
        slot.Family = null;
        slot.Family = null;
    }

    /// <summary>
    /// Shared Write_Level_Data_Block_Type_and_BTS operation: preserve visual index/flips,
    /// replace the collision nibble, and publish the independent low-byte behavior.
    /// </summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, ushort typeAndBts) =>
        WritePlmCollisionTypeAndBts(level, blockIndex, new RoomLevelWord(typeAndBts).CollisionType, unchecked((byte)typeAndBts));

    /// <summary>Replaces a block's collision type and BTS, keeping its visual payload.</summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, RoomCollisionType collisionType, int bts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, collisionType).Raw);
        level.SetPlmBehavior(blockIndex, new RoomBlockBehavior(bts));
    }
}
