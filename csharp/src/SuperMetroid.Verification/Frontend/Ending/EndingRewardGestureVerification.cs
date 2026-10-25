using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Ending.EndingCloudMotionVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingLogoVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingPostShotVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Ending;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Assets;

/// <summary>Checks post-credits reward actors, graphics uploads, gesture timing, and jump handoffs against bank-$8B source data.</summary>
internal static class EndingRewardGestureVerification
{
    /// <summary>Compares sixteen upload chunks, both reward gesture timelines, and jump-to-shot sequences with native data and guarded definitions.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for reward icon graphics and upload tables")]
    internal static void VerifyEndingRewardGesture()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        Suite(nameof(VerifyEndingRewardActorDefinitions), () => VerifyEndingRewardActorDefinitions(bus));
        Suite(nameof(VerifyEndingPostShot), () => VerifyEndingPostShot(bus));
        Suite(nameof(VerifyEndingLogo), () => VerifyEndingLogo(bus));
        Suite(nameof(VerifyEndingCloudMotion), () => VerifyEndingCloudMotion());
        var uploadBus = NewEndingRewardUploadDefinitionReadGuard(bus);
        var rewardArtwork = new EndingScreenArtwork(RepositoryInstallation.Installation.LoadEndingMode7Art(), null, null, null, EndingReward.Armored);
        var graphicsScreen = new EndingScreen();
        byte[] expectedGraphics = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.PostCreditsFloorCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        for (int chunk = 0; chunk < 16; chunk++)
        {
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    EndingRewardGraphicsUploadDefinitions.SourceTable + chunk * sizeof(ushort)),
                RewardIconStagingWord + EndingRewardGraphicsUploadDefinitions.ChunkOffset(chunk),
                $"reward graphics chunk {chunk}'s offset is its native source word's offset from the staging buffer");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    EndingRewardGraphicsUploadDefinitions.DestinationTable + chunk * sizeof(ushort)),
                SuperMetroid.AssetExtraction.Frontend.Ending.EndingVramDestinations.RewardChunkWordOf(chunk),
                $"reward graphics destination address {chunk} matches pinned cartridge");
            graphicsScreen.Record(EndingUploadKind.RewardChunk, chunk);
            SnesVram graphicsVram = EndingScreenFixtures.Vram(graphicsScreen, rewardArtwork);
            int uploadedBytes = (chunk + 1) * 2048;
            AssertTrue(graphicsVram.Bytes[..uploadedBytes].SequenceEqual(expectedGraphics.AsSpan(0, uploadedBytes)),
                "reward graphics DMA preserves both interleaved lanes at every upload boundary");
            AssertTrue(graphicsVram.Bytes[uploadedBytes..].All(value => value == 0),
                "reward graphics upload leaves pending chunks unwritten");
        }
        AssertEqual(0, uploadBus.ForbiddenReadAttempts,
            "reward icon upload no longer rereads either native address table");
        AssertThrows<ArgumentOutOfRangeException>(
            () => EndingRewardGraphicsUploadDefinitions.ChunkOffset(16),
            "reward icon source rejects the seventeenth transfer");
        AssertThrows<ArgumentOutOfRangeException>(
            () => SuperMetroid.AssetExtraction.Frontend.Ending.EndingVramDestinations.RewardChunkWordOf(-1),
            "reward icon destination rejects a negative transfer");
        var rewardBus = NewEndingRewardDefinitionReadGuard(bus);
        var rewardSprites = CinematicSheetFixtures.Drawer(RepositoryInstallation.Installation.LoadEndingObjectArt().RewardSprites);
        foreach (EndingReward reward in Enum.GetValues<EndingReward>())
        {
            var gesture = new EndingRewardGesture(reward);
            var maps = new HashSet<string>();
            int calls = 0;
            while (!gesture.JumpRequested && calls < 1000)
            {
                gesture.Step();
                calls++;
                maps.Add(Convert.ToHexString(ScreenSpritesFixtures.Frame(objects => gesture.Draw(objects, rewardSprites)).LowTable));
            }
            AssertTrue(gesture.JumpRequested, "native reward gesture reaches jumping-actor instruction");
            AssertEqual(reward == EndingReward.Suitless, gesture.SuitlessJumpRequested, "reward-specific jump request");
            AssertTrue(maps.Count >= 8, "reward gesture produces changing OAM, not a static idle pose");
            AssertEqual(reward == EndingReward.Suitless ? 244 : 329, calls,
                "native ED2D/EDD3 gesture durations before jump spawn");
            Console.WriteLine($"  {reward}: {calls} gesture frames, {maps.Count} OAM poses, native jump handoff.");
            var uploads = new List<int>();
            var jump = new EndingRewardJump(reward, uploads.Add);
            bool switched = false, firstMotion = false;
            int minimumY = jump.BodyY;
            int jumpCalls = 0;
            while (!jump.ShotRequested && jumpCalls++ < 500)
            {
                int beforeVelocity = jump.VerticalVelocity;
                jump.Step();
                if (beforeVelocity == -16 * 65536 && !firstMotion)
                {
                    AssertEqual(-16 * 65536 + (reward == EndingReward.Suitless ? 0x3800 : 0x7000),
                        jump.VerticalVelocity, "native jump accelerates once per active actor, before moving");
                    firstMotion = true;
                }
                minimumY = Math.Min(minimumY, jump.BodyY);
                if (!switched && jump.FallingSheetSelected)
                {
                    AssertTrue(jump.BodyY < -80, "native reward sheet switches only beyond the upper screen edge");
                    switched = true;
                }
            }
            AssertTrue(jump.ShotRequested && switched && firstMotion, "jump executes flight, sheet handoff, landing and shooting");
            AssertEqual((short)136, jump.BodyY, "native reward landing center");
            AssertTrue(uploads.SequenceEqual(Enumerable.Range(0, 16)), "landing publishes all sixteen native graphics queue entries exactly once");
            Console.WriteLine($"  {reward}: jump/landing {jumpCalls} frames, apex {minimumY}, 16 uploads, shot request.");
        }
        AssertEqual(0, rewardBus.ForbiddenReadAttempts,
            "ending reward actors never reread compiled definition records");
    }

    /// <summary>Forbids live rereads of the migrated reward icon source and destination address tables.</summary>
    private static CartridgeReadGuard NewEndingRewardUploadDefinitionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address >= EndingRewardGraphicsUploadDefinitions.SourceTable &&
                    address < EndingRewardGraphicsUploadDefinitions.DestinationTable +
                    EndingRewardJumpDefinitions.IconChunkCount * sizeof(ushort),
                address => $"Reward icon upload reread address-table byte ${address:X6}.");

    /// <summary>
    /// $7F:4000: the bank-$7F work-RAM word the cartridge stages the decompressed icon at; its chunk source words are this
    /// plus each chunk's offset (#1416: production holds only the offsets).
    /// </summary>
    private const int RewardIconStagingWord = 0x4000;

    /// <summary>The ten reward gesture and jump actors, in native spawn order.</summary>
    private static readonly ushort[] RewardActorPointers =
    [
        (ushort)EndingRewardActor.SuitlessUpper, (ushort)EndingRewardActor.SuitlessLower,
        (ushort)EndingRewardActor.SuitedBody, (ushort)EndingRewardActor.SuitedArm,
        (ushort)EndingRewardActor.HelmetedHead, (ushort)EndingRewardActor.HelmetlessHead,
        (ushort)EndingRewardActor.JumpSuitlessBody, (ushort)EndingRewardActor.JumpSuitedBody,
        (ushort)EndingRewardActor.JumpHelmetedHead, (ushort)EndingRewardActor.JumpHelmetlessHead,
    ];

    /// <summary>The original LDY operand addresses that independently identify each actor record.</summary>
    private static readonly int[] RewardActorSpawnLoads =
        [0x8be3a1, 0x8be3a7, 0x8be385, 0x8be37f, 0x8be379, 0x8be38d, 0x8bf51e, 0x8bf575, 0x8bf55d, 0x8bf56a];

    /// <summary>Matches ten native spawn operands, actor records, and four initializer origins against compiled definitions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending reward actor definitions")]
    internal static void VerifyEndingRewardActorDefinitions(ISnesAddressSpace bus)
    {

        // Original LDY operands independently identify all ten records.
        RewardActorSpawn[] spawns = [.. RewardActorPointers.Zip(RewardActorSpawnLoads, (pointer, load) => new RewardActorSpawn(pointer, load))];
        var initializers = new HashSet<ushort>();
        foreach (var spawn in spawns)
        {
            AssertEqual((byte)0xa0, bus.ReadCartridgeByte(spawn.Load), "reward native spawn uses LDY immediate");
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((spawn.Load + 1));
            AssertEqual(spawn.Pointer, pointer, "named reward record matches native spawn operand");
            EndingRewardActorDefinition actual = EndingRewardActorDefinitions.Get(
                ClosedNativeWords.Decode<EndingRewardActor>(pointer, "reward actor definition"));
            int address = 0x8b0000 | pointer;
            ushort initializer = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
            AssertEqual(initializer, (ushort)actual.Initialization, $"reward actor ${pointer:X4} initialization callback");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 2)), actual.PreInstruction.NativeAddress, $"reward actor ${pointer:X4} pre-instruction callback");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actual.Program), $"reward actor ${pointer:X4} instruction list");
            if (!initializers.Add(initializer)) continue;
            int code = 0x8b0000 | initializer;
            for (int offset = 0; offset <= 12; offset += 6)
                AssertEqual((byte)0xa9, bus.ReadCartridgeByte(code + offset), "reward initializer uses LDA immediate");
            var origin = EndingRewardActorDefinitions.GetInitialization(
                ClosedNativeWords.Decode<EndingRewardInitialization>(initializer, "reward initialization"));
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((code + 1)), origin.Position.X, "reward native initial X");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((code + 7)), origin.Position.Y, "reward native initial Y");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((code + 13)), origin.PaletteBits, "reward native initial palette");
        }
        AssertEqual(4, initializers.Count, "reward initializer domain");
        AssertEqual(RewardActorPointers.Length, Enum.GetValues<EndingRewardActor>().Length, "reward record domain");
        foreach (ushort invalid in new ushort[] { 0, 0xef32, 0xef34, 0xef45, 0xffff })
            AssertThrows<InvalidOperationException>(() => EndingRewardActorDefinitions.Get((EndingRewardActor)invalid), "reward record membership");
        foreach (ushort invalid in new ushort[] { 0, 0xf142, 0xf144, 0xffff })
            AssertThrows<InvalidOperationException>(() => EndingRewardActorDefinitions.GetInitialization((EndingRewardInitialization)invalid), "reward initializer membership");
        Console.WriteLine(
            "  Reward definitions: ten spawn pointers, thirty record words and twelve initialization operands match.");
    }

    /// <summary>Forbids live reads of all ten compiled six-byte reward actor records.</summary>
    private static CartridgeReadGuard NewEndingRewardDefinitionReadGuard(ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        foreach (ushort pointer in RewardActorPointers)
        {
            int address = EndingRewardActorDefinitions.NativeDefinitionBank | pointer;
            for (int offset = 0; offset < 3 * sizeof(ushort); offset++)
                forbidden.Add(address + offset);
        }
        return new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Ending reward actor reread definition byte ${address:X6}.");
    }

    /// <summary>Runs the ending reward actor and gesture verification command.</summary>
    [VerificationCommand("--ending-reward-definitions")]
    internal static int EndingRewardDefinitionsCommand()
    {
        Suite(nameof(VerifyEndingRewardGesture), () => VerifyEndingRewardGesture());
        return 0;
    }
}
