using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyNoobTubePlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            NoobTubePlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        AssertEqual(7, lists.Length,
            "n00b-tube program selects seven distinct physical draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            int cursor = 0x840000 | list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount, ReadWord(rom, cursor),
                    $"n00b-tube draw ${list.Pointer:X4} direction/count at ${cursor:X6}");
                for (int block = 0; block < run.LevelWords.Length; block++)
                    AssertEqual(run.LevelWords.Span[block],
                        ReadWord(rom, cursor + 2 + block * 2),
                        $"n00b-tube draw ${list.Pointer:X4} physical block {block}");
                ushort offset = (ushort)((byte)run.NextX | ((byte)run.NextY << 8));
                AssertEqual(offset,
                    ReadWord(rom, cursor + 2 + run.LevelWords.Length * 2),
                    $"n00b-tube draw ${list.Pointer:X4} signed next-run offset");
                cursor += 4 + run.LevelWords.Length * 2;
            }
        }

        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyNoobTubeNativeDrawPath(bank84, lists, list);
        Console.WriteLine(
            "  N00b-tube PLM draws: seven native layouts and all guarded production draws preserve physical blocks.");
    }

    private static void VerifyNoobTubeNativeDrawPath(
        byte[] bank84,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmShotBlockDrawDefinitions.DrawList selected)
    {
        const int width = 16;
        const int height = 16;
        const int originX = 2;
        const int originY = 2;
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x848000, bank84);
        bus.WriteBytes(0x8f9400,
        [
            unchecked((byte)RoomPlmHeaders.NoobTube),
            unchecked((byte)(RoomPlmHeaders.NoobTube >> 8)),
            originX, originY, 0, 0, 0, 0,
        ]);
        // Preserve the cartridge control stream, varying only its first draw
        // operand to exercise every native layout through the actual PLM caller.
        WriteWord(bus, 0x84d4e4, selected.Pointer);
        var guarded = new NoobTubeDrawReadGuard(bus, lists);
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9400, new Bank80SystemState(), AreaId.Maridia,
                () => new SamusState(), () => false,
                hasEvent: _ => false,
                setEvent: _ => { }),
            $"n00b tube loads for draw ${selected.Pointer:X4}");
        plms.Step(guarded, level, streamer, 0, 0, 0);

        int entryX = originX;
        int entryY = originY;
        var expected = new Dictionary<int, ushort>();
        foreach (RoomPlmShotBlockDrawDefinitions.Run run in selected.Runs.Span)
        {
            bool vertical = (run.DirectionAndCount & 0x8000) != 0;
            for (int block = 0; block < run.LevelWords.Length; block++)
            {
                int x = entryX + (vertical ? 0 : block);
                int y = entryY + (vertical ? block : 0);
                expected.Add(y * width + x, run.LevelWords.Span[block]);
            }
            entryX = originX + run.NextX;
            entryY = originY + run.NextY;
        }
        foreach ((int index, ushort word) in expected)
            AssertEqual(word, level.GetCollisionBlockByIndex(index).LevelWord,
                $"n00b-tube draw ${selected.Pointer:X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"n00b-tube draw ${selected.Pointer:X4} avoids source payload reads");
    }

    private sealed class NoobTubeDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                int length = list.Runs.Span.ToArray().Sum(run =>
                    4 + run.LevelWords.Length * 2);
                if (address >= first && address < first + length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"N00b tube reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
