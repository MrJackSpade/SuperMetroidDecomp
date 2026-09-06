using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyFileSelectMapWindow()
    {
        VerifyFileSelectAreaMapGraphics();
        var fake = new TestAddressSpace();
        WriteTestWord(fake, FileSelectMapRomData.WindowTimers, 1);
        // The lower-bound clamp must preserve .C000, or frame two will still
        // report pixel one instead of pixel two. No rendering endpoint can hide it.
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities, 0xc000);
        WriteTestWord(fake, FileSelectMapRomData.WindowVelocities + 8, 0xc000);
        var fractional = new FileSelectMapWindow(fake, 0);
        AssertTrue(!fractional.Step(), "map window timer zero is not complete");
        AssertEqual(1, fractional.Left, "map window lower clamp on first frame");
        AssertTrue(fractional.Step(), "map window completes at signed timer underflow");
        AssertEqual(2, fractional.Left, "map window clamp retains fractional X");
        AssertEqual(2, fractional.Top, "map window clamp retains fractional Y");
        fractional.Step();
        AssertEqual(2, fractional.Left, "completed map window remains stable");
        AssertThrows<ArgumentOutOfRangeException>(() => new FileSelectMapWindow(fake, 6), "Ceres has no area-select window record");

        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] durations = [52, 54, 46, 52, 52, 35];
        int[] labelX = [91, 42, 94, 206, 206, 135];
        int[] labelY = [50, 127, 181, 80, 159, 139];
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            var window = new FileSelectMapWindow(bus, area);
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

    private static void VerifyFileSelectAreaMapGraphics()
    {
        Rgba32[] main = [new(255, 8, 0), new(0, 0, 8)];
        Rgba32[] sub = [new(255, 8, 8), new(255, 255, 255, 0)];
        SnesLayerCompositor.AddSubscreen(main, sub);
        AssertEqual(new Rgba32(255, 16, 8), main[0], "subscreen uses saturated five-bit addition");
        AssertEqual(new Rgba32(0, 0, 8), main[1], "keyed subscreen selects black fixed color");

        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        int[] indices = [86, 107, 102, 81, 97, 118];
        int[] activeColors = [0x01db, 0x0bb1, 0x0013, 0x7fe0, 0x6400, 0x6417];
        var graphics = new FileSelectAreaMapGraphics(bus, 0);
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
        {
            graphics.SelectArea(area);
            AssertEqual(activeColors[area], graphics.Cgram.Colors[indices[area]], $"area {area} active map color");
            if (area != 0)
                AssertEqual(0x4a52, graphics.Cgram.Colors[86], "switching area restores Crateria inactive color");
            for (int offset = 0; offset < FileSelectMapRomData.TilemapBytes; offset++)
                AssertEqual(bus.ReadByte(FileSelectMapRomData.AreaBackgrounds + area * FileSelectMapRomData.TilemapBytes + offset),
                    graphics.Vram.ReadByte(FileSelectMapRomData.AreaBackgroundVram * 2 + offset), "selected area BG3 DMA bytes");
            Rgba32[] frame = graphics.RenderBackgrounds();
            AssertEqual(256 * 224, frame.Length, "area map physical viewport size");
            AssertTrue(frame.All(pixel => pixel.A == 255), "area map backdrop is opaque");
            AssertTrue(frame.Distinct().Count() > 16, "area map renders tile graphics, not a flat background");
            Directory.CreateDirectory("csharp/test-temp/file-select-map");
            PngWriter.WriteRgba($"csharp/test-temp/file-select-map/area-{area}.png", 256, 224, frame);
            ushort[] used = [ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue, ushort.MaxValue];
            Rgba32[] labelled = graphics.Render(used);
            AssertTrue(!labelled.SequenceEqual(graphics.Render(new ushort[6])), "visited-station masks control map labels");
            PngWriter.WriteRgba($"csharp/test-temp/file-select-map/area-{area}-labels.png", 256, 224, labelled);
        }
    }
}
