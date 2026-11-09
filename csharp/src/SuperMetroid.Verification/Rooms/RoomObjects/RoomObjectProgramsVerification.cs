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
    }

    /// <summary>The scripted rooms' instructions, with their operands.</summary>
    private static bool ScriptedRoomCueAt(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out ScriptedRoomObjectCue cue, out int length)
    {
        cue = default;
        length = 2;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
                cue = new ScriptedRoomObjectCue.QueueSoundLibrary2Maximum3(Sound: SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
                cue = new ScriptedRoomObjectCue.QueueSoundLibrary2Maximum6(Sound: SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
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
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.SetAnimalsEscapedEvent);
                return true;
            case RoomObjectInstruction.MoveRightFourBlocks:
                cue = new ScriptedRoomObjectCue.Command(ScriptedRoomObjectCueKind.MoveRightFourBlocks);
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
                cue = new(BlockReactionObjectCueKind.QueueSoundLibrary2Maximum1, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
                cue = new(BlockReactionObjectCueKind.QueueSoundLibrary2Maximum3, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)));
                length = 3;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
                cue = new(BlockReactionObjectCueKind.QueueSoundLibrary2Maximum6, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)));
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
        AssertEqual(SpeedBoosterBlockObjectProgramDefinitions.BreakSoundId, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)), "speed block break sound");
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
            AssertEqual(DownwardGateDefinitions.MovementSound, SoundEffectCommands.Decode(SoundEffectLibrary.Library3, ReadOperandByte(cartridge, cursor + 2)), "downward gate movement sound");
            AssertEqual(SoundRequestPriority.Normal, DownwardGateObjectProgramDefinitions.MovementSoundPriority, "downward gate movement sound priority");
        }
        return true;
    }

    /// <summary>The Tourian elevator access programs equal their native lists.</summary>
    internal static void VerifyTourianAccessPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), TourianAccessObjectProgramDefinitions.Programs, TourianAccessEntries,
            draw => RoomObjectDraw.Of(draw),
            (IImportCartridgeSource cartridge, ushort cursor, out TourianAccessCue cue, out int length) =>
            {
                cue = TourianAccessCue.MoveDown;
                length = 2;
                return InstructionAt<RoomObjectInstruction>(cartridge, cursor) == RoomObjectInstruction.MoveTourianAccessDown;
            }, "Tourian access");
        Console.WriteLine($"  Tourian access programs: {steps} typed steps match bank $84.");
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
                AssertEqual(EyeDoorObjectProgramDefinitions.HitSound, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)), $"{at}: hit sound");
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
            AssertEqual(SamusEaterObjectDefinitions.ChewSound, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)), "plant chew sound");
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
                AssertEqual(NoobTubeObjectDefinitions.BreakSound, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)), "n00b tube break sound");
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
