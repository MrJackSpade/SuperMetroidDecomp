using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyShotBlockPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        int wordCount = 0;
        foreach (ushort address in RoomPlmShotBlockProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(address, out ushort value),
                $"shot-block control word ${address:X4} is compiled");
            ushort sourceWord = unchecked((ushort)(
                rom.ReadByte(0x840000 | address) |
                (rom.ReadByte(0x840000 | (address + 1)) << 8)));
            AssertEqual(sourceWord, value,
                $"shot-block control word ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmShotBlockProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmShotBlockProgramDefinitions.TryReadMechanicsByte(address, out byte value),
                $"shot-block sound byte ${address:X4} is compiled");
            AssertEqual(rom.ReadByte(0x840000 | address), value,
                $"shot-block sound byte ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(64, wordCount, "all eight ordinary shot-block programs have control words");
        AssertEqual(8, byteCount, "all eight ordinary shot-block programs have sound operands");
        AssertTrue(!RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(0xa345, out _),
            "interleaved draw-list pointers are not misclassified as control words");

        int drawListCount = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount,
                    unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                        (rom.ReadByte(0x840000 | (cursor + 1)) << 8))),
                    $"shot-block draw list ${list.Pointer:X4} direction/count");
                forbidden.Add(0x840000 | cursor++);
                forbidden.Add(0x840000 | cursor++);
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word,
                        unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                            (rom.ReadByte(0x840000 | (cursor + 1)) << 8))),
                        $"shot-block draw list ${list.Pointer:X4} level word at ${cursor:X4}");
                    forbidden.Add(0x840000 | cursor++);
                    forbidden.Add(0x840000 | cursor++);
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next X");
                forbidden.Add(0x840000 | cursor++);
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor),
                    $"shot-block draw list ${list.Pointer:X4} next Y");
                forbidden.Add(0x840000 | cursor++);
            }

            drawListCount++;
        }

        AssertEqual(19, drawListCount, "all shot-block animation and restoration lists are compiled");
        AssertTrue(!RoomPlmShotBlockDrawDefinitions.TryGet(0xa344, out _),
            "uncatalogued draw pointers do not alias a nearby list");

        // Compare the real PLM handler frame by frame with and without precisely the
        // compiled control bytes forbidden. ROM equality above supplies source parity;
        // this production-path check covers the sound handoff,
        // timed frames, 384-tick respawn hold, word restoration, and permanent deletion.
        for (byte behavior = 0; behavior < 8; behavior++)
        {
            ShotBlockFixture stock = NewShotBlockFixture(behavior);
            ShotBlockFixture compiled = NewShotBlockFixture(behavior);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 420; frame++)
            {
                stock.Plms.Step(rom, stock.Level, stock.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(stock.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"shot-block BTS {behavior} active slots at frame {frame}");
                AssertEqual(stock.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"shot-block BTS {behavior} sound count at frame {frame}");
                for (int index = 0; index < 64; index++)
                    AssertEqual(stock.Level.GetCollisionBlockByIndex(index).LevelWord,
                        compiled.Level.GetCollisionBlockByIndex(index).LevelWord,
                        $"shot-block BTS {behavior} block {index} at frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"shot-block BTS {behavior} executes without control-byte ROM reads");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"shot-block BTS {behavior} completes within the retail timer window");
        }

        Console.WriteLine($"Shot-block PLMs: {wordCount} control words, {byteCount} sound bytes and {drawListCount} draw lists match ROM; all eight programs execute with source bytes forbidden.");
    }

    private sealed record ShotBlockFixture(
        RoomLevelData Level,
        BackgroundTilemapStreamer Streamer,
        RoomPlmSystem Plms);

    private static ShotBlockFixture NewShotBlockFixture(byte behavior)
    {
        const int width = 8;
        const int height = 8;
        const int blockIndex = 3 + 3 * width;
        var words = new ushort[width * height];
        words[blockIndex] = 0xc321;
        var level = new RoomLevelData(
            width, height, words, new byte[words.Length],
            new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnProjectileShotBlock(
                level, blockIndex, behavior, 0, solidBlock: true),
            $"shot-block BTS {behavior} allocates its native PLM program");
        return new ShotBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }

    private sealed class ShotBlockProgramReadGuard(ISnesAddressSpace source, HashSet<int> forbidden)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (forbidden.Contains(address))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Production PLM handler read compiled control byte ${address:X6}.");
            }

            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
