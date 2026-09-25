using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledSporeSpawnCeilingPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = SporeSpawnCeilingPlmProgramDefinitions
            .NativeWordAddresses().ToArray();
        AssertEqual(10, addresses.Length,
            "Spore Spawn ceiling owns ten instruction words");
        AssertEqual(addresses.Length, addresses.Distinct().Count(),
            "Spore Spawn ceiling instruction words do not overlap");
        foreach (ushort address in addresses)
        {
            AssertTrue(SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Spore Spawn ceiling word $84:{address:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8),
                compiled,
                $"Spore Spawn ceiling word $84:{address:X4} matches ROM");
        }
        ushort soundPointer = checked((ushort)(
            SporeSpawnCeilingPlmProgramDefinitions.Crumble + 2));
        AssertTrue(SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsByte(
                soundPointer, out byte sound),
            "Spore Spawn ceiling sound operand is compiled");
        AssertEqual(rom.ReadByte(0x840000 | soundPointer), sound,
            "Spore Spawn ceiling sound matches ROM");
        AssertTrue(!SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsWord(
                SporeSpawnCeilingPlmProgramDefinitions.EndExclusive, out _),
            "adjacent Botwoon setup code is not claimed as instruction data");

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            SporeSpawnCeilingPlmDrawDefinitions.All.ToArray();
        AssertEqual(4, draws.Length,
            "Spore Spawn ceiling has clear and three crumble appearances");
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

        VerifySporeSpawnCeiling(clear: false);
        VerifySporeSpawnCeiling(clear: true);
        Console.WriteLine(
            "Spore Spawn ceiling: two native lists and four 2x2 physical draws match ROM; crumble/clear, sound and deletion run with source bytes forbidden.");
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
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

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
