using SuperMetroid.Core.Areas;
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
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Areas.WreckedShip;

namespace SuperMetroid.Core.World;

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
[RoomLifetime]
public sealed class RoomPlmSystem : ISamusRoomPlms, IBossRoomPlms, IAreaRoomPlms, IEnemyRoomPlms, IScriptedRoomPlmHooks
{
    /// <summary>Creates collision-triggered block PLMs from Samus and projectile reactions.</summary>
    internal BlockReactionPlms BlockReactions { get; }

    /// <summary>Applies boss encounter mutations to the active room's PLM slots.</summary>
    internal BossRoomMutationPlms BossRoomMutations { get; }

    /// <summary>The game's event flags, which scripted room objects set.</summary>
    internal IGlobalEventFlags? Events { get; }

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

    /// <summary>Builds a room's PLM population into the slot table, in spawn order.</summary>
    internal PlmPopulationLoader PopulationLoader { get; }

    /// <summary>Runs save, map, and refill station room interactions.</summary>
    internal StationPlms Stations { get; }

    /// <summary>Handles collectible item reveals, pickup, and persistence.</summary>
    internal CollectiblePlms CollectiblePlms { get; }

    /// <summary>Runs eye door opening and projectile interaction instructions.</summary>
    internal EyeDoorPlms EyeDoors { get; }

    /// <summary>Handles grey door conditions and opening animation.</summary>
    internal GreyDoorPlms GreyDoors { get; }

    /// <summary>Creates one room's forty empty PLM slots and the PLM family owners that run in them.</summary>
    /// <param name="room">The room the objects live in.</param>
    /// <param name="services">The room's service owners; each PLM family receives the ones it uses.</param>
    /// <param name="visuals">The host's installed PLM artwork.</param>
    internal RoomPlmSystem(PlmRoom room, RoomPlmServices services, PlmShotBlockVisuals visuals)
    {
        Ensure.NotNull(room);
        Ensure.NotNull(services);
        Events = services.Events;
        SlotTable = new PlmSlotTable();
        Sounds = new PlmSoundQueue();
        Drawing = new PlmBlockDrawing(visuals);
        ProjectileTriggers = new ProjectileTriggerPlms(SlotTable);
        BreakableTerrain = new EnemyBreakableTerrainPlms(SlotTable);
        Quicksand = new QuicksandPlms(SlotTable);
        Scrolls = new ScrollPlms(SlotTable);
        WreckedShipTreadmill = new WreckedShipTreadmillPlms(SlotTable);
        Shaktool = new ShaktoolPlms(services.Samus, services.Events, SlotTable);
        SamusEater = new SamusEaterPlms(services.Samus, SlotTable);
        EscapeAnimals = new EscapeAnimalPlms(services.Events, SlotTable);
        SpeedBoosterEscape = new SpeedBoosterEscapePlms(services.Samus, services.Events, services.RoomFx, services.Earthquake);
        ChozoStatue = new ChozoStatuePlms(room.System, room.AreaIndex, SlotTable);
        DraygonCannons = new DraygonCannonPlms(services.Enemies);
        BombTorizoHand = new BombTorizoHandPlms(services.Samus, SlotTable);
        NoobTube = new NoobTubePlms(services.Samus, services.Events, services.RoomFx, services.Earthquake, services.Enemies, Sounds);
        Gates = new GatePlms(services.Samus, room.Level, SlotTable, Sounds);
        MotherBrainGlass = new MotherBrainGlassPlms(services.Bosses, services.Events, SlotTable, Sounds);
        Stations = new StationPlms(room.System, services.Samus, Drawing, SlotTable, Sounds);
        CollectiblePlms = new CollectiblePlms(room.System, services.Samus, services.RoomFx, Drawing, SlotTable, Sounds);
        EyeDoors = new EyeDoorPlms(room.System, services.Samus, services.Enemies, Sounds);
        DoorPlms = new DoorPlms(room.System, SlotTable, Sounds);
        GreyDoors = new GreyDoorPlms(room.System, room.AreaIndex, services.TourianStatue, Sounds, DoorPlms);
        Tourian = new TourianPlms(services.Samus, services.Events, services.Enemies, SlotTable);
        PopulationLoader = new PlmPopulationLoader(new PlmPopulationLoaderDependencies { BombTorizoHand = BombTorizoHand, CollectiblePlms = CollectiblePlms, EyeDoors = EyeDoors, Gates = Gates, MotherBrainGlass = MotherBrainGlass, SlotTable = SlotTable, SpeedBoosterEscape = SpeedBoosterEscape });
        BlockReactions = new BlockReactionPlms(room.AreaIndex, EscapeAnimals, SlotTable);
        BossRoomMutations = new BossRoomMutationPlms(SlotTable);
        foreach (IPlmFamilySource source in (IPlmFamilySource[])[Scrolls, WreckedShipTreadmill, Stations, CollectiblePlms, DoorPlms, GreyDoors, Gates, EyeDoors, DraygonCannons])
            source.AddPlmFamilies(_families);
        _families.RequireEveryFamily();
    }

    /// <summary>Each PLM family's step, run before a family slot's shared pass.</summary>
    private readonly PlmFamilyTable _families = new();

    /// <summary>Door-object instruction owner bound to this room's allocation and draw state.</summary>
    internal DoorPlms DoorPlms { get; }


