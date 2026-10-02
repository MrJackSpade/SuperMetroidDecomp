using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledSporeSpawnCeilingPlms()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual("12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling oracle revision");
        VerifySporeSpawnCeilingProgramControls(rom);
        VerifySporeSpawnCeilingProgramDraws(rom);
        VerifySporeSpawnCeilingProgramSound(rom);

        VerifySporeSpawnCeilingDrawMapping(rom);

        VerifySporeSpawnCeiling(clear: false);
        VerifySporeSpawnCeiling(clear: true);
        Console.WriteLine(
            "Spore Spawn ceiling: two native lists and four 2x2 physical draws match ROM; crumble/clear, sound and deletion run with source bytes forbidden.");
    }

    private static void VerifySporeSpawnCeilingProgramControls(SuperMetroidAddressSpace rom) =>
        VerifySporeSpawnCeilingProgramField(rom, false);

    private static void VerifySporeSpawnCeilingProgramDraws(SuperMetroidAddressSpace rom)
    {
        VerifySporeSpawnCeilingProgramField(rom, true);
        ushort[] originalOperands = [0xab17,0xab1b,0xab1f];
        for (int frame = 0; frame < originalOperands.Length; frame++)
            AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | originalOperands[frame]),
                SporeSpawnCeilingPlmDrawDefinitions.CrumbleFramePointer(frame), "Spore ceiling native frame selector");
        foreach (int invalid in new[] {int.MinValue,-1,3,4,255,256,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => SporeSpawnCeilingPlmDrawDefinitions.CrumbleFramePointer(invalid),
                "Spore ceiling frame selector bounds");
    }

    private static void VerifySporeSpawnCeilingProgramField(SuperMetroidAddressSpace rom, bool draw)
    {
        ushort[] controls = [0xab12,0xab15,0xab19,0xab1d,0xab21,0xab25];
        ushort[] operands = [0xab17,0xab1b,0xab1f,0xab23];
        ushort[] expectedOrder = [0xab12,0xab15,0xab17,0xab19,0xab1b,0xab1d,0xab1f,0xab21,0xab23,0xab25];
        AssertTrue(expectedOrder.SequenceEqual(SporeSpawnCeilingPlmProgramDefinitions.NativeWordAddresses()),
            "Spore ceiling original instruction enumeration");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort address = (ushort)raw;
            bool owned = controls.Contains(address) || operands.Contains(address);
            AssertEqual(owned, SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsWord(address, out ushort actual),
                "Spore ceiling complete word ownership including packed-byte gaps");
            if (!owned) AssertEqual((ushort)0, actual, "Spore ceiling unowned word zero");
            else if ((draw ? operands : controls).Contains(address))
            {
                AssertEqual(ReadSamusEaterPlmWord(rom, 0x840000 | raw), actual, "Spore ceiling original program field");
                AssertTrue(RoomPlmProgramDefinitions.TryReadWord(address, out ushort shared), "Spore ceiling shared word reader");
                AssertEqual(actual, shared, "Spore ceiling shared reader value");
            }
        }
    }

    private static void VerifySporeSpawnCeilingProgramSound(SuperMetroidAddressSpace rom)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            AssertEqual(raw == 0xab14,
                SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsByte((ushort)raw, out byte actual),
                "Spore ceiling byte ownership is exactly its sound operand");
            AssertEqual(raw == 0xab14 ? rom.ReadByte(0x84ab14) : (byte)0, actual,
                "Spore ceiling original sound or cleared missing byte");
        }
    }

    private static void VerifySporeSpawnCeilingDrawMapping(SuperMetroidAddressSpace rom)
    {
        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            SporeSpawnCeilingPlmDrawDefinitions.All.ToArray();
        AssertEqual(4, draws.Length,
            "Spore Spawn ceiling has clear and three crumble appearances");
        ushort[] pointers = [0x9413,0x9423,0x9433,0x9443];
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int index = Array.IndexOf(pointers, pointer);
            AssertEqual(index >= 0, SporeSpawnCeilingPlmDrawDefinitions.TryGetWord(pointer, out ushort word), "Spore ceiling scalar ownership");
            AssertEqual(index >= 0, SporeSpawnCeilingPlmDrawDefinitions.TryGet(pointer, out var dto), "Spore ceiling DTO ownership");
            if (index < 0)
            {
                AssertEqual((ushort)0, word, "missing Spore ceiling scalar zero");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing Spore ceiling DTO default");
                continue;
            }
            AssertEqual(pointer, draws[index].Pointer, "Spore ceiling native export order");
            for (int row = 0; row < 2; row++)
            for (int column = 0; column < 2; column++)
            {
                ushort native = ReadSamusEaterPlmWord(rom, 0x840000 | (pointer + row * 8 + column * 2 + 2));
                AssertEqual(native, word, "Spore ceiling calculated scalar covers each native cell");
                AssertEqual(native, dto.Runs.Span[row].LevelWords.Span[column], "Spore ceiling direct DTO word");
            }
        }
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertDrawWord(run.DirectionAndCount);
                foreach (ushort word in run.LevelWords.Span)
                    AssertDrawWord(word);
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextX),
                    $"Spore Spawn draw ${draw.Pointer:X4} signed X offset");
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextY),
                    $"Spore Spawn draw ${draw.Pointer:X4} signed Y offset");
            }
            AssertEqual(16, cursor - draw.Pointer,
                $"Spore Spawn draw ${draw.Pointer:X4} covers its native 2x2 span");

            void AssertDrawWord(ushort expected)
            {
                AssertEqual((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8),
                    expected,
                    $"Spore Spawn draw ${draw.Pointer:X4} word +{cursor - draw.Pointer}");
                cursor += 2;
            }
        }

    }

    private static void VerifySporeSpawnCeiling(bool clear)
    {
        const int width = 16;
        const int height = 32;
        ushort[] words = new ushort[width * height];
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnSporeSpawnCeiling(level,
                clear ? RoomPlmHeaders.ClearSporeSpawnCeiling :
                    RoomPlmHeaders.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = new SporeSpawnCeilingSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        int soundFrame = -1;
        for (int frame = 0; frame < 32 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, 0, 16 * 16, 0);
            if (plms.SoundRequests.Any(request =>
                    request.SoundEffect == SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        SporeSpawnCeilingPlmProgramDefinitions.CrumbleSoundId)))
                soundFrame = frame;
            if (frame % 4 == 0 && frame <= (clear ? 0 : 12))
            {
                ushort expected = clear ? (ushort)0x00ff : (ushort)(frame switch
                {
                    0 => 0x0053,
                    4 => 0x0054,
                    8 => 0x0055,
                    _ => 0x00ff,
                });
                for (int y = 30; y <= 31; y++)
                for (int x = 7; x <= 8; x++)
                    AssertEqual(expected,
                        level.GetCollisionBlockByIndex(y * width + x).LevelWord,
                        $"Spore Spawn {(clear ? "clear" : "crumble")} frame {frame} ({x},{y})");
            }
            if (plms.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 4 : 16, deletionFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} native deletion frame");
        AssertEqual(clear ? -1 : 0, soundFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} sound frame");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Spore Spawn ceiling reads no migrated program or draw bytes");
    }

    private sealed class SporeSpawnCeilingSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= SporeSpawnCeilingPlmProgramDefinitions.Crumble &&
                  pointer < SporeSpawnCeilingPlmProgramDefinitions.EndExclusive) ||
                 (pointer >= SporeSpawnCeilingPlmDrawDefinitions.ClearPointer &&
                  pointer < SporeSpawnCeilingPlmDrawDefinitions.EndExclusive)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Spore Spawn ceiling reread migrated source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
