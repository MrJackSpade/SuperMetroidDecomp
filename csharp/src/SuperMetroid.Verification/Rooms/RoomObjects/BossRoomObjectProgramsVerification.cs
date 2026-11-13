using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Phantoon;
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

    /// <summary>Phantoon's door changes play as their native lists do: each draws its frame for one update and ends (#1678).</summary>
    internal static void VerifyPhantoonDoorPrograms(ISnesAddressSpace rom) =>
        CompareChanges(rom, PhantoonDoorEntries, PhantoonDoorChangeDefinitions.Changes, NativeTerrainChanges.NoSteps, "Phantoon door");

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
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(expected[index].Draw),
                (int)cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)), $"{at} image {index} draw");
        }
        after = cursor;
        return expected.Count;
    }

    /// <summary>
    /// Mother Brain's glass plays as its native lists do (#1554): its entry shows the cleared glass once the boss is dead
    /// and the shattered glass once it was destroyed, each for one update before it ends, and otherwise arms its hit
    /// count and runs on into its stages. Each stage shows its image for one update and repeats itself until its hits;
    /// a stage with a burst throws its first shards, shows the next image for its burst updates and throws its second;
    /// the last shows the shattered glass, sets the destroyed event and ends. Every image, hold, threshold and shard is
    /// compared with <see cref="MotherBrainGlassStageDefinitions"/>.
    /// </summary>
    internal static void VerifyMotherBrainGlassPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, MotherBrainGlassDraw image, int updates, string at)
        {
            AssertEqual(updates, (int)Word(cursor), $"{at} updates");
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image)), (int)Word(cursor + 2), $"{at} image");
        }
        void AssertBurst(int cursor, IReadOnlyList<MotherBrainGlassShard> shards, string at)
        {
            AssertEqual(RoomObjectInstruction.SpawnFourMotherBrainGlassShards, At(cursor), $"{at} throws shards");
            for (int shard = 0; shard < 4; shard++)
                AssertEqual(shards[shard], MotherBrainNativeWords.GlassShards.Decode(Word(cursor + 2 + shard * 2), "glass shard spawn parameter"),
                    $"{at} shard {shard}");
        }

        int entry = Word(RoomObjectNativeWords.Word(RoomObjectHeaderId.MotherBrainGlass) + 2);
        AssertEqual(RoomObjectInstruction.GotoIfAreaBossBitSet, At(entry), "glass first checks Mother Brain");
        AssertEqual(ProgressionTableCodec.EncodeBossMask(BossBits.AreaBoss), ReadOperandByte(cartridge, entry + 2), "glass area-boss bit");
        int bossDead = Word(entry + 3);
        AssertEqual(RoomObjectInstruction.GotoIfEventSet, At(entry + 5), "glass then checks its destroyed event");
        AssertEqual((ushort)StoryEvent.MotherBrainGlassDestroyed.Number(), Word(entry + 7), "glass destroyed event");
        int noGlass = Word(entry + 9);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(entry + 11), "glass then arms its hit count");
        AssertEqual(MotherBrainGlassObjectProgramDefinitionsConstants.HitFrameBehavior, Word(entry + 13), "glass hit count");
        AssertShown(bossDead, MotherBrainGlassStageDefinitions.ClearedImage, 1, "cleared glass");
        AssertEqual(RoomObjectInstruction.Delete, At(bossDead + 4), "cleared glass ends");
        AssertShown(noGlass, MotherBrainGlassStageDefinitions.ShatteredImage, 1, "already shattered glass");
        AssertEqual(RoomObjectInstruction.Delete, At(noGlass + 4), "already shattered glass ends");

        int cursor = entry + 15;
        IReadOnlyList<MotherBrainGlassStage> stages = MotherBrainGlassStageDefinitions.Stages;
        for (int index = 0; index < stages.Count; index++)
        {
            MotherBrainGlassStage stage = stages[index];
            string at = $"glass stage {index}";
            int start = cursor;
            AssertShown(cursor, stage.Image, 1, at);
            AssertEqual(RoomObjectInstruction.GotoIfRoomArgumentLess, At(cursor + 4), $"{at} holds until its hits");
            AssertEqual(stage.BreaksAtHits, (int)Word(cursor + 6), $"{at} hits");
            AssertEqual(start, (int)Word(cursor + 8), $"{at} repeats itself");
            cursor += 10;
            bool last = index + 1 == stages.Count;
            MotherBrainGlassDraw next = last ? MotherBrainGlassStageDefinitions.ShatteredImage : stages[index + 1].Image;
            if (stage.Burst is { } burst)
            {
                AssertBurst(cursor, burst.First, $"{at} first burst");
                AssertShown(cursor + 10, next, MotherBrainGlassStageDefinitions.BurstFrameDuration, $"{at} burst image");
                AssertBurst(cursor + 14, burst.Second, $"{at} second burst");
                cursor += 24;
            }
            else
            {
                AssertTrue(!last, "the last glass stage bursts");
            }
        }
        AssertShown(cursor, MotherBrainGlassStageDefinitions.ShatteredImage, MotherBrainGlassStageDefinitions.FinalShatterDuration, "shattered glass");
        AssertEqual(RoomObjectInstruction.SetEvent, At(cursor + 4), "shattered glass sets its event");
        AssertEqual((ushort)StoryEvent.MotherBrainGlassDestroyed.Number(), Word(cursor + 6), "glass sets its destroyed event");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 8), "shattered glass ends");
        Console.WriteLine($"  Mother Brain glass: {stages.Count} typed stages match bank $84.");
    }

    /// <summary>
    /// The Bomb Torizo hand plays as its native list does (#1554): it shows the intact hand for one update, arms its wake
    /// for Samus's Bombs and sleeps; past the sleep each crumble step drops its fragment and shows its image for its
    /// updates, then the boss music is queued and the hand ends. Every step is compared with
    /// <see cref="BombTorizoHandCrumbleDefinitions"/>; the debris character load between the first two steps takes no time.
    /// </summary>
    internal static void VerifyBombTorizoHandPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, BombTorizoHandDraw image, int updates, string at)
        {
            AssertEqual(updates, (int)Word(cursor), $"{at} updates");
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image)), (int)Word(cursor + 2), $"{at} image");
        }

        int cursor = Word(RoomObjectNativeWords.Word(RoomObjectHeaderId.BombTorizoHand) + 2);
        AssertShown(cursor, BombTorizoHandCrumbleDefinitions.RestingImage, 1, "resting hand");
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(cursor + 4), "hand arms its wake");
        AssertEqual(BombTorizoHandObjectProgramDefinitionsConstants.WakeIfSamusHasBombsFrameBehavior, Word(cursor + 6), "hand bomb wake");
        AssertEqual(RoomObjectInstruction.Sleep, At(cursor + 8), "hand rests until Samus has Bombs");
        cursor += 10;
        IReadOnlyList<BombTorizoHandCrumbleStep> crumble = BombTorizoHandCrumbleDefinitions.Crumble;
        for (int index = 0; index < crumble.Count; index++)
        {
            BombTorizoHandCrumbleStep step = crumble[index];
            string at = $"hand crumble step {index}";
            if (At(cursor) == RoomObjectInstruction.CopyFromRamToVram)
                cursor += 9;
            if (step.FragmentBefore is { } fragment)
            {
                AssertEqual(RoomObjectInstruction.SpawnTorizoStatueBreaking, At(cursor), $"{at} drops a fragment");
                AssertEqual(fragment * sizeof(ushort), (int)Word(cursor + 2), $"{at} fragment");
                cursor += 4;
            }
            AssertShown(cursor, step.Image, step.Updates, at);
            cursor += 4;
        }
        AssertEqual(RoomObjectInstruction.QueueSongOneMusicTrack, At(cursor), "the crumbled hand queues the boss music");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 2), "the crumbled hand ends");
        Console.WriteLine($"  Bomb Torizo hand: {crumble.Count} typed crumble steps match bank $84.");
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