    /// <summary>
    /// Executes the PLM handler with the frame boundary's newly pressed controller bits.
    /// Resident programs such as the n00b tube consume edges, not held input.
    /// </summary>
    public void Step(RoomLevelData level, PlmFrameInputs frame)
    {
        Ensure.NotNull(level);
        Sounds.Clear();
        Gates.PublishPendingDownwardGateSounds();
        Stations.BeginFrame();
        MotherBrainGlass.BeginMotherBrainGlassFrame();
        CollectiblePlms.BeginCollectibleFrame();
        BombTorizoHand.BeginBombTorizoHandFrame();

        var context = new PlmStepContext(this, level, frame);
        foreach (PlmSlot slot in SlotTable.InProcessingOrder)
        {
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
                $"PLM {slot.Header} runs neither a typed program nor a family step.")).Advance(slot, context);
        }

    }

    /// <summary>
    /// <c>$84:86BC Instruction_PLM_Delete</c>: releases the slot. Native lists and typed programs end a PLM the same way.
    /// </summary>
    internal void DeletePlm(PlmSlot slot)
    {
        slot.Delete();
        BombTorizoHandPlms.MarkBombTorizoHandDeleted(slot);
        MotherBrainGlass.OnPlmDeleted(slot);
        // Semantic family references are discriminators, not retained debug
        // history. A later spawn can reuse this slot and must never inherit
        // door-only pre-handler behavior.
        slot.Family = null;
    }

    /// <inheritdoc/>
    BlockReactionPlms IRoomPlms.BlockReactions => BlockReactions;

    /// <inheritdoc/>
    IGlobalEventFlags? IRoomPlms.Events => Events;

    /// <inheritdoc/>
    PlmSoundQueue IRoomPlms.Sounds => Sounds;

    /// <inheritdoc/>
    IPlmDrawing IRoomPlms.Drawing => Drawing;

    /// <inheritdoc/>
    CollectiblePlms IRoomPlms.CollectiblePlms => CollectiblePlms;

    /// <inheritdoc/>
    StationPlms IRoomPlms.Stations => Stations;

    /// <inheritdoc/>
    DoorPlms IRoomPlms.DoorPlms => DoorPlms;

    /// <inheritdoc/>
    EyeDoorPlms IRoomPlms.EyeDoors => EyeDoors;

    /// <inheritdoc/>
    GreyDoorPlms IRoomPlms.GreyDoors => GreyDoors;

    /// <inheritdoc/>
    GatePlms IRoomPlms.Gates => Gates;

    /// <inheritdoc/>
    NoobTubePlms IRoomPlms.NoobTube => NoobTube;

    /// <inheritdoc/>
    SamusEaterPlms IRoomPlms.SamusEater => SamusEater;

    /// <inheritdoc/>
    ScrollPlms IRoomPlms.Scrolls => Scrolls;

    /// <inheritdoc/>
    SpeedBoosterEscapePlms IRoomPlms.SpeedBoosterEscape => SpeedBoosterEscape;

    /// <inheritdoc/>
    IScriptedRoomPlmHooks IRoomPlms.ScriptedHooks => this;

    /// <inheritdoc/>
    void IRoomPlms.DeletePlm(PlmSlot slot) => DeletePlm(slot);

    /// <inheritdoc/>
    ProjectileTriggerPlms ISamusRoomPlms.ProjectileTriggers => ProjectileTriggers;

    /// <inheritdoc/>
    QuicksandPlms ISamusRoomPlms.Quicksand => Quicksand;

    /// <inheritdoc/>
    IChozoStatueHand ISamusRoomPlms.ChozoStatueHand => ChozoStatue;

    /// <inheritdoc/>
    BombTorizoHandPlms IBossRoomPlms.BombTorizoHand => BombTorizoHand;

    /// <inheritdoc/>
    DraygonCannonPlms IBossRoomPlms.DraygonCannons => DraygonCannons;

    /// <inheritdoc/>
    MotherBrainGlassPlms IBossRoomPlms.MotherBrainGlass => MotherBrainGlass;

    /// <inheritdoc/>
    TourianPlms IAreaRoomPlms.Tourian => Tourian;

    /// <inheritdoc/>
    ShaktoolPlms IEnemyRoomPlms.Shaktool => Shaktool;

    /// <inheritdoc/>
    IPlmPreInstruction IScriptedRoomPlmHooks.PreInstruction(ScriptedRoomPlmPreInstruction preInstruction) => preInstruction switch
    {
        ScriptedRoomPlmPreInstruction.ShaktoolRoomController => ShaktoolRoomPreInstructions.Controller,
        ScriptedRoomPlmPreInstruction.OldTourianWallWaitForSamus => TourianPreInstructions.OldTourianEscapeShaftWallWaitForSamus,
        ScriptedRoomPlmPreInstruction.LowerNorfairChozoHandWait => ChozoStatuePreInstructions.WaitForLowerNorfairHand,
        _ => throw new InvalidOperationException($"Undefined {nameof(ScriptedRoomPlmPreInstruction)} {preInstruction}."),
    };

    /// <inheritdoc/>
    bool IScriptedRoomPlmHooks.HasLoweredLowerNorfairAcid => ChozoStatue.HasLoweredLowerNorfairAcid;

    /// <inheritdoc/>
    void IScriptedRoomPlmHooks.SetLoweredAcidHeight() => ChozoStatue.SetLoweredAcidHeight();

    /// <inheritdoc/>
    void IScriptedRoomPlmHooks.TransformSpikesToSlopes(RoomLevelData level) => ChozoStatuePlms.TransformSpikesToSlopes(level);

    /// <inheritdoc/>
    void IScriptedRoomPlmHooks.RevertSlopesToSpikes(RoomLevelData level) => ChozoStatuePlms.RevertSlopesToSpikes(level);
}
