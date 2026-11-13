using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Botwoon's timed wall crumble and clear PLMs against bank 84 data.</summary>
internal static class BotwoonWallObjectsVerification
{
    /// <summary>Compares program and nine-block draw words, then checks both wall paths through deletion.</summary>
    internal static void VerifyCompiledBotwoonWallObjects()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(BossRoomObjectProgramsVerification.VerifyBotwoonWallPrograms), () => BossRoomObjectProgramsVerification.VerifyBotwoonWallPrograms(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList draw =
            RoomObjectBotwoonWallVisualFiles.All.Single();
        AssertEqual((ushort)0x930f, RoomObjectDrawAddressCodec.Of(draw.Draw),
            "Botwoon clear draw pointer");
        AssertEqual(1, draw.Runs.Length, "Botwoon clear has one vertical run");
        AssertEqual(RoomObjectRunDirection.Vertical, draw.Runs.Span[0].Direction,
            "Botwoon clear runs down its column");
        AssertEqual(9, draw.Runs.Span[0].Count,
            "Botwoon clear is nine blocks tall");
        int cursor = RoomObjectDrawAddressCodec.Of(draw.Draw);
        AssertRunHeader(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor), draw.Runs.Span[0],
            "Botwoon clear run header");
        cursor += 2;
        foreach (RoomCollisionType collision in draw.Runs.Span[0].Collisions.Span)
            AssertCollision(collision);
        RoomObjectFrameImage picture = InstalledRoomObjectBotwoonWallVisuals.Stock().Image(draw.Draw);
        if (!BotwoonWallObjectDraws.TryDescribe(draw.Draw, out RoomBlockShape? described))
            throw new InvalidOperationException("Botwoon's wall clear has no shape.");
        RoomBlockShape shape = described;
        AssertEqual(9, shape.Cells.Count, "Botwoon clear shape covers exactly the column's nine cells");
        for (int index = 0; index < 9; index++)
        {
            RoomTerrainBlock native = RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x849311 + index * 2));
            Offset16<RoomBlocks> place = new(0, index);
            AssertEqual(place, draw.Runs.Span[0].CellAt(index), "Botwoon clear run places the native word down its column");
            AssertEqual(native.Collision, shape.CollisionAt(place), "Botwoon clear shape collision matches the native word");
            AssertEqual(native.Appearance, picture.At(place), "Botwoon installed clear picture shows the native word's block");
        }
        foreach (Offset16<RoomBlocks> outside in new Offset16<RoomBlocks>[] { new(0, -1), new(0, 9), new(-1, 0), new(1, 0) })
            AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(outside), "Botwoon clear shape covers only its column");
        AssertTrue(!BotwoonWallObjectDraws.TryDescribe(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0), out _),
            "Botwoon's crumble frames are shot-block draws, not the wall's own");
        AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
            unchecked((byte)RoomObjectRunContinuationCodecConstants.X(draw, 0)),
            "Botwoon clear final X offset");
        AssertEqual(rom.ReadCartridgeByte(0x840000 | cursor++),
            unchecked((byte)RoomObjectRunContinuationCodecConstants.Y(draw, 0)),
            "Botwoon clear final Y offset");
        AssertEqual(BotwoonWallObjectDrawDefinitions.EndExclusive, cursor,
            "Botwoon clear draw covers exactly its native span");

        Suite(nameof(VerifyBotwoonWall), () => VerifyBotwoonWall(clear: false));
        Suite(nameof(VerifyBotwoonWall), () => VerifyBotwoonWall(clear: true));
        Console.WriteLine(
            "Botwoon wall: typed programs match ROM, and they and the nine-block clear draw run their timed crumble/clear paths without source reads.");

        void AssertCollision(RoomCollisionType expected)
        {
            AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0x840000 | cursor)).Collision,
                expected, $"Botwoon draw collision +{cursor - RoomObjectDrawAddressCodec.Of(draw.Draw)}");

            cursor += 2;
        }
    }

    /// <summary>Checks the crumble's nine row cadence and sound requests or the immediate clear path.</summary>
    private static void VerifyBotwoonWall(bool clear)
    {
        const int width = 32;
        const int height = 16;
        var words = new RoomTerrainBlock[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.Air);
        RoomLevelData level = CreateRoom(new(width, height), words,
            new RoomBlockBehavior[words.Length]);
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BossRoomMutations.BotwoonWall.TrySpawn(level,
                clear ? BotwoonWallMutation.Clear :
                    BotwoonWallMutation.Crumble),
            $"Botwoon {(clear ? "clear" : "crumble")} PLM allocates");
        var guarded = NewBotwoonWallSourceGuard(new TestAddressSpace());
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            RoomScrollGrid.PowerOnCells(), new Extent16<RoomScreens>(2, 1), RoomScrollState.RedBoundary);
        int deletionFrame = -1;
        int soundCount = 0;
        for (int frame = 0; frame < 220 && roomObjects.SlotTable.ActiveCount != 0; frame++)
        {
            roomObjects.Step(level, scrolls);
            soundCount += roomObjects.Sounds.SoundRequests.Count(request =>
                request.SoundEffect == BotwoonWallMutationDefinitions.CrumbleSound);
            if (clear && frame == 0)
            {
                for (int y = 4; y <= 12; y++)
                    AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.Air),
                        level.GetCollisionBlock(new Position16<RoomBlocks>(15, y)).Terrain,
                        $"Botwoon clear row {y}");
            }
            if (!clear && frame >= 63 && frame <= 203 &&
                (frame - 63) % 4 == 0)
            {
                int row = (frame - 63) / 16;
                int phase = (frame - 63) / 4 % 4;
                RoomTerrainBlock appearance = phase switch
                {
                    0 => RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air),
                    1 => RoomTerrainBlock.Of(0x054, BlockMirroring.None, RoomCollisionType.Air),
                    2 => RoomTerrainBlock.Of(0x055, BlockMirroring.None, RoomCollisionType.Air),
                    _ => BlankAirBlock.Value,
                };
                AssertEqual(appearance,
                    level.GetCollisionBlock(new Position16<RoomBlocks>(15, 4 + row)).Terrain,
                    $"Botwoon crumble frame {frame}, row {row}");
                if (row == 0 && phase == 0)
                {
                    AssertEqual(RoomScrollState.Blue, scrolls.ReadState(0),
                        "Botwoon crumble opens first scroll cell");
                    AssertEqual(RoomScrollState.Blue, scrolls.ReadState(1),
                        "Botwoon crumble opens second scroll cell");
                }
            }
            if (roomObjects.SlotTable.ActiveCount == 0)
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
                    (BossRoomObjectNativeListAddresses.IsTypedProgramByte(pointer) ||
                     (pointer >= RoomObjectDrawSourceDefinitions.BotwoonWall[BotwoonWallDraw.Clear].Offset &&
                      pointer < BotwoonWallObjectDrawDefinitions.EndExclusive) ||
                     (pointer >= RoomObjectDrawSourceDefinitions.SingleBreakupFirst.Offset &&
                      pointer < RoomObjectDrawSourceDefinitions.HorizontalBreakupFirst.Offset));
            }, address => $"Botwoon wall reread migrated source ${address:X6}.");

    /// <summary>Runs Botwoon wall program, draw, and timed execution checks.</summary>
    [VerificationCommand("--botwoon-wall-plms")]
    internal static int BotwoonWallObjectsCommand()
    {
        Suite(nameof(VerifyCompiledBotwoonWallObjects), () => VerifyCompiledBotwoonWallObjects());
        return 0;
    }
}
