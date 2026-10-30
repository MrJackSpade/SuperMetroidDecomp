using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares Direct3D tile and color-math output with the WARP reference.</summary>
internal static class TileSmokeTests
{
    /// <summary>Checks background geometry, priority, scrolling, transparency, brightness, and plane selection.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        VerifyFourthPlaneByteSelection(device, renderer);
        var random = new Random(32111);
        byte[] bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes);
        RgbColor[] palette = Enumerable.Range(0, SnesCgram.ColorCount).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = FramePaint.FromNative(palette, []);
        ScreenLayerPictures upper = VramTileFixtures.Tiles(bytes, palette, 0x6000, TileInkDepth.SixteenColours);
        ScreenTileMap Cells(int tilemapWord, ushort width, ushort height, ScreenLayerPictures tiles) =>
            ScreenTileMapConstants.Capture(bytes, tilemapWord, new Extent16<TilemapCells>(width, height), tiles);
        int count = 0;
        foreach (ushort width in new ushort[] { 32, 64 })
        foreach (ushort height in new ushort[] { 32, 64 })
        foreach (bool? priority in new bool?[] { null, false, true })
        foreach (ushort scroll in new ushort[] { 0, 7, 255, 511, ushort.MaxValue })
            Check(new DisplayLayer[] { new ScreenTilesLayer(Cells(0x7ffe, width, height, upper), new Position16<TilePlanePixels>(scroll, scroll), priority) }, "4bpp geometry/priority/scroll");
        ScreenLayerPictures font = VramTileFixtures.Tiles(bytes, palette, 0x7ff8, TileInkDepth.FourColours);
        foreach (bool? priority in new bool?[] { null, false, true })
        foreach (ushort scroll in new ushort[] { 0, 1, 223, 255, ushort.MaxValue })
            Check(new DisplayLayer[] { new ScreenTilesLayer(Cells(0x7fff, 32, 32, font), new Position16<TilePlanePixels>(0, scroll), priority) }, "2bpp vertical scroll/priority");
        ScreenLayerPictures lower = VramTileFixtures.Tiles(bytes, palette, 0, TileInkDepth.SixteenColours);
        ScreenLayerPictures text = VramTileFixtures.Tiles(bytes, palette, 0x4000, TileInkDepth.FourColours);
        for (byte brightness = 0; brightness <= 15; brightness++)
            Check(new DisplayLayer[] {
                new ScreenTilesLayer(Cells(0x4000, 64, 32, lower), new Position16<TilePlanePixels>(3, 11), false),
                new ScreenTilesLayer(Cells(0x4800, 32, 64, upper), new Position16<TilePlanePixels>(65535, 255), true),
                new ScreenTilesLayer(Cells(0x5800, 32, 32, text), new Position16<TilePlanePixels>(0, 13), null),
                new ScreenTintLayer(3, 17, 31) }, "ordered planes/fixed-color/fades", brightness);
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} tile/color-math GPU comparisons passed.");

        void Check(DisplayLayer[] layers, string name, byte brightness = 15)
        {
            var frame = new DisplayPacket(new(++count, 1, 0), new LayeredScene(memory, layers, DisplayBrightness.Combine(brightness, 13, 11)));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame), renderer.RenderForReadback(frame), $"{device.Kind}: {name} #{count}");
        }
    }

    /// <summary>
    /// Checks a flipped tile's fourth-plane byte reaches its picture (#1403): the byte an earlier shader decode misread,
    /// selecting the wrong palette color.
    /// </summary>
    private static void VerifyFourthPlaneByteSelection(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new byte[SnesVram.ByteCount];
        // A flipped tile's bottom row puts plane three in the high byte of a packed
        // uint. The adjacent second byte is deliberately different: the original
        // optimized shader selected it and produced palette index ten instead of two.
        for (int row = 0; row < 8; row++)
        {
            int address = (0x6000 + 4 * 16) * 2 + row * 2;
            vram[address] = 182; vram[address + 1] = 95;
            vram[address + 16] = 54; vram[address + 17] = 144;
        }
        vram[(0x6000 + 4 * 16) * 2 + 29] = 233;
        for (int i = 0; i < 1024; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(vram.AsSpan(0x4000 * 2 + i * 2), 0xec04);
        var palette = new RgbColor[SnesCgram.ColorCount];
        palette[3 * 16 + 2] = new RgbColor(31, 0, 0); palette[3 * 16 + 10] = new RgbColor(0, 0, 31);
        var memory = FramePaint.FromNative(palette, []);
        var frame = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
            new DisplayLayer[] { new ScreenTilesLayer(VramTileFixtures.Cells(vram, palette, 0x4000, 0x6000, TileInkDepth.SixteenColours, new Extent16<TilemapCells>(32, 32)),
                new Position16<TilePlanePixels>(0, 0), null) }, 15));
        var expected = DisplayRendering.Render(frame);
        if (expected[0] != new RgbaPixel(255, 0, 0))
            throw new InvalidOperationException("Fourth-plane fixture no longer selects color two.");
        PixelComparison.Verify(frame, expected, renderer.RenderForReadback(frame), $"{device.Kind}: fourth-plane byte selection");
    }
}
