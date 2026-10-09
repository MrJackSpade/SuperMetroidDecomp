using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies the compiled Kraid PLM programs, physical draws, and live mutation behavior against retail data.</summary>
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

        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1,
            12, 0x013c, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground2,
            12, 0x0131, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground3,
            12, 0x0130, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidPlatformVariant1,
            3, 0x0131, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidPlatformVariant2,
            3, 0x0130, 1));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.ClearKraidCeiling,
            1, 0x013c, 15));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.CrumbleKraidSpikes,
            264, 0x0111, 22));
        Suite(nameof(VerifyKraidMutation), () => VerifyKraidMutation(RoomPlmHeaders.ClearKraidSpikes,
            1, 0x0111, 22));
        Console.WriteLine(
            "Kraid room PLMs: eight reachable programs and ten physical draws match ROM; all live ceiling/spike paths, timing and collision run without source reads.");
    }

    /// <summary>Runs one ceiling or spike mutation through its full lifetime and checks timing, final blocks, and source isolation.</summary>
    /// <param name="header">Kraid room-PLM header that selects the mutation behavior.</param>
    /// <param name="expectedDeletionFrame">Frame on which the PLM is expected to finish and be removed.</param>
    /// <param name="firstFinalWord">Expected first final tile word for the selected mutation.</param>
    /// <param name="changedBlockCount">Number of neighboring blocks modified by the scenario.</param>
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

    /// <summary>Forwards memory access while rejecting reads from migrated Kraid mutation programs and draw lists.</summary>
    /// <param name="source">Underlying address space used for permitted reads and all writes.</param>
    private sealed class KraidRoomSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Count of attempted reads within migrated Kraid source ranges.</summary>
        internal int ForbiddenReadAttempts { get; private set; }

        /// <summary>Routes importer reads through the source-range guard.</summary>
        /// <param name="address">CPU address requested from the importer.</param>
        /// <returns>The underlying byte when the address is not a migrated Kraid range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects Kraid source ranges that should be served by compiled definitions, forwarding other reads.</summary>
        /// <param name="address">CPU address requested by the PLM system.</param>
        /// <returns>The byte from the wrapped address space for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address lies in a migrated Kraid program or draw range.</exception>
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

        /// <summary>Forwards writes to the wrapped address space without modifying the read guard.</summary>
        /// <param name="address">CPU address to write.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
