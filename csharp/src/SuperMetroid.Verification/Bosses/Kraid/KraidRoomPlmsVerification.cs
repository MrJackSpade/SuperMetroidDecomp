using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidDrawMappingsVerification;
using static SuperMetroid.Verification.Rooms.Plms.BossRoomPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid ceiling, platform, and spike PLMs against native programs and draw words.</summary>
internal static class KraidRoomPlmsVerification
{
    /// <summary>Compares eight reachable programs and ten draws, then checks live mutation timing and collision.</summary>
    internal static void VerifyCompiledKraidRoomPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKraidRoomPrograms), () => VerifyKraidRoomPrograms(rom));

        Suite(nameof(VerifyKraidDrawAddresses), () => VerifyKraidDrawAddresses());
        Suite(nameof(VerifyKraidDrawOwnerClassification), () => VerifyKraidDrawOwnerClassification(rom));
        Suite(nameof(VerifyKraidDrawShapes), () => VerifyKraidDrawShapes(rom));
        Suite(nameof(VerifyKraidDrawWords), () => VerifyKraidDrawWords(rom));
        Suite(nameof(VerifyKraidDrawVisualIds), () => VerifyKraidDrawVisualIds());

        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidCeilingIntoBackground1,
            12, 0x013c, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidCeilingIntoBackground2,
            12, 0x0131, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidCeilingIntoBackground3,
            12, 0x0130, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidPlatformVariant1,
            3, 0x0131, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidPlatformVariant2,
            3, 0x0130, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.ClearKraidCeiling,
            1, 0x013c, 15));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.CrumbleKraidSpikes,
            264, 0x0111, 22));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(PlmHeaderId.ClearKraidSpikes,
            1, 0x0111, 22));
        Console.WriteLine(
            "Kraid room PLMs: eight reachable programs and ten physical draws match ROM; all live ceiling/spike paths, timing and collision run without source reads.");
    }

    /// <summary>Runs one ceiling or spike PLM to deletion and checks every final room block without converted ROM reads.</summary>
    private static void VerifyKraidMutation(
        PlmHeaderId header, int expectedDeletionFrame, ushort firstFinalWord,
        int changedBlockCount)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int x = 5; x < 5 + changedBlockCount; x++)
            words[5 * width + x] = 0x8123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnKraidRoomMutation(level, new(5, 5), header),
            $"Kraid mutation ${(int)header:X4} allocates");
        AssertEqual((ushort)0x8123,
            level.GetCollisionBlockByIndex(5 * width + 5).PackedWord.Raw,
            $"Kraid mutation ${(int)header:X4} setup preserves tile priority");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        for (int frame = 0; frame <= expectedDeletionFrame + 1 &&
             plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            if (frame == 0 && header is
                PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                PlmHeaderId.CrumbleKraidSpikes)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(5 * width + 5).PackedWord.Raw,
                    $"Kraid mutation ${(int)header:X4} first crumble frame");
            if (plms.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(expectedDeletionFrame, deletionFrame,
            $"Kraid mutation ${(int)header:X4} native deletion frame");
        for (int offset = 0; offset < changedBlockCount; offset++)
        {
            ushort expected = header is PlmHeaderId.ClearKraidCeiling
                ? offset == 0 ? (ushort)0x013c :
                    (ushort)(offset % 2 == 1 ? 0x0131 : 0x0130)
                : header is PlmHeaderId.CrumbleKraidSpikes or
                    PlmHeaderId.ClearKraidSpikes
                    ? (ushort)(offset % 2 == 0 ? 0x0111 : 0x0110)
                    : firstFinalWord;
            AssertEqual(expected,
                level.GetCollisionBlockByIndex(5 * width + 5 + offset).PackedWord.Raw,
                $"Kraid mutation ${(int)header:X4} final block {offset}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid mutation ${(int)header:X4} reads no migrated source bytes");
    }

    /// <summary>Forbids bank-$84 Kraid program and draw bytes after their conversion to compiled definitions.</summary>
    internal static CartridgeReadGuard NewKraidRoomSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => (SnesAddressMath.BankNumber(address)) == 0x84 &&
                (BossRoomPlmNativeListAddresses.IsTypedProgramByte(SnesAddressMath.OffsetInBank(address)) ||
                 ((SnesAddressMath.OffsetInBank(address)) >= (ushort)KraidRoomDraw.CrumbleFirst &&
                  (SnesAddressMath.OffsetInBank(address)) < KraidRoomPlmDrawDefinitions.EndExclusive)),
            address => $"Kraid mutation reread migrated source ${address:X6}.");

    /// <summary>Runs the compiled Kraid room-PLM verification suite.</summary>
    [VerificationCommand("--kraid-room-plms")]
    internal static int KraidRoomPlmsCommand()
    {
        Suite(nameof(VerifyCompiledKraidRoomPlms), () => VerifyCompiledKraidRoomPlms());
        return 0;
    }
}
