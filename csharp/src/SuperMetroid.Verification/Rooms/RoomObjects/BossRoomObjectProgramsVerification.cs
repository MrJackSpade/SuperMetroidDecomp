using SuperMetroid.AssetExtraction.Audio;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.BossRoomObjectNativeListAddresses;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// #1296: the boss-room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native
/// list step by step: draws and durations, loop counts, fall-through, deletion, and the cue of every other instruction.
/// The families' runtime suites check the room mutations those programs make.
/// </summary>
internal static class BossRoomObjectProgramsVerification
{
    /// <summary>Botwoon's wall programs equal their native lists.</summary>
    internal static void VerifyBotwoonWallPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BotwoonWallObjectProgramDefinitions.Programs, BotwoonWallEntries,
            BotwoonWallDrawList, BotwoonWallCueAt, "Botwoon wall");
        Console.WriteLine($"  Botwoon wall programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw a Botwoon wall frame shows: the crumble frames are the shared single shot-block breakup.</summary>
    private static RoomObjectDraw BotwoonWallDrawList(BotwoonWallDraw frame) => frame switch
    {
        BotwoonWallDraw.CrumbleFirst => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0),
        BotwoonWallDraw.CrumbleSecond => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 1),
        BotwoonWallDraw.CrumbleThird => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 2),
        BotwoonWallDraw.CrumbleFourth => ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 3),
        BotwoonWallDraw.Clear => BotwoonWallObjectDrawDefinitions.Clear,
        _ => throw new InvalidOperationException($"Undefined {nameof(BotwoonWallDraw)} {frame}."),
    };

    /// <summary>The Kraid room programs equal their native lists.</summary>
    internal static void VerifyKraidRoomPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), KraidRoomObjectProgramDefinitions.Programs, KraidRoomEntries,
            draw => RoomObjectDraw.Of(draw), KraidRoomCueAt, "Kraid room");
        Console.WriteLine($"  Kraid room programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Crocomire's arena programs equal their native lists.</summary>
    internal static void VerifyCrocomireArenaPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), CrocomireArenaObjectProgramDefinitions.Programs, CrocomireArenaEntries,
            draw => RoomObjectDraw.Of(draw), NoCues<RoomObjectNoCue>(), "Crocomire arena");
        Console.WriteLine($"  Crocomire arena programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Mother Brain's fake-death programs equal their native lists.</summary>
    internal static void VerifyMotherBrainFakeDeathPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), MotherBrainFakeDeathObjectProgramDefinitions.Programs,
            MotherBrainFakeDeathEntries, draw => RoomObjectDraw.Of(draw), NoCues<RoomObjectNoCue>(), "Mother Brain fake death");
        Console.WriteLine($"  Mother Brain fake-death programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Spore Spawn's ceiling programs equal their native lists.</summary>
    internal static void VerifySporeSpawnCeilingPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SporeSpawnCeilingObjectProgramDefinitions.Programs, SporeSpawnCeilingEntries,
            draw => RoomObjectDraw.Of(draw),
            (IImportCartridgeSource cartridge, ushort cursor, out SporeSpawnCeilingCue cue, out int length) =>
            {
                cue = SporeSpawnCeilingCue.QueueCrumbleSound;
                length = 3;
                if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) != RoomObjectInstruction.QueueSoundLibrary2Maximum6)
                    return false;
                AssertEqual(SporeSpawnCeilingObjectProgramDefinitions.CrumbleSoundId, SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)), "Spore Spawn ceiling sound");
                AssertEqual(SoundRequestPriority.Normal, SporeSpawnCeilingObjectProgramDefinitions.CrumbleSoundPriority, "Spore Spawn ceiling sound priority");
                return true;
            }, "Spore Spawn ceiling");
        Console.WriteLine($"  Spore Spawn ceiling programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Draygon's cannon programs equal their native lists, each facing's links and branches staying on its side.</summary>
    internal static void VerifyDraygonCannonPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), DraygonCannonObjectProgramDefinitions.Programs, DraygonCannonEntries,
            draw => RoomObjectDraw.Of(draw), DraygonCannonCueAt, "Draygon cannon");
        Console.WriteLine($"  Draygon cannon programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The cannon's link, missile-wake, hit-count and damage instructions with their operands.</summary>
    private static bool DraygonCannonCueAt(IImportCartridgeSource cartridge, ushort cursor, out DraygonCannonCue cue, out int length)
    {
        ushort leftStart = DraygonCannonEntries.Single(entry => entry.Program == DraygonCannonProgram.LeftStart).Entry;
        bool right = cursor < leftStart;
        DraygonCannonProgram Entry(ushort address) => DraygonCannonEntries.Single(entry => entry.Entry == address).Program;
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        (bool known, cue, length) = instruction switch
        {
            RoomObjectInstruction.LinkInstruction => new DraygonCannonCueAtCase(true, DraygonCannonCue.LinkHit, 4),
            RoomObjectInstruction.InstallFrameBehavior => new DraygonCannonCueAtCase(true, DraygonCannonCue.ArmMissileWake, 4),
            RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual => new DraygonCannonCueAtCase(true, DraygonCannonCue.CountHit, 5),
            RoomObjectInstruction.DamageDraygonCannonFacingRight or RoomObjectInstruction.DamageDraygonCannonFacingLeft => new DraygonCannonCueAtCase(true, DraygonCannonCue.Damage, 2),
            RoomObjectInstruction.SetGreyDoorFrameBehavior or
                RoomObjectInstruction.Sleep or
                RoomObjectInstruction.Delete or
                RoomObjectInstruction.ClearFrameBehavior or
                RoomObjectInstruction.GotoIfDoorBitSet or
                RoomObjectInstruction.IncrementDoorHitCounterAndGoto or
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
                RoomObjectInstruction.SetLoweredAcidHeight => new DraygonCannonCueAtCase(false, default, 0),
            _ => new DraygonCannonCueAtCase(false, default, 0),
        };
        if (!known)
            return false;
        switch (cue)
        {
            case DraygonCannonCue.LinkHit:
                AssertEqual(right ? DraygonCannonProgram.RightHit : DraygonCannonProgram.LeftHit,
                    Entry(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2))), "cannon links its own hit program");
                break;
            case DraygonCannonCue.ArmMissileWake:
                AssertEqual(DraygonNativeWords.MissileHitFrameBehavior, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "cannon missile wake");
                break;
            case DraygonCannonCue.CountHit:
                AssertEqual(DraygonCannonDefinitions.HitsToDestroy, ReadOperandByte(cartridge, cursor + 2), "cannon hits to destroy");
                AssertEqual(right ? DraygonCannonProgram.RightDestroy : DraygonCannonProgram.LeftDestroy,
                    Entry(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 3))), "cannon destroys its own side");
                break;
            case DraygonCannonCue.Damage:
                AssertEqual(right ? RoomObjectInstruction.DamageDraygonCannonFacingRight : RoomObjectInstruction.DamageDraygonCannonFacingLeft,
                    instruction, "cannon damage faces its own side");
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DraygonCannonCue)} {cue}.");
        }
        return true;
    }

    /// <summary>Mother Brain's glass programs equal their native lists, every stage repeating itself until its hits.</summary>
    internal static void VerifyMotherBrainGlassPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), MotherBrainGlassObjectProgramDefinitions.Programs, MotherBrainGlassEntries,
            draw => RoomObjectDraw.Of(draw), MotherBrainGlassCueAt, "Mother Brain glass");
        Console.WriteLine($"  Mother Brain glass programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The glass's branch, wake, hit-threshold, shard-burst and event instructions with their operands.</summary>
    private static bool MotherBrainGlassCueAt(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out MotherBrainGlassObjectCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        MotherBrainGlassObjectProgram Entry(ushort address) => MotherBrainGlassEntries.Single(entry => entry.Entry == address).Program;
        MotherBrainGlassShard GlassShard(ushort word) => MotherBrainNativeWords.GlassShards.Decode(word, "glass shard spawn parameter");
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        switch (instruction)
        {
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
                AssertEqual((byte)BossBits.AreaBoss, ReadOperandByte(cartridge, cursor + 2), "glass area-boss bit");
                AssertEqual(MotherBrainGlassObjectProgram.BossDead, Entry(Word(3)), "glass boss-dead target");
                cue = MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SkipIfBossDead);
                length = 5;
                return true;
            case RoomObjectInstruction.GotoIfEventSet:
                AssertEqual((ushort)EventNumber.MotherBrainGlassDestroyed.ToEventNumber(), Word(2), "glass destroyed event");
                AssertEqual(MotherBrainGlassObjectProgram.NoGlass, Entry(Word(4)), "glass no-glass target");
                cue = MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SkipIfGlassDestroyed);
                length = 6;
                return true;
            case RoomObjectInstruction.InstallFrameBehavior:
                AssertEqual(MotherBrainGlassObjectProgramDefinitionsConstants.HitFrameBehavior, Word(2), "glass hit wake");
                cue = MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.ArmHitWake);
                length = 4;
                return true;
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
                ushort stage = MotherBrainGlassEntries.Where(entry => entry.Entry <= cursor).Max(entry => entry.Entry);
                AssertEqual(stage, Word(4), $"glass stage at $84:{cursor:X4} repeats itself");
                cue = MotherBrainGlassObjectCue.RepeatUntil(Word(2));
                length = 6;
                return true;
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
                cue = MotherBrainGlassObjectCue.Burst(GlassShard(Word(2)), GlassShard(Word(4)), GlassShard(Word(6)), GlassShard(Word(8)));
                length = 10;
                return true;
            case RoomObjectInstruction.SetEvent:
                AssertEqual((ushort)EventNumber.MotherBrainGlassDestroyed.ToEventNumber(), Word(2), "glass sets its destroyed event");
                cue = MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SetGlassDestroyed);
                length = 4;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
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
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
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
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
            case RoomObjectInstruction.SetObjectBtsToOne:
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

    /// <summary>The Bomb Torizo hand's programs equal its native list.</summary>
    internal static void VerifyBombTorizoHandPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BombTorizoHandObjectProgramDefinitions.Programs, BombTorizoHandEntries,
            draw => RoomObjectDraw.Of(draw), BombTorizoHandCueAt, "Bomb Torizo hand");
        AssertEqual(BombTorizoHandProgram.Crumble, BombTorizoHandObjectProgramDefinitions.ProgramsAfterSleep[BombTorizoHandProgram.Sleeping],
            "the woken hand continues after its sleep");
        AssertEqual(unchecked((ushort)(BombTorizoHandEntries[0].Entry + 10)), BombTorizoHandEntries[1].Entry,
            "the crumble list follows the sleeping list's sleep");
        Console.WriteLine($"  Bomb Torizo hand programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The hand's wake, fragment and music instructions with their operands.</summary>
    private static bool BombTorizoHandCueAt(IImportCartridgeSource cartridge, ushort cursor, out BombTorizoHandCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.InstallFrameBehavior:
                AssertEqual(BombTorizoHandObjectProgramDefinitionsConstants.WakeIfSamusHasBombsFrameBehavior, Word(2), "hand bomb wake");
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.ArmBombWake);
                length = 4;
                return true;
            case RoomObjectInstruction.SpawnTorizoStatueBreaking:
                AssertEqual(0, Word(2) % sizeof(ushort), "hand fragment operand is a two-byte table offset");
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, Word(2) / sizeof(ushort));
                length = 4;
                return true;
            case RoomObjectInstruction.QueueSongOneMusicTrack:
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.QueueMusic);
                length = 2;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
            case RoomObjectInstruction.CopyFromRamToVram:
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
            case RoomObjectInstruction.ClearFrameBehavior:
            case RoomObjectInstruction.LinkInstruction:
            case RoomObjectInstruction.GotoIfDoorBitSet:
            case RoomObjectInstruction.IncrementDoorHitCounterAndGoto:
            case RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomObjectInstruction.GotoIfSamusNear:
            case RoomObjectInstruction.Goto:
            case RoomObjectInstruction.DecrementTimerAndGoto:
            case RoomObjectInstruction.SetEightBitTimer:
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
            case RoomObjectInstruction.GotoIfEventSet:
            case RoomObjectInstruction.SetEvent:
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
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
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
            case RoomObjectInstruction.SetObjectBtsToOne:
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
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

    /// <summary>Botwoon's scroll, crumble-sound and move-down instructions.</summary>
    private static bool BotwoonWallCueAt(IImportCartridgeSource cartridge, ushort cursor, out BotwoonWallCue cue, out int length)
    {
        if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) == RoomObjectInstruction.QueueSoundLibrary2Maximum6)
        {
            AssertEqual(BotwoonWallObjectProgramDefinitions.CrumbleSound,
                SoundEffectCommands.Decode(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)),
                "Botwoon wall crumble sound");
            AssertEqual(SoundRequestPriority.Normal, BotwoonWallObjectProgramDefinitions.CrumbleSoundPriority, "Botwoon wall crumble sound priority");
            cue = BotwoonWallCue.QueueCrumbleSound;
            length = 3;
            return true;
        }
        BossRoomObjectNativeRoutine routine = InstructionAt<BossRoomObjectNativeRoutine>(cartridge, cursor);
        cue = routine == BossRoomObjectNativeRoutine.BotwoonWallMoveDown ? BotwoonWallCue.MoveDownOneBlock : BotwoonWallCue.SetScrollsBlue;
        length = 2;
        return routine is BossRoomObjectNativeRoutine.BotwoonWallScrollsBlue or BossRoomObjectNativeRoutine.BotwoonWallMoveDown;
    }

    /// <summary>The Kraid room's move-right instruction.</summary>
    private static bool KraidRoomCueAt(IImportCartridgeSource cartridge, ushort cursor, out KraidRoomCue cue, out int length)
    {
        cue = KraidRoomCue.MoveRightOneBlock;
        length = 2;
        return InstructionAt<BossRoomObjectNativeRoutine>(cartridge, cursor) == BossRoomObjectNativeRoutine.KraidRoomMoveRight;
    }
}
