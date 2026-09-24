using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyColoredDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            ColoredDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        AssertEqual(48, lists.Length,
            "three colored-door families each have four orientations and four frames");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            AssertEqual(1, list.Runs.Length,
                $"colored cap ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadWord(rom, source),
                $"colored cap ${list.Pointer:X4} direction/count matches ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"colored cap ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < 4; block++)
                AssertEqual(run.LevelWords.Span[block],
                    ReadWord(rom, source + 2 + block * 2),
                    $"colored cap ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0, ReadWord(rom, source + 10),
                $"colored cap ${list.Pointer:X4} has a zero offset terminator");
        }

        // A synthetic population selects each real resident header. Only the bank-$8F
        // population is synthetic; setup and first-draw instruction bytes are copied
        // from the pinned bank-$84 cartridge. Blocking all authored draw payloads
        // proves that the actual PLM handler uses the compiled definitions.
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = rom.ReadByte(0x848000 + offset);
        ushort[] headers =
        [
            RoomPlmHeaders.YellowDoorFacingLeft,
            RoomPlmHeaders.YellowDoorFacingRight,
            RoomPlmHeaders.YellowDoorFacingUp,
            RoomPlmHeaders.YellowDoorFacingDown,
            RoomPlmHeaders.GreenDoorFacingLeft,
            RoomPlmHeaders.GreenDoorFacingRight,
            RoomPlmHeaders.GreenDoorFacingUp,
            RoomPlmHeaders.GreenDoorFacingDown,
            RoomPlmHeaders.RedDoorFacingLeft,
            RoomPlmHeaders.RedDoorFacingRight,
            RoomPlmHeaders.RedDoorFacingUp,
            RoomPlmHeaders.RedDoorFacingDown,
        ];
        foreach (ushort header in headers)
        {
            var bus = new TestAddressSpace();
            bus.WriteBytes(0x848000, bank84);
            const ushort population = 0x9000;
            bus.WriteBytes(0x8f0000 | population,
            [
                unchecked((byte)header), unchecked((byte)(header >> 8)),
                4, 4, 0, 0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], new byte[0x400 * 8]);
            var plms = new RoomPlmSystem();
            var guarded = new ColoredDoorDrawReadGuard(bus, lists);
            AssertEqual(1, plms.LoadRoomPopulation(guarded, level,
                    level.CreateBackgroundStreamer(), new SnesVram(), population,
                    new Bank80SystemState(), AreaId.Crateria,
                    () => new SamusState(), () => false),
                $"resident colored-door header ${header:X4} loads");
            ushort initial = ReadWord(rom, 0x840000 | (header + 2));
            ushort firstDraw = ReadWord(rom, 0x840000 | (initial + 14));
            AssertTrue(ColoredDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"resident colored-door header ${header:X4} selects compiled art");
            plms.Step(guarded, level, level.CreateBackgroundStreamer(), 0, 0, 0);
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"resident colored-door ${header:X4} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored-door ${header:X4} avoids draw payload ROM reads");
        }
        Console.WriteLine(
            "  Colored-door PLM draws: 48 native lists and all 12 resident first draws match with ROM payload reads forbidden.");
    }

    private sealed class ColoredDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                if (address >= first &&
                    address < first + ColoredDoorPlmDrawDefinitions.DrawListBytes)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Resident colored door reread draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
