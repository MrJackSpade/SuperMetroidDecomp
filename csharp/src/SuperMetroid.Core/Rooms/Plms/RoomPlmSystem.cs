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
    }

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
        {
            slot.Active = false;
            slot.PlantHeldPosition = Position16<RoomPixels>.Zero;
            slot.HeaderPointer = 0;
            slot.BlockIndex = 0;
            slot.RestoreLevelWord = 0;
            slot.InstructionPointer = 0;
            slot.InstructionTimer = NativeCountdown.Start(0);
            slot.PreInstruction = 0;
            slot.RoomArgument = 0;
            slot.LoopTimer = 0;
            slot.LinkInstruction = 0;
            slot.Item = null;
            slot.Scroll = null;
            slot.ColoredDoor = null;
            slot.GreyDoor = null;
            slot.Station = null;
            slot.IsElevatorPlatform = false;
            slot.Treadmill = null;
            slot.Gate = null;
            slot.EyeDoor = null;
            slot.DraygonCannon = null;
        }
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
    public IReadOnlyList<PlmTilemapUpdate> Step(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmView view, PlmFrameInputs frame)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(level);
        Ensure.NotNull(streamer);
        Sounds._soundRequests.Clear();
        Gates.PublishPendingDownwardGateSounds();
        Drawing._tilemapUpdates.Clear();
        Stations._stationActivationEvents.Clear();
        MotherBrainGlass.BeginMotherBrainGlassFrame();
        CollectiblePlms.BeginCollectibleFrame();
        BombTorizoHand.BeginBombTorizoHandFrame();

        for (int slotIndex = SlotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = SlotTable._slots[slotIndex];
            if (!slot.Active)
                continue;

            if (ScrollPlms.TryStepScrollPlm(level, frame.Scrolls, slot))
                continue;

            if (WreckedShipTreadmillPlms.TryStepWreckedShipTreadmill(level, streamer, slot))
                continue;

            if (Stations.TryStepStation(bus, level, streamer, slot, view))
            {
                continue;
            }

            if (CollectiblePlms.TryStepCollectible(bus, level, streamer, slot, view))
            {
                continue;
            }

            DoorPlms.RunColoredDoorPreInstruction(slot);
            GreyDoors.RunGreyDoorPreInstruction(slot, frame.EnemyDeaths, frame.EnemyDeathQuota);
            Tourian.RunMetroidsClearedPreInstruction(slot, frame.EnemyDeaths, frame.EnemyDeathQuota);
            Shaktool.RunShaktoolRoomPreInstruction(slot, frame.Scrolls, frame.PowerBombExplosionStatus);
            SamusEater.RunSamusEaterPreInstruction(slot);
            SpeedBoosterEscape.RunSpeedBoosterEscapePreInstruction(slot);
            Tourian.RunOldTourianEscapeShaftWallPreInstruction(slot);
            WreckedShipAtticPlms.RunWreckedShipAtticPreInstruction(slot);
            BombTorizoHand.RunBombTorizoHandPreInstruction(slot);
            ChozoStatuePlms.RunChozoStatuePreInstruction(level, slot);
            MotherBrainGlassPlms.RunMotherBrainGlassPreInstruction(slot);
            NoobTube.RunNoobTubePreInstruction(slot, frame.ControllerNewInput);
            Gates.RunDownwardGatePreInstruction(slot);
            EyeDoors.RunEyeDoorPreInstruction(slot);
            DraygonCannonPlms.RunDraygonCannonPreInstruction(slot);
            if (!slot.Active)
                continue;

            slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
            if (!slot.InstructionTimer.IsZero)
                continue;

            if (slot.Animation is { } animation)
                animation.Advance(slot, new PlmAnimationContext(this, bus, level, streamer, view, frame));
            else
                ExecuteInstructionStream(bus, level, streamer, slot, view, frame.CollectedItems);
        }

        return Drawing._tilemapUpdates;
    }

    /// <summary>Dispatches bank-$84 controls until a timed draw pair is installed; rejects uncompiled or runaway chains.</summary>
    private void ExecuteInstructionStream(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view, ushort collectedItems)
    {
        // An instruction list may execute multiple negative instruction words before it
        // reaches a positive timer/draw pair. The guard catches corrupt ROM/test data while
        // remaining far above the longest straight-line chain in these two retail lists.
        for (int dispatchCount = 0; dispatchCount < 16; dispatchCount++)
        {
            ushort instruction = ReadProgramWord(bus, slot.InstructionPointer);
            if (!PlmListWordDefinitions.IsInstruction(instruction))
            {
                ushort drawPointer = ReadProgramWord(
                    bus,
                    unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                slot.InstructionTimer = NativeCountdown.FromRaw(instruction);
                Drawing.DrawPlmInstruction(bus, level, streamer, slot.HeaderPointer, slot.BlockIndex, drawPointer, view);
                return;
            }

            RoomPlmInstruction code = ClosedNativeWords.Decode<RoomPlmInstruction>(
                instruction, "bank-$84 PLM instruction");
            switch (code)
            {
                case RoomPlmInstruction.SetAnimalsEscapedEvent:
                    (Bindings.Events ?? throw new InvalidOperationException("Animal rescue requires the room event owner.")).Set(EventNumber.CrittersEscaped);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;
                case RoomPlmInstruction.InstallPreInstruction:
                    // `$84:86C1` is shared infrastructure, not a Mother Brain special.
                    // The operand is the bank-$84 pre-instruction run before this slot's
                    // timer on subsequent handler passes. Both glass and Bomb Torizo's
                    // sleeping hand use this exact four-byte form.
                    slot.PreInstruction = ReadProgramWord(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    continue;

                case RoomPlmInstruction.ClearPreInstruction:
                    // $84:86CA installs the shared RTS callback. Zero is the C# semantic
                    // representation of that inert routine for non-family-specific slots.
                    slot.PreInstruction = 0;
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.QueueSoundLibrary2Maximum1:
                    // `$84:8C79` is the shot-block queue form. It has the same odd-byte
                    // operand layout as `$8C10/$8C46`, but permits only one pending sound.
                    byte singleSoundId = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    Sounds._soundRequests.Add(Sounds.CreateSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, singleSoundId), MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
                    // `$84:8C7C` is the direct LDA/JSL form used by the power-bomb-gated
                    // shot-block lists; `$8C79` enters the same max-one queue routine through
                    // a short branch. Both consume the identical odd-byte sound operand.
                    byte directSingleSoundId = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    Sounds._soundRequests.Add(Sounds.CreateSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, directSingleSoundId), MaximumQueued: 1));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
                    // $84:8C10 consumes one byte after its pointer. The following timer's
                    // low byte is read as A's harmless high byte by the 16-bit LDA.
                    byte soundId = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    Sounds._soundRequests.Add(Sounds.CreateSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, soundId), MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
                    // `$84:8C46` has the same odd-byte operand layout as `$8C10`, but the
                    // collision-bomb list's crumble sound `$06` uses the stricter queue cap.
                    byte cappedSoundId = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    Sounds._soundRequests.Add(Sounds.CreateSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, cappedSoundId), MaximumQueued: 3));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
                    // Door lists use `$84:8C19` for open/close sounds. Like the adjacent
                    // library-two opcodes, the 16-bit native load intentionally consumes
                    // only one argument byte before advancing Y by one.
                    byte doorSoundId = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    Sounds._soundRequests.Add(Sounds.CreateSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, doorSoundId), MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.Goto:
                    // `$84:8724` replaces Y with the following little-endian pointer. All
                    // eight collision entry lists use it to share their dimension-specific
                    // respawning/permanent animation tail.
                    ushort gotoTarget = ReadProgramWord(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    slot.InstructionPointer = gotoTarget;
                    continue;

                case RoomPlmInstruction.SetEightBitTimer:
                    // `$84:874E` consumes an odd one-byte operand into PLM_Timers, not the
                    // instruction countdown. Botwoon's list seeds nine vertical rows here.
                    slot.LoopTimer = ReadProgramByte(
                        bus, unchecked((ushort)(slot.InstructionPointer + 2)));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case RoomPlmInstruction.DecrementTimerAndGoto:
                    // `$84:873F` always decrements the independent PLM_Timers word. A
                    // nonzero result jumps through the following pointer; zero consumes it.
                    NativeWordCounterStep counted = NativeWordCounter.Decrement(slot.LoopTimer);
                    slot.LoopTimer = counted.Value;
                    if (!counted.IsZero)
                    {
                        slot.InstructionPointer = ReadProgramWord(
                            bus,
                            unchecked((ushort)(slot.InstructionPointer + 2)));
                    }
                    else
                    {
                        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    }
                    continue;


                case RoomPlmInstruction.MoveRightFourBlocks:
                    // Eight bytes of native block index are four whole level words.
                    slot.BlockIndex = checked(slot.BlockIndex + 4);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;


                case RoomPlmInstruction.GotoIfSamusHasNoBombs:
                    // Native receives Y on the operand after the JSR address. Owning Bombs
                    // skips that two-byte target; lacking Bombs replaces Y with the target.
                    slot.InstructionPointer = collectedItems.HasAny(SamusEquipmentFlags.Bombs)
                        ? unchecked((ushort)(slot.InstructionPointer + 4))
                        : ReadProgramWord(
                            bus,
                            unchecked((ushort)(slot.InstructionPointer + 2)));
                    continue;

                case RoomPlmInstruction.SetPlmBtsToOne:
                    level.SetBehavior(slot.BlockIndex, 1);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.SetPlmBtsFromByte:
                    // `$84:8AF1` consumes the byte immediately following the opcode. Door
                    // closing lists use it after their final visual frame to restore the
                    // shootable blue-door BTS before drawing the permanent closed cap.
                    level.SetBehavior(
                        slot.BlockIndex,
                        ReadProgramByte(bus, unchecked((ushort)(slot.InstructionPointer + 2))));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                // Both cartridge entries join the same draw-and-yield tail. Neither
                // consumes an operand, so the next instruction remains pointer + 2.
                case RoomPlmInstruction.DrawPlmBlockClone:
                case RoomPlmInstruction.DrawPlmBlock:
                    // $84:8B17 restores PLM_Vars to level data, builds a one-block custom
                    // draw list, sets timer one, and exits the handler. Deletion therefore
                    // occurs on the next PLM pass rather than this restoration pass.
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    slot.InstructionTimer = NativeCountdown.Start(1);
                    Drawing.DrawLevelWord(level, streamer, slot.BlockIndex, slot.RestoreLevelWord, view);
                    return;

                case RoomPlmInstruction.Delete:
                    DeletePlm(slot);
                    return;

                case RoomPlmInstruction.Sleep:
                    // Sleep decrements Y back onto itself and exits. Colored-door hit
                    // animations use it as the linked idle target, so expose that semantic
                    // transition while retaining the self-rewinding instruction pointer.
                    slot.InstructionTimer = NativeCountdown.Start(1);
                    return;

                default:
                    if (ChozoStatue.TryExecuteChozoStatueInstruction(bus, level, slot, code))
                        continue;
                    throw new InvalidDataException(
                        $"Movement-owned PLM reached untranslated bank-$84 instruction ${instruction:X4}.");
            }
        }

        throw new InvalidDataException("Movement-owned PLM instruction chain did not reach a timer.");
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
        slot.ColoredDoor = null;
        slot.GreyDoor = null;
        slot.EyeDoor = null;
        slot.DraygonCannon = null;
    }

    /// <summary>Reads a wrapped PLM instruction word through the low-bank live WRAM alias.</summary>
    internal static ushort ReadPlmWorkRamWord(ISnesMutableMemory memory, ushort offset) =>
        new PlmLowBankAliasMemory(memory).Word(offset);

    /// <summary>Reads a wrapped PLM instruction byte through the low-bank live WRAM alias.</summary>
    internal static byte ReadPlmWorkRamByte(ISnesMutableMemory memory, ushort offset) =>
        new PlmLowBankAliasMemory(memory).Byte(offset);

    /// <summary>Reads only compiled bank-$84 control words, or live WRAM after a low-window wrap; draw-list payloads have separate ownership.</summary>
    internal static ushort ReadProgramWord(ISnesAddressSpace bus, ushort address) =>
        RoomPlmProgramDefinitions.TryReadWord(address, out ushort value)
            ? value
            : address < RoomPlmMemoryLayout.WorkRamMirrorEnd
                ? ReadPlmWorkRamWord(bus as ISnesMutableMemory ??
                    throw new InvalidOperationException("A wrapped PLM instruction needs live WRAM."), address)
                : throw new InvalidDataException(
                    $"PLM program word $84:{address:X4} has no compiled definition.");

    /// <summary>Reads a compiled bank-$84 control byte or live wrapped WRAM without falling back to cartridge data.</summary>
    internal static byte ReadProgramByte(ISnesAddressSpace bus, ushort address) =>
        RoomPlmProgramDefinitions.TryReadByte(address, out byte value)
            ? value
            : address < RoomPlmMemoryLayout.WorkRamMirrorEnd
                ? ReadPlmWorkRamByte(bus as ISnesMutableMemory ??
                    throw new InvalidOperationException("A wrapped PLM instruction needs live WRAM."), address)
                : throw new InvalidDataException(
                    $"PLM program byte $84:{address:X4} has no compiled definition.");

    /// <summary>
    /// Shared Write_Level_Data_Block_Type_and_BTS operation: preserve visual index/flips,
    /// replace the collision nibble, and publish the independent low-byte behavior.
    /// </summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, ushort typeAndBts) =>
        WritePlmCollisionTypeAndBts(level, blockIndex, new RoomLevelWord(typeAndBts).CollisionType, unchecked((byte)typeAndBts));

    /// <summary>Replaces a block's collision type and BTS, keeping its visual payload.</summary>
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, RoomCollisionType collisionType, byte bts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, collisionType).Raw);
        level.SetPlmBehavior(blockIndex, bts);
    }
}
