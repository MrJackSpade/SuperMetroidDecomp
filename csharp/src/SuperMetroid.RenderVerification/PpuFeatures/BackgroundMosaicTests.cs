using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Artwork;
namespace SuperMetroid.RenderVerification.PpuFeatures;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks BG2 mosaic sampling against an independent pixel oracle and the Direct3D renderer.</summary>
internal static class BackgroundMosaicTests
{
    /// <summary>Tests every mosaic size with normal, subscreen, additive, and BG1 modes while preserving OBJ pixels.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        for (int color = 1; color <= 15; color++) cgram.SetColor(color, new FadeLevelColour(color, 15 - color, 0).ToColour());
        // A repeating asymmetric pixel pattern makes both sampling axes observable.
        byte[] character = new byte[32];
        for (int y = 0; y < 8; y++)
        for (int x = 0; x < 8; x++)
        {
            int color = 1 + (x + 2 * y) % 15;
            for (int plane = 0; plane < 4; plane++)
                character[(plane / 2) * 16 + y * 2 + plane % 2] |= (byte)(((color >> plane) & 1) << (7 - x));
        }
        vram.LoadBytes(0, character);
        for (int row = 0; row < 8; row++) vram.LoadBytes(32 + row * 2, new byte[] { 255, 0 });
        cgram.SetColor(241, new RgbColor(0, 0, 255));
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 96; oam[1] = 64; oam[2] = 1; oam[3] = 0x3e;
        var memory = FramePaint.FromOamGameplay(vram.Bytes, cgram.Colors, oam, 1, 0);
        // BG1's plane and BG2's loaded tilemap show tile word zero, the pattern, drawn from their pictures (#1403).
        var rooms = RoomBackgroundsFixtures.Uniform(vram.Bytes, cgram.Colors, default);
        // Every gameplay line scrolls differently on both axes (#1468): a sixteen-sample ramp across and down.
        System.Collections.Immutable.ImmutableArray<int> ramp = [.. Enumerable.Range(0, 16)];
        var horizontal = new SampledScrollWave(7, ramp, 3 * 256, 0, 32, 224);
        var vertical = new SampledScrollWave(65530, ramp, 256, 0, 32, 224);
        int cases = 0;
        foreach (int size in Enumerable.Range(1, 16))
        foreach (int mode in Enumerable.Range(0, 4))
        {
            var registers = new GameplaySceneView(new Position16<RoomPixels>(0, 0), new Position16<BackgroundPixels>(0, 0),
                ScreenLayers.Sprites | (mode == 2 ? ScreenLayers.None : ScreenLayers.Background) |
                    (mode == 3 ? ScreenLayers.Foreground : ScreenLayers.None),
                BackgroundPixelation: new(size));
            var ordinary = new GameplaySceneLayer(registers, rooms, new BackgroundLineScrolls { Horizontal = horizontal, Vertical = vertical });
            // Mode two shows the pixelated background only lying behind the backdrop, which it brightens; mode one blends nothing.
            DisplayLayer layer = mode is 0 or 3 ? ordinary : new GameplayBlendLayer(ordinary, ScreenWindow.Closed, false,
                new GameplayBlend
                {
                    Blended = mode == 2 ? BlendedLayers.Backdrop : default,
                    Operation = BlendOperation.Add,
                    Halved = false,
                    Colour = RgbColor.Black,
                }, backgroundBehind: mode == 2);
            var scene = new LayeredScene(memory, [layer], 15);
            cases++;
            var packet = new DisplayPacket(0, scene);
            var actual = DisplayRendering.Render(scene);
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                int expectedColor = 0;
                if (y >= 32)
                {
                    // Independent scalar oracle: choose the screen block, then
                    // current-row scroll, wrap within the authored 8x8 pattern. The view's scrolls are the
                    // pixels the screen's top shows, with no further row (#1514).
                    int sx = (x / size * size + ScrollShapeLines.Scroll(horizontal, y)) & 7;
                    int sy = (y / size * size + ScrollShapeLines.Scroll(vertical, y)) & 7;
                    if (mode == 3) { sx = x & 7; sy = y & 7; }
                    expectedColor = 1 + (sx + 2 * sy) % 15;
                }
                if (x is >= 96 and < 104 && y is >= 64 and < 72) expectedColor = 241;
                if (actual[y * 256 + x] != cgram.GetRgba(expectedColor))
                    throw new InvalidDataException($"Mosaic size {size}, mode {mode}: wrong pixel ({x},{y}).");
            }
            PixelComparison.Verify(packet, actual, DisplayRendering.Render(packet), "mosaic WARP reference");
            PixelComparison.Verify(packet, actual, renderer.RenderForReadback(packet), $"{device.Kind}: mosaic size {size}, mode {mode}");
        }
        Console.WriteLine($"{device.Kind}: {cases} mosaic frames match independent pixels (ordinary, main BG2, additive BG2; HUD/OBJ/BG1 unaffected).");
        RunPicturedPlane(device, renderer);
    }

    /// <summary>
    /// Checks a pixelated pictured plane (#1774), as the Zebes reveal draws its planet picture: at every block width, each
    /// square block of the screen shows what the unpixelated plane shows at the block's top-left pixel, a clear pixel there
    /// leaving the whole block to the backdrop; the device draws the same pixels as WARP.
    /// </summary>
    private static void RunPicturedPlane(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(1774);
        byte[] bytes = new byte[SnesVram.ByteCount];
        random.NextBytes(bytes);
        RgbColor[] palette = [.. Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => RgbColor.FromWord((ushort)random.Next(32768)))];
        var paint = FramePaint.FromNative(palette, []);
        // The reveal's layout: a 32-by-32 tilemap at word $5C00 over sixteen-colour characters at word $6000.
        PicturedPlane plane = VramPictureFixtures.Plane(bytes, palette, 0x5c00, 0x6000, TileInkDepth.SixteenColours, new Extent16<CharacterCells>(32, 32));
        var unpixelated = new LayeredScene(paint, [new PicturedPlaneLayer(plane, Offset16<PlanePixels>.Zero)], 15);
        RgbaPixel[] clear = DisplayRendering.Render(unpixelated);
        RgbaPixel backdrop = DisplayRendering.Render(new LayeredScene(paint, [], 15))[0];
        if (!clear.Contains(backdrop) || clear.All(pixel => pixel == backdrop))
            throw new InvalidDataException("The pixelated plane fixture must show both picture pixels and clear pixels.");
        foreach (int size in Enumerable.Range(1, 16))
        {
            var packet = new DisplayPacket(0, new LayeredScene(paint,
                [new PicturedPlaneLayer(plane, Offset16<PlanePixels>.Zero) { Pixelation = new(size) }], 15));
            var expected = new RgbaPixel[clear.Length];
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
                expected[y * 256 + x] = clear[y / size * size * 256 + x / size * size];
            RgbaPixel[] reference = DisplayRendering.Render(packet);
            PixelComparison.Verify(packet, expected, reference, $"pixelated plane size {size}: WARP against block sampling");
            PixelComparison.Verify(packet, reference, renderer.RenderForReadback(packet), $"{device.Kind}: pixelated plane size {size}");
        }
        Console.WriteLine($"{device.Kind}: 16 pixelated pictured-plane frames match block sampling and WARP.");
    }
}
