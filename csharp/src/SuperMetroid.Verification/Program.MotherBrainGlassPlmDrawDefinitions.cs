using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyMotherBrainGlassPlmDrawDefinitions(
        SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            MotherBrainGlassPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        AssertEqual(11, lists.Length,
            "Mother Brain glass program selects eleven distinct physical draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            int cursor = 0x840000 | list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount, ReadWord(rom, cursor),
                    $"glass draw ${list.Pointer:X4} direction/count at ${cursor:X6}");
                for (int block = 0; block < run.LevelWords.Length; block++)
                    AssertEqual(run.LevelWords.Span[block],
                        ReadWord(rom, cursor + 2 + block * 2),
                        $"glass draw ${list.Pointer:X4} physical block {block}");
                ushort offset = (ushort)((byte)run.NextX | ((byte)run.NextY << 8));
                AssertEqual(offset,
                    ReadWord(rom, cursor + 2 + run.LevelWords.Length * 2),
                    $"glass draw ${list.Pointer:X4} signed next-run offset");
                cursor += 4 + run.LevelWords.Length * 2;
            }
        }

        var bank84 = new byte[0x8000];
        for (int index = 0; index < bank84.Length; index++)
            bank84[index] = rom.ReadByte(0x848000 + index);
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            VerifyMotherBrainGlassNativeDrawPath(bank84, lists, list);
        Console.WriteLine(
            "  Mother Brain glass PLM: 11 native multi-run layouts and all guarded production draws preserve physical blocks.");
    }

    private static void VerifyMotherBrainGlassNativeDrawPath(
        byte[] bank84,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists,
        RoomPlmShotBlockDrawDefinitions.DrawList selected)
    {
        const int width = 32;
        const int height = 16;
        const int originX = 9;
        const int originY = 5;
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x848000, bank84);
        bus.WriteBytes(0x8f9000,
        [
            unchecked((byte)RoomPlmHeaders.MotherBrainGlass),
            unchecked((byte)(RoomPlmHeaders.MotherBrainGlass >> 8)),
            originX, originY, 0x00, 0x80, 0x00, 0x00,
        ]);
        // Keep the native glass program's control path and replace only its first
        // presentation operand, so every compiled layout is exercised by the real
        // PLM interpreter at the cartridge-authored draw call site.
        WriteWord(bus, 0x84d213, selected.Pointer);
        var guarded = new MotherBrainGlassDrawReadGuard(bus, lists);
        var level = new RoomLevelData(width, height,
            new ushort[width * height], new byte[width * height],
            new ushort[width * height], new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Tourian,
                () => new SamusState(), () => false,
                hasAreaBossBit: _ => false,
                hasEvent: _ => false,
                setEvent: _ => { }),
            $"Mother Brain glass loads for draw ${selected.Pointer:X4}");
        AssertTrue(plms.MotherBrainGlassWasLoaded,
            $"glass header retains its PLM owner for draw ${selected.Pointer:X4}");
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
                $"glass draw ${selected.Pointer:X4} writes native physical block {index}");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"glass draw ${selected.Pointer:X4} avoids source payload reads");
    }

    private sealed class MotherBrainGlassDrawReadGuard(
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
                        $"Mother Brain glass reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
