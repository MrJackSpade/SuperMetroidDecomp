using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledCrocomireArenaPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = CrocomireArenaPlmProgramDefinitions.NativeWordAddresses().ToArray();
        AssertEqual(15, addresses.Length, "Crocomire owns fifteen instruction words");
        foreach (ushort address in addresses)
        {
            AssertTrue(CrocomireArenaPlmProgramDefinitions.TryReadMechanicsWord(
                address, out ushort compiled), $"Crocomire instruction ${address:X4} is compiled");
            AssertEqual(ReadRomWord(rom, address), compiled,
                $"Crocomire instruction ${address:X4} matches pinned ROM");
        }
        AssertTrue(!CrocomireArenaPlmProgramDefinitions.TryReadMechanicsWord(
                CrocomireArenaPlmProgramDefinitions.EndExclusive, out _),
            "following save-station instruction is not claimed");

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            CrocomireArenaPlmDrawDefinitions.All.ToArray();
        AssertEqual(5, draws.Length, "Crocomire owns five draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertEqual(ReadRomWord(rom, cursor), run.DirectionAndCount,
                    $"Crocomire draw ${draw.Pointer:X4} direction/count at ${cursor:X4}");
                cursor += 2;
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(ReadRomWord(rom, cursor), word,
                        $"Crocomire draw ${draw.Pointer:X4} word at ${cursor:X4}");
                    cursor += 2;
                }
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextX),
                    $"Crocomire draw ${draw.Pointer:X4} X offset");
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextY),
                    $"Crocomire draw ${draw.Pointer:X4} Y offset");
            }
            AssertTrue(cursor <= CrocomireArenaPlmDrawDefinitions.EndExclusive,
                $"Crocomire draw ${draw.Pointer:X4} stays in bounded region");
        }

        VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireBridge,
            (x, y) => y == 0 && x < 10 ? (ushort)0x0080 : (ushort)0x8123);
        VerifyCrocomireMutation(RoomPlmHeaders.CrumbleCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x810b : (ushort)0x8123);
        VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireBridgeBlock,
            (x, y) => x == 0 && y == 0 ? (ushort)0x0080 : (ushort)0x8123);
        VerifyCrocomireMutation(RoomPlmHeaders.ClearCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, false) : (ushort)0x8123);
        VerifyCrocomireMutation(RoomPlmHeaders.CreateCrocomireInvisibleWall,
            (x, y) => x < 3 && y < 8 ? WallWord(x, y, true) : (ushort)0x8123);
        Console.WriteLine("Crocomire PLMs: five programs and physical draws match ROM; all mutations execute without source reads.");

        static ushort ReadRomWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(0x840000 | address) |
                bus.ReadByte(0x840000 | (address + 1)) << 8);
    }

    private static ushort WallWord(int x, int y, bool solid)
    {
        ushort value = y switch
        {
            0 or 6 or 7 => 0x0080,
            1 or 3 => checked((ushort)(0x0107 + x)),
            2 or 4 => checked((ushort)(0x0127 + x)),
            5 => checked((ushort)(0x0147 + x)),
            _ => throw new ArgumentOutOfRangeException(nameof(y)),
        };
        return solid ? (ushort)(value | 0x8000) : value;
    }

    private static void VerifyCrocomireMutation(ushort header,
        Func<int, int, ushort> expectedWord)
    {
        const int width = 32;
        const int height = 16;
        ushort[] sourceWords = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(width, height, sourceWords,
            new byte[sourceWords.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnCrocomireArenaMutation(level, 5, 3, header),
            $"Crocomire header ${header:X4} allocates");
        var guard = new CrocomireSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.ActiveCount,
            $"Crocomire header ${header:X4} persists for native draw frame");
        for (int y = 0; y < 9; y++)
        for (int x = 0; x < 11; x++)
            AssertEqual(expectedWord(x, y),
                level.GetCollisionBlockByIndex((3 + y) * width + 5 + x).LevelWord,
                $"Crocomire header ${header:X4} physical block ({x},{y})");
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            $"Crocomire header ${header:X4} deletes on next frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire header ${header:X4} reads no migrated ROM records");
    }

    private sealed class CrocomireSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= CrocomireArenaPlmProgramDefinitions.ClearBridge &&
                  pointer < CrocomireArenaPlmProgramDefinitions.EndExclusive) ||
                 (pointer >= CrocomireArenaPlmDrawDefinitions.ClearBridge &&
                  pointer < CrocomireArenaPlmDrawDefinitions.EndExclusive)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Crocomire PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
