using SuperMetroid.AssetExtraction.Progression;
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
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Rooms.RoomObjects.BossRoomObjectNativeListAddresses;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// The boss-room room objects beside their bank-$84 lists. Each terrain change (#1554) must play as its native list
/// does, frame by frame: its draws and how long each stays, the moves and sounds between them, and when it ends. Each
/// program typed by #1296 must equal its native list step by step. The families' runtime suites check the room changes
/// these make.
/// </summary>
internal static class BossRoomObjectProgramsVerification
{
    /// <summary>Botwoon's wall changes play as their native lists do; only the crumble opens the scrolls as it starts.</summary>
    internal static void VerifyBotwoonWallPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int frames = 0;
        foreach ((BotwoonWallMutation mutation, ushort entry) in BotwoonWallEntries)
        {
            TerrainChange change = mutation == BotwoonWallMutation.Crumble ? BotwoonWallMutationDefinitions.Crumble : BotwoonWallMutationDefinitions.Clear;
            frames += NativeTerrainChanges.Compare(cartridge, change, entry, BotwoonWallStepAt, $"Botwoon wall {mutation}", out bool began);
            AssertEqual(mutation == BotwoonWallMutation.Crumble, began, $"Botwoon wall {mutation} opens the scrolls exactly when it crumbles");
        }
        AssertEqual(Enum.GetValues<BotwoonWallMutation>().Length, BotwoonWallEntries.Count, "every Botwoon wall mutation has a native list");
        Console.WriteLine($"  Botwoon wall changes: {frames} typed frames match bank $84.");
    }

    /// <summary>The Kraid room's changes play as their native lists do.</summary>
    internal static void VerifyKraidRoomPrograms(ISnesAddressSpace rom) =>
        CompareChanges(rom, KraidRoomEntries, KraidRoomMutationDefinitions.Changes, KraidRoomStepAt, "Kraid room");

    /// <summary>Crocomire's arena changes play as their native lists do.</summary>
    internal static void VerifyCrocomireArenaPrograms(ISnesAddressSpace rom) =>
        CompareChanges(rom, CrocomireArenaEntries, CrocomireArenaMutationDefinitions.Changes, NativeTerrainChanges.NoSteps, "Crocomire arena");

    /// <summary>Mother Brain's fake-death changes play as their native lists do.</summary>
    internal static void VerifyMotherBrainFakeDeathPrograms(ISnesAddressSpace rom) =>
        CompareChanges(rom, MotherBrainFakeDeathEntries, MotherBrainRoomMutationDefinitions.Changes, NativeTerrainChanges.NoSteps, "Mother Brain fake death");

    /// <summary>Spore Spawn's ceiling changes play as their native lists do.</summary>
    internal static void VerifySporeSpawnCeilingPrograms(ISnesAddressSpace rom) =>
        CompareChanges(rom, SporeSpawnCeilingEntries, SporeSpawnCeilingMutationDefinitions.Changes,
            (IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step) =>
                TryCrumbleSound(cartridge, cursor, SporeSpawnCeilingMutationDefinitions.CrumbleSoundId, "Spore Spawn ceiling", out step),
            "Spore Spawn ceiling");

    /// <summary>
    /// Plays every mutation's native list beside its typed change; every mutation has a list and none of these changes
    /// opens anything as it starts.
    /// </summary>
    private static void CompareChanges<TMutation>(ISnesAddressSpace rom, IReadOnlyList<NativeProgramEntry<TMutation>> entries,
        IReadOnlyDictionary<TMutation, TerrainChange> changes, NativeTerrainChanges.StepReader steps, string family)
        where TMutation : struct, Enum
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TMutation>().Length, entries.Count, $"every {family} mutation has a native list");
        AssertEqual(entries.Count, changes.Count, $"every {family} mutation has a typed change");
        int frames = 0;
        foreach ((TMutation mutation, ushort entry) in entries)
        {
            frames += NativeTerrainChanges.Compare(cartridge, changes[mutation], entry, steps, $"{family} {mutation}", out bool began);
            AssertTrue(!began, $"{family} {mutation} opens nothing as it starts");
        }
        Console.WriteLine($"  {family} changes: {frames} typed frames match bank $84.");
    }

    /// <summary>A queued crumble sound of the enemies' library at normal priority, which comes with the next frame.</summary>
    private static bool TryCrumbleSound(IImportCartridgeSource cartridge, ushort cursor, SoundEffectId expected, string family,
        out NativeTerrainChanges.NativeStep step)
    {
        step = default;
        if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) != RoomObjectInstruction.QueueSoundLibrary2Maximum6)
            return false;
        SoundEffectId sound = SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2));
        AssertEqual(expected, sound, $"{family} crumble sound");
        step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero, sound, Begins: false, Length: 3);
        return true;
    }

    /// <summary>
    /// Draygon's cannons play as their native lists do (#1554), each facing's links and branches staying on its side: a
    /// whole cannon links its hit, arms the missile wake and rests on its shield; a hit counts to the destroying third,
    /// flashes the shield and returns to it; the destroying hit or a destroyed setup destroys its own side and smokes for
    /// good. Every image and how long it stays is compared with <see cref="DraygonCannonAnimationDefinitions"/>.
    /// </summary>
    internal static void VerifyDraygonCannonPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        int frames = 0;
        foreach ((RoomObjectHeaderId header, DraygonCannonSetup setup) in DraygonCannonDefinitions.Setups)
        {
            DraygonCannonLook look = DraygonCannonAnimationDefinitions.Looks[setup.Orientation];
            string at = $"Draygon cannon {header}";
            int entry = Word(RoomObjectNativeWords.Word(header) + 2);
            int destroy = entry;
            if (setup.Start == DraygonCannonStart.Whole)
            {
                AssertEqual(RoomObjectInstruction.LinkInstruction, At(entry), $"{at} first links its hit");
                int hit = Word(entry + 2);
                AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(entry + 4), $"{at} then arms its missile wake");
                AssertEqual(DraygonNativeWords.MissileHitFrameBehavior, Word(entry + 6), $"{at} missile wake");
                int idle = entry + 8;
                frames += CompareCannonFrames(cartridge, idle, [look.Shield], out int afterShield, $"{at} shield");
                AssertEqual(RoomObjectInstruction.Sleep, At(afterShield), $"{at} rests on its shield");
                AssertEqual(RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual, At(hit), $"{at} hit is counted first");
                AssertEqual(DraygonCannonDefinitions.HitsToDestroy, ReadOperandByte(cartridge, hit + 2), $"{at} hits to destroy");
                destroy = Word(hit + 3);
                frames += CompareCannonFrames(cartridge, hit + 5, look.HitFlash, out int afterFlash, $"{at} hit flash");
                AssertEqual(RoomObjectInstruction.Goto, At(afterFlash), $"{at} returns after its flash");
                AssertEqual(idle, (int)Word(afterFlash + 2), $"{at} returns to its shield");
            }
            AssertEqual(setup.Orientation == DraygonCannonOrientation.Right
                    ? RoomObjectInstruction.DamageDraygonCannonFacingRight
                    : RoomObjectInstruction.DamageDraygonCannonFacingLeft,
                At(destroy), $"{at} destroys its own side");
            int smoke = destroy + 2;
            frames += CompareCannonFrames(cartridge, smoke, look.Smoke, out int afterSmoke, $"{at} smoke");
            AssertEqual(RoomObjectInstruction.Goto, At(afterSmoke), $"{at} smoke repeats");
            AssertEqual(smoke, (int)Word(afterSmoke + 2), $"{at} smoke loops to its first image");
        }
        Console.WriteLine($"  Draygon cannons: {frames} typed images match bank $84.");
    }

    /// <summary>Compares the native draws from <paramref name="cursor"/> with <paramref name="expected"/>, image for image.</summary>
    private static int CompareCannonFrames(IImportCartridgeSource cartridge, int cursor, IReadOnlyList<TerrainChangeFrame> expected,
        out int after, string at)
    {
        for (int index = 0; index < expected.Count; index++, cursor += 4)
        {
            int updates = cartridge.ReadCartridgeWord(ProgramBank | cursor);
            AssertTrue(updates < 0x8000, $"{at} image {index} is a draw");
            AssertEqual(expected[index].Updates, updates, $"{at} image {index} updates");
            AssertEqual((int)SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(expected[index].Draw),
                (int)cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), $"{at} image {index} draw");
        }
        after = cursor;
        return expected.Count;
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
                AssertEqual(ProgressionTableCodec.EncodeBossMask(BossBits.AreaBoss), ReadOperandByte(cartridge, cursor + 2), "glass area-boss bit");
                AssertEqual(MotherBrainGlassObjectProgram.BossDead, Entry(Word(3)), "glass boss-dead target");
                cue = MotherBrainGlassObjectCue.Of(MotherBrainGlassObjectCueKind.SkipIfBossDead);
                length = 5;
                return true;
            case RoomObjectInstruction.GotoIfEventSet:
                AssertEqual((ushort)StoryEvent.MotherBrainGlassDestroyed.Number(), Word(2), "glass destroyed event");
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
                AssertEqual((ushort)StoryEvent.MotherBrainGlassDestroyed.Number(), Word(2), "glass sets its destroyed event");
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

    /// <summary>Botwoon's crumble sound, its opening of the scrolls and its move down a block.</summary>
    private static bool BotwoonWallStepAt(IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step)
    {
        if (TryCrumbleSound(cartridge, cursor, BotwoonWallMutationDefinitions.CrumbleSound, "Botwoon wall", out step))
            return true;
        BossRoomObjectNativeRoutine routine = InstructionAt<BossRoomObjectNativeRoutine>(cartridge, cursor);
        if (routine == BossRoomObjectNativeRoutine.BotwoonWallScrollsBlue)
        {
            step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero, Sound: null, Begins: true, Length: 2);
            return true;
        }
        if (routine == BossRoomObjectNativeRoutine.BotwoonWallMoveDown)
        {
            step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(0, 1), Sound: null, Begins: false, Length: 2);
            return true;
        }
        return false;
    }

    /// <summary>The Kraid room's move right a block.</summary>
    private static bool KraidRoomStepAt(IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step)
    {
        step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(1, 0), Sound: null, Begins: false, Length: 2);
        return InstructionAt<BossRoomObjectNativeRoutine>(cartridge, cursor) == BossRoomObjectNativeRoutine.KraidRoomMoveRight;
    }
}
