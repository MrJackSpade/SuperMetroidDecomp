using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Botwoon's timed wall crumble and clear PLMs against bank 84 data.</summary>
internal static class BotwoonWallPlmsVerification
{
    /// <summary>Compares program and nine-block draw words, then checks both wall paths through deletion.</summary>
    internal static void VerifyCompiledBotwoonWallPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(BossRoomPlmProgramsVerification.VerifyBotwoonWallPrograms), () => BossRoomPlmProgramsVerification.VerifyBotwoonWallPrograms(rom));

        RoomPlmShotBlockDrawDefinitions.DrawList draw =
            BotwoonWallPlmDrawDefinitions.All.Single();
        AssertEqual((ushort)0x930f, PlmDrawAddressCodec.Of(draw.Draw),
            "Botwoon clear draw pointer");
        AssertEqual(1, draw.Runs.Length, "Botwoon clear has one vertical run");
        AssertEqual(0x8009, draw.Runs.Span[0].DirectionAndCount,
            "Botwoon clear is nine blocks tall");
        int cursor = PlmDrawAddressCodec.Of(draw.Draw);
        AssertWord(checked((ushort)draw.Runs.Span[0].DirectionAndCount));
        foreach (ushort word in draw.Runs.Span[0].LevelWords.Span)
            AssertWord(word);
        for (int index = 0; index < 9; index++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x849311 + index * 2),
                BotwoonWallPlmDrawDefinitions.LevelWordAt(index), "Botwoon calculated clear word matches native");
        foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
            AssertThrows<ArgumentOutOfRangeException>(() => BotwoonWallPlmDrawDefinitions.LevelWordAt(index), "Botwoon fill rejects invalid block index");
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
            "Botwoon wall: typed programs match ROM, and they and the nine-block clear draw run their timed crumble/clear paths without source reads.");

        void AssertWord(ushort expected)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor),
                expected, $"Botwoon draw word +{cursor - PlmDrawAddressCodec.Of(draw.Draw)}");
            cursor += 2;
        }
    }

    /// <summary>Checks the crumble's nine row cadence and sound requests or the immediate clear path.</summary>
    private static void VerifyBotwoonWall(bool clear)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = 0x0123;
        RoomLevelData level = CreateRoom(new(width, height), words,
            new byte[words.Length], blockDefinitions: new BackgroundTile[0x400 * 4]);
        var plms = PlmFixtures.Unbound();
        AssertTrue(plms.BossRoomMutations.TrySpawnBotwoonWall(level,
                clear ? PlmHeaderId.ClearBotwoonWall :
                    PlmHeaderId.CrumbleBotwoonWall),
            $"Botwoon {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewBotwoonWallSourceGuard(new TestAddressSpace());
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            new RoomScrollStorage(), new Extent16<RoomScreens>(2, 1), RoomScrollState.RedBoundary);
        int deletionFrame = -1;
        int soundCount = 0;
        for (int frame = 0; frame < 220 && plms.SlotTable.ActiveCount != 0; frame++)
        {
            plms.Step(level, scrolls);
            soundCount += plms.Sounds.SoundRequests.Count(request =>
                request.SoundEffect == BotwoonWallPlmProgramDefinitions.CrumbleSound);
            if (clear && frame == 0)
            {
                for (int y = 4; y <= 12; y++)
                    AssertEqual((ushort)0x00ff,
                        level.GetCollisionBlockByIndex(y * width + 15).PackedWord.Raw,
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
                    level.GetCollisionBlockByIndex((4 + row) * width + 15).PackedWord.Raw,
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

    /// <summary>Forbids compiled program and draw reads while allowing persistent room-scroll WRAM access.</summary>
    internal static CartridgeReadGuard NewBotwoonWallSourceGuard(ISnesAddressSpace source) =>
        new MutableCartridgeReadGuard(source)
            .Forbid(address =>
            {
                int pointer = SnesAddressMath.OffsetInBank(address);
                return (SnesAddressMath.BankNumber(address)) == 0x84 &&
                    (BossRoomPlmNativeListAddresses.IsTypedProgramByte(pointer) ||
                     (pointer >= PlmDrawSourceDefinitions.BotwoonWall[BotwoonWallDraw.Clear].Offset &&
                      pointer < BotwoonWallPlmDrawDefinitions.EndExclusive) ||
                     (pointer >= PlmDrawSourceDefinitions.SingleBreakupFirst.Offset &&
                      pointer < PlmDrawSourceDefinitions.HorizontalBreakupFirst.Offset));
            }, address => $"Botwoon wall reread migrated source ${address:X6}.");

    /// <summary>Runs Botwoon wall program, draw, and timed execution checks.</summary>
    [VerificationCommand("--botwoon-wall-plms")]
    internal static int BotwoonWallPlmsCommand()
    {
        Suite(nameof(VerifyCompiledBotwoonWallPlms), () => VerifyCompiledBotwoonWallPlms());
        return 0;
    }
}
