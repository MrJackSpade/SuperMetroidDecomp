using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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

    /// <summary>Resolves constructed planes through scroll offsets, a door window and an overlay.</summary>
    internal static void VerifyRoomBackgroundSources()
    {
        // Block n's picture paints the tiles of words n*4 .. n*4+3; the level words select blocks with flips (#1431).
        var definitions = new BackgroundTile[16 * BlockTiles];
        for (int word = 0; word < definitions.Length; word++)
            definitions[word] = BackgroundTileWordBytes.Tile(word);
        var blocks = new ushort[Side * Side];
        for (int block = 0; block < blocks.Length; block++)
            blocks[block] = (ushort)((block % 4) << 10 | block);
        var plane = new RoomLevelPlane(new Extent16<RoomBlocks>(Side, Side), blocks);
        var offset = new Offset16<TilePlanePixels>(0x10, -0x08);
        // The destination and the door's source room draw block pictures of different characters (#1403).
        var random = new Random(1346);
        var palette = new RgbColor[SnesCgram.ColorCount];
        byte[] destinationImage = new byte[SnesVram.ByteCount], sourceImage = new byte[SnesVram.ByteCount];
        random.NextBytes(destinationImage);
        random.NextBytes(sourceImage);
        var layer = new RoomBackgroundLayer(plane.Cells, offset, RoomBlockFixtures.Pictures(definitions, destinationImage, palette));

        // Each level pixel shows its level word's whole block, by index and mirroring (#1431), drawn through the offset.
        AssertEqual(offset, layer.ScrollOffset, "the layer is drawn through the plane's scroll offset");
        int checks = 0;
        for (int y = 0; y < Side * RoomLevelPlane.BlockPixels; y += 3)
        for (int x = 0; x < Side * RoomLevelPlane.BlockPixels; x += 5)
        {
            var level = new Position16<TilePlanePixels>((ushort)x, (ushort)y);
            var word = new RoomLevelWord(blocks[y / RoomLevelPlane.BlockPixels * Side + x / RoomLevelPlane.BlockPixels]);
            AssertTrue(layer.Blocks.TryCellAt(level, out RoomBlockCell cell, out Position16<TilePlanePixels> within),
                $"level pixel {x},{y} is inside the room");
            AssertEqual(new RoomBlockCell(word.VisualBlockIndex, word.VisualFlipFlags), cell,
                $"level pixel {x},{y} shows the level word's block and mirroring");
            AssertEqual(new Position16<TilePlanePixels>((ushort)(x % RoomLevelPlane.BlockPixels), (ushort)(y % RoomLevelPlane.BlockPixels)), within,
                $"level pixel {x},{y} lies at its place within the block");
            checks++;
        }
        AssertTrue(!layer.Blocks.TryCellAt(new Position16<TilePlanePixels>(0, Side * RoomLevelPlane.BlockPixels), out _, out _),
            "a level pixel below the room shows no block");
        AssertTrue(!layer.Blocks.TryCellAt(new Position16<TilePlanePixels>(0, 0) + new Offset16<TilePlanePixels>(-1, 0), out _, out _),
            "a scroll left of the room shows no block");

        // The door's source screen covers the window at its scroll, below the HUD and the first blank scanline.
        var sourceBlocks = Enumerable.Repeat((ushort)15, Side * Side).ToArray();
        RoomBlockPictureTable sourcePictures = RoomBlockFixtures.Pictures(definitions, sourceImage, palette);
        var source = new RoomBackgroundLayer(new RoomLevelPlane(new Extent16<RoomBlocks>(Side, Side), sourceBlocks).Cells,
            new Offset16<TilePlanePixels>(0, 0), sourcePictures);
        var sourceScroll = new Position16<TilePlanePixels>(0, 0);
        var door = RoomDoorBackground.Begin(source, sourceScroll);
        var top = new Position16<TilePlanePixels>(0x18, ScreenGeometry.BackgroundRowOffset + ScreenGeometry.GameplayHudHeightPixels);
        AssertTrue(door.Covers(top) && ReferenceEquals(door.SourceRoom.Pictures, sourcePictures),
            "the door window's first gameplay line shows the source room");
        AssertTrue(!door.Covers(top + new Offset16<TilePlanePixels>(0, -1)),
            "the line above the window shows the destination room");

        Console.WriteLine($"  Room backgrounds: {checks} level samples and the door window agree.");
    }

    /// <summary>A PLM's block change reaches the next published plane, and an earlier plane keeps its blocks.</summary>
    internal static void VerifyPlmBlockChangeReachesPlane()
    {
        RoomLevelData level = CreateRoom(new(Side, Side), new ushort[Side * Side], new byte[Side * Side]);
        RoomLevelPlane before = level.ForegroundPlane;
        AssertTrue(ReferenceEquals(before, level.ForegroundPlane), "an unchanged room publishes one plane");
        const int block = 2 * Side + 1;
        PlmLevelWrites.DrawLevelWord(level, block, 0x8007);
        RoomLevelPlane after = level.ForegroundPlane;
        AssertEqual(new RoomBlockCell(0x007, LevelBlockFlipFlags.None), after.Cells.Cells[block],
            "the next plane shows the PLM's block");
        AssertEqual(new RoomBlockCell(0x000, LevelBlockFlipFlags.None), before.Cells.Cells[block],
            "the earlier plane keeps the block it published");
        Console.WriteLine("  Room backgrounds: a PLM block change reaches the next plane only.");
    }

    /// <summary>
    /// Loads retail rooms and checks the published description: BG1 is the room's foreground plane at the BG1 offset,
    /// BG2 is its layer-2 plane only when layer 2 streams, and BG1 at the scroll shows the level at the camera.
    /// </summary>
    internal static void VerifyPublishedRoomBackgrounds()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        // The room-state data selects layer-2 scroll $00/$C0 for the main shaft (streams) and $01/$01 for Kraid (fixed).
        foreach ((RoomHeaderDefinition room, bool streams) in new[] { new VerifyPublishedRoomBackgroundsCase(RoomHeaderDefinitions.GreenBrinstarMainShaft, true), new VerifyPublishedRoomBackgroundsCase(RoomHeaderDefinitions.Kraid, false) })
        {
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0x30));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            runtime.PublishDisplayFrame();
            RoomBackgrounds published = runtime.Display.Latched.Rooms
                ?? throw new InvalidOperationException("A loaded room publishes its backgrounds.");
            RoomLevelData level = runtime.Room.LoadedLevelData;
            AssertTrue(ReferenceEquals(level.ForegroundPlane.Cells, published.Bg1.Blocks), $"{room.Name} BG1 is the room's foreground plane");
            AssertEqual(runtime.BackgroundScroll.Bg1Offset.AsBgPlaneOffset(), published.Bg1.ScrollOffset, $"{room.Name} BG1 offset");
            AssertEqual(streams, published.Bg2 is not null, $"{room.Name} BG2 comes from level data only when layer 2 streams");
            if (published.Bg2 is { } bg2)
            {
                AssertTrue(ReferenceEquals(level.BackgroundPlane.Cells, bg2.Blocks), $"{room.Name} BG2 is the room's layer-2 plane");
                AssertEqual(runtime.BackgroundScroll.Bg2Offset.AsBgPlaneOffset(), bg2.ScrollOffset, $"{room.Name} BG2 offset");
            }
            Position16<TilePlanePixels> scroll = runtime.BackgroundScroll.Bg1Scroll.AsBgPlaneScroll();
            Position16<TilePlanePixels> camera = runtime.BackgroundScroll.Layer1Position.AsBgPlaneScroll();
            for (int y = 0; y < 192; y += 37)
            for (int x = 0; x < 256; x += 41)
            {
                // The block BG1 shows at a screen pixel, through its scroll and offset, is the level's block at the camera.
                var screen = new Offset16<TilePlanePixels>((short)x, (short)y);
                AssertTrue(published.Bg1.Blocks.TryCellAt(scroll - published.Bg1.ScrollOffset + screen, out RoomBlockCell shown, out var shownWithin) &&
                        level.ForegroundPlane.Cells.TryCellAt(camera + screen, out RoomBlockCell expected, out var expectedWithin)
                        && shown == expected && shownWithin == expectedWithin,
                    $"{room.Name} BG1 at screen {x},{y} shows the level at the camera");
            }
        }
        Console.WriteLine("  Room backgrounds: published planes, offsets and camera agree for streaming and fixed BG2 rooms.");
    }

    /// <summary>Runs the room background checks.</summary>
    [VerificationCommand("--room-backgrounds")]
    internal static int RoomBackgroundsCommand()
    {
        Suite(nameof(VerifyRoomBackgroundSources), () => VerifyRoomBackgroundSources());
        Suite(nameof(VerifyPlmBlockChangeReachesPlane), () => VerifyPlmBlockChangeReachesPlane());
        Suite(nameof(VerifyPublishedRoomBackgrounds), () => VerifyPublishedRoomBackgrounds());
        return 0;
    }
}
