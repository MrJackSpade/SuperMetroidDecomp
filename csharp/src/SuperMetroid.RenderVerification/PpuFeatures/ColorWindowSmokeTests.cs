using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares Direct3D on the graphics device and the WARP reference for color windows, subscreen math, and message masks.</summary>
internal static class ColorWindowSmokeTests
{
    /// <summary>Exercises exact pixel comparisons across scanline windows and representative BG and message geometry.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32114);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        RgbColor[] palette = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = new FramePaint(palette, []);
        Layer3Cells bg3 = Layer3CellsFixtures.FromVram(vram, palette, 0x7ff8);
        ScreenLayerPictures tiles = VramTileFixtures.Tiles(vram, palette, 0x6000, BgBitDepth.FourBpp);
        var background = new TileRenderLayer(ScreenTileMapConstants.Capture(vram, 0x4000, new Extent16<TilemapCells>(32, 32), tiles), new Position16<BgPlanePixels>(3, 11), null);
        ScreenTileMap subscreen = VramTileFixtures.Cells(vram, palette, 0x5800, 0x4000, BgBitDepth.TwoBpp, TileViewportRenderLayer.MapSize);
        int count = 0;
        for (int sample = 0; sample < 64; sample++)
        {
            var lines = new ColorAddWindow[224];
            for (int y = 0; y < lines.Length; y++)
                lines[y] = (y % 5) switch {
                    0 => ColorAddWindow.Empty,
                    1 => new(0, 255, 255, 0, 1),
                    2 => new(255, 255, 0, 255, 127),
                    3 => new(0, 0, 127, 1, 255),
                    _ => new((byte)random.Next(256), (byte)random.Next(256),
                        (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)) };
            Check(new ScanlineColorAddRenderLayer(lines));
        }
        Check(GameplayEffectCapture.CaptureCeresHaze(false));
        Check(GameplayEffectCapture.CaptureCeresHaze(true));
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (int first in new[] { 0, 32, 223, 224 })
        foreach (ExpandedColorMathOperation operation in Enum.GetValues<ExpandedColorMathOperation>())
        foreach (ushort offset in new ushort[] { 0, 255, 511, ushort.MaxValue })
        {
            var scrolls = new Position16<BgPlanePixels>[224];
            for (int y = 0; y < scrolls.Length; y++)
                scrolls[y] = new(unchecked((ushort)(offset + y * 7)), unchecked((ushort)(offset - y * 3)));
            var math = new FxPlaneBlendRenderLayer(bg3, height == 32 ? 32 : 0, height, first, operation, scrolls);
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new RenderLayer[] { background, math }, 15));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: BG color math {count}, {operation}, height {height}, first {first}, offset {offset}");
        }
        foreach (ushort width in new ushort[] { 32, 64 })
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (bool? priority in new bool?[] { null, false, true })
        foreach (bool masked in new[] { false, true })
        {
            var coverage = masked ? new TileRenderLayer(ScreenTileMapConstants.Capture(vram, 0x7ffe, new Extent16<TilemapCells>(width, height), tiles),
                new Position16<BgPlanePixels>(65535, 511), priority) : null;
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new RenderLayer[] { background,
                    new FixedColorAddRenderLayer(1, 2, 3), new TileAddRenderLayer(subscreen, coverage) }, 13));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: subscreen {count}, masked {masked}, geometry {width}x{height}, priority {priority}");
        }
        for (int rows = 3; rows <= 6; rows++)
        for (int radius = 0; radius <= 24; radius++)
        {
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new RenderLayer[] { background,
                    new MessageBoxRenderLayer(SuperMetroid.Core.Rooms.Backgrounds.RoomBackgroundsFixtures.SeededMessagePicture(random, palette, rows), radius) }, 13));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: message {count}, rows {rows}, radius {radius}");
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} exact color-effect/message comparisons passed.");

        void Check(ScanlineColorAddRenderLayer windows)
        {
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory,
                new RenderLayer[] { background,
                    windows, new FixedColorAddRenderLayer(1, 2, 3), windows }, 13), [new FadeLevel(11)]);
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: color-window sample {count}");
        }
    }
}
