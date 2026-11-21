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
    /// <summary>
    /// Tests the pixelation the game draws (#2563): a pixelated pictured plane. The gameplay scene's own background is never
    /// pixelated, so only its pictured planes and a boss's body carry a pixelation.
    /// </summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer) => RunPicturedPlane(device, renderer);

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
        var unpixelated = new LayeredScene(paint, [new PicturedPlaneLayer(plane, Offset16<PlanePixels>.Zero)], DisplayBrightness.Full);
        RgbaPixel[] clear = DisplayRendering.Render(unpixelated);
        RgbaPixel backdrop = DisplayRendering.Render(new LayeredScene(paint, [], DisplayBrightness.Full))[0];
        if (!clear.Contains(backdrop) || clear.All(pixel => pixel == backdrop))
            throw new InvalidDataException("The pixelated plane fixture must show both picture pixels and clear pixels.");
        foreach (int size in Enumerable.Range(1, 16))
        {
            var packet = new DisplayPacket(0, new LayeredScene(paint,
                [new PicturedPlaneLayer(plane, Offset16<PlanePixels>.Zero) { Pixelation = new(size) }], DisplayBrightness.Full));
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
