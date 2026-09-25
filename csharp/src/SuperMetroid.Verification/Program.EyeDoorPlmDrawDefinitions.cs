using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyEyeDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            EyeDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        AssertEqual(23, lists.Length,
            "mirrored eye, middle and bottom components select 23 distinct draw lists");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            AssertEqual(1, list.Runs.Length,
                $"eye-door draw ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadWord(rom, source),
                $"eye-door draw ${list.Pointer:X4} direction/count matches ROM");
            for (int block = 0; block < run.LevelWords.Length; block++)
                AssertEqual(run.LevelWords.Span[block],
                    ReadWord(rom, source + 2 + block * 2),
                    $"eye-door draw ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0,
                ReadWord(rom, source + 2 + run.LevelWords.Length * 2),
                $"eye-door draw ${list.Pointer:X4} terminates after its physical words");
        }

        VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Left, lists);
        VerifyEyeDoorNativeDrawPath(EyeDoorOrientation.Right, lists);
        Console.WriteLine(
            "  Eye-door PLM draws: 23 native lists and both three-component first-draw paths avoid payload ROM reads.");
    }

    private static void VerifyEyeDoorNativeDrawPath(
        EyeDoorOrientation orientation,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists)
    {
        const int width = 16;
        var bus = new TestAddressSpace();
        SeedEyeDoorFixtureRom(bus, orientation);
        ushort[] pointers = orientation == EyeDoorOrientation.Left
            ? [0x9c03, 0x9c2b, 0x9c3d]
            : [0x9c5b, 0x9c83, 0x9c95];
        WriteWord(bus, 0x84e01a, pointers[0]);
        WriteWord(bus, 0x84e10e, pointers[1]);
        WriteWord(bus, 0x84e20e, pointers[2]);

        var level = new RoomLevelData(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], new byte[0x400 * 8]);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        var plms = new RoomPlmSystem();
        var guarded = new EyeDoorDrawReadGuard(bus, lists);
        AssertEqual(3, plms.LoadRoomPopulation(guarded, level, streamer,
                new SnesVram(), 0x9000, new Bank80SystemState(), AreaId.Brinstar,
                () => new SamusState(), () => false,
                spawnEyeDoorProjectile: _ => { }),
            $"{orientation} eye-door three-component population loads");
        plms.Step(guarded, level, streamer, 0, 0, 0);
        int[] origins = [4 * width + 7, 6 * width + 7, 8 * width + 7];
        for (int component = 0; component < pointers.Length; component++)
        {
            AssertTrue(EyeDoorPlmDrawDefinitions.TryGet(pointers[component], out var list),
                $"{orientation} component {component} selects compiled draw data");
            RoomPlmShotBlockDrawDefinitions.Run run = list.Runs.Span[0];
            int stride = (run.DirectionAndCount & 0x8000) != 0 ? width : 1;
            for (int block = 0; block < run.LevelWords.Length; block++)
                AssertEqual(run.LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origins[component] + block * stride).LevelWord,
                    $"{orientation} component {component} draws physical block {block}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{orientation} eye-door first draws avoid bank-$84 payload reads");
    }

    private sealed class EyeDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                int first = 0x840000 | list.Pointer;
                int length = 4 + list.Runs.Span[0].LevelWords.Length * 2;
                if (address >= first && address < first + length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Eye door reread compiled draw payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
