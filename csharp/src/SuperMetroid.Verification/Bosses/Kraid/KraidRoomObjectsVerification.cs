using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Bosses.Kraid.KraidDrawMappingsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.BossRoomObjectProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid ceiling, platform, and spike PLMs against native programs and draw words.</summary>
internal static class KraidRoomObjectsVerification
{
    /// <summary>Compares eight reachable programs and ten draws, then checks live mutation timing and collision.</summary>
    internal static void VerifyCompiledKraidRoomObjects()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKraidRoomPrograms), () => VerifyKraidRoomPrograms(rom));

        Suite(nameof(VerifyKraidDrawAddresses), () => VerifyKraidDrawAddresses());
        Suite(nameof(VerifyKraidDrawOwnerClassification), () => VerifyKraidDrawOwnerClassification(rom));
        Suite(nameof(VerifyKraidDrawShapes), () => VerifyKraidDrawShapes(rom));
        Suite(nameof(VerifyKraidDrawWords), () => VerifyKraidDrawWords(rom));
        Suite(nameof(VerifyKraidDrawVisualIds), () => VerifyKraidDrawVisualIds());

        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumbleCeilingIntoBackground1,
            12, RoomTerrainBlock.Of(0x13c, BlockMirroring.None, RoomCollisionType.Air), 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumbleCeilingIntoBackground2,
            12, RoomTerrainBlock.Of(0x131, BlockMirroring.None, RoomCollisionType.Air), 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumbleCeilingIntoBackground3,
            12, RoomTerrainBlock.Of(0x130, BlockMirroring.None, RoomCollisionType.Air), 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumblePlatformVariant1,
            3, RoomTerrainBlock.Of(0x131, BlockMirroring.None, RoomCollisionType.Air), 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumblePlatformVariant2,
            3, RoomTerrainBlock.Of(0x130, BlockMirroring.None, RoomCollisionType.Air), 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.ClearCeiling,
            1, RoomTerrainBlock.Of(0x13c, BlockMirroring.None, RoomCollisionType.Air), 15));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.CrumbleSpikes,
            264, RoomTerrainBlock.Of(0x111, BlockMirroring.None, RoomCollisionType.Air), 22));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(KraidRoomMutation.ClearSpikes,
            1, RoomTerrainBlock.Of(0x111, BlockMirroring.None, RoomCollisionType.Air), 22));
        Console.WriteLine(
            "Kraid room PLMs: eight reachable programs and ten physical draws match ROM; all live ceiling/spike paths, timing and collision run without source reads.");
    }

    /// <summary>Runs one ceiling or spike PLM to deletion and checks every final room block without converted ROM reads.</summary>
    private static void VerifyKraidMutation(
        KraidRoomMutation mutation, int expectedDeletionFrame, RoomTerrainBlock firstFinalWord,
        int changedBlockCount)
    {
        const int width = 32;
        const int height = 16;
        var words = new RoomTerrainBlock[width * height];
        for (int x = 5; x < 5 + changedBlockCount; x++)
            words[5 * width + x] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock);
        RoomLevelData level = CreateRoom(new(width, height), words,
            new RoomBlockBehavior[words.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.KraidRoom.TrySpawn(level, new(new(5, 5), mutation)),
            $"Kraid mutation {mutation} allocates");
        AssertEqual(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock),
            level.GetCollisionBlock(new Position16<RoomBlocks>(5, 5)).Terrain,
            $"Kraid mutation {mutation} setup preserves tile priority");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        int deletionFrame = -1;
        for (int frame = 0; frame <= expectedDeletionFrame + 1 &&
             roomObjects.SlotTable.ActiveCount != 0; frame++)
        {
            roomObjects.Step(level);
            if (frame == 0 && mutation is
                KraidRoomMutation.CrumbleCeilingIntoBackground1 or
                KraidRoomMutation.CrumbleCeilingIntoBackground2 or
                KraidRoomMutation.CrumbleCeilingIntoBackground3 or
                KraidRoomMutation.CrumbleSpikes)
                AssertEqual(RoomTerrainBlock.Of(0x180, BlockMirroring.None, RoomCollisionType.SolidBlock),
                    level.GetCollisionBlock(new Position16<RoomBlocks>(5, 5)).Terrain,
                    $"Kraid mutation {mutation} first crumble frame");
            if (roomObjects.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(expectedDeletionFrame, deletionFrame,
            $"Kraid mutation {mutation} native deletion frame");
        for (int offset = 0; offset < changedBlockCount; offset++)
        {
            RoomTerrainBlock expected = mutation is KraidRoomMutation.ClearCeiling
                ? RoomTerrainBlock.Of(offset == 0 ? 0x13c : offset % 2 == 1 ? 0x131 : 0x130, BlockMirroring.None, RoomCollisionType.Air)
                : mutation is KraidRoomMutation.CrumbleSpikes or
                    KraidRoomMutation.ClearSpikes
                    ? RoomTerrainBlock.Of(offset % 2 == 0 ? 0x111 : 0x110, BlockMirroring.None, RoomCollisionType.Air)
                    : firstFinalWord;
            AssertEqual(expected,
                level.GetCollisionBlock(new Position16<RoomBlocks>(5 + offset, 5)).Terrain,
                $"Kraid mutation {mutation} final block {offset}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid mutation {mutation} reads no migrated source bytes");
    }

    /// <summary>Forbids bank-$84 Kraid program and draw bytes after their conversion to compiled definitions.</summary>
    internal static CartridgeReadGuard NewKraidRoomSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => (SnesAddressMath.BankNumber(address)) == 0x84 &&
                (BossRoomObjectNativeListAddresses.IsTypedProgramByte(SnesAddressMath.OffsetInBank(address)) ||
                 ((SnesAddressMath.OffsetInBank(address)) >= RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(KraidRoomDraw.CrumbleFirst)) &&
                  (SnesAddressMath.OffsetInBank(address)) < KraidRoomObjectDrawDefinitions.EndExclusive)),
            address => $"Kraid mutation reread migrated source ${address:X6}.");

    /// <summary>Runs the compiled Kraid room-PLM verification suite.</summary>
    [VerificationCommand("--kraid-room-plms")]
    internal static int KraidRoomObjectsCommand()
    {
        Suite(nameof(VerifyCompiledKraidRoomObjects), () => VerifyCompiledKraidRoomObjects());
        return 0;
    }
}
