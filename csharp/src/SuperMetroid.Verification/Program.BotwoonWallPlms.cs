using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledBotwoonWallPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = BotwoonWallPlmProgramDefinitions
            .NativeWordAddresses().ToArray();
        AssertEqual(18, addresses.Length,
            "Botwoon wall owns eighteen instruction words");
        AssertEqual(addresses.Length, addresses.Distinct().Count(),
            "Botwoon wall instruction words do not overlap");
        foreach (ushort address in addresses)
        {
            AssertTrue(BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Botwoon wall word $84:{address:X4} is compiled");
            AssertEqual((ushort)(rom.ReadByte(0x840000 | address) |
                    rom.ReadByte(0x840000 | (address + 1)) << 8),
                compiled,
                $"Botwoon wall word $84:{address:X4} matches ROM");
        }
        foreach (ushort address in new ushort[]
        {
            BotwoonWallPlmProgramDefinitions.Crumble + 2,
            BotwoonWallPlmProgramDefinitions.Crumble + 7,
        })
        {
            AssertTrue(BotwoonWallPlmProgramDefinitions.TryReadMechanicsByte(
                    address, out byte compiled),
                $"Botwoon wall byte $84:{address:X4} is compiled");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"Botwoon wall byte $84:{address:X4} matches ROM");
        }
        AssertTrue(!BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                BotwoonWallPlmProgramDefinitions.CrumbleEndExclusive, out _),
            "Botwoon scroll callback is not claimed as program data");
        AssertTrue(!BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                BotwoonWallPlmProgramDefinitions.ClearEndExclusive, out _),
            "following Kraid program is not claimed by Botwoon");

        RoomPlmShotBlockDrawDefinitions.DrawList draw =
            BotwoonWallPlmDrawDefinitions.Clear;
        AssertEqual(BotwoonWallPlmDrawDefinitions.ClearPointer, draw.Pointer,
            "Botwoon clear draw pointer");
        AssertEqual(1, draw.Runs.Length, "Botwoon clear has one vertical run");
        AssertEqual((ushort)0x8009, draw.Runs.Span[0].DirectionAndCount,
            "Botwoon clear is nine blocks tall");
        int cursor = draw.Pointer;
        AssertWord(draw.Runs.Span[0].DirectionAndCount);
        foreach (ushort word in draw.Runs.Span[0].LevelWords.Span)
            AssertWord(word);
        AssertEqual(rom.ReadByte(0x840000 | cursor++),
            unchecked((byte)draw.Runs.Span[0].NextX),
            "Botwoon clear final X offset");
        AssertEqual(rom.ReadByte(0x840000 | cursor++),
            unchecked((byte)draw.Runs.Span[0].NextY),
            "Botwoon clear final Y offset");
        AssertEqual(BotwoonWallPlmDrawDefinitions.EndExclusive, cursor,
            "Botwoon clear draw covers exactly its native span");

        VerifyBotwoonWall(clear: false);
        VerifyBotwoonWall(clear: true);
        Console.WriteLine(
            "Botwoon wall: ROM-matched programs and nine-block clear draw run their timed crumble/clear paths without source reads.");

        void AssertWord(ushort expected)
        {
            AssertEqual((ushort)(rom.ReadByte(0x840000 | cursor) |
                    rom.ReadByte(0x840000 | (cursor + 1)) << 8),
                expected, $"Botwoon draw word +{cursor - draw.Pointer}");
            cursor += 2;
        }
    }

    private static void VerifyBotwoonWall(bool clear)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = 0x0123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnBotwoonWall(level,
                clear ? RoomPlmHeaders.ClearBotwoonWall :
                    RoomPlmHeaders.CrumbleBotwoonWall),
            $"Botwoon {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = new BotwoonWallSourceGuard(new TestAddressSpace());
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            guarded, 2, 1, RoomScrollState.RedBoundary);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        int soundCount = 0;
        for (int frame = 0; frame < 220 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, 0, 0, 0, scrolls);
            soundCount += plms.SoundRequests.Count(request =>
                request.SoundEffect == SoundEffectId.FromCartridge(
                    SoundEffectLibrary.Library2,
                    BotwoonWallPlmProgramDefinitions.CrumbleSoundId));
            if (clear && frame == 0)
            {
                for (int y = 4; y <= 12; y++)
                    AssertEqual((ushort)0x00ff,
                        level.GetCollisionBlockByIndex(y * width + 15).LevelWord,
                        $"Botwoon clear row {y}");
            }
            if (!clear && frame >= 63 && frame <= 203 &&
                (frame - 63) % 4 == 0)
            {
                int row = (frame - 63) / 16;
                int phase = (frame - 63) / 4 % 4;
                ushort appearance = phase switch
                {
                    0 => 0x0053,
                    1 => 0x0054,
                    2 => 0x0055,
                    _ => 0x00ff,
                };
                AssertEqual(appearance,
                    level.GetCollisionBlockByIndex((4 + row) * width + 15).LevelWord,
                    $"Botwoon crumble frame {frame}, row {row}");
                if (row == 0 && phase == 0)
                {
                    AssertEqual(RoomScrollState.Blue, scrolls.ReadState(0),
                        "Botwoon crumble opens first scroll cell");
                    AssertEqual(RoomScrollState.Blue, scrolls.ReadState(1),
                        "Botwoon crumble opens second scroll cell");
                }
            }
            if (plms.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 1 : 207, deletionFrame,
            $"Botwoon {(clear ? "clear" : "crumble")} deletion frame");
        AssertEqual(clear ? 0 : 9, soundCount,
            $"Botwoon {(clear ? "clear" : "crumble")} sound count");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Botwoon wall reads no migrated program or draw bytes");
    }

    private sealed class BotwoonWallSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= BotwoonWallPlmProgramDefinitions.Crumble &&
                  pointer < BotwoonWallPlmProgramDefinitions.CrumbleEndExclusive) ||
                 (pointer >= BotwoonWallPlmProgramDefinitions.Clear &&
                  pointer < BotwoonWallPlmProgramDefinitions.ClearEndExclusive) ||
                 (pointer >= BotwoonWallPlmDrawDefinitions.ClearPointer &&
                  pointer < BotwoonWallPlmDrawDefinitions.EndExclusive) ||
                 (pointer >= RoomPlmShotBlockDrawDefinitions.SingleFrame0 &&
                  pointer < RoomPlmShotBlockDrawDefinitions.HorizontalFrame0)))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Botwoon wall reread migrated source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
