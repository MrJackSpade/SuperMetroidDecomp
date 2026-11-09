using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
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
    /// <summary>The n00b tube's programs equal their native lists.</summary>
    internal static void VerifyNoobTubePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), NoobTubeObjectProgramDefinitions.Programs, NoobTubeEntries,
            draw => RoomObjectDraw.Of(draw), NoobTubeCueAt, "n00b tube");
        Console.WriteLine($"  N00b tube programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The shared delete and sleep programs equal their one-instruction native lists.</summary>
    internal static void VerifySharedPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), RoomObjectSharedProgramDefinitions.Programs, SharedEntries,
            draw => throw new InvalidOperationException($"The shared programs never draw; got {draw}."), NoCues<RoomObjectNoCue>(), "shared PLM");
        Console.WriteLine($"  Shared PLM programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The lavaquake program equals its native list; each install names its phase's pre-instruction.</summary>
    internal static void VerifySpeedBoosterEscapePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SpeedBoosterEscapeObjectProgramDefinitions.Programs,
            SpeedBoosterEscapeEntries, draw => throw new InvalidOperationException($"The lavaquake program never draws; got {draw}."),
            (IImportCartridgeSource cartridge, ushort cursor, out SpeedBoosterEscapeFrameBehavior cue, out int length) =>
            {
                bool installs = InstructionAt<RoomObjectInstruction>(cartridge, cursor) == RoomObjectInstruction.InstallFrameBehavior;
                cue = installs
                    ? RoomObjectFrameBehaviorNativeWords.SpeedBoosterEscape.Decode(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "lavaquake pre-instruction")
                    : default;
                length = 4;
                return installs;
            }, "Speed Booster lavaquake");
        // A wake steps over the sleep that closes each phase, onto the next phase's list.
        for (int phase = 0; phase < SpeedBoosterEscapeEntries.Count; phase++)
        {
            (SpeedBoosterEscapeProgram program, ushort entry) = SpeedBoosterEscapeEntries[phase];
            SpeedBoosterEscapeProgram? expected = phase + 1 < SpeedBoosterEscapeEntries.Count ? SpeedBoosterEscapeEntries[phase + 1].Program : null;
            if (expected is { } next)
                AssertEqual(unchecked((ushort)(entry + 6)), SpeedBoosterEscapeEntries[phase + 1].Entry, $"lavaquake {next} follows {program}'s sleep");
            AssertEqual(expected, SpeedBoosterEscapePhases.PhaseAfter(program), $"lavaquake phase after {program}");
        }
        Console.WriteLine($"  Speed Booster lavaquake program: {steps} typed steps match bank $84.");
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
            frame => ScriptedRoomObjectDrawDefinitions.DrawLists[frame], ScriptedRoomCueAt, "scripted room");
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

    /// <summary>The scripted rooms' instructions, with their operands.</summary>
    private static bool ScriptedRoomCueAt(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out ScriptedRoomObjectCue cue, out int length)
    {
        cue = default;
        length = 2;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.InstallFrameBehavior:
                ushort routine = cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2));
                ScriptedRoomObjectFrameBehavior installed =
                    routine == RoomObjectFrameBehaviorNativeRoutines.Of(SuperMetroid.Core.Enemies.Shaktool.ShaktoolRoomFrameBehaviors.Controller) ? ScriptedRoomObjectFrameBehavior.ShaktoolRoomController
                    : routine == RoomObjectFrameBehaviorNativeRoutines.Of(SuperMetroid.Core.Areas.Tourian.TourianFrameBehaviors.OldTourianEscapeShaftWallWaitForSamus) ? ScriptedRoomObjectFrameBehavior.OldTourianWallWaitForSamus
                    : routine == RoomObjectFrameBehaviorNativeRoutines.Of(SuperMetroid.Core.Enemies.Chozo.ChozoStatueFrameBehaviors.WaitForLowerNorfairHand) ? ScriptedRoomObjectFrameBehavior.LowerNorfairChozoHandWait
                    : throw new InvalidDataException($"Scripted room list installs untranslated pre-instruction ${routine:X4}.");
                cue = new ScriptedRoomObjectCue.InstallFrameBehavior(FrameBehavior: installed);
                length = 4;
                return true;
            case RoomObjectInstruction.ClearFrameBehavior:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.ClearFrameBehavior);
                return true;
            case RoomObjectInstruction.GotoIfEventSet:
                AssertEqual(StoryEvent.LowerNorfairChozoLoweredAcid.Number(), cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "Chozo hand event");
                AssertEqual(ScriptedRoomEntries.Single(entry => entry.Program == ScriptedRoomObjectProgram.LowerNorfairChozoHandLowered).Entry,
                    cartridge.ReadCartridgeWord(ProgramBank | (cursor + 4)), "Chozo hand lowered-acid target");
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.ContinueIfLowerNorfairAcidLowered);
                length = 6;
                return true;
            case RoomObjectInstruction.SetLoweredAcidHeight:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.SetLoweredAcidHeight);
                return true;
            case RoomObjectInstruction.TransformSpikesToSlopes:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.TransformSpikesToSlopes);
                return true;
            case RoomObjectInstruction.RevertSlopesToSpikes:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.RevertSlopesToSpikes);
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
            case RoomObjectInstruction.MoveRightFourBlocks:
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
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
            case RoomObjectInstruction.SamusEaterDamage:
            case RoomObjectInstruction.SamusEaterReleaseImmunity:
            case RoomObjectInstruction.MoveTourianAccessDown:
            default:
                return false;
        }
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

    /// <summary>The downward gate's programs equal their native lists, and each wake continues after the sleep it ends.</summary>
    internal static void VerifyDownwardGatePrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), DownwardGatePrograms.Programs, DownwardGateEntries,
            draw => RoomObjectDraw.Of(draw), DownwardGateCueAt, "downward gate");
        foreach ((DownwardGateProgram program, ushort entry) in DownwardGateEntries)
        {
            if (DownwardGatePrograms.ProgramAfterSleep(program) is not { } next)
                continue;
            AssertEqual(unchecked((ushort)(entry + DownwardGateSleepContinuationOffset)), DownwardGateEntries.Single(e => e.Program == next).Entry,
                $"downward gate {next} continues {program} past its sleep");
        }
        ushort firstTrigger = DownwardGateEntries.Single(e => e.Program == DownwardGateProgram.TriggerBlueLeft).Entry;
        AssertTrue(DownwardGateShotBlockDefinitions.Triggers.Select(trigger => trigger.Program).SequenceEqual(
                DownwardGateEntries.Where(e => e.Entry >= firstTrigger).OrderBy(e => e.Entry).Select(e => e.Program)),
            "downward gate triggers are selected in native list order");
        Console.WriteLine($"  Downward gate programs: {steps} typed steps match bank $84.");
    }

    /// <summary>
    /// The native instruction pointer a downward gate's typed program stands for: the address of the native instruction
    /// its cursor rests on, for comparisons with native gate traces.
    /// </summary>
    internal static ushort DownwardGateNativeInstruction(ISnesAddressSpace rom, DownwardGateObjectAnimation gate)
    {
        downwardGateAddresses ??= StepAddresses(CartridgeImportSource.Require(rom), DownwardGatePrograms.Programs,
            DownwardGateEntries, draw => RoomObjectDraw.Of(draw), DownwardGateCueAt, "downward gate");
        return downwardGateAddresses[new ProgramStep<DownwardGateProgram>(gate.Cursor.Program, gate.Cursor.Position)];
    }

    /// <summary>The downward gate's step addresses, walked once.</summary>
    private static IReadOnlyDictionary<ProgramStep<DownwardGateProgram>, ushort>? downwardGateAddresses;

    /// <summary>The gate's trigger, pre-instruction, projectile and sound instructions with their operands.</summary>
    private static bool DownwardGateCueAt(IImportCartridgeSource cartridge, ushort cursor, out DownwardGateCue cue, out int length)
    {
        ushort operand = cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2));
        (bool known, cue, length) = InstructionAt<RoomObjectInstruction>(cartridge, cursor) switch
        {
            RoomObjectInstruction.ClearDownwardGateTrigger => new DownwardGateCueAtCase(true, DownwardGateCue.ClearTrigger, 2),
            RoomObjectInstruction.InstallFrameBehavior => new DownwardGateCueAtCase(true,
                RoomObjectFrameBehaviorNativeWords.DownwardGate.Decode(operand, "downward gate pre-instruction") == DownwardGateFrameBehavior.WakeIfTriggered
                ? DownwardGateCue.ArmWakeIfTriggered : DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow, 4),
            RoomObjectInstruction.SpawnDownwardGateProjectile => new DownwardGateCueAtCase(true, DownwardGateCue.SpawnMovingGate, 4),
            RoomObjectInstruction.WakeDownwardGateProjectile => new DownwardGateCueAtCase(true, DownwardGateCue.WakeGateProjectile, 4),
            RoomObjectInstruction.QueueSoundLibrary3Maximum6 => new DownwardGateCueAtCase(true, DownwardGateCue.QueueMovementSound, 3),
            RoomObjectInstruction.SetGreyDoorFrameBehavior or
                RoomObjectInstruction.Sleep or
                RoomObjectInstruction.Delete or
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
                RoomObjectInstruction.SetObjectBtsFromByte or
                RoomObjectInstruction.DrawObjectBlock or
                RoomObjectInstruction.DrawObjectBlockClone or
                RoomObjectInstruction.QueueSoundLibrary2Maximum6 or
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
                RoomObjectInstruction.SetLoweredAcidHeight => new DownwardGateCueAtCase(false, default, 0),
            _ => new DownwardGateCueAtCase(false, default, 0),
        };
        if (!known)
            return false;
        if (cue == DownwardGateCue.ArmWakeIfTriggeredOrSamusBelow)
            AssertEqual(RoomObjectFrameBehaviorNativeWords.DownwardGate.Word(DownwardGateFrameBehavior.WakeIfTriggeredOrSamusBelow), operand, "downward gate closed wake");
        if (cue == DownwardGateCue.SpawnMovingGate)
            AssertEqual(EnemyProjectileNativeWords.Kinds.Word(RoomEnemyProjectileKind.DownwardGateMoving), operand, "downward gate moving projectile");
        if (cue == DownwardGateCue.QueueMovementSound)
        {
            AssertEqual(DownwardGateDefinitions.MovementSound, SoundEffectCommands.Decode(SoundEffectLibrary.Room, ReadOperandByte(cartridge, cursor + 2)), "downward gate movement sound");
            AssertEqual(SoundRequestPriority.Normal, DownwardGateObjectProgramDefinitions.MovementSoundPriority, "downward gate movement sound priority");
        }
        return true;
    }

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

    /// <summary>The Wrecked Ship attic's program equals its native list.</summary>
    internal static void VerifyWreckedShipAtticPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), WreckedShipAtticObjectProgramDefinitions.Programs, WreckedShipAtticEntries,
            draw => throw new InvalidOperationException($"The attic program never draws; got {draw}."),
            (IImportCartridgeSource cartridge, ushort cursor, out WreckedShipAtticCue cue, out int length) =>
            {
                cue = WreckedShipAtticCue.InstallInertFrameBehavior;
                length = 4;
                if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) != RoomObjectInstruction.InstallFrameBehavior)
                    return false;
                AssertEqual(WreckedShipAtticObjectRomData.NoOpCallback, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "attic pre-instruction");
                return true;
            }, "Wrecked Ship attic");
        Console.WriteLine($"  Wrecked Ship attic program: {steps} typed steps match bank $84.");
    }

    /// <summary>The blue caps play as their native lists do (#1554).</summary>
    internal static void VerifyBlueDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareBlueCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Blue door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every coloured cap plays, counts its hits and takes its shot as its native lists do (#1554).</summary>
    internal static void VerifyColoredDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareColoredCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Coloured door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every grey cap, Bomb Torizo's included, plays and counts its hit as its native lists do (#1554).</summary>
    internal static void VerifyGreyDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareGreyCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Grey door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every eye door's eye, body and bottom, both facings, play as their native lists do (#1554).</summary>
    internal static void VerifyEyeDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareEyeDoors(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Eye doors: {images} typed images match bank $84.");
    }

    /// <summary>The Brinstar plants' programs equal their native lists.</summary>
    internal static void VerifySamusEaterPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SamusEaterObjectProgramDefinitions.Programs, SamusEaterEntries,
            draw => RoomObjectDraw.Of(draw), SamusEaterCueAt, "Brinstar plant");
        Console.WriteLine($"  Brinstar plant programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The plants' hold, chew-sound, damage, release and clear instructions.</summary>
    private static bool SamusEaterCueAt(IImportCartridgeSource cartridge, ushort cursor, out SamusEaterCue cue, out int length)
    {
        (bool known, cue, length) = InstructionAt<RoomObjectInstruction>(cartridge, cursor) switch
        {
            RoomObjectInstruction.InstallFrameBehavior => new SamusEaterCueAtCase(true, SamusEaterCue.HoldSamus, 4),
            RoomObjectInstruction.QueueSoundLibrary2Maximum6 => new SamusEaterCueAtCase(true, SamusEaterCue.QueueChewSound, 3),
            RoomObjectInstruction.SamusEaterDamage => new SamusEaterCueAtCase(true, SamusEaterCue.DamageSamus, 2),
            RoomObjectInstruction.SamusEaterReleaseImmunity => new SamusEaterCueAtCase(true, SamusEaterCue.ReleaseSamus, 2),
            RoomObjectInstruction.ClearFrameBehavior => new SamusEaterCueAtCase(true, SamusEaterCue.ClearFrameBehavior, 2),
            RoomObjectInstruction.SetGreyDoorFrameBehavior or
                RoomObjectInstruction.Sleep or
                RoomObjectInstruction.Delete or
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
                RoomObjectInstruction.SetObjectBtsFromByte or
                RoomObjectInstruction.DrawObjectBlock or
                RoomObjectInstruction.DrawObjectBlockClone or
                RoomObjectInstruction.QueueSoundLibrary3Maximum6 or
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
                RoomObjectInstruction.MoveTourianAccessDown or
                RoomObjectInstruction.TransformSpikesToSlopes or
                RoomObjectInstruction.RevertSlopesToSpikes or
                RoomObjectInstruction.SetLoweredAcidHeight => new SamusEaterCueAtCase(false, default, 0),
            _ => new SamusEaterCueAtCase(false, default, 0),
        };
        if (!known)
            return false;
        if (cue == SamusEaterCue.HoldSamus)
            AssertEqual(SamusEaterObjectDefinitionsConstants.HoldFrameBehavior, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "plant hold pre-instruction");
        if (cue == SamusEaterCue.QueueChewSound)
        {
            AssertEqual(SamusEaterObjectDefinitions.ChewSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), "plant chew sound");
            AssertEqual(SoundRequestPriority.Normal, SamusEaterObjectProgramDefinitions.ChewSoundPriority, "plant chew sound priority");
        }
        return true;
    }

    /// <summary>The n00b tube's native instructions, with their event, link, pre-instruction and sound operands.</summary>
    private static bool NoobTubeCueAt(IImportCartridgeSource cartridge, ushort cursor, out NoobTubeCue cue, out int length)
    {
        ushort Operand(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        NoobTubeProgram Entry(ushort address) => NoobTubeEntries.Single(entry => entry.Entry == address).Program;
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        (bool known, cue, length) = instruction switch
        {
            RoomObjectInstruction.GotoIfEventSet => new NoobTubeCueAtCase(true, NoobTubeCue.SkipIfBroken, 6),
            RoomObjectInstruction.LinkInstruction => new NoobTubeCueAtCase(true, Entry(Operand(2)) == NoobTubeProgram.WaitForInput ? NoobTubeCue.LinkInputWait : NoobTubeCue.LinkBreak, 4),
            RoomObjectInstruction.InstallFrameBehavior =>
                new NoobTubeCueAtCase(true, RoomObjectFrameBehaviorNativeWords.NoobTube.Decode(Operand(2), "n00b tube pre-instruction") == NoobTubeObjectFrameBehavior.WakeOnPowerBomb
                    ? NoobTubeCue.ArmPowerBombWake : NoobTubeCue.ArmInputWake, 4),
            RoomObjectInstruction.ClearFrameBehavior => new NoobTubeCueAtCase(true, NoobTubeCue.ClearFrameBehavior, 2),
            RoomObjectInstruction.LockSamus => new NoobTubeCueAtCase(true, NoobTubeCue.LockSamus, 2),
            RoomObjectInstruction.SpawnNoobTubeCrack => new NoobTubeCueAtCase(true, NoobTubeCue.SpawnCrack, 2),
            RoomObjectInstruction.QueueSoundLibrary2Maximum6 => new NoobTubeCueAtCase(true, NoobTubeCue.QueueBreakSound, 3),
            RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles => new NoobTubeCueAtCase(true, NoobTubeCue.SpawnShardsAndBubbles, 2),
            RoomObjectInstruction.TriggerNoobTubeEarthquake => new NoobTubeCueAtCase(true, NoobTubeCue.TriggerEarthquake, 2),
            RoomObjectInstruction.SetEvent => new NoobTubeCueAtCase(true, NoobTubeCue.SetBrokenEvent, 4),
            RoomObjectInstruction.EnableNoobTubeWaterPhysics => new NoobTubeCueAtCase(true, NoobTubeCue.EnableWaterPhysics, 2),
            RoomObjectInstruction.UnlockSamus => new NoobTubeCueAtCase(true, NoobTubeCue.UnlockSamus, 2),
            RoomObjectInstruction.SetGreyDoorFrameBehavior or
                RoomObjectInstruction.Sleep or
                RoomObjectInstruction.Delete or
                RoomObjectInstruction.GotoIfDoorBitSet or
                RoomObjectInstruction.IncrementDoorHitCounterAndGoto or
                RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual or
                RoomObjectInstruction.GotoIfSamusNear or
                RoomObjectInstruction.Goto or
                RoomObjectInstruction.DecrementTimerAndGoto or
                RoomObjectInstruction.SetEightBitTimer or
                RoomObjectInstruction.CopyFromRamToVram or
                RoomObjectInstruction.GotoIfAreaBossBitSet or
                RoomObjectInstruction.SetObjectBtsFromByte or
                RoomObjectInstruction.DrawObjectBlock or
                RoomObjectInstruction.DrawObjectBlockClone or
                RoomObjectInstruction.QueueSoundLibrary3Maximum6 or
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
                RoomObjectInstruction.MoveRightFourBlocks or
                RoomObjectInstruction.SetAnimalsEscapedEvent or
                RoomObjectInstruction.SamusEaterDamage or
                RoomObjectInstruction.SamusEaterReleaseImmunity or
                RoomObjectInstruction.MoveTourianAccessDown or
                RoomObjectInstruction.TransformSpikesToSlopes or
                RoomObjectInstruction.RevertSlopesToSpikes or
                RoomObjectInstruction.SetLoweredAcidHeight => new NoobTubeCueAtCase(false, default, 0),
            _ => new NoobTubeCueAtCase(false, default, 0),
        };
        if (!known)
            return false;
        switch (cue)
        {
            case NoobTubeCue.SkipIfBroken:
                AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), Operand(2), "n00b tube skip event");
                AssertEqual(NoobTubeProgram.AlreadyBroken, Entry(Operand(4)), "n00b tube skip target");
                break;
            case NoobTubeCue.LinkBreak:
                AssertEqual(NoobTubeProgram.Break, Entry(Operand(2)), "n00b tube break link");
                break;
            case NoobTubeCue.ArmInputWake:
                AssertEqual(RoomObjectFrameBehaviorNativeWords.NoobTube.Word(NoobTubeObjectFrameBehavior.WakeOnAcceptedInput), Operand(2), "n00b tube input wake");
                break;
            case NoobTubeCue.QueueBreakSound:
                AssertEqual(NoobTubeObjectDefinitions.BreakSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), "n00b tube break sound");
                AssertEqual(SoundRequestPriority.Normal, NoobTubeObjectProgramDefinitions.BreakSoundPriority, "n00b tube break sound priority");
                break;
            case NoobTubeCue.SetBrokenEvent:
                AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), Operand(2), "n00b tube broken event");
                break;
            case NoobTubeCue.LinkInputWait:
            case NoobTubeCue.ArmPowerBombWake:
            case NoobTubeCue.ClearFrameBehavior:
            case NoobTubeCue.LockSamus:
            case NoobTubeCue.SpawnCrack:
            case NoobTubeCue.SpawnShardsAndBubbles:
            case NoobTubeCue.TriggerEarthquake:
            case NoobTubeCue.EnableWaterPhysics:
            case NoobTubeCue.UnlockSamus:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(NoobTubeCue)} {cue}.");
        }
        return true;
    }
}
