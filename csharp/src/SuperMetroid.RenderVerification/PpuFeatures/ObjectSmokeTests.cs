using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.PpuFeatures;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Compares the WARP reference and the graphics device's Direct3D sprite layers across sprite sizes, counts, a plane behind them, and overlap.</summary>
internal static class ObjectSmokeTests
{
    /// <summary>Renders seeded OAM and VRAM samples plus a transparent overlapping-sprite case on the given device.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(32112);
        var vram = new byte[SnesVram.ByteCount];
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        random.NextBytes(vram);
        random.NextBytes(oam);
        RgbColor[] palette = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        PicturedPlane plane = VramPictureFixtures.Plane(vram, palette, 0x4000, 0x6000, TileInkDepth.SixteenColours, new Extent16<TilemapCells>(32, 32));
        int cases = 0;
        for (int size = 0; size < 8; size++)
        foreach (int count in new[] { 0, 1, 37, 128 })
        foreach (byte selection in new byte[] { 0, 11, 31 })
        {
            byte obsel = (byte)((size << 5) | selection);
            var memory = FramePaint.FromOam(vram, palette, oam, count, obsel);
            Check(memory, obsel);
            Check(memory, obsel, new PicturedPlaneLayer(plane, new Offset16<PlanePixels>(3, 11)));
        }
        // The earlier draw covers the later one; a transparent hole in it must still expose the later draw.
        Array.Clear(vram); Array.Clear(oam); Array.Clear(palette);
        palette[129] = new RgbColor(255, 0, 0);
        palette[145] = new RgbColor(0, 0, 255);
        for (int row = 0; row < 8; row++) vram[row * 2] = 0x7f;
        for (int row = 0; row < 8; row++) vram[32 + row * 2] = 0xff;
        oam[6] = 1; oam[7] = 0x32;
        var overlap = FramePaint.FromOam(vram, palette, oam, 2, 0);
        Check(overlap, 0);
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {cases} exact OBJ comparisons passed.");

        void Check(FramePaint memory, byte obsel, params DisplayLayer[] behind)
        {
            cases++;
            var frame = new DisplayPacket(0, memory.SpritesScene(15, SpriteBlend.Drawn, behind));
            PixelComparison.Verify(frame, DisplayRendering.Render(frame),
                renderer.RenderForReadback(frame), $"{device.Kind}: OBJ sample {cases}, OBSEL {obsel}, parts {memory.SpriteParts().Length}");
        }
    }
}
