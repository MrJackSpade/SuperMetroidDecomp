using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Rendering;

/// <summary>Checks PPU color math and render-packet compatibility for OBJ and BG subscreen layers.</summary>
internal static class ObjSubscreenVerification
{
    /// <summary>Checks additive OBJ composition and delegates the related Mode 7, BG, and fixed-color cases.</summary>
    internal static void VerifyObjSubscreenAddition()
    {
        Suite(nameof(VerifyMode7ObjSubtraction), () => VerifyMode7ObjSubtraction());
        Suite(nameof(VerifyMode7Bg1Addition), () => VerifyMode7Bg1Addition());
        Suite(nameof(VerifyBg4SubscreenAddition), () => VerifyBg4SubscreenAddition());
        Suite(nameof(VerifyObjFixedColor), () => VerifyObjFixedColor());
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int row = 0; row < 8; row++) vram[row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 0);
        var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
            new RenderLayer[] { new SpriteRenderLayer(true) }, 15));

        var pixels = DisplayRendering.Render(packet);
        AssertEqual(new RgbaPixel(255, 0, 255), pixels[8 * 256 + 8], "OBJ subscreen adds blue to red backdrop");
        AssertEqual(new RgbaPixel(255, 0, 0), pixels[0], "transparent OBJ preserves backdrop");
        var ordinary = new DisplayPacket(new(2, 1, 0), new LayeredScene(memory,
            new RenderLayer[] { new SpriteRenderLayer() }, 15));
    }

    /// <summary>Checks Mode 7 BG1 self-addition and OBJ palette eligibility after main-screen priority selection.</summary>
    private static void VerifyMode7Bg1Addition()
    {
        foreach (bool opaqueBg in new[] { false, true })
        foreach (byte palette in new byte[] { 0, 4 })
        foreach (byte priority in new byte[] { 0, 1, 2, 3 })
        {
            byte[] vram = new byte[SnesPpuLayout.VramByteCount];
            if (opaqueBg) for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
            for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
            ushort[] colors = new ushort[256];
            colors[0] = 3 << 10; colors[1] = 8 << 5; colors[129 + palette * 16] = 12;
            byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(palette * 2 + priority * 16);
            var registers = new AffineTransform(256, 0, 0, 256, new Offset16<FloorPixels>(0, 0), new Offset16<FloorPixels>(0, 0));
            var memory = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3);
            var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
                new RenderLayer[] { new FloorRenderLayer(registers, AffineFloorFixtures.Floor(vram, ToColors(colors)), AddBg1Subscreen: true) }, 15));
            var pixels = DisplayRendering.Render(packet);
            RgbaPixel expected = opaqueBg && priority == 0 ? new(0, 132, 0)
                : opaqueBg && palette == 4 ? new(99, 66, 0) : new(99, 0, 0);
            AssertEqual(expected, pixels[8 * 256 + 8], "Mode7 main priority and OBJ palette eligibility before addition");
            AssertEqual(opaqueBg ? new RgbaPixel(0, 132, 0) : new RgbaPixel(0, 0, 24), pixels[0],
                "BG1 doubles itself but transparent BG1 preserves unmodified backdrop");
            AssertThrows<ArgumentException>(() => new LayeredScene(memory,
                new RenderLayer[] { new FloorRenderLayer(registers, AffineFloorFixtures.Floor(vram, ToColors(colors)), true, true) }, 15), "reject contradictory Mode7 math modes");
        }
    }

    /// <summary>Checks per-channel Mode 7 minus OBJ subtraction and rejection of old packets that cannot encode it.</summary>
    private static void VerifyMode7ObjSubtraction()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
        for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[1] = 31 | (12 << 5);
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3);
        var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
            new RenderLayer[] { new FloorRenderLayer(new(256, 0, 0, 256, default, default), AffineFloorFixtures.Floor(vram, ToColors(colors)), true) }, 15));
        var pixels = DisplayRendering.Render(packet);
        AssertEqual(new RgbaPixel(198, 99, 0), pixels[8 * 256 + 8], "Mode7 minus OBJ clamps each five-bit component without halving");
        AssertEqual(new RgbaPixel(255, 99, 0), pixels[0], "transparent OBJ leaves Mode7 color unchanged");

    }

    /// <summary>Checks OBJ fixed-color addition, palette and priority filters, and legacy priority-packet decoding.</summary>
    private static void VerifyObjFixedColor()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int y = 0; y < 8; y++) vram[y * 2] = 255;
        ushort[] palette = new ushort[256];
        palette[0] = 31 << 10;
        palette[129] = palette[193] = 7;
        foreach (byte objPalette in new byte[] { 0, 4 })
        {
            byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(objPalette * 2);
            var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(
                FramePaint.FromOam(vram, ToColors(palette), oam, 1, 0),
                new RenderLayer[] { new SpriteRenderLayer(FixedColor: new(8, 4, 2)) }, 15));
            var pixels = DisplayRendering.Render(packet);
            AssertEqual(objPalette == 4 ? new RgbaPixel(123, 33, 16) : new RgbaPixel(57, 0, 0), pixels[8 * 256 + 8],
                "OBJ fixed RGB addition respects palette eligibility and five-bit expansion");
            AssertEqual(new RgbaPixel(0, 0, 255), pixels[0], "OBJ-only fixed color does not alter uncovered backdrop");

            foreach (byte selectedPriority in new byte[] { 0, 1 })
            {
                var filtered = new DisplayPacket(new(1, 1, 0), new LayeredScene(
                    FramePaint.FromOam(vram, ToColors(palette), oam, 1, 0),
                    new RenderLayer[] { new SpritePriorityRenderLayer(selectedPriority, new(8, 4, 2)) }, 15));
                var filteredPixels = DisplayRendering.Render(filtered);
                AssertEqual(selectedPriority == 0 ? pixels[8 * 256 + 8] : new RgbaPixel(0, 0, 255),
                    filteredPixels[8 * 256 + 8], "fixed OBJ color obeys both priority filtering and palette eligibility");

            }
        }
    }

    /// <summary>Checks four-bit BG subscreen decoding, object masking, and BG-versus-OBJ priority before addition.</summary>
    private static void VerifyBg4SubscreenAddition()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        // Color four depends on the third bitplane: a mistaken 2-bpp decode sees zero.
        for (int row = 0; row < 8; row++) vram[row * 2 + 16] = 255;
        for (int tile = 0; tile < 1024; tile++) vram[0xe000 + tile * 2 + 1] = 4;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[20] = 31 << 10;
        // The subscreen and its coverage are the one-screen maps at a tilemap word, read as the fixture stands when drawn.
        ScreenTileMap Cells(int tilemapWord) => VramTileFixtures.Cells(vram, ToColors(colors), tilemapWord, 0, BgBitDepth.FourBpp, TileViewportRenderLayer.MapSize);
        var memory = new FramePaint(ToColors(colors), []);
        var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
            new RenderLayer[] { new TileAddRenderLayer(Cells(0x7000)) }, 15));
        AssertTrue(DisplayRendering.Render(packet).All(pixel => pixel == new RgbaPixel(255, 0, 255)),
            "four-bit subscreen resolves upper bitplanes and sixteen-color palette stride before addition");

        for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
        colors[129] = 31 << 5;
        colors[193] = 31 << 5;
        for (int tile = 0; tile < 1024; tile++) vram[0xd000 + tile * 2] = 1;
        foreach (byte palette in new byte[] { 0, 4 })
        {
            byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(palette * 2);
            var masked = new DisplayPacket(new(3, 1, 0), new LayeredScene(
                FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3),
                new RenderLayer[] { new SpriteRenderLayer(), new TileAddRenderLayer(Cells(0x7000),
                    new TileRenderLayer(Cells(0x6800), new Position16<BgPlanePixels>(0, 0), null), MainObjects: true) }, 15));
            var pixels = DisplayRendering.Render(masked);
            AssertEqual(new RgbaPixel(255, 0, 0), pixels[0], "masked subscreen does not add to backdrop");
            AssertEqual(palette == 4 ? new RgbaPixel(0, 255, 255) : new RgbaPixel(0, 255, 0), pixels[8 * 256 + 8],
                "only winning OBJ palettes four through seven participate in main-screen color math");

        }
        foreach (byte priority in new byte[] { 1, 2, 3 })
        foreach (bool highBackground in new[] { false, true })
        {
            byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(priority << 4);
            vram[0xe000 + (32 + 1) * 2 + 1] = (byte)(4 | (highBackground ? 32 : 0));
            var composite = new DisplayPacket(new(2, 1, 0), new LayeredScene(
                FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3),
                new RenderLayer[] { new TileAddRenderLayer(Cells(0x7000), IncludeObjects: true) }, 15));
            bool objectWins = priority >= (highBackground ? 3 : 2);
            AssertEqual(objectWins ? new RgbaPixel(255, 255, 0) : new RgbaPixel(255, 0, 255),
                DisplayRendering.Render(composite)[8 * 256 + 8],
                "subscreen selects BG2 or OBJ by priority before addition, never sums both");

        }
    }
}
