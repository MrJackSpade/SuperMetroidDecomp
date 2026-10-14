using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using static AssertionsVerification;
using static FileSelectMapEntryVerification;
using static FileSelectRoomMapVerification;
using static MapWindowMotionDefinitionsVerification;
using static RetailPresentationFixtureVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>Checks area-map graphics and the file-select room-map transition window.</summary>
internal static class FileSelectMapWindowVerification
{
    /// <summary>Creates identical editable label anchors for all six selectable areas.</summary>
    private static WorldMapLabelLayout CreateWindowLabelFixture(Position16<OamPixels> label)
    {
        var points = Enumerable.Range(0, 6).ToDictionary(area => ((SuperMetroid.Core.Game.AreaId)area).ToString(),
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
        var fractional = new FileSelectMapWindow(fake, 0, ReadCartridgeMapWindowMotion(fake, 0), CreateWindowLabelFixture(new(1, 1)));
        // This arithmetic fixture starts below the authored label domain to exercise the lower clamp.
        var edges = fractional.edges;
        Array.Clear(edges);
        AssertTrue(!fractional.Step(), "map window timer zero is not complete");
        AssertEqual(1, fractional.Left, "map window lower clamp on first frame");
        AssertTrue(fractional.Step(), "map window completes at signed timer underflow");
        AssertEqual(2, fractional.Left, "map window clamp retains fractional X");
        AssertEqual(2, fractional.Top, "map window clamp retains fractional Y");
        fractional.Step();
        AssertEqual(2, fractional.Left, "completed map window remains stable");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectMapWindow(fake, 6), "Ceres has no area-select window record");

        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyMapWindowLeftVelocities), () => VerifyMapWindowLeftVelocities(bus));
        Suite(nameof(VerifyMapWindowRightVelocities), () => VerifyMapWindowRightVelocities(bus));
        Suite(nameof(VerifyMapWindowTopVelocities), () => VerifyMapWindowTopVelocities(bus));
        Suite(nameof(VerifyMapWindowBottomVelocities), () => VerifyMapWindowBottomVelocities(bus));
        Suite(nameof(VerifyMapWindowTimers), () => VerifyMapWindowTimers(bus));
        int[] durations = [52, 54, 46, 52, 52, 35];
        int[] labelX = [91, 42, 94, 206, 206, 135];
        int[] labelY = [50, 127, 181, 80, 159, 139];
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var window = new FileSelectMapWindow(bus, area, RetailPresentationFixture().Labels);
            AssertEqual(labelX[area], window.Left, "map window starts at native area label X");
            AssertEqual(labelY[area], window.Top, "map window starts at native area label Y");
            AssertEqual(window.Left, window.Right, "map window starts with zero width");
            AssertEqual(window.Top, window.Bottom, "map window starts with zero height");
            for (int frame = 1; frame <= durations[area]; frame++)
            {
                bool done = window.Step();
                AssertEqual(frame == durations[area], done, $"area {area} window completion frame {frame}");
                AssertTrue(window.Left >= 1 && window.Right <= 255 && window.Top >= 1 && window.Bottom <= 224,
                    $"area {area} window edges remain in native clamp bounds");
            }
            AssertEqual(1, window.Left, $"area {area} expanded left edge");
            AssertEqual(255, window.Right, $"area {area} expanded right edge");
            AssertEqual(1, window.Top, $"area {area} expanded top edge");
            AssertEqual(224, window.Bottom, $"area {area} expanded bottom edge");
        }
    }

    /// <summary>Checks inclusive horizontal/exclusive bottom masking during expansion and return to the area map.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyFileSelectMapWindowComposition()
    {
        TestAddressSpace fake = CreateComposedMapWindowCartridge();
        var window = new FileSelectMapWindow(fake, 0, ReadCartridgeMapWindowMotion(fake, 0), CreateWindowLabelFixture(new(40, 30)));
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

        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var returning = FileSelectMapWindow.CreateReturn(bus, 4, RetailPresentationFixture().Labels);
        AssertEqual(8, returning.Left, "native return rectangle left inset");
        AssertEqual(248, returning.Right, "native return rectangle right inset");
        AssertEqual(8, returning.Top, "native return rectangle top inset");
        AssertEqual(216, returning.Bottom, "native return rectangle bottom inset");
        for (int tick = 1; tick <= 40; tick++)
        {
            AssertEqual(tick == 40, returning.Step(), "Maridia return is twelve updates shorter than expansion");
            AssertEqual(8 + 4 * tick, returning.Left, "return subtracts native negative left velocity");
            AssertEqual(SnesSixteenSixteen.WholeOf(248 * 65536 - 0xf800 * tick), returning.Right, "return retains right fraction");
            AssertEqual(SnesSixteenSixteen.WholeOf(8 * 65536 + 0x31400 * tick), returning.Top, "return retains top fraction");
            AssertEqual(SnesSixteenSixteen.WholeOf(216 * 65536 - 0x1e000 * tick), returning.Bottom, "return retains bottom fraction");
        }
        AssertTrue(FileSelectMapWindowCompositor.Composite(area, frame, returning).All(pixel => pixel == areaColor),
            "completed return disables room frame and restores entire area scene");
        var graphics = new FileSelectRoomMapGraphics(bus, new SuperMetroid.Core.Game.Bank80SystemState(),
            SuperMetroid.Core.Game.AreaId.Maridia, mapPresentation: RetailPresentationFixture());
        Rgba32[] frameOnly = graphics.RenderFrameOnly().ToArray();
        graphics.Vram.LoadBytes(0xa000, new byte[] { 0x34, 0x12 });
        AssertTrue(frameOnly.SequenceEqual(graphics.RenderFrameOnly()), "transition frame never samples room-map BG1 tiles");
    }

    /// <summary>Checks active map colors, native BG3 tilemap transfers, backdrop rendering, and visited-station labels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    private static void VerifyFileSelectAreaMapGraphics()
    {
        Rgba32[] main = [new(255, 8, 0), new(0, 0, 8)];
        Rgba32[] sub = [new(255, 8, 8), new(255, 255, 255, 0)];
        SnesLayerCompositor.AddSubscreen(main, sub);
        AssertEqual(new Rgba32(255, 16, 8), main[0], "subscreen uses saturated five-bit addition");
        AssertEqual(new Rgba32(0, 0, 8), main[1], "keyed subscreen selects black fixed color");

        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] indices = [86, 107, 102, 81, 97, 118];
        int[] activeColors = [0x01db, 0x0bb1, 0x0013, 0x7fe0, 0x6400, 0x6417];
        var maps = RetailPresentationFixture();
        var graphics = new FileSelectAreaMapGraphics(bus, 0, maps.Tiles, maps.Palettes, maps.Screens, maps.WorldArtwork, maps.Sprites);
        graphics.BindLabels(maps.Labels);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            graphics.SelectArea(area);
            AssertEqual(activeColors[area], graphics.Cgram.Colors[indices[area]], $"area {area} active map color");
            if (area != 0)
                AssertEqual(0x4a52, graphics.Cgram.Colors[86], "switching area restores Crateria inactive color");
            for (int offset = 0; offset < FileSelectMapRomData.TilemapBytes; offset++)
                AssertEqual(FileSelectAreaMapGraphicsRomByte(bus, FileSelectMapRomData.AreaBackgrounds + area * FileSelectMapRomData.TilemapBytes + offset),
                    graphics.Vram.ReadByte(FileSelectMapRomData.AreaBackgroundVram * 2 + offset), "selected area BG3 DMA bytes");
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
        WriteTestWord(fake, FileSelectMapRomData.WindowTimers, 1);
        // The lower-bound clamp must preserve .C000, or frame two will still
        // report pixel one instead of pixel two. No rendering endpoint can hide it.
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities, 0xc000);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 8, 0xc000);
        return fake;
    }

    /// <summary>Synthetic cartridge with a label at (40, 30), a two-frame timer and one-pixel edge velocities.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for file-select map-window composition")]
    private static TestAddressSpace CreateComposedMapWindowCartridge()
    {
        var fake = new TestAddressSpace();
        WriteTestWord(fake, FileSelectMapRomData.LabelPositions, 40);
        WriteTestWord(fake, FileSelectMapRomData.LabelPositions + 2, 30);
        WriteTestWord(fake, FileSelectMapRomData.WindowTimers, 2);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 2, 0xffff);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 6, 1);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 10, 0xffff);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 14, 1);
        return fake;
    }

    /// <summary>Runs the file-select map graphics and transition-window fixture checks.</summary>
    [VerificationCommand("--file-map-fixtures")]
    internal static int FileMapFixturesCommand()
    {
     VerifyFileSelectMapWindow(); return 0; 
    }
}
