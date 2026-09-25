using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledTourianAccessPlmPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = TourianAccessPlmProgramDefinitions.NativeWordAddresses()
            .ToArray();
        AssertEqual(16, addresses.Length,
            "Tourian access has sixteen compiled instruction words");
        AssertEqual(addresses.Length, addresses.Distinct().Count(),
            "Tourian access instruction word addresses are unique");
        foreach (ushort address in addresses)
        {
            AssertTrue(TourianAccessPlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Tourian access control word $84:{address:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8),
                compiled, $"Tourian access control word $84:{address:X4} matches ROM");
        }
        ushort timerByteAddress = checked((ushort)(
            TourianAccessPlmProgramDefinitions.Crumble + 2));
        AssertTrue(TourianAccessPlmProgramDefinitions.TryReadMechanicsByte(
                timerByteAddress, out byte rows),
            "Tourian crumble loop count is compiled");
        AssertEqual(rom.ReadByte(0x840000 | timerByteAddress), rows,
            "Tourian crumble loop count matches ROM");
        AssertTrue(!TourianAccessPlmProgramDefinitions.TryReadMechanicsWord(
                TourianStatueRomData.MoveAccessDown, out _),
            "adjacent move-down machine code is not claimed as a PLM program");

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            TourianAccessPlmDrawDefinitions.All.ToArray();
        AssertEqual(5, draws.Length, "Tourian access owns four frames and a six-row clear");
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertTourianDrawWord(run.DirectionAndCount);
                foreach (ushort word in run.LevelWords.Span)
                    AssertTourianDrawWord(word);
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextX),
                    $"Tourian draw ${draw.Pointer:X4} run X offset matches ROM");
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextY),
                    $"Tourian draw ${draw.Pointer:X4} run Y offset matches ROM");
            }
            AssertEqual(checked((ushort)(cursor - draw.Pointer)),
                checked((ushort)(draw.Pointer ==
                    TourianAccessPlmDrawDefinitions.ClearPointer ? 72 : 12)),
                $"Tourian draw ${draw.Pointer:X4} has the complete native span");

            void AssertTourianDrawWord(ushort expected)
            {
                AssertEqual((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8),
                    expected, $"Tourian draw ${draw.Pointer:X4} word at +{cursor - draw.Pointer} matches ROM");
                cursor += 2;
            }
        }

        VerifyFloor(clear: true);
        VerifyFloor(clear: false);
        Console.WriteLine(
            "Tourian access PLMs: both native programs and five draws match ROM; complete clear and six-row crumble run without source reads.");
    }

    private static void VerifyFloor(bool clear)
    {
        const int width = 16;
        const int height = 20;
        ushort[] words = new ushort[width * height];
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnTourianAccess(level, clear),
            $"Tourian {(clear ? "clear" : "crumble")} PLM allocates");
        var bus = new TourianAccessSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        for (int frame = 0; frame < 120 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(bus, level, streamer, 0, 0, 0);
            if (frame == 0 && !clear)
                AssertEqual((ushort)0x0053,
                    level.GetCollisionBlockByIndex(12 * width + 6).LevelWord,
                    "Tourian crumble first frame draws the native first breakup word");
            if (plms.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 1 : 96, deletionFrame,
            $"Tourian {(clear ? "clear" : "crumble")} PLM deletes on its native frame");
        for (int y = 12; y < 18; y++)
        for (int x = 6; x < 10; x++)
            AssertEqual((ushort)0x00ff,
                level.GetCollisionBlockByIndex(y * width + x).LevelWord,
                $"Tourian {(clear ? "clear" : "crumble")} clears ({x},{y}) physically");
        AssertEqual(0, bus.ForbiddenReadAttempts,
            $"Tourian {(clear ? "clear" : "crumble")} reads no migrated ROM bytes");
    }

    private sealed class TourianAccessSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= TourianAccessPlmProgramDefinitions.Crumble &&
                  pointer < TourianStatueRomData.MoveAccessDown) ||
                 (pointer >= TourianAccessPlmProgramDefinitions.Clear &&
                  pointer < TourianAccessPlmProgramDefinitions.Clear + 6) ||
                 (pointer >= TourianAccessPlmDrawDefinitions.EmptyRowPointer &&
                  pointer < 0x930f)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Tourian access PLM reread compiled source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
