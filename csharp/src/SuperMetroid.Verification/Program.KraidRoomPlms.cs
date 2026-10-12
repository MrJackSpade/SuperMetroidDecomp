using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledKraidRoomPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyKraidRoomProgramMapping), () => VerifyKraidRoomProgramMapping(rom));

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
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnKraidRoomMutation(level, new(5, 5), header),
            $"Kraid mutation ${(int)header:X4} allocates");
        AssertEqual((ushort)0x8123,
            level.GetCollisionBlockByIndex(5 * width + 5).LevelWord,
            $"Kraid mutation ${(int)header:X4} setup preserves tile priority");
        var guard = NewKraidRoomSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        for (int frame = 0; frame <= expectedDeletionFrame + 1 &&
             plms.ActiveCount != 0; frame++)
        {
            plms.Step(guard, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
            if (frame == 0 && header is
                PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                PlmHeaderId.CrumbleKraidSpikes)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(5 * width + 5).LevelWord,
                    $"Kraid mutation ${(int)header:X4} first crumble frame");
            if (plms.ActiveCount == 0)
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
                level.GetCollisionBlockByIndex(5 * width + 5 + offset).LevelWord,
                $"Kraid mutation ${(int)header:X4} final block {offset}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid mutation ${(int)header:X4} reads no migrated source bytes");
    }

    private static CartridgeReadGuard NewKraidRoomSourceGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => (address >> 16) == 0x84 &&
                (((address & 0xffff) >= KraidRoomPlmProgramDefinitions.CrumbleCeilingBackground1 &&
                  (address & 0xffff) < KraidRoomPlmProgramDefinitions.MoveRightCallback) ||
                 ((address & 0xffff) >= KraidRoomPlmProgramDefinitions.ClearSpikes &&
                  (address & 0xffff) < KraidRoomPlmProgramDefinitions.EndExclusive) ||
                 ((address & 0xffff) >= (ushort)KraidRoomDraw.CrumbleFirst &&
                  (address & 0xffff) < KraidRoomPlmDrawDefinitions.EndExclusive)),
            address => $"Kraid mutation reread migrated source ${address:X6}.");
}
