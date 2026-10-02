using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledTourianAccessPlmPrograms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian draw native oracle revision");
        VerifyTourianAccessPlmDefinitions(rom);
        VerifyTourianAccessProgramControls(rom);
        VerifyTourianAccessProgramDraws(rom);
        VerifyTourianAccessProgramLoopCount(rom);

        VerifyTourianAccessPhysicalDrawMapping(rom);

        VerifyFloor(clear: true);
        VerifyFloor(clear: false);
        Console.WriteLine(
            "Tourian access PLMs: both native programs and five draws match ROM; complete clear and six-row crumble run without source reads.");
    }

    private static void VerifyTourianAccessProgramControls(SuperMetroidAddressSpace rom) =>
        VerifyTourianAccessProgramField(rom, false);

    private static void VerifyTourianAccessProgramDraws(SuperMetroidAddressSpace rom)
    {
        VerifyTourianAccessProgramField(rom, true);
        ushort[] originalOperands = [0xaaea,0xaaee,0xaaf2,0xaaf6];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | originalOperands[frame]),
                TourianAccessPlmDrawDefinitions.CrumbleFramePointer(frame), "Tourian access native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,4,5,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => TourianAccessPlmDrawDefinitions.CrumbleFramePointer(invalid),
                "Tourian access frame selector bounds");
    }

    private static void VerifyTourianAccessProgramField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xaae5,0xaae8,0xaaec,0xaaf0,0xaaf4,0xaaf8,0xaafa,0xaafc,0xaafe,0xab0c,0xab10];
        ushort[] operands = [0xaaea,0xaaee,0xaaf2,0xaaf6,0xab0e];
        ushort[] expectedOrder = [0xaae5,0xaae8,0xaaea,0xaaec,0xaaee,0xaaf0,0xaaf2,0xaaf4,0xaaf6,0xaaf8,0xaafa,0xaafc,0xaafe,0xab0c,0xab0e,0xab10];
        AssertTrue(expectedOrder.SequenceEqual(TourianAccessPlmProgramDefinitions.NativeWordAddresses()),
            "Tourian access original instruction enumeration");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = controls.Contains(address) || operands.Contains(address);
            AssertEqual(owned, TourianAccessPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actual),
                "Tourian access complete word ownership including packed-byte gaps");
            if (!owned) AssertEqual((ushort)0, actual, "Tourian access unowned word zero");
            else if ((draw ? operands : controls).Contains(address))
            {
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | raw), actual, "Tourian access original program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Tourian access shared word reader");
                AssertEqual(actual, shared, "Tourian access shared reader value");
            }
        }
    }

    private static void VerifyTourianAccessProgramLoopCount(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            AssertEqual(raw == 0xaae7,
                TourianAccessPlmProgramDefinitions.TryReadMechanicsByte((ushort)raw, out byte actual),
                "Tourian access byte ownership is exactly its loop-count operand");
            AssertEqual(raw == 0xaae7 ? rom.ReadByte(0x84aae7) : (byte)0, actual,
                "Tourian access original loop count or cleared missing byte");
        }
    }

    private static void VerifyTourianAccessPhysicalDrawMapping(SuperMetroidAddressSpace rom)
    {
        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            TourianAccessPlmDrawDefinitions.All.ToArray();
        AssertEqual(5, draws.Length, "Tourian access owns four frames and a six-row clear");
        ushort[] pointers = [0x9297,0x92a3,0x92af,0x92bb,0x92c7];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, TourianAccessPlmDrawDefinitions.TryDescribe(pointer, out int rows, out ushort word), "Tourian draw descriptor ownership");
            AssertEqual(index >= 0, TourianAccessPlmDrawDefinitions.TryGet(pointer, out var dto), "Tourian draw DTO ownership");
            if (index < 0)
            {
                AssertEqual(0, rows, "missing Tourian rows zero");
                AssertEqual((ushort)0, word, "missing Tourian word zero");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing Tourian DTO default");
                continue;
            }
            AssertEqual(pointer, draws[index].Pointer, "Tourian draw export order");
            AssertEqual(pointer == 0x92c7 ? 6 : 1, rows, "Tourian native row count");
            AssertEqual(rows, dto.Runs.Length, "Tourian DTO row count");
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < 4; column++)
            {
                ushort native = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + row * 12 + column * 2 + 2));
                AssertEqual(native, word, "Tourian scalar covers every native cell");
                AssertEqual(native, dto.Runs.Span[row].LevelWords.Span[column], "Tourian direct DTO word");
            }
        }
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
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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
