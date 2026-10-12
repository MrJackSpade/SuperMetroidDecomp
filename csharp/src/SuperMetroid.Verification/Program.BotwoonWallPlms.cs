using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static AssertionsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

internal static class BotwoonWallPlmsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Botwoon wall PLMs")]
    internal static void VerifyCompiledBotwoonWallPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = [0xab31,0xab34,0xab36,0xab39,0xab3b,0xab3d,0xab3f,0xab41,0xab43,
            0xab45,0xab47,0xab49,0xab4b,0xab4d,0xab4f,0xab67,0xab69,0xab6b];
        AssertTrue(addresses.SequenceEqual(BotwoonWallPlmProgramDefinitions.NativeWordAddresses()),
            "Botwoon program enumeration matches independent native positions");
        AssertEqual(18, addresses.Length,
            "Botwoon wall owns eighteen instruction words");
        AssertEqual(addresses.Length, addresses.Distinct().Count(),
            "Botwoon wall instruction words do not overlap");
        foreach (ushort address in addresses)
        {
            AssertTrue(BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                    address, out ushort compiled),
                $"Botwoon wall word $84:{address:X4} is compiled");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | address),
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
            AssertEqual(rom.ReadCartridgeByte(0x840000 | address), compiled,
                $"Botwoon wall byte $84:{address:X4} matches ROM");
        }
        AssertTrue(!BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                BotwoonWallPlmProgramDefinitions.CrumbleEndExclusive, out _),
            "Botwoon scroll callback is not claimed as program data");
        AssertTrue(!BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(
                BotwoonWallPlmProgramDefinitions.ClearEndExclusive, out _),
            "following Kraid program is not claimed by Botwoon");
        var knownWords = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool wordFound = BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort word);
            AssertEqual(knownWords.Contains((ushort)address), wordFound, "Botwoon full word ownership includes packed unaligned controls");
            if (!wordFound) AssertEqual((ushort)0, word, "Botwoon unknown word output cleared");
            bool byteFound = BotwoonWallPlmProgramDefinitions.TryReadMechanicsByte((ushort)address, out byte value);
            AssertEqual(address is 0xab33 or 0xab38, byteFound, "Botwoon packed timer/sound ownership");
            if (!byteFound) AssertEqual((byte)0, value, "Botwoon unknown byte output cleared");
        }

        RoomPlmShotBlockDrawDefinitions.DrawList draw =
            BotwoonWallPlmDrawDefinitions.All.Single();
        AssertEqual(BotwoonWallPlmDrawDefinitions.ClearPointer, draw.Pointer,
            "Botwoon clear draw pointer");
        AssertEqual(1, draw.Runs.Length, "Botwoon clear has one vertical run");
        AssertEqual((ushort)0x8009, draw.Runs.Span[0].DirectionAndCount,
            "Botwoon clear is nine blocks tall");
        int cursor = draw.Pointer;
        AssertWord(draw.Runs.Span[0].DirectionAndCount);
        foreach (ushort word in draw.Runs.Span[0].LevelWords.Span)
            AssertWord(word);
        for (int index = 0; index < 9; index++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x849311 + index * 2),
                BotwoonWallPlmDrawDefinitions.LevelWordAt(index), "Botwoon calculated clear word matches native");
        foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
            AssertThrows<IndexOutOfRangeException>(() => BotwoonWallPlmDrawDefinitions.LevelWordAt(index), "Botwoon fill rejects invalid block index");
        AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
            unchecked((byte)draw.Runs.Span[0].Next.X),
            "Botwoon clear final X offset");
        AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
            unchecked((byte)draw.Runs.Span[0].Next.Y),
            "Botwoon clear final Y offset");
        AssertEqual(BotwoonWallPlmDrawDefinitions.EndExclusive, cursor,
            "Botwoon clear draw covers exactly its native span");

        Suite(nameof(VerifyBotwoonWall), () => VerifyBotwoonWall(clear: false));
        Suite(nameof(VerifyBotwoonWall), () => VerifyBotwoonWall(clear: true));
        Console.WriteLine(
            "Botwoon wall: ROM-matched programs and nine-block clear draw run their timed crumble/clear paths without source reads.");

        void AssertWord(ushort expected)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
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
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.BossRoomMutations.TrySpawnBotwoonWall(level,
                clear ? PlmHeaderId.ClearBotwoonWall :
                    PlmHeaderId.CrumbleBotwoonWall),
            $"Botwoon {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewBotwoonWallSourceGuard(new TestAddressSpace());
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            guarded, new Extent16<RoomScreens>(2, 1), RoomScrollState.RedBoundary);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        int deletionFrame = -1;
        int soundCount = 0;
        for (int frame = 0; frame < 220 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
            soundCount += plms.Sounds.SoundRequests.Count(request =>
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
            if (plms.SlotTable.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 1 : 207, deletionFrame,
            $"Botwoon {(clear ? "clear" : "crumble")} deletion frame");
        AssertEqual(clear ? 0 : 9, soundCount,
            $"Botwoon {(clear ? "clear" : "crumble")} sound count");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "Botwoon wall reads no migrated program or draw bytes");
    }

    // Room scroll storage keeps WRAM bytes across rooms; the guard serves WRAM, guarding only cartridge data.
    internal static CartridgeReadGuard NewBotwoonWallSourceGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    ((pointer >= BotwoonWallPlmProgramDefinitions.Crumble &&
                      pointer < BotwoonWallPlmProgramDefinitions.CrumbleEndExclusive) ||
                     (pointer >= BotwoonWallPlmProgramDefinitions.Clear &&
                      pointer < BotwoonWallPlmProgramDefinitions.ClearEndExclusive) ||
                     (pointer >= BotwoonWallPlmDrawDefinitions.ClearPointer &&
                      pointer < BotwoonWallPlmDrawDefinitions.EndExclusive) ||
                     (pointer >= RoomPlmShotBlockDrawDefinitions.SingleFrame0 &&
                      pointer < RoomPlmShotBlockDrawDefinitions.HorizontalFrame0));
            }, address => $"Botwoon wall reread migrated source ${address:X6}.");

    [VerificationCommand("--botwoon-wall-plms")]
    internal static int BotwoonWallPlmsCommand()
    {
        Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
        return 0;
    }
}
