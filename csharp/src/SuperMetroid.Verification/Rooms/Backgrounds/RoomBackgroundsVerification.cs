using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Hud;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Rooms.Backgrounds;

/// <summary>
/// Checks the room backgrounds the display description publishes (#1346): BG1 and BG2 resolved from level data
/// through the scroll registers, the door's source screen, words written over BG1, and PLM block changes.
/// </summary>
internal static class RoomBackgroundsVerification
{
    /// <summary>Blocks along each side of the constructed planes.</summary>
    private const int Side = 4;

    /// <summary>Tiles a fixture paints into each block picture: top left, top right, bottom left, bottom right.</summary>
    private const int BlockTiles = 4;

    /// <summary>Resolves constructed planes, a door window and an overlay.</summary>
    internal static void VerifyRoomBackgroundSources()
    {
        // Block n's picture paints the tiles of words n*4 .. n*4+3; the appearances select blocks with mirroring (#1431).
        var definitions = new BackgroundTile[16 * BlockTiles];
        for (int word = 0; word < definitions.Length; word++)
            definitions[word] = BackgroundTileWordBytes.Tile(word);
        BlockMirroring[] mirrorings = [BlockMirroring.None, BlockMirroring.Horizontal, BlockMirroring.Vertical,
            BlockMirroring.Horizontal | BlockMirroring.Vertical];
        var blocks = new RoomBlockAppearance[Side * Side];
        for (int block = 0; block < blocks.Length; block++)
            blocks[block] = RoomBlockAppearance.Of(block, mirrorings[block % mirrorings.Length]);
        var plane = new RoomLevelPlane(new Extent16<RoomBlocks>(Side, Side), blocks);
        // The destination and the door's source room draw block pictures of different characters (#1403).
        var random = new Random(1346);
        var palette = new RgbColor[SnesCgram.ColorCount];
        byte[] destinationImage = new byte[SnesVram.ByteCount], sourceImage = new byte[SnesVram.ByteCount];
        random.NextBytes(destinationImage);
        random.NextBytes(sourceImage);
        var layer = new RoomBackgroundLayer(plane.Cells, RoomBlockFixtures.Pictures(definitions, destinationImage, palette));

        // Each level pixel shows its appearance's whole block, by index and mirroring (#1431).
        int checks = 0;
        for (int y = 0; y < Side * RoomLevelPlane.BlockPixels; y += 3)
        for (int x = 0; x < Side * RoomLevelPlane.BlockPixels; x += 5)
        {
            var level = new Position16<PlanePixels>((ushort)x, (ushort)y);
            RoomBlockAppearance shown = blocks[y / RoomLevelPlane.BlockPixels * Side + x / RoomLevelPlane.BlockPixels];
            AssertTrue(layer.Blocks.TryCellAt(level, out RoomBlockCell cell, out Position16<PlanePixels> within),
                $"level pixel {x},{y} is inside the room");
            AssertEqual(new RoomBlockCell(shown.Block, shown.Mirroring), cell,
                $"level pixel {x},{y} shows the appearance's block and mirroring");
            AssertEqual(new Position16<PlanePixels>((ushort)(x % RoomLevelPlane.BlockPixels), (ushort)(y % RoomLevelPlane.BlockPixels)), within,
                $"level pixel {x},{y} lies at its place within the block");
            checks++;
        }
        AssertTrue(!layer.Blocks.TryCellAt(new Position16<PlanePixels>(0, Side * RoomLevelPlane.BlockPixels), out _, out _),
            "a level pixel below the room shows no block");
        AssertTrue(!layer.Blocks.TryCellAt(new Position16<PlanePixels>(0, 0) + new Offset16<PlanePixels>(-1, 0), out _, out _),
            "a scroll left of the room shows no block");

        // The door's source screen covers the window where the destination's view begins, below the HUD and the first
        // blank scanline, and shows the source room from the row the source's view showed there (#1555).
        var sourceBlocks = Enumerable.Repeat(RoomBlockAppearance.Of(15), Side * Side).ToArray();
        RoomBlockPictureTable sourcePictures = RoomBlockFixtures.Pictures(definitions, sourceImage, palette);
        var source = new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(Side, Side), sourceBlocks).Cells, sourcePictures);
        var sourceView = new Position16<PlanePixels>(0x20, 0x10);
        var destinationView = new Position16<PlanePixels>(0x300, 0x100);
        var door = RoomDoorBackground.Begin(source, sourceView, destinationView, downward: false);
        var belowHud = new Offset16<PlanePixels>(0, ScreenGeometry.LayerRowsHiddenAboveScreen + ScreenGeometry.GameplayHudHeightPixels);
        Position16<PlanePixels> top = destinationView + belowHud + new Offset16<PlanePixels>(0x18, 0);
        AssertTrue(door.Covers(top) && ReferenceEquals(door.SourceRoom.Pictures, sourcePictures),
            "the door window's first gameplay line shows the source room");
        AssertTrue(!door.Covers(top + new Offset16<PlanePixels>(0, -1)),
            "the line above the window shows the destination room");
        AssertEqual(sourceView + belowHud, door.SourceOrigin, "the window's first gameplay pixel shows the source room's row below its view");
        AssertEqual(destinationView + belowHud - new Offset16<PlanePixels>(0, 1),
            RoomDoorBackground.Begin(source, sourceView, destinationView, downward: true).WindowOrigin,
            "a downward door's source screen lies one row higher on the destination");

        Console.WriteLine($"  Room backgrounds: {checks} level samples and the door window agree.");
    }

    /// <summary>A PLM's block change reaches the next published plane, and an earlier plane keeps its blocks.</summary>
    internal static void VerifyObjectBlockChangeReachesPlane()
    {
        RoomLevelData level = CreateRoom(new(Side, Side), new RoomTerrainBlock[Side * Side], new RoomBlockBehavior[Side * Side]);
        RoomLevelPlane before = level.ForegroundPlane;
        AssertTrue(ReferenceEquals(before, level.ForegroundPlane), "an unchanged room publishes one plane");
        const int block = 2 * Side + 1;
        level.SetObjectTerrain(block, RoomTerrainBlock.Of(7, BlockMirroring.None, RoomCollisionType.SolidBlock));
        RoomLevelPlane after = level.ForegroundPlane;
        AssertEqual(new RoomBlockCell(0x007, BlockMirroring.None), after.Cells.Cells[block],
            "the next plane shows the PLM's block");
        AssertEqual(new RoomBlockCell(0x000, BlockMirroring.None), before.Cells.Cells[block],
            "the earlier plane keeps the block it published");
        Console.WriteLine("  Room backgrounds: a PLM block change reaches the next plane only.");
    }

    /// <summary>
    /// Loads retail rooms and checks the published description: BG1 is the room's foreground plane, BG2 is its layer-2
    /// plane only when layer 2 streams, and BG1 at the scroll shows the level at the camera.
    /// </summary>
    internal static void VerifyPublishedRoomBackgrounds()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The room-state data selects layer-2 scroll $00/$C0 for the main shaft (streams) and $01/$01 for Kraid (fixed).
        foreach ((RoomDefinition room, bool streams) in new[] { new VerifyPublishedRoomBackgroundsCase(RoomDefinitions.GreenBrinstarMainShaft, true), new VerifyPublishedRoomBackgroundsCase(RoomDefinitions.Kraid, false) })
        {
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0x30));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.PublishDisplayFrame();
            RoomBackgrounds published = runtime.Display.Latched.Rooms
                ?? throw new InvalidOperationException("A loaded room publishes its backgrounds.");
            RoomLevelData level = runtime.Room.LoadedLevelData;
            AssertTrue(ReferenceEquals(level.ForegroundPlane.Cells, published.Foreground.Blocks), $"{room.Name} BG1 is the room's foreground plane");
            AssertEqual(streams, published.Background is not null, $"{room.Name} BG2 comes from level data only when layer 2 streams");
            if (published.Background is { } bg2)
            {
                AssertTrue(ReferenceEquals(level.BackgroundPlane.Cells, bg2.Blocks), $"{room.Name} BG2 is the room's layer-2 plane");
            }
            Position16<PlanePixels> scroll = runtime.BackgroundScroll.ForegroundScroll.AsPlaneScroll();
            Position16<PlanePixels> camera = runtime.BackgroundScroll.ForegroundPosition.AsPlaneScroll();
            for (int y = 0; y < 192; y += 37)
            for (int x = 0; x < 256; x += 41)
            {
                // The block BG1 shows at a screen pixel, through its scroll, is the level's block at the camera.
                var screen = new Offset16<PlanePixels>((short)x, (short)y);
                AssertTrue(published.Foreground.Blocks.TryCellAt(scroll + screen, out RoomBlockCell shown, out var shownWithin) &&
                        level.ForegroundPlane.Cells.TryCellAt(camera + screen, out RoomBlockCell expected, out var expectedWithin)
                        && shown == expected && shownWithin == expectedWithin,
                    $"{room.Name} BG1 at screen {x},{y} shows the level at the camera");
            }
        }
        Console.WriteLine("  Room backgrounds: published planes, scrolls and camera agree for streaming and fixed BG2 rooms.");
    }

    /// <summary>Runs the room background checks.</summary>
    [VerificationCommand("--room-backgrounds")]
    internal static int RoomBackgroundsCommand()
    {
        Suite(nameof(VerifyRoomBackgroundSources), () => VerifyRoomBackgroundSources());
        Suite(nameof(VerifyObjectBlockChangeReachesPlane), () => VerifyObjectBlockChangeReachesPlane());
        Suite(nameof(VerifyPublishedRoomBackgrounds), () => VerifyPublishedRoomBackgrounds());
        return 0;
    }
}
