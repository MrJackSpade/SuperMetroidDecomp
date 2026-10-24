using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Plms.BossRoomPlmNativeListAddresses;
using static SuperMetroid.Verification.Rooms.Plms.NativePlmListDefinitions;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>
/// #1296: the boss-room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native
/// list step by step: draws and durations, loop counts, fall-through, deletion, and the cue of every other instruction.
/// The families' runtime suites check the room mutations those programs make.
/// </summary>
internal static class BossRoomPlmProgramsVerification
{
    /// <summary>Botwoon's wall programs equal their native lists.</summary>
    internal static void VerifyBotwoonWallPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BotwoonWallPlmProgramDefinitions.Programs, BotwoonWallEntries,
            BotwoonWallDrawList, BotwoonWallCueAt, "Botwoon wall");
        Console.WriteLine($"  Botwoon wall programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The draw a Botwoon wall frame shows: the crumble frames are the shared single shot-block breakup.</summary>
    private static PlmDraw BotwoonWallDrawList(BotwoonWallDraw frame) => frame switch
    {
        BotwoonWallDraw.CrumbleFirst => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 0),
        BotwoonWallDraw.CrumbleSecond => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 1),
        BotwoonWallDraw.CrumbleThird => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 2),
        BotwoonWallDraw.CrumbleFourth => RoomPlmShotBlockDrawDefinitions.Breakup(RoomPlmShotBlockDrawDefinitions.Shape.Single, 3),
        BotwoonWallDraw.Clear => BotwoonWallPlmDrawDefinitions.Clear,
        _ => throw new InvalidOperationException($"Undefined {nameof(BotwoonWallDraw)} {frame}."),
    };

    /// <summary>The Kraid room programs equal their native lists.</summary>
    internal static void VerifyKraidRoomPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), KraidRoomPlmProgramDefinitions.Programs, KraidRoomEntries,
            draw => PlmDraw.Of(draw), KraidRoomCueAt, "Kraid room");
        Console.WriteLine($"  Kraid room programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Crocomire's arena programs equal their native lists.</summary>
    internal static void VerifyCrocomireArenaPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), CrocomireArenaPlmProgramDefinitions.Programs, CrocomireArenaEntries,
            draw => PlmDraw.Of(draw), NoCues<PlmNoCue>(), "Crocomire arena");
        Console.WriteLine($"  Crocomire arena programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Mother Brain's fake-death programs equal their native lists.</summary>
    internal static void VerifyMotherBrainFakeDeathPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), MotherBrainFakeDeathPlmProgramDefinitions.Programs,
            MotherBrainFakeDeathEntries, draw => PlmDraw.Of(draw), NoCues<PlmNoCue>(), "Mother Brain fake death");
        Console.WriteLine($"  Mother Brain fake-death programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Spore Spawn's ceiling programs equal their native lists.</summary>
    internal static void VerifySporeSpawnCeilingPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), SporeSpawnCeilingPlmProgramDefinitions.Programs, SporeSpawnCeilingEntries,
            draw => PlmDraw.Of(draw),
            (IImportCartridgeSource cartridge, ushort cursor, out SporeSpawnCeilingCue cue, out int length) =>
            {
                cue = SporeSpawnCeilingCue.QueueCrumbleSound;
                length = 3;
                if (InstructionAt<RoomPlmInstruction>(cartridge, cursor) != RoomPlmInstruction.QueueSoundLibrary2Maximum6)
                    return false;
                AssertEqual(SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId, ReadOperandByte(cartridge, cursor + 2), "Spore Spawn ceiling sound");
                AssertEqual((byte)6, SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundMaximumQueued, "Spore Spawn ceiling sound queue cap");
                return true;
            }, "Spore Spawn ceiling");
        Console.WriteLine($"  Spore Spawn ceiling programs: {steps} typed steps match bank $84.");
    }

    /// <summary>Draygon's cannon programs equal their native lists, each facing's links and branches staying on its side.</summary>
    internal static void VerifyDraygonCannonPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), DraygonCannonPlmProgramDefinitions.Programs, DraygonCannonEntries,
            draw => PlmDraw.Of(draw), DraygonCannonCueAt, "Draygon cannon");
        Console.WriteLine($"  Draygon cannon programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The cannon's link, missile-wake, hit-count and damage instructions with their operands.</summary>
    private static bool DraygonCannonCueAt(IImportCartridgeSource cartridge, ushort cursor, out DraygonCannonCue cue, out int length)
    {
        ushort leftStart = DraygonCannonEntries.Single(entry => entry.Program == DraygonCannonProgram.LeftStart).Entry;
        bool right = cursor < leftStart;
        DraygonCannonProgram Entry(ushort address) => DraygonCannonEntries.Single(entry => entry.Entry == address).Program;
        RoomPlmInstruction instruction = InstructionAt<RoomPlmInstruction>(cartridge, cursor);
        (bool known, cue, length) = instruction switch
        {
            RoomPlmInstruction.LinkInstruction => new DraygonCannonCueAtCase(true, DraygonCannonCue.LinkHit, 4),
            RoomPlmInstruction.InstallPreInstruction => new DraygonCannonCueAtCase(true, DraygonCannonCue.ArmMissileWake, 4),
            RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual => new DraygonCannonCueAtCase(true, DraygonCannonCue.CountHit, 5),
            RoomPlmInstruction.DamageDraygonCannonFacingRight or RoomPlmInstruction.DamageDraygonCannonFacingLeft => new DraygonCannonCueAtCase(true, DraygonCannonCue.Damage, 2),
            RoomPlmInstruction.SetGreyDoorPreInstruction or
                RoomPlmInstruction.Sleep or
                RoomPlmInstruction.Delete or
                RoomPlmInstruction.ClearPreInstruction or
                RoomPlmInstruction.GotoIfDoorBitSet or
                RoomPlmInstruction.IncrementDoorHitCounterAndGoto or
                RoomPlmInstruction.GotoIfSamusNear or
                RoomPlmInstruction.Goto or
                RoomPlmInstruction.DecrementTimerAndGoto or
                RoomPlmInstruction.SetEightBitTimer or
                RoomPlmInstruction.CopyFromRamToVram or
                RoomPlmInstruction.GotoIfAreaBossBitSet or
                RoomPlmInstruction.GotoIfEventSet or
                RoomPlmInstruction.SetEvent or
                RoomPlmInstruction.SetPlmBtsFromByte or
                RoomPlmInstruction.DrawPlmBlock or
                RoomPlmInstruction.DrawPlmBlockClone or
                RoomPlmInstruction.QueueSoundLibrary2Maximum6 or
                RoomPlmInstruction.QueueSoundLibrary3Maximum6 or
                RoomPlmInstruction.ClearDownwardGateTrigger or
                RoomPlmInstruction.SpawnDownwardGateProjectile or
                RoomPlmInstruction.WakeDownwardGateProjectile or
                RoomPlmInstruction.ShootEyeDoorProjectile or
                RoomPlmInstruction.SpawnEyeDoorSweat or
                RoomPlmInstruction.SpawnTwoEyeDoorSmoke or
                RoomPlmInstruction.SpawnEyeDoorSmoke or
                RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight or
                RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft or
                RoomPlmInstruction.QueueSoundLibrary2Maximum3 or
                RoomPlmInstruction.QueueSoundLibrary2Maximum1 or
                RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct or
                RoomPlmInstruction.GotoIfSamusHasNoBombs or
                RoomPlmInstruction.SetPlmBtsToOne or
                RoomPlmInstruction.GotoIfRoomArgumentLess or
                RoomPlmInstruction.SpawnFourMotherBrainGlassShards or
                RoomPlmInstruction.SpawnTorizoStatueBreaking or
                RoomPlmInstruction.QueueSongOneMusicTrack or
                RoomPlmInstruction.EnableNoobTubeWaterPhysics or
                RoomPlmInstruction.SpawnNoobTubeCrack or
                RoomPlmInstruction.TriggerNoobTubeEarthquake or
                RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles or
                RoomPlmInstruction.LockSamus or
                RoomPlmInstruction.UnlockSamus or
                RoomPlmInstruction.MoveRightFourBlocks or
                RoomPlmInstruction.SetAnimalsEscapedEvent or
                RoomPlmInstruction.SamusEaterDamage or
                RoomPlmInstruction.SamusEaterReleaseImmunity or
                RoomPlmInstruction.MoveTourianAccessDown or
                RoomPlmInstruction.TransformSpikesToSlopes or
                RoomPlmInstruction.RevertSlopesToSpikes or
                RoomPlmInstruction.SetLoweredAcidHeight => new DraygonCannonCueAtCase(false, default, 0),
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
                AssertEqual(DraygonCannonRomDataConstants.MissileHitPreInstruction, cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), "cannon missile wake");
                break;
            case DraygonCannonCue.CountHit:
                AssertEqual(DraygonCannonRomData.HitsToDestroy, ReadOperandByte(cartridge, cursor + 2), "cannon hits to destroy");
                AssertEqual(right ? DraygonCannonProgram.RightDestroy : DraygonCannonProgram.LeftDestroy,
                    Entry(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 3))), "cannon destroys its own side");
                break;
            case DraygonCannonCue.Damage:
                AssertEqual(right ? RoomPlmInstruction.DamageDraygonCannonFacingRight : RoomPlmInstruction.DamageDraygonCannonFacingLeft,
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
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), MotherBrainGlassPlmProgramDefinitions.Programs, MotherBrainGlassEntries,
            draw => PlmDraw.Of(draw), MotherBrainGlassCueAt, "Mother Brain glass");
        Console.WriteLine($"  Mother Brain glass programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The glass's branch, wake, hit-threshold, shard-burst and event instructions with their operands.</summary>
    private static bool MotherBrainGlassCueAt(IImportCartridgeSource cartridge, ushort cursor, [MaybeNullWhen(false)] out MotherBrainGlassPlmCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        MotherBrainGlassPlmProgram Entry(ushort address) => MotherBrainGlassEntries.Single(entry => entry.Entry == address).Program;
        RoomPlmInstruction instruction = InstructionAt<RoomPlmInstruction>(cartridge, cursor);
        switch (instruction)
        {
            case RoomPlmInstruction.GotoIfAreaBossBitSet:
                AssertEqual((byte)BossBits.AreaBoss, ReadOperandByte(cartridge, cursor + 2), "glass area-boss bit");
                AssertEqual(MotherBrainGlassPlmProgram.BossDead, Entry(Word(3)), "glass boss-dead target");
                cue = MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SkipIfBossDead);
                length = 5;
                return true;
            case RoomPlmInstruction.GotoIfEventSet:
                AssertEqual((ushort)EventNumber.MotherBrainGlassDestroyed, Word(2), "glass destroyed event");
                AssertEqual(MotherBrainGlassPlmProgram.NoGlass, Entry(Word(4)), "glass no-glass target");
                cue = MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SkipIfGlassDestroyed);
                length = 6;
                return true;
            case RoomPlmInstruction.InstallPreInstruction:
                AssertEqual(MotherBrainGlassPlmProgramDefinitionsConstants.HitPreInstruction, Word(2), "glass hit wake");
                cue = MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.ArmHitWake);
                length = 4;
                return true;
            case RoomPlmInstruction.GotoIfRoomArgumentLess:
                ushort stage = MotherBrainGlassEntries.Where(entry => entry.Entry <= cursor).Max(entry => entry.Entry);
                AssertEqual(stage, Word(4), $"glass stage at $84:{cursor:X4} repeats itself");
                cue = MotherBrainGlassPlmCue.RepeatUntil(Word(2));
                length = 6;
                return true;
            case RoomPlmInstruction.SpawnFourMotherBrainGlassShards:
                cue = MotherBrainGlassPlmCue.Burst((MotherBrainGlassShard)Word(2), (MotherBrainGlassShard)Word(4),
                    (MotherBrainGlassShard)Word(6), (MotherBrainGlassShard)Word(8));
                length = 10;
                return true;
            case RoomPlmInstruction.SetEvent:
                AssertEqual((ushort)EventNumber.MotherBrainGlassDestroyed, Word(2), "glass sets its destroyed event");
                cue = MotherBrainGlassPlmCue.Of(MotherBrainGlassPlmCueKind.SetGlassDestroyed);
                length = 4;
                return true;
            case RoomPlmInstruction.SetGreyDoorPreInstruction:
            case RoomPlmInstruction.Sleep:
            case RoomPlmInstruction.Delete:
            case RoomPlmInstruction.ClearPreInstruction:
            case RoomPlmInstruction.LinkInstruction:
            case RoomPlmInstruction.GotoIfDoorBitSet:
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
            case RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomPlmInstruction.GotoIfSamusNear:
            case RoomPlmInstruction.Goto:
            case RoomPlmInstruction.DecrementTimerAndGoto:
            case RoomPlmInstruction.SetEightBitTimer:
            case RoomPlmInstruction.CopyFromRamToVram:
            case RoomPlmInstruction.SetPlmBtsFromByte:
            case RoomPlmInstruction.DrawPlmBlock:
            case RoomPlmInstruction.DrawPlmBlockClone:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
            case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
            case RoomPlmInstruction.ClearDownwardGateTrigger:
            case RoomPlmInstruction.SpawnDownwardGateProjectile:
            case RoomPlmInstruction.WakeDownwardGateProjectile:
            case RoomPlmInstruction.ShootEyeDoorProjectile:
            case RoomPlmInstruction.SpawnEyeDoorSweat:
            case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
            case RoomPlmInstruction.SpawnEyeDoorSmoke:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomPlmInstruction.DamageDraygonCannonFacingRight:
            case RoomPlmInstruction.DamageDraygonCannonFacingLeft:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomPlmInstruction.GotoIfSamusHasNoBombs:
            case RoomPlmInstruction.SetPlmBtsToOne:
            case RoomPlmInstruction.SpawnTorizoStatueBreaking:
            case RoomPlmInstruction.QueueSongOneMusicTrack:
            case RoomPlmInstruction.EnableNoobTubeWaterPhysics:
            case RoomPlmInstruction.SpawnNoobTubeCrack:
            case RoomPlmInstruction.TriggerNoobTubeEarthquake:
            case RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomPlmInstruction.LockSamus:
            case RoomPlmInstruction.UnlockSamus:
            case RoomPlmInstruction.MoveRightFourBlocks:
            case RoomPlmInstruction.SetAnimalsEscapedEvent:
            case RoomPlmInstruction.SamusEaterDamage:
            case RoomPlmInstruction.SamusEaterReleaseImmunity:
            case RoomPlmInstruction.MoveTourianAccessDown:
            case RoomPlmInstruction.TransformSpikesToSlopes:
            case RoomPlmInstruction.RevertSlopesToSpikes:
            case RoomPlmInstruction.SetLoweredAcidHeight:
            default:
                cue = default;
                length = 0;
                return false;
        }
    }

    /// <summary>The Bomb Torizo hand's programs equal its native list.</summary>
    internal static void VerifyBombTorizoHandPrograms(ISnesAddressSpace rom)
    {
        int steps = CompareProgramSet(CartridgeImportSource.Require(rom), BombTorizoHandPlmProgramDefinitions.Programs, BombTorizoHandEntries,
            draw => PlmDraw.Of(draw), BombTorizoHandCueAt, "Bomb Torizo hand");
        AssertEqual(BombTorizoHandProgram.Crumble, BombTorizoHandPlmProgramDefinitions.ProgramAfterSleep(BombTorizoHandProgram.Sleeping),
            "the woken hand continues after its sleep");
        AssertEqual(unchecked((ushort)(BombTorizoHandEntries[0].Entry + 10)), BombTorizoHandEntries[1].Entry,
            "the crumble list follows the sleeping list's sleep");
        Console.WriteLine($"  Bomb Torizo hand programs: {steps} typed steps match bank $84.");
    }

    /// <summary>The hand's wake, fragment and music instructions with their operands.</summary>
    private static bool BombTorizoHandCueAt(IImportCartridgeSource cartridge, ushort cursor, out BombTorizoHandCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.InstallPreInstruction:
                AssertEqual(BombTorizoHandPlmProgramDefinitionsConstants.WakeIfSamusHasBombsPreInstruction, Word(2), "hand bomb wake");
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.ArmBombWake);
                length = 4;
                return true;
            case RoomPlmInstruction.SpawnTorizoStatueBreaking:
                AssertEqual(0, Word(2) % sizeof(ushort), "hand fragment operand is a two-byte table offset");
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.SpawnFragment, Word(2) / sizeof(ushort));
                length = 4;
                return true;
            case RoomPlmInstruction.QueueSongOneMusicTrack:
                cue = new BombTorizoHandCue(BombTorizoHandCueKind.QueueMusic);
                length = 2;
                return true;
            case RoomPlmInstruction.SetGreyDoorPreInstruction:
            case RoomPlmInstruction.CopyFromRamToVram:
            case RoomPlmInstruction.Sleep:
            case RoomPlmInstruction.Delete:
            case RoomPlmInstruction.ClearPreInstruction:
            case RoomPlmInstruction.LinkInstruction:
            case RoomPlmInstruction.GotoIfDoorBitSet:
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
            case RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomPlmInstruction.GotoIfSamusNear:
            case RoomPlmInstruction.Goto:
            case RoomPlmInstruction.DecrementTimerAndGoto:
            case RoomPlmInstruction.SetEightBitTimer:
            case RoomPlmInstruction.GotoIfAreaBossBitSet:
            case RoomPlmInstruction.GotoIfEventSet:
            case RoomPlmInstruction.SetEvent:
            case RoomPlmInstruction.SetPlmBtsFromByte:
            case RoomPlmInstruction.DrawPlmBlock:
            case RoomPlmInstruction.DrawPlmBlockClone:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
            case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
            case RoomPlmInstruction.ClearDownwardGateTrigger:
            case RoomPlmInstruction.SpawnDownwardGateProjectile:
            case RoomPlmInstruction.WakeDownwardGateProjectile:
            case RoomPlmInstruction.ShootEyeDoorProjectile:
            case RoomPlmInstruction.SpawnEyeDoorSweat:
            case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
            case RoomPlmInstruction.SpawnEyeDoorSmoke:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomPlmInstruction.DamageDraygonCannonFacingRight:
            case RoomPlmInstruction.DamageDraygonCannonFacingLeft:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomPlmInstruction.GotoIfSamusHasNoBombs:
            case RoomPlmInstruction.SetPlmBtsToOne:
            case RoomPlmInstruction.GotoIfRoomArgumentLess:
            case RoomPlmInstruction.SpawnFourMotherBrainGlassShards:
            case RoomPlmInstruction.EnableNoobTubeWaterPhysics:
            case RoomPlmInstruction.SpawnNoobTubeCrack:
            case RoomPlmInstruction.TriggerNoobTubeEarthquake:
            case RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomPlmInstruction.LockSamus:
            case RoomPlmInstruction.UnlockSamus:
            case RoomPlmInstruction.MoveRightFourBlocks:
            case RoomPlmInstruction.SetAnimalsEscapedEvent:
            case RoomPlmInstruction.SamusEaterDamage:
            case RoomPlmInstruction.SamusEaterReleaseImmunity:
            case RoomPlmInstruction.MoveTourianAccessDown:
            case RoomPlmInstruction.TransformSpikesToSlopes:
            case RoomPlmInstruction.RevertSlopesToSpikes:
            case RoomPlmInstruction.SetLoweredAcidHeight:
            default:
                cue = default;
                length = 0;
                return false;
        }
    }

    /// <summary>Botwoon's scroll, crumble-sound and move-down instructions.</summary>
    private static bool BotwoonWallCueAt(IImportCartridgeSource cartridge, ushort cursor, out BotwoonWallCue cue, out int length)
    {
        if (InstructionAt<RoomPlmInstruction>(cartridge, cursor) == RoomPlmInstruction.QueueSoundLibrary2Maximum6)
        {
            AssertEqual(BotwoonWallPlmProgramDefinitions.CrumbleSound,
                SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, ReadOperandByte(cartridge, cursor + 2)),
                "Botwoon wall crumble sound");
            AssertEqual((byte)6, BotwoonWallPlmProgramDefinitions.CrumbleSoundMaximumQueued, "Botwoon wall crumble sound queue cap");
            cue = BotwoonWallCue.QueueCrumbleSound;
            length = 3;
            return true;
        }
        BossRoomPlmNativeRoutine routine = InstructionAt<BossRoomPlmNativeRoutine>(cartridge, cursor);
        cue = routine == BossRoomPlmNativeRoutine.BotwoonWallMoveDown ? BotwoonWallCue.MoveDownOneBlock : BotwoonWallCue.SetScrollsBlue;
        length = 2;
        return routine is BossRoomPlmNativeRoutine.BotwoonWallScrollsBlue or BossRoomPlmNativeRoutine.BotwoonWallMoveDown;
    }

    /// <summary>The Kraid room's move-right instruction.</summary>
    private static bool KraidRoomCueAt(IImportCartridgeSource cartridge, ushort cursor, out KraidRoomCue cue, out int length)
    {
        cue = KraidRoomCue.MoveRightOneBlock;
        length = 2;
        return InstructionAt<BossRoomPlmNativeRoutine>(cartridge, cursor) == BossRoomPlmNativeRoutine.KraidRoomMoveRight;
    }
}
