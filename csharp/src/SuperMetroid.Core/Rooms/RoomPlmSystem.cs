using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

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
    /// <summary>The <see cref="BlockReactionPlms"/> owner of this RoomPlmSystem.</summary>
    internal BlockReactionPlms BlockReactions { get; }

    /// <summary>The <see cref="BossRoomMutationPlms"/> owner of this RoomPlmSystem.</summary>
    internal BossRoomMutationPlms BossRoomMutations { get; }

    /// <summary>The <see cref="PlmSlotTable"/> owner of this RoomPlmSystem.</summary>
    internal PlmRoomBindings Bindings { get; }

    /// <summary>Gets slot table.</summary>
    internal PlmSlotTable SlotTable { get; }

    /// <summary>The <see cref="PlmSoundQueue"/> owner of this RoomPlmSystem.</summary>
    internal PlmSoundQueue Sounds { get; }

    /// <summary>The <see cref="PlmBlockDrawing"/> owner of this RoomPlmSystem.</summary>
    internal PlmBlockDrawing Drawing { get; }

    /// <summary>The <see cref="ProjectileTriggerPlms"/> owner of this RoomPlmSystem.</summary>
    internal ProjectileTriggerPlms ProjectileTriggers { get; }

    /// <summary>The <see cref="EnemyBreakableTerrainPlms"/> owner of this RoomPlmSystem.</summary>
    internal EnemyBreakableTerrainPlms BreakableTerrain { get; }

    /// <summary>The <see cref="QuicksandPlms"/> owner of this RoomPlmSystem.</summary>
    internal QuicksandPlms Quicksand { get; }

    /// <summary>The <see cref="ShaktoolPlms"/> owner of this RoomPlmSystem.</summary>
    internal ShaktoolPlms Shaktool { get; }

    /// <summary>The <see cref="SamusEaterPlms"/> owner of this RoomPlmSystem.</summary>
    internal SamusEaterPlms SamusEater { get; }

    /// <summary>The <see cref="TourianPlms"/> owner of this RoomPlmSystem.</summary>
    internal TourianPlms Tourian { get; }

    /// <summary>The <see cref="EscapeAnimalPlms"/> owner of this RoomPlmSystem.</summary>
    internal EscapeAnimalPlms EscapeAnimals { get; }

    /// <summary>The <see cref="SpeedBoosterEscapePlms"/> owner of this RoomPlmSystem.</summary>
    internal SpeedBoosterEscapePlms SpeedBoosterEscape { get; }

    /// <summary>The <see cref="ChozoStatuePlms"/> owner of this RoomPlmSystem.</summary>
    internal ChozoStatuePlms ChozoStatue { get; }

    /// <summary>The <see cref="ScrollPlms"/> owner of this RoomPlmSystem.</summary>
    internal ScrollPlms Scrolls { get; }

    /// <summary>The <see cref="DraygonCannonPlms"/> owner of this RoomPlmSystem.</summary>
    internal DraygonCannonPlms DraygonCannons { get; }

    /// <summary>The <see cref="BombTorizoHandPlms"/> owner of this RoomPlmSystem.</summary>
    internal BombTorizoHandPlms BombTorizoHand { get; }

    /// <summary>The <see cref="NoobTubePlms"/> owner of this RoomPlmSystem.</summary>
    internal NoobTubePlms NoobTube { get; }

    /// <summary>The <see cref="WreckedShipTreadmillPlms"/> owner of this RoomPlmSystem.</summary>
    internal WreckedShipTreadmillPlms WreckedShipTreadmill { get; }

    /// <summary>The <see cref="GatePlms"/> owner of this RoomPlmSystem.</summary>
    internal GatePlms Gates { get; }

    /// <summary>The <see cref="MotherBrainGlassPlms"/> owner of this RoomPlmSystem.</summary>
    internal MotherBrainGlassPlms MotherBrainGlass { get; }

    /// <summary>The <see cref="PlmPopulationLoader"/> owner of this RoomPlmSystem.</summary>
    internal PlmPopulationLoader PopulationLoader { get; }

    /// <summary>The <see cref="StationPlms"/> owner of this RoomPlmSystem.</summary>
    internal StationPlms Stations { get; }

    /// <summary>The <see cref="CollectiblePlms"/> owner of this RoomPlmSystem.</summary>
    internal CollectiblePlms CollectiblePlms { get; }

    /// <summary>The <see cref="EyeDoorPlms"/> owner of this RoomPlmSystem.</summary>
    internal EyeDoorPlms EyeDoors { get; }

    /// <summary>The <see cref="GreyDoorPlms"/> owner of this RoomPlmSystem.</summary>
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
        GreyDoors = new GreyDoorPlms(Bindings, Drawing, Sounds);
        DoorPlms = new DoorPlms(Bindings, Drawing, SlotTable, Sounds);
        Tourian = new TourianPlms(Bindings, SlotTable);
        PopulationLoader = new PlmPopulationLoader(new PlmPopulationLoaderDependencies { Bindings = Bindings, BombTorizoHand = BombTorizoHand, CollectiblePlms = CollectiblePlms, EyeDoors = EyeDoors, Gates = Gates, MotherBrainGlass = MotherBrainGlass, SlotTable = SlotTable, SpeedBoosterEscape = SpeedBoosterEscape });
        BlockReactions = new BlockReactionPlms(Bindings, EscapeAnimals, SlotTable);
        BossRoomMutations = new BossRoomMutationPlms(SlotTable);
    }

    /// <summary>Gets door plms.</summary>
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
            slot.InstructionTimer = 0;
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

            if (DoorPlms.TryStepColoredDoor(bus, level, streamer, slot, view))
            {
                continue;
            }

            if (GreyDoors.TryStepGreyDoor(bus, level, streamer, slot, view, frame.EnemyDeaths, frame.EnemyDeathQuota))
            {
                continue;
            }

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

            slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
            if (slot.InstructionTimer != 0)
                continue;

            ExecuteInstructionStream(bus, level, streamer, slot, view, frame.Scrolls, frame.CollectedItems);
        }

        return Drawing._tilemapUpdates;
    }

    /// <summary>Execute instruction stream.</summary>
    private void ExecuteInstructionStream(ISnesAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view, RoomScrollGrid? scrolls, ushort collectedItems)
    {
        // An instruction list may execute multiple negative instruction words before it
        // reaches a positive timer/draw pair. The guard catches corrupt ROM/test data while
        // remaining far above the longest straight-line chain in these two retail lists.
        for (int dispatchCount = 0; dispatchCount < 16; dispatchCount++)
        {
            ushort instruction = ReadProgramWord(bus, slot.InstructionPointer);
            if ((instruction & 0x8000) == 0)
            {
                ushort drawPointer = ReadProgramWord(
                    bus,
                    unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                slot.InstructionTimer = instruction;
                Drawing.DrawPlmInstruction(bus, level, streamer, slot.HeaderPointer, slot.BlockIndex, drawPointer, view);
                return;
            }

            RoomPlmInstruction code = ClosedNativeWords.Decode<RoomPlmInstruction>(
                instruction, "bank-$84 PLM instruction");
            switch (code)
            {
                case RoomPlmInstruction.SamusEaterDamage:
                    SamusEater.RequirePlantSamus().LiquidPhysics.AccumulatePeriodicDamage(0, 2);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;
                case RoomPlmInstruction.SamusEaterReleaseImmunity:
                    SamusEater.RequirePlantSamus().InvincibilityTimer = 0x30;
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;
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

                case RoomPlmInstruction.ClearDownwardGateTrigger:
                    slot.LoopTimer = 0;
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.SpawnDownwardGateProjectile:
                    Gates._downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
                        DownwardGateProjectileOperation.Spawn,
                        ReadProgramWord(bus, unchecked((ushort)(slot.InstructionPointer + 2))),
                        slot.BlockIndex));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    continue;

                case RoomPlmInstruction.WakeDownwardGateProjectile:
                    // $BBF0 consumes but does not use the following list address. It locates
                    // the actor whose variable-E stores this PLM's native block byte index.
                    Gates._downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
                        DownwardGateProjectileOperation.Wake,
                        DefinitionPointer: 0,
                        slot.BlockIndex));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                    continue;

                case RoomPlmInstruction.Goto:
                    // `$84:8724` replaces Y with the following little-endian pointer. All
                    // eight collision entry lists use it to share their dimension-specific
                    // respawning/permanent animation tail.
                    ushort gotoTarget = ReadProgramWord(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    if (DoorPlms.TryCompleteResidentDoorClosing(bus, level, streamer, slot, gotoTarget, view))
                    {
                        return;
                    }
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
                    slot.LoopTimer = unchecked((ushort)(slot.LoopTimer - 1));
                    if (slot.LoopTimer != 0)
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

                case RoomPlmInstruction.SetBotwoonScrollsBlue:
                    if (scrolls is null)
                    {
                        throw new InvalidOperationException(
                            "Botwoon's scroll PLM requires the active room scroll grid.");
                    }
                    // `$84:AB51` performs one 16-bit store of $0101 at Scrolls. Express the
                    // two raw leading cells semantically while retaining the same WRAM mirror.
                    scrolls.SetLogicalState(new(0, 0), RoomScrollState.Blue);
                    scrolls.SetLogicalState(new(1, 0), RoomScrollState.Blue);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.MoveRightFourBlocks:
                    // Eight bytes of native block index are four whole level words.
                    slot.BlockIndex = checked(slot.BlockIndex + 4);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.MoveRightOneBlock:
                    // The cartridge increments its byte offset twice; our index
                    // already addresses whole level words. Drawing stays list-owned.
                    slot.BlockIndex = checked(slot.BlockIndex + 1);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case RoomPlmInstruction.MoveBotwoonPlmDownOneBlock:
                case RoomPlmInstruction.MoveTourianAccessDown:
                    // Native PLM_BlockIndices are byte offsets, so AB59 adds room width
                    // twice. C# stores logical word indexes; adding width once is identical.
                    slot.BlockIndex = checked(slot.BlockIndex + level.Size.X);
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
                    slot.InstructionTimer = 1;
                    Drawing.DrawLevelWord(level, streamer, slot.BlockIndex, slot.RestoreLevelWord, view);
                    return;

                case RoomPlmInstruction.Delete:
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
                    return;

                case RoomPlmInstruction.Sleep:
                    // Sleep decrements Y back onto itself and exits. Colored-door hit
                    // animations use it as the linked idle target, so expose that semantic
                    // transition while retaining the self-rewinding instruction pointer.
                    slot.InstructionTimer = 1;
                    if (slot.ColoredDoor is not null)
                        slot.ColoredDoor.Phase = ColoredDoorPhase.Waiting;
                    return;

                default:
                    if (ChozoStatue.TryExecuteChozoStatueInstruction(bus, level, slot, code))
                        continue;
                    if (BombTorizoHand.TryExecuteBombTorizoHandInstruction(bus, slot, code))
                        continue;
                    if (MotherBrainGlass.TryExecuteMotherBrainGlassInstruction(bus, slot, code))
                        continue;
                    if (NoobTube.TryExecuteNoobTubeInstruction(bus, slot, code))
                        continue;
                    if (EyeDoors.TryExecuteEyeDoorInstruction(bus, level, slot, code))
                        continue;
                    if (DraygonCannons.TryExecuteDraygonCannonInstruction(bus, level, slot, code))
                        continue;
                    throw new InvalidDataException(
                        $"Movement-owned PLM reached untranslated bank-$84 instruction ${instruction:X4}.");
            }
        }

        throw new InvalidDataException("Movement-owned PLM instruction chain did not reach a timer.");
    }

    /// <summary>Reads plm work ram word.</summary>
    internal static ushort ReadPlmWorkRamWord(ISnesMutableMemory memory, ushort offset) =>
        new PlmLowBankAliasMemory(memory).Word(offset);

    /// <summary>Reads plm work ram byte.</summary>
    internal static byte ReadPlmWorkRamByte(ISnesMutableMemory memory, ushort offset) =>
        new PlmLowBankAliasMemory(memory).Byte(offset);

    // Instruction control is compiled independently of PLM draw-list payloads. A
    // compiled family claims only its own exact control addresses; all other bank-$84
    // identities must have compiled mechanics.
    // A low-window wrap may still read live WRAM; no branch can read cartridge bytes.
    /// <summary>Reads program word.</summary>
    internal static ushort ReadProgramWord(ISnesAddressSpace bus, ushort address) =>
        RoomPlmProgramDefinitions.TryReadWord(address, out ushort value)
            ? value
            : address < RoomPlmMemoryLayout.WorkRamMirrorEnd
                ? ReadPlmWorkRamWord(bus as ISnesMutableMemory ??
                    throw new InvalidOperationException("A wrapped PLM instruction needs live WRAM."), address)
                : throw new InvalidDataException(
                    $"PLM program word $84:{address:X4} has no compiled definition.");

    /// <summary>Reads program byte.</summary>
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
    internal static void WritePlmCollisionTypeAndBts(RoomLevelData level, int blockIndex, ushort typeAndBts)
    {
        var original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord;
        var replacement = new RoomLevelWord(typeAndBts);
        level.SetPlmForegroundEntry(blockIndex, RoomLevelWord.Create(original.VisualBlockIndex,
            original.VisualFlipFlags, replacement.CollisionType).Raw);
        level.SetPlmBehavior(blockIndex, unchecked((byte)typeAndBts));
    }
}
