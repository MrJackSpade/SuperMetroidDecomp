using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBlueDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        static ushort ReadNativeWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        AssertEqual(16, lists.Length, "four orientations each have four blue-cap frames");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
        {
            ReadOnlySpan<RoomPlmShotBlockDrawDefinitions.Run> runs = list.Runs.Span;
            AssertEqual(1, runs.Length, $"blue-cap ${list.Pointer:X4} has one run");
            RoomPlmShotBlockDrawDefinitions.Run run = runs[0];
            int source = 0x840000 | list.Pointer;
            AssertEqual(run.DirectionAndCount, ReadNativeWord(rom, source),
                $"blue-cap ${list.Pointer:X4} direction/count matches ROM");
            AssertEqual(4, run.LevelWords.Length,
                $"blue-cap ${list.Pointer:X4} has four physical words");
            for (int block = 0; block < 4; block++)
                AssertEqual(run.LevelWords.Span[block], ReadNativeWord(rom, source + 2 + block * 2),
                    $"blue-cap ${list.Pointer:X4} block {block} matches ROM");
            AssertEqual((ushort)0, ReadNativeWord(rom, source + 10),
                $"blue-cap ${list.Pointer:X4} ends at its signed-offset terminator");
        }

        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            ushort openingList = orientation switch
            {
                ColoredDoorOrientation.Left => RoomPlmInstructionLists.BlueDoorFacingLeftOpening,
                ColoredDoorOrientation.Right => RoomPlmInstructionLists.BlueDoorFacingRightOpening,
                ColoredDoorOrientation.Up => RoomPlmInstructionLists.BlueDoorFacingUpOpening,
                ColoredDoorOrientation.Down => RoomPlmInstructionLists.BlueDoorFacingDownOpening,
                _ => throw new InvalidDataException("Unknown blue-door orientation."),
            };
            ushort firstDraw = ReadNativeWord(rom, 0x840000 | (openingList + 5));
            AssertTrue(BlueDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"{orientation} opening program selects a compiled draw");

            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], new byte[8]);
            var plms = new RoomPlmSystem();
            RoomBlockBehavior behavior = new(unchecked((byte)(
                RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (byte)orientation)));
            AssertTrue(plms.TrySpawnBlueDoorOpening(level, origin, behavior,
                new SamusProjectileTypeWord(0)),
                $"{orientation} blue door allocates its native opening actor");
            var guarded = new BlueDoorDrawReadGuard(rom, lists);
            plms.Step(guarded, level, level.CreateBackgroundStreamer(),
                0x1000, 0x1000, 0);
            int stride = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right
                ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).LevelWord,
                    $"{orientation} native draw writes physical block {block} without ROM payload reads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} opening avoids all blue-cap draw-list ROM bytes");
        }

        Console.WriteLine("  Blue-door PLM draws: sixteen exact cartridge lists; four native opening paths reject ROM payload reads.");
    }

    private sealed class BlueDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in lists)
            {
                if (address >= (0x840000 | list.Pointer) &&
                    address < (0x840000 | list.Pointer) + BlueDoorPlmDrawDefinitions.DrawListBytes)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Production blue-door draw reread bank-$84 payload ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
