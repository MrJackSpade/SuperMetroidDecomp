using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectMapEntryVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.FileSelectRoomMapVerification;
using static SuperMetroid.Verification.Frontend.Map.MapWindowMotionDefinitionsVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
namespace SuperMetroid.Verification.Frontend.FileSelect;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rendering;

/// <summary>Checks area-map graphics and the file-select room-map transition window.</summary>
internal static class FileSelectMapWindowVerification
{
    /// <summary>Creates identical editable label anchors for all six selectable areas.</summary>
    private static WorldMapLabelPositions CreateWindowLabelFixture(Position16<SpritePixels> label)
    {
        var points = Enumerable.Range(0, 6).ToDictionary(area => ((SuperMetroid.Core.Progression.AreaId)area).ToString(),
            _ => label);
        using var stream = new MemoryStream();
        WorldMapLabelFile.Write(stream, new() { Version = 1, Areas = points });
        stream.Position = 0;
        return WorldMapLabelFile.Read(stream);
    }
    /// <summary>Checks edges stopping at the screen, fractional speeds, native area durations and bounds, rendering composition, and label anchors.</summary>
    internal static void VerifyFileSelectMapWindow()
    {
        Suite(nameof(VerifyFileSelectAreaMapGraphics), () => VerifyFileSelectAreaMapGraphics());
        Suite(nameof(VerifyFileSelectRoomMapGraphics), () => VerifyFileSelectRoomMapGraphics());
        Suite(nameof(VerifyFileSelectMapWindowComposition), () => VerifyFileSelectMapWindowComposition());
        Suite(nameof(VerifyFileSelectMapEntry), () => VerifyFileSelectMapEntry());
        // A reveal opening one and a half pixels a side per update from a label one pixel inside the screen's corner: its
        // left and top edges stop at the screen's edge, its right and bottom ones keep their fractions (#1555). The right
        // edge holds its last column, as the cartridge's window does, so the column past it is one further.
        SignedSixteenSixteen pixelAndHalf = SignedSixteenSixteen.FromParts(1, 0x8000);
        var fractional = new FileSelectMapWindow(AreaId.Crateria, new FileSelectMapWindowMotion(2, -pixelAndHalf, pixelAndHalf, -pixelAndHalf, pixelAndHalf),
            CreateWindowLabelFixture(new(1, 1)));
        AssertTrue(!fractional.Step(), "the reveal is not complete before its last move");
        AssertEqual(new VerifyWorldMapLabelsCase(0, 0, 3, 2),
            new VerifyWorldMapLabelsCase(fractional.Left, fractional.Top, fractional.Right, fractional.Bottom),
            "edges past the screen's edge stop at it; the others keep their fractions");
        AssertTrue(fractional.Step(), "the reveal completes on its last move");
        AssertEqual(new VerifyWorldMapLabelsCase(0, 0, 5, 4),
            new VerifyWorldMapLabelsCase(fractional.Left, fractional.Top, fractional.Right, fractional.Bottom),
            "a stopped edge stays at the screen's edge and a moving edge carries its fraction");
        fractional.Step();
        AssertEqual(5, fractional.Right, "a completed reveal stays where it ended");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectMapWindow(AreaId.Ceres), "Ceres has no area-select window record");

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMapWindowLeftVelocities), () => VerifyMapWindowLeftVelocities(bus));
        Suite(nameof(VerifyMapWindowRightVelocities), () => VerifyMapWindowRightVelocities(bus));
        Suite(nameof(VerifyMapWindowTopVelocities), () => VerifyMapWindowTopVelocities(bus));
        Suite(nameof(VerifyMapWindowBottomVelocities), () => VerifyMapWindowBottomVelocities(bus));
        Suite(nameof(VerifyMapWindowTimers), () => VerifyMapWindowTimers(bus));
        int[] durations = [52, 54, 46, 52, 52, 35];
        int[] labelX = [91, 42, 94, 206, 206, 135];
        int[] labelY = [50, 127, 181, 80, 159, 139];
        foreach (AreaId areaId in AreaIds.Zebes)
        {
            int area = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(areaId);
            var window = new FileSelectMapWindow(areaId, RetailPresentationFixture().Labels);
            AssertEqual(labelX[area], window.Left, "map window starts at native area label X");
            AssertEqual(labelY[area], window.Top, "map window starts at native area label Y");
            AssertEqual(window.Left + 1, window.Right, "map window starts one column wide, its edges on the label");
            AssertEqual(window.Top, window.Bottom, "map window starts with zero height");
            AssertTrue(window.IsEmpty, "a zero-size reveal shows none of the room");
            for (int frame = 1; frame <= durations[area]; frame++)
            {
                bool done = window.Step();
                AssertEqual(frame == durations[area], done, $"area {area} window completion frame {frame}");
                AssertTrue(window.Left >= 0 && window.Right <= 256 && window.Top >= 0 && window.Bottom <= 224,
                    $"area {area} window edges remain on the screen");
            }
            AssertEqual(new VerifyWorldMapLabelsCase(0, 0, 256, 224),
                new VerifyWorldMapLabelsCase(window.Left, window.Top, window.Right, window.Bottom), $"area {area} reveal reaches the whole screen");
        }
    }

    /// <summary>Checks the reveal's rectangle, its right and bottom edges past it, during expansion and return to the area map.</summary>
    private static void VerifyFileSelectMapWindowComposition()
    {
        TestAddressSpace fake = CreateComposedMapWindowCartridge();
        var window = new FileSelectMapWindow(AreaId.Crateria, ReadCartridgeMapWindowMotion(fake, 0), CreateWindowLabelFixture(new(40, 30)));
        RgbaPixel areaColor = new(255, 0, 0), frameColor = new(0, 0, 255);
        RgbaPixel[] area = Enumerable.Repeat(areaColor, 256 * 224).ToArray();
        RgbaPixel[] frame = Enumerable.Repeat(frameColor, 256 * 224).ToArray();
        RgbaPixel[] start = FileSelectMapWindowCompositor.Composite(area, frame, window);
        AssertTrue(start.All(pixel => pixel == areaColor), "a zero-size reveal at the area label shows none of the room frame");
        window.Step();
        RgbaPixel[] next = FileSelectMapWindowCompositor.Composite(area, frame, window);
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(y >= 29 && y < 31 && x >= 39 && x < 42 ? frameColor : areaColor,
                next[y * 256 + x], "the reveal shows the room frame inside its rectangle around the label");
        window.Step(); window.Step();
        AssertTrue(FileSelectMapWindowCompositor.Composite(area, frame, window).All(pixel => pixel == frameColor),
            "completed expansion disables window and displays entire empty room frame");

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var returning = FileSelectMapWindow.CreateReturn(AreaId.Maridia, RetailPresentationFixture().Labels);
        AssertEqual(new VerifyWorldMapLabelsCase(8, 8, 249, 216),
            new VerifyWorldMapLabelsCase(returning.Left, returning.Top, returning.Right, returning.Bottom), "native return rectangle insets");
        for (int tick = 1; tick <= 40; tick++)
        {
            AssertEqual(tick == 40, returning.Step(), "Maridia return is twelve updates shorter than expansion");
            AssertEqual(8 + 4 * tick, returning.Left, "return moves the left edge back by its speed");
            AssertEqual(SixteenSixteen.WholeOf(248 * 65536 - 0xf800 * tick) + 1, returning.Right, "return retains right fraction");
            AssertEqual(SixteenSixteen.WholeOf(8 * 65536 + 0x31400 * tick), returning.Top, "return retains top fraction");
            AssertEqual(SixteenSixteen.WholeOf(216 * 65536 - 0x1e000 * tick), returning.Bottom, "return retains bottom fraction");
        }
        AssertTrue(FileSelectMapWindowCompositor.Composite(area, frame, returning).All(pixel => pixel == areaColor),
            "completed return disables room frame and restores entire area scene");
        var graphics = new FileSelectRoomMapGraphics(new SuperMetroid.Core.Progression.SharedGameState(new SharedRandom()),
            SuperMetroid.Core.Progression.AreaId.Maridia, mapPresentation: RetailPresentationFixture(), sheets: MenuSheetFixtures.For(RetailPresentationFixture()));
        RgbaPixel[] frameOnly = graphics.RenderFrameOnly().ToArray();
        AssertTrue(frameOnly.Length > 0 && graphics.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly).Layers is [PicturedPlaneLayer],
            "transition frame shows only the frame's picture, never the room map's cells");
    }

    /// <summary>Checks active map colors, native BG3 tilemap transfers, an opaque drawn map, and visited-station labels.</summary>
    private static void VerifyFileSelectAreaMapGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] indices = [86, 107, 102, 81, 97, 118];
        int[] activeColors = [0x01db, 0x0bb1, 0x0013, 0x7fe0, 0x6400, 0x6417];
        var maps = RetailPresentationFixture();
        var graphics = new FileSelectAreaMapGraphics(AreaId.Crateria, maps.Backdrops, MenuSheetFixtures.For(maps));
        graphics.BindLabels(maps.Labels);
        foreach (AreaId areaId in AreaIds.Zebes)
        {
            int area = SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(areaId);
            graphics.SelectArea(areaId);
            // The selected area's pictures show the colours the import pre-coloured its world-map sheet in (#1512).
            SpriteImage foreground = MenuSheetFixtures.For(maps).Get(SuperMetroid.Core.Frontend.Artwork.ScreenTileSheetDefinitions.AreaMapForegrounds[areaId])
                .Frame(SuperMetroid.Core.Frontend.Map.AreaMapPictures.WorldForegroundFrame).Sheet;
            // The sheet's colours laid out by the palette index each was composed as (#1544).
            RgbColor[] standard = SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ComposedColours(foreground);
            AssertEqual(activeColors[area], standard[indices[area]], $"area {area} active map color");
            if (area != 0)
                AssertEqual(0x4a52, standard[86], "another area's sheet shows Crateria inactive");
            AssertEqual(InstalledFixtures.ImportOf(maps).Palettes.World(areaId)[0], graphics.screen.Backdrop, $"area {area} shows its world-map backdrop");
            // The selected area's image is built once and every tilemap byte read from it.
            var shownArea = MenuScreenFixtures.Vram(graphics, maps);
            for (int offset = 0; offset < MapSourceDefinitions.Menu.TilemapBytes; offset++)
                AssertEqual(FileSelectAreaMapGraphicsRomByte(bus, MapSourceDefinitions.FileSelectMap.AreaBackgrounds + area * MapSourceDefinitions.Menu.TilemapBytes + offset),
                    shownArea.ReadByte(FileSelectMapNativeSources.AreaBackgroundVram * 2 + offset), "selected area BG3 DMA bytes");
            RgbaPixel[] frame = graphics.RenderBackgrounds();
            AssertEqual(256 * 224, frame.Length, "area map physical viewport size");
            AssertTrue(frame.All(pixel => pixel.A == 255), "area map backdrop is opaque");
            AssertTrue(frame.Distinct().Count() > 16, "area map renders tile graphics, not a flat background");

            ushort[] used = [ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue];
            RgbaPixel[] labelled = graphics.Render(used).ToArray();
            AssertTrue(!labelled.SequenceEqual(graphics.Render(new ushort[6])), "visited-station masks control map labels");

        }
    }

    /// <summary>Reads one cartridge map-background byte for the native BG3 transfer comparison.</summary>
    private static byte FileSelectAreaMapGraphicsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Synthetic cartridge with a label at (40, 30), a two-frame timer and one-pixel edge velocities.</summary>
    private static TestAddressSpace CreateComposedMapWindowCartridge()
    {
        var fake = new TestAddressSpace();
        WriteTestWord(fake, MapSourceDefinitions.FileSelectMap.LabelPositions, 40);
        WriteTestWord(fake, MapSourceDefinitions.FileSelectMap.LabelPositions + 2, 30);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowTimers, 2);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities + 2, 0xffff);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities + 6, 1);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities + 10, 0xffff);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities + 14, 1);
        return fake;
    }

    /// <summary>Runs the file-select map graphics and transition-window fixture checks.</summary>
    [VerificationCommand("--file-map-fixtures")]
    internal static int FileMapFixturesCommand()
    {
     VerifyFileSelectMapWindow(); return 0; 
    }
}
