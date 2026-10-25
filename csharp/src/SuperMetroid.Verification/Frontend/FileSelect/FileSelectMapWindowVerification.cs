using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
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
using SuperMetroid.Core.Frontend.Menus;

/// <summary>Checks area-map graphics and the file-select room-map transition window.</summary>
internal static class FileSelectMapWindowVerification
{
    /// <summary>Creates identical editable label anchors for all six selectable areas.</summary>
    private static WorldMapLabelLayout CreateWindowLabelFixture(Position16<OamPixels> label)
    {
        var points = Enumerable.Range(0, 6).ToDictionary(area => ((SuperMetroid.Core.Progression.AreaId)area).ToString(),
            _ => label);
        using var stream = new MemoryStream();
        WorldMapLabelLayout.Write(stream, new() { Version = 1, Areas = points });
        stream.Position = 0;
        return WorldMapLabelLayout.Load(stream);
    }
    /// <summary>Checks fractional clamping, native area durations and bounds, rendering composition, and label anchors.</summary>
    internal static void VerifyFileSelectMapWindow()
    {
        Suite(nameof(VerifyFileSelectAreaMapGraphics), () => VerifyFileSelectAreaMapGraphics());
        Suite(nameof(VerifyFileSelectRoomMapGraphics), () => VerifyFileSelectRoomMapGraphics());
        Suite(nameof(VerifyFileSelectMapWindowComposition), () => VerifyFileSelectMapWindowComposition());
        Suite(nameof(VerifyFileSelectMapEntry), () => VerifyFileSelectMapEntry());
        TestAddressSpace fake = CreateFractionalMapWindowCartridge();
        var fractional = new FileSelectMapWindow(0, ReadCartridgeMapWindowMotion(fake, 0), CreateWindowLabelFixture(new(1, 1)));
        // This arithmetic fixture starts below the authored label domain to exercise the lower clamp.
        var edges = fractional.edges;
        Array.Clear(edges);
        AssertTrue(!fractional.Step(), "map window timer zero is not complete");
        AssertEqual(1, fractional.TopLeft.X, "map window lower clamp on first frame");
        AssertTrue(fractional.Step(), "map window completes at signed timer underflow");
        AssertEqual(2, fractional.TopLeft.X, "map window clamp retains fractional X");
        AssertEqual(2, fractional.TopLeft.Y, "map window clamp retains fractional Y");
        fractional.Step();
        AssertEqual(2, fractional.TopLeft.X, "completed map window remains stable");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectMapWindow(6), "Ceres has no area-select window record");

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMapWindowLeftVelocities), () => VerifyMapWindowLeftVelocities(bus));
        Suite(nameof(VerifyMapWindowRightVelocities), () => VerifyMapWindowRightVelocities(bus));
        Suite(nameof(VerifyMapWindowTopVelocities), () => VerifyMapWindowTopVelocities(bus));
        Suite(nameof(VerifyMapWindowBottomVelocities), () => VerifyMapWindowBottomVelocities(bus));
        Suite(nameof(VerifyMapWindowTimers), () => VerifyMapWindowTimers(bus));
        int[] durations = [52, 54, 46, 52, 52, 35];
        int[] labelX = [91, 42, 94, 206, 206, 135];
        int[] labelY = [50, 127, 181, 80, 159, 139];
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            var window = new FileSelectMapWindow(area, RetailPresentationFixture().Labels);
            AssertEqual(labelX[area], window.TopLeft.X, "map window starts at native area label X");
            AssertEqual(labelY[area], window.TopLeft.Y, "map window starts at native area label Y");
            AssertEqual(window.TopLeft.X, window.BottomRight.X, "map window starts with zero width");
            AssertEqual(window.TopLeft.Y, window.BottomRight.Y, "map window starts with zero height");
            for (int frame = 1; frame <= durations[area]; frame++)
            {
                bool done = window.Step();
                AssertEqual(frame == durations[area], done, $"area {area} window completion frame {frame}");
                AssertTrue(window.TopLeft.X >= 1 && window.BottomRight.X <= 255 && window.TopLeft.Y >= 1 && window.BottomRight.Y <= 224,
                    $"area {area} window edges remain in native clamp bounds");
            }
            AssertEqual(1, window.TopLeft.X, $"area {area} expanded left edge");
            AssertEqual(255, window.BottomRight.X, $"area {area} expanded right edge");
            AssertEqual(1, window.TopLeft.Y, $"area {area} expanded top edge");
            AssertEqual(224, window.BottomRight.Y, $"area {area} expanded bottom edge");
        }
    }

    /// <summary>Checks inclusive horizontal/exclusive bottom masking during expansion and return to the area map.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyFileSelectMapWindowComposition()
    {
        TestAddressSpace fake = CreateComposedMapWindowCartridge();
        var window = new FileSelectMapWindow(0, ReadCartridgeMapWindowMotion(fake, 0), CreateWindowLabelFixture(new(40, 30)));
        Rgba32 areaColor = new(255, 0, 0), frameColor = new(0, 0, 255);
        Rgba32[] area = Enumerable.Repeat(areaColor, 256 * 224).ToArray();
        Rgba32[] frame = Enumerable.Repeat(frameColor, 256 * 224).ToArray();
        Rgba32[] start = FileSelectMapWindowCompositor.Composite(area, frame, window);
        AssertEqual(1, start.Count(pixel => pixel == frameColor), "zero-size HDMA window still covers one pixel on one row");
        AssertEqual(frameColor, start[30 * 256 + 40], "initial window is anchored at area label");
        window.Step();
        Rgba32[] next = FileSelectMapWindowCompositor.Composite(area, frame, window);
        for (int y = 0; y < 224; y++)
        for (int x = 0; x < 256; x++)
            AssertEqual(y >= 29 && y < 31 && x >= 39 && x <= 41 ? frameColor : areaColor,
                next[y * 256 + x], "window uses inclusive horizontal and exclusive bottom bounds");
        window.Step(); window.Step();
        AssertTrue(FileSelectMapWindowCompositor.Composite(area, frame, window).All(pixel => pixel == frameColor),
            "completed expansion disables window and displays entire empty room frame");

        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var returning = FileSelectMapWindow.CreateReturn(4, RetailPresentationFixture().Labels);
        AssertEqual(8, returning.TopLeft.X, "native return rectangle left inset");
        AssertEqual(248, returning.BottomRight.X, "native return rectangle right inset");
        AssertEqual(8, returning.TopLeft.Y, "native return rectangle top inset");
        AssertEqual(216, returning.BottomRight.Y, "native return rectangle bottom inset");
        for (int tick = 1; tick <= 40; tick++)
        {
            AssertEqual(tick == 40, returning.Step(), "Maridia return is twelve updates shorter than expansion");
            AssertEqual(8 + 4 * tick, returning.TopLeft.X, "return subtracts native negative left velocity");
            AssertEqual(SixteenSixteen.WholeOf(248 * 65536 - 0xf800 * tick), returning.BottomRight.X, "return retains right fraction");
            AssertEqual(SixteenSixteen.WholeOf(8 * 65536 + 0x31400 * tick), returning.TopLeft.Y, "return retains top fraction");
            AssertEqual(SixteenSixteen.WholeOf(216 * 65536 - 0x1e000 * tick), returning.BottomRight.Y, "return retains bottom fraction");
        }
        AssertTrue(FileSelectMapWindowCompositor.Composite(area, frame, returning).All(pixel => pixel == areaColor),
            "completed return disables room frame and restores entire area scene");
        var graphics = new FileSelectRoomMapGraphics(new SuperMetroid.Core.Progression.SharedGameState(),
            SuperMetroid.Core.Progression.AreaId.Maridia, mapPresentation: RetailPresentationFixture(), sheets: MenuSheetFixtures.For(RetailPresentationFixture()));
        Rgba32[] frameOnly = graphics.RenderFrameOnly().ToArray();
        graphics.screen.Tilemaps.AreaMap[0] = 0x1234;
        AssertTrue(frameOnly.SequenceEqual(graphics.RenderFrameOnly()), "transition frame never samples room-map BG1 tiles");
    }

    /// <summary>Checks active map colors, native BG3 tilemap transfers, an opaque drawn map, and visited-station labels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void VerifyFileSelectAreaMapGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] indices = [86, 107, 102, 81, 97, 118];
        int[] activeColors = [0x01db, 0x0bb1, 0x0013, 0x7fe0, 0x6400, 0x6417];
        var maps = RetailPresentationFixture();
        var graphics = new FileSelectAreaMapGraphics(0, maps.Tiles, maps.Palettes, maps.Screens, maps.WorldArtwork, maps.Sprites, MenuSheetFixtures.For(maps));
        graphics.BindLabels(maps.Labels);
        for (int area = 0; area < FileSelectMapDefinitions.AreaCount; area++)
        {
            graphics.SelectArea(area);
            AssertEqual(activeColors[area], graphics.screen.Palette.Colors[indices[area]], $"area {area} active map color");
            if (area != 0)
                AssertEqual(0x4a52, graphics.screen.Palette.Colors[86], "switching area restores Crateria inactive color");
            for (int offset = 0; offset < MapSourceDefinitions.Menu.TilemapBytes; offset++)
                AssertEqual(FileSelectAreaMapGraphicsRomByte(bus, MapSourceDefinitions.FileSelectMap.AreaBackgrounds + area * MapSourceDefinitions.Menu.TilemapBytes + offset),
                    MenuScreenFixtures.Vram(graphics.screen).ReadByte(FileSelectMapNativeSources.AreaBackgroundVram * 2 + offset), "selected area BG3 DMA bytes");
            Rgba32[] frame = graphics.RenderBackgrounds();
            AssertEqual(256 * 224, frame.Length, "area map physical viewport size");
            AssertTrue(frame.All(pixel => pixel.A == 255), "area map backdrop is opaque");
            AssertTrue(frame.Distinct().Count() > 16, "area map renders tile graphics, not a flat background");

            ushort[] used = [ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue];
            Rgba32[] labelled = graphics.Render(used).ToArray();
            AssertTrue(!labelled.SequenceEqual(graphics.Render(new ushort[6])), "visited-station masks control map labels");

        }
    }

    /// <summary>Reads one cartridge map-background byte for the native BG3 transfer comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select area map graphics")]
    private static byte FileSelectAreaMapGraphicsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Synthetic cartridge with a one-frame map-window timer and .C000 edge velocities.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for fractional file-select map-window motion")]
    private static TestAddressSpace CreateFractionalMapWindowCartridge()
    {
        var fake = new TestAddressSpace();
        WriteTestWord(fake, FileSelectMapNativeSources.WindowTimers, 1);
        // The lower-bound clamp must preserve .C000, or frame two will still
        // report pixel one instead of pixel two. No rendering endpoint can hide it.
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities, 0xc000);
        WriteTestWord(fake, FileSelectMapNativeSources.WindowVelocities + 8, 0xc000);
        return fake;
    }

    /// <summary>Synthetic cartridge with a label at (40, 30), a two-frame timer and one-pixel edge velocities.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for file-select map-window composition")]
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
