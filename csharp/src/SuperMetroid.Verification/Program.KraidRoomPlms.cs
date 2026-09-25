using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledKraidRoomPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = KraidRoomPlmProgramDefinitions
            .NativeWordAddresses().ToArray();
        AssertEqual(55, addresses.Length,
            "Kraid ceiling/spikes own fifty-five instruction words");
        AssertEqual(addresses.Length, addresses.Distinct().Count(),
            "Kraid room instruction words do not overlap");
        foreach (ushort address in addresses)
        {
            AssertTrue(KraidRoomPlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Kraid room word $84:{address:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8),
                compiled,
                $"Kraid room word $84:{address:X4} matches ROM");
        }
        ushort loopCountAddress = checked((ushort)(
            KraidRoomPlmProgramDefinitions.CrumbleSpikes + 2));
        AssertTrue(KraidRoomPlmProgramDefinitions.TryReadMechanicsByte(
                loopCountAddress, out byte loopCount),
            "Kraid spike-loop count is compiled");
        AssertEqual(rom.ReadByte(0x840000 | loopCountAddress), loopCount,
            "Kraid spike-loop count matches ROM");
        AssertTrue(!KraidRoomPlmProgramDefinitions.TryReadMechanicsWord(
                KraidRoomPlmProgramDefinitions.MoveRightCallback, out _),
            "Kraid move-right callback is not instruction data");
        AssertTrue(!KraidRoomPlmProgramDefinitions.TryReadMechanicsWord(
                KraidRoomPlmProgramDefinitions.EndExclusive, out _),
            "following Mother Brain program is not claimed by Kraid");

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            KraidRoomPlmDrawDefinitions.All.ToArray();
        AssertEqual(10, draws.Length,
            "Kraid room owns ten reachable physical draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertWord(run.DirectionAndCount);
                foreach (ushort word in run.LevelWords.Span)
                    AssertWord(word);
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextX),
                    $"Kraid draw ${draw.Pointer:X4} signed X offset");
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextY),
                    $"Kraid draw ${draw.Pointer:X4} signed Y offset");
            }
            AssertTrue(cursor <= KraidRoomPlmDrawDefinitions.EndExclusive,
                $"Kraid draw ${draw.Pointer:X4} remains in its bounded region");

            void AssertWord(ushort expected)
            {
                AssertEqual((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8),
                    expected,
                    $"Kraid draw ${draw.Pointer:X4} word +{cursor - draw.Pointer}");
                cursor += 2;
            }
        }

        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1,
            12, 0x013c, 1);
        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2,
            12, 0x0131, 1);
        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3,
            12, 0x0130, 1);
        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidPlatformVariant1,
            3, 0x0131, 1);
        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidPlatformVariant2,
            3, 0x0130, 1);
        VerifyKraidMutation(RoomPlmHeaders.ClearKraidCeiling,
            1, 0x013c, 15);
        VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidSpikes,
            264, 0x0111, 22);
        VerifyKraidMutation(RoomPlmHeaders.ClearKraidSpikes,
            1, 0x0111, 22);
        Console.WriteLine(
            "Kraid room PLMs: eight reachable programs and ten physical draws match ROM; all live ceiling/spike paths, timing and collision run without source reads.");
    }

    private static void VerifyKraidMutation(
        ushort header, int expectedDeletionFrame, ushort firstFinalWord,
        int changedBlockCount)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int x = 5; x < 5 + changedBlockCount; x++)
            words[5 * width + x] = 0x8123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnKraidRoomMutation(level, 5, 5, header),
            $"Kraid mutation ${header:X4} allocates");
        AssertEqual((ushort)0x8123,
            level.GetCollisionBlockByIndex(5 * width + 5).LevelWord,
            $"Kraid mutation ${header:X4} setup preserves tile priority");
        var guard = new KraidRoomSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        for (int frame = 0; frame <= expectedDeletionFrame + 1 &&
             plms.ActiveCount != 0; frame++)
        {
            plms.Step(guard, level, streamer, 0, 0, 0);
            if (frame == 0 && header is
                RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1 or
                RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2 or
                RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3 or
                RoomPlmHeaders.CrumbleKraidSpikes)
                AssertEqual((ushort)0x8180,
                    level.GetCollisionBlockByIndex(5 * width + 5).LevelWord,
                    $"Kraid mutation ${header:X4} first crumble frame");
            if (plms.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(expectedDeletionFrame, deletionFrame,
            $"Kraid mutation ${header:X4} native deletion frame");
        for (int offset = 0; offset < changedBlockCount; offset++)
        {
            ushort expected = header is RoomPlmHeaders.ClearKraidCeiling
                ? offset == 0 ? (ushort)0x013c :
                    (ushort)(offset % 2 == 1 ? 0x0131 : 0x0130)
                : header is RoomPlmHeaders.CrumbleKraidSpikes or
                    RoomPlmHeaders.ClearKraidSpikes
                    ? (ushort)(offset % 2 == 0 ? 0x0111 : 0x0110)
                    : firstFinalWord;
            AssertEqual(expected,
                level.GetCollisionBlockByIndex(5 * width + 5 + offset).LevelWord,
                $"Kraid mutation ${header:X4} final block {offset}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid mutation ${header:X4} reads no migrated source bytes");
    }

    private sealed class KraidRoomSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= KraidRoomPlmProgramDefinitions.CrumbleCeilingBackground1 &&
                  pointer < KraidRoomPlmProgramDefinitions.MoveRightCallback) ||
                 (pointer >= KraidRoomPlmProgramDefinitions.ClearSpikes &&
                  pointer < KraidRoomPlmProgramDefinitions.EndExclusive) ||
                 (pointer >= KraidRoomPlmDrawDefinitions.CrumbleFirst &&
                  pointer < KraidRoomPlmDrawDefinitions.EndExclusive)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid mutation reread migrated source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
