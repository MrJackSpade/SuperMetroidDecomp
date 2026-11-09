using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectNativeListAddresses;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// #1296: the room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native list
/// step by step: draws and durations, deletion, sleeps, and the cue of every other instruction with its operands. The
/// families' runtime suites check what those cues do.
/// </summary>
internal static class RoomObjectProgramsVerification
{
    /// <summary>
    /// The glass tube's native lists do what <see cref="NoobTubeObject"/> does (#1554): a tube broken earlier restores the
    /// water and ends; otherwise it links its input wait, arms its Power Bomb wait, shows the intact tube and sleeps; the
    /// input wait links the break, arms its input wait and sleeps; the break stops waiting, locks Samus, cracks, shows each
    /// cracking image, shatters with its sound, debris and earthquake, shows the open tube, sets the broken event,
    /// restores the water, releases Samus and ends. Every image and operand is compared with
    /// <see cref="NoobTubeObjectDefinitions"/>.
    /// </summary>
    internal static void VerifyNoobTubePrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, TerrainChangeFrame frame, string at)
        {
            AssertEqual(frame.Updates, (int)Word(cursor), $"{at} updates");
            AssertEqual((int)SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(frame.Draw), (int)Word(cursor + 2), $"{at} image");
        }

        int main = NoobTubeList;
        AssertEqual(RoomObjectInstruction.GotoIfEventSet, At(main), "tube first checks whether it is broken");
        AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), (int)Word(main + 2), "tube broken event");
        int alreadyBroken = Word(main + 4);
        AssertEqual(RoomObjectInstruction.LinkInstruction, At(main + 6), "tube links its input wait");
        int waitForInput = Word(main + 8);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(main + 10), "tube arms its Power Bomb wait");
        AssertEqual(NoobTubePhase.WaitingForPowerBomb, RoomObjectFrameBehaviorNativeWords.NoobTube.Decode(Word(main + 12), "tube wait"), "tube Power Bomb wait");
        AssertShown(main + 14, new TerrainChangeFrame(RoomObjectDraw.Of(NoobTubeObjectDefinitions.IntactImage), 1), "intact tube");
        AssertEqual(RoomObjectInstruction.Sleep, At(main + 18), "intact tube waits");

        AssertEqual(RoomObjectInstruction.EnableNoobTubeWaterPhysics, At(alreadyBroken), "broken tube restores the water");
        AssertEqual(RoomObjectInstruction.Delete, At(alreadyBroken + 2), "broken tube ends");

        AssertEqual(RoomObjectInstruction.LinkInstruction, At(waitForInput), "input wait links the break");
        int cursor = Word(waitForInput + 2);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(waitForInput + 4), "tube arms its input wait");
        AssertEqual(NoobTubePhase.WaitingForInput, RoomObjectFrameBehaviorNativeWords.NoobTube.Decode(Word(waitForInput + 6), "tube wait"), "tube input wait");
        AssertEqual(RoomObjectInstruction.Sleep, At(waitForInput + 8), "tube waits for input");

        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, At(cursor), "break stops waiting");
        AssertEqual(RoomObjectInstruction.LockSamus, At(cursor + 2), "break locks Samus");
        AssertEqual(RoomObjectInstruction.SpawnNoobTubeCrack, At(cursor + 4), "break cracks the glass");
        cursor += 6;
        for (int index = 0; index < NoobTubeObjectDefinitions.Cracking.Count; index++, cursor += 4)
            AssertShown(cursor, NoobTubeObjectDefinitions.Cracking[index], $"cracking image {index}");
        AssertEqual(RoomObjectInstruction.QueueSoundLibrary2Maximum6, At(cursor), "the tube shatters with its sound");
        AssertEqual(NoobTubeObjectDefinitions.BreakSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), "tube break sound");
        AssertEqual(SoundRequestPriority.Normal, NoobTubeObjectDefinitions.BreakSoundPriority, "tube break sound priority");
        AssertEqual(RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles, At(cursor + 3), "the tube throws its shards and bubbles");
        AssertEqual(RoomObjectInstruction.TriggerNoobTubeEarthquake, At(cursor + 5), "the tube shakes the room");
        AssertShown(cursor + 7, NoobTubeObjectDefinitions.Shattered, "open tube");
        AssertEqual(RoomObjectInstruction.SetEvent, At(cursor + 11), "the open tube sets its event");
        AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), (int)Word(cursor + 13), "tube broken event set");
        AssertEqual(RoomObjectInstruction.EnableNoobTubeWaterPhysics, At(cursor + 15), "the open tube restores the water");
        AssertEqual(RoomObjectInstruction.UnlockSamus, At(cursor + 17), "the open tube releases Samus");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 19), "the open tube ends");
        Console.WriteLine("  N00b tube lists match its typed course in bank $84.");
    }

    /// <summary>The shared delete and sleep programs equal their one-instruction native lists.</summary>
    internal static void VerifySharedPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), RoomObjectSharedProgramDefinitions.Programs, SharedEntries,
            draw => throw new InvalidOperationException($"The shared programs never draw; got {draw}."), NoCues<RoomObjectNoCue>(), "shared PLM");
        Console.WriteLine($"  Shared PLM programs: {steps} typed steps match bank $84.");
    }

    /// <summary>
    /// The lavaquake's native list does what <see cref="SpeedBoosterEscapeObject"/> does (#1554): three waits in order,
    /// each installed and slept on until it wakes the next, the last never waking.
    /// </summary>
    internal static void VerifySpeedBoosterEscapePrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        SpeedBoosterEscapePhase[] waits =
            [SpeedBoosterEscapePhase.WaitingForSpeedBooster, SpeedBoosterEscapePhase.WaitingForSamusLeft, SpeedBoosterEscapePhase.AdvancingLava];
        for (int index = 0; index < waits.Length; index++)
        {
            int cursor = SpeedBoosterEscapeList + index * 6;
            AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"lavaquake wait {index} is installed");
            AssertEqual(waits[index], RoomObjectFrameBehaviorNativeWords.SpeedBoosterEscape.Decode(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)),
                "lavaquake wait"), $"lavaquake wait {index} is the typed object's");
            AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, cursor + 4), $"lavaquake wait {index} sleeps until it wakes the next");
        }
        Console.WriteLine($"  Speed Booster lavaquake list: {waits.Length} waits match bank $84.");
    }

    /// <summary>The block-reaction programs equal their native lists; the restore instruction draws the saved block.</summary>
    internal static void VerifyBlockReactionPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BlockReactionObjectProgramDefinitions.Programs, BlockReactionEntries,
            frame => BlockReactionObjectDrawDefinitions.DrawLists[frame], BlockReactionCueAt, "block reaction", BlockReactionRestoreAt);
        Console.WriteLine($"  Block-reaction programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The scripted room programs equal their native lists.</summary>
    internal static void VerifyScriptedRoomPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), ScriptedRoomObjectProgramDefinitions.Programs, ScriptedRoomEntries,
            frame => ScriptedRoomObjectDrawDefinitions.DrawLists[frame], NoCues<RoomObjectNoCue>(), "scripted room");
        Console.WriteLine($"  Scripted room programs: {steps} typed steps match bank $84.");
        VerifyScriptedTerrainChanges(rom);
    }

    /// <summary>
    /// The room objects that left the scripted program set play as their native lists do (#1554); the critters' wall
    /// marks the critters escaped and the elevatube plays its sound as each ends.
    /// </summary>
    private static void VerifyScriptedTerrainChanges(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var changes = new Dictionary<string, TerrainChange>
        {
            [nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube)] = ScriptedTerrainChangeDefinitions.MaridiaElevatube,
            [nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough)] = ScriptedTerrainChangeDefinitions.CritterWallBreakthrough,
            [nameof(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage)] = ScriptedTerrainChangeDefinitions.CrateriaEscapePassage,
            [nameof(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak)] = ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak,
        };
        int frames = 0;
        foreach ((string name, ushort entry) in ScriptedTerrainChangeEntries)
        {
            frames += NativeTerrainChanges.Compare(cartridge, changes[name], entry, ScriptedTerrainStepAt, name, out bool began,
                out IReadOnlyList<NativeTerrainChanges.NativeStep> atEnd);
            AssertTrue(!began, $"{name} opens nothing as it starts");
            string ending = string.Join(", ", atEnd.Select(step => step.Effect ?? $"{step.Sound} at {step.Priority}"));
            string expected = name == nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough) ? "critters escaped"
                : name == nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube) ? $"{(SoundEffectId?)EnemySound.Elevatube} at {SoundRequestPriority.Normal}"
                : "";
            AssertEqual(expected, ending, $"{name} acts as it ends exactly as its typed object does");
        }
        Console.WriteLine($"  Scripted terrain changes: {frames} typed frames match bank $84.");
    }

    /// <summary>The enemies' crumble and elevatube sounds at their priorities, the critters' escape and the four-block move right.</summary>
    private static bool ScriptedTerrainStepAt(IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step)
    {
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        if (instruction is RoomObjectInstruction.QueueSoundLibrary2Maximum3 or RoomObjectInstruction.QueueSoundLibrary2Maximum6)
        {
            step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero,
                SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), Begins: false, Length: 3,
                priority: instruction == RoomObjectInstruction.QueueSoundLibrary2Maximum3 ? SoundRequestPriority.Low : SoundRequestPriority.Normal);
            return true;
        }
        if (instruction == RoomObjectInstruction.SetAnimalsEscapedEvent)
        {
            step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero, Sound: null, Begins: false, Length: 2, Effect: "critters escaped");
            return true;
        }
        if (instruction == RoomObjectInstruction.MoveRightFourBlocks)
        {
            step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(4, 0), Sound: null, Begins: false, Length: 2);
            return true;
        }
        step = default;
        return false;
    }

    /// <summary>The block reactions' sound queues and grapple BTS write.</summary>
    private static bool BlockReactionCueAt(IImportCartridgeSource cartridge, ushort cursor, out BlockReactionObjectCue cue, out int length)
    {
        cue = default;
        length = 0;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
                cue = new(BlockReactionObjectCueKind.QueueSoundWhenIdle, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
                cue = new(BlockReactionObjectCueKind.QueueSoundAtLowPriority, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
                cue = new(BlockReactionObjectCueKind.QueueSoundAtNormalPriority, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.SetObjectBtsToOne:
                cue = new(BlockReactionObjectCueKind.RestoreRespawningGrappleBehavior, null);
                length = 2;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
            case RoomObjectInstruction.InstallFrameBehavior:
            case RoomObjectInstruction.ClearFrameBehavior:
            case RoomObjectInstruction.LinkInstruction:
            case RoomObjectInstruction.GotoIfDoorBitSet:
            case RoomObjectInstruction.IncrementDoorHitCounterAndGoto:
            case RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomObjectInstruction.GotoIfSamusNear:
            case RoomObjectInstruction.Goto:
            case RoomObjectInstruction.DecrementTimerAndGoto:
            case RoomObjectInstruction.SetEightBitTimer:
            case RoomObjectInstruction.CopyFromRamToVram:
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
            case RoomObjectInstruction.GotoIfEventSet:
            case RoomObjectInstruction.SetEvent:
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary3Maximum6:
            case RoomObjectInstruction.ClearDownwardGateTrigger:
            case RoomObjectInstruction.SpawnDownwardGateProjectile:
            case RoomObjectInstruction.WakeDownwardGateProjectile:
            case RoomObjectInstruction.ShootEyeDoorProjectile:
            case RoomObjectInstruction.SpawnEyeDoorSweat:
            case RoomObjectInstruction.SpawnTwoEyeDoorSmoke:
            case RoomObjectInstruction.SpawnEyeDoorSmoke:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomObjectInstruction.DamageDraygonCannonFacingRight:
            case RoomObjectInstruction.DamageDraygonCannonFacingLeft:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
            case RoomObjectInstruction.SpawnTorizoStatueBreaking:
            case RoomObjectInstruction.QueueSongOneMusicTrack:
            case RoomObjectInstruction.EnableNoobTubeWaterPhysics:
            case RoomObjectInstruction.SpawnNoobTubeCrack:
            case RoomObjectInstruction.TriggerNoobTubeEarthquake:
            case RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomObjectInstruction.LockSamus:
            case RoomObjectInstruction.UnlockSamus:
            case RoomObjectInstruction.MoveRightFourBlocks:
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
            case RoomObjectInstruction.SamusEaterDamage:
            case RoomObjectInstruction.SamusEaterReleaseImmunity:
            case RoomObjectInstruction.MoveTourianAccessDown:
            case RoomObjectInstruction.TransformSpikesToSlopes:
            case RoomObjectInstruction.RevertSlopesToSpikes:
            case RoomObjectInstruction.SetLoweredAcidHeight:
            default:
                return false;
        }
    }

    /// <summary>The restore instruction, which draws the saved block for one update.</summary>
    private static bool BlockReactionRestoreAt(IImportCartridgeSource cartridge, ushort cursor, out BlockReactionObjectFrame frame, out int duration, out int length)
    {
        frame = BlockReactionObjectFrame.RestoredBlock;
        duration = 1;
        length = 2;
        return InstructionAt<RoomObjectInstruction>(cartridge, cursor) == RoomObjectInstruction.DrawObjectBlock;
    }

    /// <summary>The Speed Booster blocks' programs equal their native lists; both restore instructions draw the saved block.</summary>
    internal static void VerifySpeedBoosterBlockPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SpeedBoosterBlockPrograms.Programs,
            SpeedBoosterBlockEntries, SpeedBoosterBlockDrawList, SpeedBoosterBlockCueAt, "Speed Booster block", SpeedBoosterBlockRestoreAt);
        Console.WriteLine($"  Speed Booster block programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The blocks' break-sound instruction.</summary>
    private static bool SpeedBoosterBlockCueAt(IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterBlockCue cue, out int length)
    {
        cue = SpeedBoosterBlockCue.QueueBreakSound;
        length = 3;
        if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) != RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct)
            return false;
        AssertEqual(SpeedBoosterBlockObjectProgramDefinitions.BreakSoundId, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), "speed block break sound");
        AssertEqual(SoundRequestPriority.WhenIdle, SpeedBoosterBlockObjectProgramDefinitions.BreakSoundPriority, "speed block break sound priority");
        return true;
    }

    /// <summary>The two restore instructions, which draw the saved block for one update.</summary>
    private static bool SpeedBoosterBlockRestoreAt(IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterBlockFrame frame, out int duration, out int length)
    {
        frame = SpeedBoosterBlockFrame.RestoredBlock;
        duration = 1;
        length = 2;
        return InstructionAt<RoomObjectInstruction>(cartridge, cursor) is RoomObjectInstruction.DrawObjectBlock or RoomObjectInstruction.DrawObjectBlockClone;
    }

    /// <summary>The bank-$84 draw list a Speed Booster block frame draws.</summary>
    private static RoomObjectDraw SpeedBoosterBlockDrawList(SpeedBoosterBlockFrame frame) => frame switch
    {
        SpeedBoosterBlockFrame.Crumble0 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(0),
        SpeedBoosterBlockFrame.Crumble1 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(1),
        SpeedBoosterBlockFrame.Crumble2 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(2),
        SpeedBoosterBlockFrame.Crumble3 => SpeedBoosterBlockObjectDraws.CrumbleDrawList(3),
        SpeedBoosterBlockFrame.BombReveal => SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw,
        SpeedBoosterBlockFrame.RestoredBlock => throw new InvalidOperationException("The restored block is drawn by its native restore instruction."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpeedBoosterBlockFrame)} {frame}."),
    };

    /// <summary>
    /// The downward gate's native lists do what <see cref="DownwardGateObject"/> does (#1554): the closed gate shows for
    /// one update, clears its trigger, arms its closed wake and sleeps; past the sleep it wakes its projectile, plays the
    /// movement sound and rises image by image into the open gate, which shows for one update, clears its trigger, arms
    /// its open wake and sleeps; past that sleep it shows the open gate, spawns its moving projectile with the movement
    /// sound and lowers image by image into the closed gate. Every image, hold and operand is compared with
    /// <see cref="DownwardGateMovementDefinitions"/>.
    /// </summary>
    internal static void VerifyDownwardGatePrograms(ISnesAddressSpace rom)
    {
        IReadOnlyDictionary<GateState, ushort> addresses = DownwardGateAddresses(rom);
        Console.WriteLine($"  Downward gate lists: {addresses.Count} typed gate states match bank $84.");
    }

    /// <summary>
    /// The native instruction a downward gate in the typed state of <paramref name="gate"/> rests on at the end of an
    /// update, for comparisons with native gate traces: the instruction after the image it shows, or the sleep it rests
    /// on.
    /// </summary>
    internal static ushort DownwardGateNativeInstruction(ISnesAddressSpace rom, DownwardGateObject gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        downwardGateAddresses ??= DownwardGateAddresses(rom);
        int frame = gate.Phase is DownwardGatePhase.Closing or DownwardGatePhase.Reopening ? gate.Frame : 0;
        return downwardGateAddresses.TryGetValue(new GateState(gate.Phase, frame), out ushort address)
            ? address
            : throw new InvalidDataException($"A downward gate in {gate.Phase} at image {frame} has no native instruction.");
    }

    /// <summary>The native instruction of every typed gate state, walked once.</summary>
    private static IReadOnlyDictionary<GateState, ushort>? downwardGateAddresses;

    /// <summary>
    /// Walks the gate's native lists from its header, checking each against the typed course, and returns the instruction
    /// each typed state rests on.
    /// </summary>
    private static Dictionary<GateState, ushort> DownwardGateAddresses(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, TerrainChangeFrame frame, string at)
        {
            AssertEqual(frame.Updates, (int)Word(cursor), $"{at} updates");
            AssertEqual((int)SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(frame.Draw), (int)Word(cursor + 2), $"{at} image");
        }
        void AssertMovementSound(int cursor, string at)
        {
            AssertEqual(RoomObjectInstruction.QueueSoundLibrary3Maximum6, At(cursor), $"{at} movement sound");
            AssertEqual(DownwardGateDefinitions.MovementSound, SoundEffectCommands.Decode(SoundEffectLibrary.Room, ReadOperandByte(cartridge, cursor + 2)),
                $"{at} movement sound identity");
            AssertEqual(SoundRequestPriority.Normal, DownwardGateMovementDefinitions.MovementSoundPriority, $"{at} movement sound priority");
        }
        // Each resting list: its image for one update, the trigger cleared, its wake armed, the sleep; the continuation
        // after the sleep follows it.
        int Resting(int list, RoomObjectDraw image, DownwardGatePhase showing, DownwardGatePhase resting, Dictionary<GateState, ushort> into)
        {
            AssertShown(list, new TerrainChangeFrame(image, 1), $"{showing}");
            AssertEqual(RoomObjectInstruction.ClearDownwardGateTrigger, At(list + 4), $"{resting} clears the trigger");
            AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(list + 6), $"{resting} arms its wake");
            AssertEqual(resting, RoomObjectFrameBehaviorNativeWords.DownwardGate.Decode(Word(list + 8), "downward gate wake"), $"{resting} wake");
            AssertEqual(RoomObjectInstruction.Sleep, At(list + 10), $"{resting} sleeps");
            into[new GateState(showing, 0)] = checked((ushort)(list + 4));
            into[new GateState(resting, 0)] = checked((ushort)(list + 10));
            return list + 12;
        }

        var addresses = new Dictionary<GateState, ushort>();
        int closed = Word(RoomObjectNativeWords.Word(RoomObjectHeaderId.DownwardGate) + 2);
        addresses[new GateState(DownwardGatePhase.Starting, 0)] = checked((ushort)closed);
        int reopen = Resting(closed, DownwardGateMovementDefinitions.ClosedImage, DownwardGatePhase.ShowingClosed, DownwardGatePhase.RestingClosed, addresses);
        AssertEqual(RoomObjectInstruction.WakeDownwardGateProjectile, At(reopen), "reopening wakes the gate projectile");
        AssertMovementSound(reopen + 4, "reopening");
        int cursor = reopen + 7;
        for (int index = 0; index < DownwardGateMovementDefinitions.Reopening.Count; index++, cursor += 4)
        {
            AssertShown(cursor, DownwardGateMovementDefinitions.Reopening[index], $"reopening image {index}");
            addresses[new GateState(DownwardGatePhase.Reopening, index)] = checked((ushort)(cursor + 4));
        }
        AssertEqual(RoomObjectInstruction.Goto, At(cursor), "reopening goes on to the open gate");
        int open = Word(cursor + 2);
        int close = Resting(open, DownwardGateMovementDefinitions.OpenImage, DownwardGatePhase.ShowingOpen, DownwardGatePhase.RestingOpen, addresses);
        cursor = close;
        for (int index = 0; index < DownwardGateMovementDefinitions.Closing.Count; index++, cursor += 4)
        {
            if (index == DownwardGateMovementDefinitions.ClosingMovementStart)
            {
                AssertEqual(RoomObjectInstruction.SpawnDownwardGateProjectile, At(cursor), "closing spawns the moving gate");
                AssertEqual(EnemyProjectileNativeWords.Kinds.Word(RoomEnemyProjectileKind.DownwardGateMoving), Word(cursor + 2), "downward gate moving projectile");
                AssertMovementSound(cursor + 4, "closing");
                cursor += 7;
            }
            AssertShown(cursor, DownwardGateMovementDefinitions.Closing[index], $"closing image {index}");
            addresses[new GateState(DownwardGatePhase.Closing, index)] = checked((ushort)(cursor + 4));
        }
        AssertEqual(closed, cursor, "closing runs on into the closed gate");
        return addresses;
    }

    /// <summary>A typed gate state: its phase and, while it moves, the image it shows.</summary>
    /// <param name="Phase">The gate's phase.</param>
    /// <param name="Frame">The image of its movement, or zero at rest.</param>
    private readonly record struct GateState(DownwardGatePhase Phase, int Frame);

    /// <summary>The Tourian elevator access changes play as their native lists do.</summary>
    internal static void VerifyTourianAccessPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TourianAccessFloor>().Length, TourianAccessEntries.Count, "every Tourian access floor has a native list");
        int frames = 0;
        foreach ((TourianAccessFloor floor, ushort entry) in TourianAccessEntries)
        {
            frames += NativeTerrainChanges.Compare(cartridge, TourianAccessFloors.ForState(floor).Change, entry,
                (IImportCartridgeSource source, ushort cursor, out NativeTerrainChanges.NativeStep step) =>
                {
                    step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(0, 1), Sound: null, Begins: false, Length: 2);
                    return InstructionAt<RoomObjectInstruction>(source, cursor) == RoomObjectInstruction.MoveTourianAccessDown;
                }, $"Tourian access {floor}", out bool began);
            AssertTrue(!began, $"Tourian access {floor} opens nothing as it starts");
        }
        Console.WriteLine($"  Tourian access changes: {frames} typed frames match bank $84.");
    }

    /// <summary>
    /// The Wrecked Ship attic's native list does what <see cref="WreckedShipAtticObject"/> does (#1554): it installs a
    /// routine that changes nothing and sleeps for good, so the object only holds its place.
    /// </summary>
    internal static void VerifyWreckedShipAtticPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, WreckedShipAtticList), "attic installs its routine");
        AssertEqual(WreckedShipAtticObjectRomData.NoOpCallback, cartridge.ReadCartridgeWord(ProgramBank | (WreckedShipAtticList + 2)), "attic pre-instruction");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, WreckedShipAtticList + 4), "attic sleeps for good");
        Console.WriteLine("  Wrecked Ship attic list: installs its inert routine and sleeps, as its typed object holds its place.");
    }

    /// <summary>The blue caps' programs equal their native lists.</summary>
    internal static void VerifyBlueDoorPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BlueDoorObjectProgramDefinitions.Programs, DoorObjectNativeListAddresses.BlueEntries,
            BlueDoorDrawList, BlueDoorCueAt, "blue door");
        Console.WriteLine($"  Blue door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw list a blue program's frame draws: the program names the facing.</summary>
    private static RoomObjectDraw BlueDoorDrawList(BlueDoorProgram program, BlueDoorFrame frame)
    {
        ColoredDoorOrientation orientation = BlueDoorObjectProgramDefinitions.Orientations[program];
        return frame switch
        {
            BlueDoorFrame.Clear => DoorCaps.Clear(orientation),
            BlueDoorFrame.Blue0 => DoorCaps.Blue(orientation, 0),
            BlueDoorFrame.Blue1 => DoorCaps.Blue(orientation, 1),
            BlueDoorFrame.Blue2 => DoorCaps.Blue(orientation, 2),
            BlueDoorFrame.Blue3 => DoorCaps.Blue(orientation, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(BlueDoorFrame)} {frame}."),
        };
    }

    /// <summary>The blue caps' sound and BTS-restore instructions.</summary>
    private static bool BlueDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, out BlueDoorCue cue, out int length)
    {
        (bool known, cue, length) = InstructionAt<RoomObjectInstruction>(cartridge, cursor) switch
        {
            RoomObjectInstruction.QueueSoundLibrary3Maximum6 => new BlueDoorCueAtCase(true, DoorObjectNativeProgramOracle.DoorSoundCue(DoorObjectNativeProgramOracle.QueuedLibrary3Sound(cartridge, cursor), BlueDoorCue.QueueOpenSound, BlueDoorCue.QueueCloseSound), 3),
            RoomObjectInstruction.SetObjectBtsFromByte => new BlueDoorCueAtCase(true, BlueDoorCue.RestoreBlueBehavior, 3),
            RoomObjectInstruction.SetGreyDoorFrameBehavior or
                RoomObjectInstruction.Sleep or
                RoomObjectInstruction.Delete or
                RoomObjectInstruction.InstallFrameBehavior or
                RoomObjectInstruction.ClearFrameBehavior or
                RoomObjectInstruction.LinkInstruction or
                RoomObjectInstruction.GotoIfDoorBitSet or
                RoomObjectInstruction.IncrementDoorHitCounterAndGoto or
                RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual or
                RoomObjectInstruction.GotoIfSamusNear or
                RoomObjectInstruction.Goto or
                RoomObjectInstruction.DecrementTimerAndGoto or
                RoomObjectInstruction.SetEightBitTimer or
                RoomObjectInstruction.CopyFromRamToVram or
                RoomObjectInstruction.GotoIfAreaBossBitSet or
                RoomObjectInstruction.GotoIfEventSet or
                RoomObjectInstruction.SetEvent or
                RoomObjectInstruction.DrawObjectBlock or
                RoomObjectInstruction.DrawObjectBlockClone or
                RoomObjectInstruction.QueueSoundLibrary2Maximum6 or
                RoomObjectInstruction.ClearDownwardGateTrigger or
                RoomObjectInstruction.SpawnDownwardGateProjectile or
                RoomObjectInstruction.WakeDownwardGateProjectile or
                RoomObjectInstruction.ShootEyeDoorProjectile or
                RoomObjectInstruction.SpawnEyeDoorSweat or
                RoomObjectInstruction.SpawnTwoEyeDoorSmoke or
                RoomObjectInstruction.SpawnEyeDoorSmoke or
                RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight or
                RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft or
                RoomObjectInstruction.DamageDraygonCannonFacingRight or
                RoomObjectInstruction.DamageDraygonCannonFacingLeft or
                RoomObjectInstruction.QueueSoundLibrary2Maximum3 or
                RoomObjectInstruction.QueueSoundLibrary2Maximum1 or
                RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct or
                RoomObjectInstruction.GotoIfSamusHasNoBombs or
                RoomObjectInstruction.SetObjectBtsToOne or
                RoomObjectInstruction.GotoIfRoomArgumentLess or
                RoomObjectInstruction.SpawnFourMotherBrainGlassShards or
                RoomObjectInstruction.SpawnTorizoStatueBreaking or
                RoomObjectInstruction.QueueSongOneMusicTrack or
                RoomObjectInstruction.EnableNoobTubeWaterPhysics or
                RoomObjectInstruction.SpawnNoobTubeCrack or
                RoomObjectInstruction.TriggerNoobTubeEarthquake or
                RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles or
                RoomObjectInstruction.LockSamus or
                RoomObjectInstruction.UnlockSamus or
                RoomObjectInstruction.MoveRightFourBlocks or
                RoomObjectInstruction.SetAnimalsEscapedEvent or
                RoomObjectInstruction.SamusEaterDamage or
                RoomObjectInstruction.SamusEaterReleaseImmunity or
                RoomObjectInstruction.MoveTourianAccessDown or
                RoomObjectInstruction.TransformSpikesToSlopes or
                RoomObjectInstruction.RevertSlopesToSpikes or
                RoomObjectInstruction.SetLoweredAcidHeight => new BlueDoorCueAtCase(false, default, 0),
            _ => new BlueDoorCueAtCase(false, default, 0),
        };
        return known;
    }

    /// <summary>Every coloured cap's programs equal their native lists.</summary>
    internal static void VerifyColoredDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            var entries = DoorObjectNativeListAddresses.ColoredEntries(color, orientation);
            steps += CompareProgramSet(cartridge, ColoredDoorObjectProgramDefinitions.Sets[new(color, orientation)], entries,
                frame => ColoredCapDraw(color, orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out ColoredDoorCue cue, out int length) =>
                    ColoredDoorCueAt(source, cursor, color, orientation, entries, out cue, out length),
                $"{color} {orientation} door");
        }
        Console.WriteLine($"  Coloured door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw list a coloured cap's frame draws.</summary>
    private static RoomObjectDraw ColoredCapDraw(ColoredDoorColor color, ColoredDoorOrientation orientation, DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorCaps.Clear(orientation),
        DoorCapFrame.Closed0 => DoorCaps.Colored(color, orientation, 0),
        DoorCapFrame.Closed1 => DoorCaps.Colored(color, orientation, 1),
        DoorCapFrame.Closed2 => DoorCaps.Colored(color, orientation, 2),
        DoorCapFrame.Closed3 => DoorCaps.Colored(color, orientation, 3),
        DoorCapFrame.BlueFlash => DoorCaps.Blue(orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <summary>A coloured cap's branch, link, wake, hit-count and sound instructions with their operands.</summary>
    private static bool ColoredDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, ColoredDoorColor color, ColoredDoorOrientation orientation,
        IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> entries, out ColoredDoorCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        ushort EntryOf(ColoredDoorProgram program) => entries.Single(entry => entry.Program == program).Entry;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.GotoIfDoorBitSet:
                AssertEqual(DoorObjectNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorObjectProgramDefinitions.ClosedPrograms[orientation]).Entry,
                    Word(2), $"{color} {orientation} door becomes its blue cap");
                cue = ColoredDoorCue.BranchIfOpened;
                length = 4;
                return true;
            case RoomObjectInstruction.LinkInstruction:
                AssertEqual(EntryOf(ColoredDoorProgram.Hit), Word(2), $"{color} {orientation} door links its hit");
                cue = ColoredDoorCue.LinkHit;
                length = 4;
                return true;
            case RoomObjectInstruction.InstallFrameBehavior:
                AssertEqual(DoorsNativeWords.ColoredDoorFrameBehaviors.Word(ColoredDoorObjectProgramDefinitions.Wakes[color]), Word(2), $"{color} {orientation} door shot wake");
                cue = ColoredDoorCue.ArmWake;
                length = 4;
                return true;
            case RoomObjectInstruction.IncrementDoorHitCounterAndGoto:
                AssertEqual(ColoredDoorObjectProgramDefinitions.HitThresholds[color], ReadOperandByte(cartridge, cursor + 2), $"{color} door hit threshold");
                AssertEqual(EntryOf(ColoredDoorProgram.Open), Word(3), $"{color} {orientation} door opens at its threshold");
                cue = ColoredDoorCue.CountHit;
                length = 5;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary3Maximum6:
                SoundEffectId sound = DoorObjectNativeProgramOracle.QueuedLibrary3Sound(cartridge, cursor);
                cue = sound == ColoredDoorObjectProgramDefinitions.RedHitSound
                    ? ColoredDoorCue.QueueHitSound
                    : DoorObjectNativeProgramOracle.DoorSoundCue(sound, ColoredDoorCue.QueueOpenSound, ColoredDoorCue.QueueCloseSound);
                length = 3;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
            case RoomObjectInstruction.ClearFrameBehavior:
            case RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomObjectInstruction.GotoIfSamusNear:
            case RoomObjectInstruction.Goto:
            case RoomObjectInstruction.DecrementTimerAndGoto:
            case RoomObjectInstruction.SetEightBitTimer:
            case RoomObjectInstruction.CopyFromRamToVram:
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
            case RoomObjectInstruction.GotoIfEventSet:
            case RoomObjectInstruction.SetEvent:
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
            case RoomObjectInstruction.ClearDownwardGateTrigger:
            case RoomObjectInstruction.SpawnDownwardGateProjectile:
            case RoomObjectInstruction.WakeDownwardGateProjectile:
            case RoomObjectInstruction.ShootEyeDoorProjectile:
            case RoomObjectInstruction.SpawnEyeDoorSweat:
            case RoomObjectInstruction.SpawnTwoEyeDoorSmoke:
            case RoomObjectInstruction.SpawnEyeDoorSmoke:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomObjectInstruction.DamageDraygonCannonFacingRight:
            case RoomObjectInstruction.DamageDraygonCannonFacingLeft:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
            case RoomObjectInstruction.SetObjectBtsToOne:
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
            case RoomObjectInstruction.SpawnTorizoStatueBreaking:
            case RoomObjectInstruction.QueueSongOneMusicTrack:
            case RoomObjectInstruction.EnableNoobTubeWaterPhysics:
            case RoomObjectInstruction.SpawnNoobTubeCrack:
            case RoomObjectInstruction.TriggerNoobTubeEarthquake:
            case RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomObjectInstruction.LockSamus:
            case RoomObjectInstruction.UnlockSamus:
            case RoomObjectInstruction.MoveRightFourBlocks:
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
            case RoomObjectInstruction.SamusEaterDamage:
            case RoomObjectInstruction.SamusEaterReleaseImmunity:
            case RoomObjectInstruction.MoveTourianAccessDown:
            case RoomObjectInstruction.TransformSpikesToSlopes:
            case RoomObjectInstruction.RevertSlopesToSpikes:
            case RoomObjectInstruction.SetLoweredAcidHeight:
            default:
                cue = default;
                length = 0;
                return false;
        }
    }

    /// <summary>Every grey cap's programs, Bomb Torizo's included, equal their native lists.</summary>
    internal static void VerifyGreyDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        foreach (GreyDoorVariant variant in DoorObjectNativeListAddresses.GreyVariants)
            steps += CompareProgramSet(cartridge, GreyDoorObjectProgramDefinitions.Sets[variant], DoorObjectNativeListAddresses.GreyEntries(variant),
                frame => DoorObjectNativeProgramOracle.GreyCapDraw(variant.Orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out GreyDoorCue cue, out int length) =>
                    DoorObjectNativeProgramOracle.GreyDoorCueAt(source, cursor, variant, out cue, out length),
                variant.BombTorizo ? "Bomb Torizo door" : $"grey {variant.Orientation} door");
        Console.WriteLine($"  Grey door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Every eye-door component's programs, both facings, equal their native lists.</summary>
    internal static void VerifyEyeDoorPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int steps = 0;
        HashSet<EyeDoorObjectProgram> covered = [];
        foreach (EyeDoorOrientation orientation in Enum.GetValues<EyeDoorOrientation>())
        foreach (EyeDoorComponent component in Enum.GetValues<EyeDoorComponent>())
        {
            var entries = DoorObjectNativeListAddresses.EyeDoorEntries(orientation, component);
            covered.UnionWith(entries.Select(entry => entry.Program));
            steps += CompareProgramSubset(cartridge, EyeDoorObjectProgramDefinitions.Sets[orientation], entries,
                (EyeDoorObjectProgram _, EyeDoorObjectFrame frame) => EyeDoorDraws.DrawList(orientation, component, frame),
                (IImportCartridgeSource source, ushort cursor, out EyeDoorObjectCue cue, out int length) =>
                    EyeDoorCueAt(source, cursor, orientation, entries, out cue, out length),
                $"{orientation} eye door {component}");
        }
        AssertEqual(Enum.GetValues<EyeDoorObjectProgram>().Length, covered.Count, "every typed eye-door program has a native entry");
        Console.WriteLine($"  Eye door programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The eye door's branch, proximity, link, wake, projectile, smoke, hit, conversion and handoff instructions.</summary>
    private static bool EyeDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, EyeDoorOrientation orientation,
        IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> entries, out EyeDoorObjectCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        EyeDoorObjectProgram ProgramAt(ushort entry) => entries.Single(e => e.Entry == entry).Program;
        string at = $"{orientation} eye door at $84:{cursor:X4}";
        // Since #1414 the shot and sweat carry the eye's facing; the cartridge's operands are the byte offsets of the
        // facing's origin pair ($86:B65B) and launch pair ($86:B6B1), which LookupStream4 ties to the typed values.
        const ushort LeftOperand = 0;
        const ushort RightShotOperand = 20;
        const ushort RightSweatOperand = 4;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.GotoIfDoorBitSet:
                cue = ProgramAt(Word(2)) switch
                {
                    EyeDoorObjectProgram.EyeOpened => EyeDoorObjectCue.ConvertIfOpened,
                    EyeDoorObjectProgram.ComponentDelete => EyeDoorObjectCue.DeleteIfOpened,
                    EyeDoorObjectProgram.EyeStart or
                        EyeDoorObjectProgram.EyeIdle or
                        EyeDoorObjectProgram.EyeActivate or
                        EyeDoorObjectProgram.EyeWatch or
                        EyeDoorObjectProgram.EyeClose or
                        EyeDoorObjectProgram.EyeHit or
                        EyeDoorObjectProgram.EyeDestroyed or
                        EyeDoorObjectProgram.EyeOpening or
                        EyeDoorObjectProgram.ComponentStart or
                        EyeDoorObjectProgram.ComponentIdle or
                        EyeDoorObjectProgram.ComponentActivate or
                        EyeDoorObjectProgram.ComponentCycle => throw new InvalidDataException($"{at}: door-bit branch to {ProgramAt(Word(2))}."),
                    var target => throw new InvalidDataException($"{at}: door-bit branch to {target}."),
                };
                length = 4;
                return true;
            case RoomObjectInstruction.GotoIfSamusNear:
                var near = new EyeDoorProximity(new(ReadOperandByte(cartridge, cursor + 2), ReadOperandByte(cartridge, cursor + 3)), ProgramAt(Word(4)));
                cue = Enum.GetValues<EyeDoorObjectCue>().Single(candidate => candidate is EyeDoorObjectCue.ActivateIfSamusNear
                    or EyeDoorObjectCue.CloseIfSamusInFront or EyeDoorObjectCue.WatchIfSamusNear or EyeDoorObjectCue.PulseIfSamusNear
                    or EyeDoorObjectCue.KeepPulsingIfSamusNear && EyeDoorObjectProgramDefinitions.Proximities[candidate] == near);
                length = 6;
                return true;
            case RoomObjectInstruction.LinkInstruction:
                cue = ProgramAt(Word(2)) switch
                {
                    EyeDoorObjectProgram.EyeHit => EyeDoorObjectCue.LinkHit,
                    EyeDoorObjectProgram.ComponentDelete => EyeDoorObjectCue.LinkDelete,
                    EyeDoorObjectProgram.EyeStart or
                        EyeDoorObjectProgram.EyeIdle or
                        EyeDoorObjectProgram.EyeActivate or
                        EyeDoorObjectProgram.EyeWatch or
                        EyeDoorObjectProgram.EyeClose or
                        EyeDoorObjectProgram.EyeDestroyed or
                        EyeDoorObjectProgram.EyeOpening or
                        EyeDoorObjectProgram.EyeOpened or
                        EyeDoorObjectProgram.ComponentStart or
                        EyeDoorObjectProgram.ComponentIdle or
                        EyeDoorObjectProgram.ComponentActivate or
                        EyeDoorObjectProgram.ComponentCycle => throw new InvalidDataException($"{at}: link to {ProgramAt(Word(2))}."),
                    var target => throw new InvalidDataException($"{at}: link to {target}."),
                };
                length = 4;
                return true;
            case RoomObjectInstruction.InstallFrameBehavior:
                cue = DoorsNativeWords.EyeDoorFrameBehaviors.Decode(Word(2), "eye-door pre-instruction") switch
                {
                    EyeDoorFrameBehavior.MissileHit => EyeDoorObjectCue.ArmMissileWake,
                    EyeDoorFrameBehavior.WakeWhenDoorBitSet => EyeDoorObjectCue.ArmDoorBitWake,
                    var wake => throw new InvalidDataException($"{at}: wake {wake}."),
                };
                length = 4;
                return true;
            case RoomObjectInstruction.ShootEyeDoorProjectile:
                AssertEqual(orientation == EyeDoorOrientation.Right ? RightShotOperand : LeftOperand, Word(2), $"{at}: shot operand selects the facing's origin");
                cue = EyeDoorObjectCue.Shoot;
                length = 4;
                return true;
            case RoomObjectInstruction.SpawnEyeDoorSweat:
                AssertEqual(orientation == EyeDoorOrientation.Right ? RightSweatOperand : LeftOperand, Word(2), $"{at}: sweat operand selects the facing's launch");
                cue = EyeDoorObjectCue.SpawnSweat;
                length = 4;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
                AssertEqual(EyeDoorObjectProgramDefinitions.HitSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), $"{at}: hit sound");
                cue = EyeDoorObjectCue.QueueHitSound;
                length = 3;
                return true;
            case RoomObjectInstruction.SpawnTwoEyeDoorSmoke:
                cue = EyeDoorObjectCue.SpawnTwoSmoke;
                length = 2;
                return true;
            case RoomObjectInstruction.SpawnEyeDoorSmoke:
                cue = EyeDoorObjectCue.SpawnSmoke;
                length = 2;
                return true;
            case RoomObjectInstruction.ClearFrameBehavior:
                cue = EyeDoorObjectCue.ClearFrameBehavior;
                length = 2;
                return true;
            case RoomObjectInstruction.IncrementDoorHitCounterAndGoto:
                AssertEqual(EyeDoorObjectProgramDefinitions.HitThreshold, ReadOperandByte(cartridge, cursor + 2), $"{at}: hit threshold");
                AssertEqual(EyeDoorObjectProgram.EyeDestroyed, ProgramAt(Word(3)), $"{at}: the third hit destroys the eye");
                cue = EyeDoorObjectCue.CountHit;
                length = 5;
                return true;
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight:
                AssertEqual(EyeDoorOrientation.Right, orientation, $"{at}: blue conversion facing");
                cue = EyeDoorObjectCue.MakeBlue;
                length = 2;
                return true;
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft:
                AssertEqual(EyeDoorOrientation.Left, orientation, $"{at}: blue conversion facing");
                cue = EyeDoorObjectCue.MakeBlue;
                length = 2;
                return true;
            case RoomObjectInstruction.Goto:
                ColoredDoorOrientation facing = orientation == EyeDoorOrientation.Right ? ColoredDoorOrientation.Right : ColoredDoorOrientation.Left;
                AssertEqual(DoorObjectNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorObjectProgramDefinitions.ClosedPrograms[facing]).Entry,
                    Word(2), $"{at}: the eye becomes its blue cap");
                cue = EyeDoorObjectCue.BecomeBlueDoor;
                length = 4;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
            case RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomObjectInstruction.DecrementTimerAndGoto:
            case RoomObjectInstruction.SetEightBitTimer:
            case RoomObjectInstruction.CopyFromRamToVram:
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
            case RoomObjectInstruction.GotoIfEventSet:
            case RoomObjectInstruction.SetEvent:
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary3Maximum6:
            case RoomObjectInstruction.ClearDownwardGateTrigger:
            case RoomObjectInstruction.SpawnDownwardGateProjectile:
            case RoomObjectInstruction.WakeDownwardGateProjectile:
            case RoomObjectInstruction.DamageDraygonCannonFacingRight:
            case RoomObjectInstruction.DamageDraygonCannonFacingLeft:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
            case RoomObjectInstruction.SetObjectBtsToOne:
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
            case RoomObjectInstruction.SpawnTorizoStatueBreaking:
            case RoomObjectInstruction.QueueSongOneMusicTrack:
            case RoomObjectInstruction.EnableNoobTubeWaterPhysics:
            case RoomObjectInstruction.SpawnNoobTubeCrack:
            case RoomObjectInstruction.TriggerNoobTubeEarthquake:
            case RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomObjectInstruction.LockSamus:
            case RoomObjectInstruction.UnlockSamus:
            case RoomObjectInstruction.MoveRightFourBlocks:
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
            case RoomObjectInstruction.SamusEaterDamage:
            case RoomObjectInstruction.SamusEaterReleaseImmunity:
            case RoomObjectInstruction.MoveTourianAccessDown:
            case RoomObjectInstruction.TransformSpikesToSlopes:
            case RoomObjectInstruction.RevertSlopesToSpikes:
            case RoomObjectInstruction.SetLoweredAcidHeight:
            default:
                cue = default;
                length = 0;
                return false;
        }
    }

    /// <summary>
    /// A Brinstar plant's native list plays as its typed plant does (#1554): it installs the hold and counts its chews;
    /// each chew's images, and the sound and damage before them, repeat while the count runs; then the release immunity,
    /// the end of the hold, the closed mouth and the idle plant, and the end. Every image, count and bite is compared
    /// with <paramref name="plant"/>.
    /// </summary>
    internal static void VerifySamusEaterList(ISnesAddressSpace rom, int list, SamusEaterObjectDefinition plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, SamusEaterDraw image, int updates, string at)
        {
            AssertEqual(updates, (int)Word(cursor), $"{at} updates");
            AssertEqual((int)SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image)), (int)Word(cursor + 2), $"{at} image");
        }

        string plantName = plant.Ceiling ? "ceiling plant" : "floor plant";
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(list), $"{plantName} first holds Samus");
        AssertEqual(SamusEaterObjectDefinitionsConstants.HoldFrameBehavior, Word(list + 2), $"{plantName} hold");
        AssertEqual(RoomObjectInstruction.SetEightBitTimer, At(list + 4), $"{plantName} counts its chews");
        AssertEqual(plant.Chews, (int)ReadOperandByte(cartridge, list + 6), $"{plantName} chews");
        int chew = list + 7;
        int cursor = chew;
        for (int index = 0; index < plant.Chew.Count; index++)
        {
            SamusEaterChewFrame frame = plant.Chew[index];
            string at = $"{plantName} chew image {index}";
            if (frame.BiteBefore == SamusEaterBite.SoundAndDamage)
            {
                AssertEqual(RoomObjectInstruction.QueueSoundLibrary2Maximum6, At(cursor), $"{at} chew sound");
                AssertEqual(SamusEaterObjectDefinitions.ChewSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)),
                    $"{at} chew sound identity");
                AssertEqual(SoundRequestPriority.Normal, SamusEaterObjectDefinitions.ChewSoundPriority, $"{at} chew sound priority");
                cursor += 3;
            }
            if (frame.BiteBefore != SamusEaterBite.None)
            {
                AssertEqual(RoomObjectInstruction.SamusEaterDamage, At(cursor), $"{at} damage");
                cursor += 2;
            }
            AssertShown(cursor, frame.Image, frame.Updates, at);
            cursor += 4;
        }
        AssertEqual(RoomObjectInstruction.DecrementTimerAndGoto, At(cursor), $"{plantName} repeats its chew");
        AssertEqual(chew, (int)Word(cursor + 2), $"{plantName} repeats from its first chew image");
        AssertEqual(RoomObjectInstruction.SamusEaterReleaseImmunity, At(cursor + 4), $"{plantName} releases Samus");
        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, At(cursor + 6), $"{plantName} stops holding");
        AssertShown(cursor + 8, plant.Released, SamusEaterObjectDefinitions.ReleaseDuration, $"{plantName} closed mouth");
        AssertShown(cursor + 12, plant.Idle, 1, $"{plantName} idle");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 16), $"{plantName} ends");
    }
}
