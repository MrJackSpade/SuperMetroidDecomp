using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;

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
        byte[] vram = new byte[SnesVram.ByteCount];
        for (int row = 0; row < 8; row++) vram[row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 0);
        var packet = new DisplayPacket(0, new LayeredScene(new FramePaint(memory.Backdrop),
            new DisplayLayer[] { new SpritesLayer(memory.SpriteDraws, SpriteBlend.Glows) }, 15));

        var pixels = DisplayRendering.Render(packet);
        AssertEqual(new RgbaPixel(255, 0, 255), pixels[8 * 256 + 8], "a glowing sprite adds its blue to the red backdrop");
        AssertEqual(new RgbaPixel(255, 0, 0), pixels[0], "a glowing sprite's transparent pixels leave the backdrop");
    }

    /// <summary>
    /// Checks a floor that adds its own colour: it shows over the sprites drawn behind the room and adds its colour to
    /// itself and to the blending sprites in front of it (#1514). A sprite's depth is its frame's, which the import takes
    /// from its priority: priorities zero and one draw behind the whole room, two and three in front of the floor.
    /// </summary>
    private static void VerifyMode7Bg1Addition()
    {
        var registers = new FloorView(0, FloorView.OwnSize, new Offset16<FloorPixels>(0, 0), new Offset16<FloorPixels>(0, 0));
        foreach (bool opaqueBg in new[] { false, true })
        {
            byte[] vram = new byte[SnesVram.ByteCount];
            if (opaqueBg) for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
            for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
            ushort[] floorColors = new ushort[256];
            floorColors[0] = 3 << 10; floorColors[1] = 8 << 5;
            // The floor's pixels ink only colours zero and one, so it is built once per image; the sprite's palette and
            // priority change only its draw.
            var floor = AffineFloorFixtures.Floor(vram, ToColors(floorColors));
            foreach (byte palette in new byte[] { 0, 4 })
            {
                ushort[] colors = [.. floorColors];
                colors[129 + palette * 16] = 12;
                var shownColours = ToColors(colors);
                foreach (byte priority in new byte[] { 0, 1, 2, 3 })
                {
                    byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
                    oam[0] = 8; oam[1] = 8; oam[3] = (byte)(palette * 2 + priority * 16);
                    var memory = FramePaint.FromOam(vram, shownColours, oam, 1, 3);
                    var packet = new DisplayPacket(0, new LayeredScene(memory,
                        new DisplayLayer[] { new TurningFloorLayer(registers, floor, AddsForeground: true) }, 15));
                    var pixels = DisplayRendering.Render(packet);
                    bool behindFloor = opaqueBg && priority < 2;
                    RgbaPixel expected = behindFloor ? new(0, 132, 0)
                        : opaqueBg && palette == 4 ? new(99, 66, 0) : new(99, 0, 0);
                    AssertEqual(expected, pixels[8 * 256 + 8], "a sprite behind the room lies under the floor; a blending sprite in front takes its colour");
                    AssertEqual(opaqueBg ? new RgbaPixel(0, 132, 0) : new RgbaPixel(0, 0, 24), pixels[0],
                        "BG1 doubles itself but transparent BG1 preserves unmodified backdrop");
                    AssertThrows<ArgumentException>(() => new LayeredScene(memory,
                        new DisplayLayer[] { new TurningFloorLayer(registers, floor, true, true) }, 15), "reject contradictory Mode7 math modes");
                }
            }
        }
    }

    /// <summary>Checks per-channel Mode 7 minus OBJ subtraction and rejection of old packets that cannot encode it.</summary>
    private static void VerifyMode7ObjSubtraction()
    {
        byte[] vram = new byte[SnesVram.ByteCount];
        for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
        for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[1] = 31 | (12 << 5);
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3);
        var packet = new DisplayPacket(0, new LayeredScene(memory,
            new DisplayLayer[] { new TurningFloorLayer(new FloorView(0, FloorView.OwnSize, default, default), AffineFloorFixtures.Floor(vram, ToColors(colors)), true) }, 15));
        var pixels = DisplayRendering.Render(packet);
        AssertEqual(new RgbaPixel(198, 99, 0), pixels[8 * 256 + 8], "Mode7 minus OBJ clamps each five-bit component without halving");
        AssertEqual(new RgbaPixel(255, 99, 0), pixels[0], "transparent OBJ leaves Mode7 color unchanged");

    }

    /// <summary>Checks a sprite layer's tint: added to the sprites that blend, five-bit channels expanded, never to the backdrop (#1514).</summary>
    private static void VerifyObjFixedColor()
    {
        byte[] vram = new byte[SnesVram.ByteCount];
        for (int y = 0; y < 8; y++) vram[y * 2] = 255;
        ushort[] palette = new ushort[256];
        palette[0] = 31 << 10;
        palette[129] = palette[193] = 7;
        foreach (byte objPalette in new byte[] { 0, 4 })
        {
            byte[] oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(objPalette * 2);
            FramePaint memory = FramePaint.FromOam(vram, ToColors(palette), oam, 1, 0);
            var packet = new DisplayPacket(0, new LayeredScene(new FramePaint(memory.Backdrop),
                new DisplayLayer[] { new SpritesLayer(memory.SpriteDraws, tint: new RgbColor(66, 33, 16)) }, 15));
            var pixels = DisplayRendering.Render(packet);
            AssertEqual(objPalette == 4 ? new RgbaPixel(123, 33, 16) : new RgbaPixel(57, 0, 0), pixels[8 * 256 + 8],
                "a sprite tint brightens only the sprites that blend, its five-bit channels expanded");
            AssertEqual(new RgbaPixel(0, 0, 255), pixels[0], "a sprite tint does not alter the uncovered backdrop");
        }
    }

    /// <summary>Checks a glowing plane's four-bit decoding and where it glows over a plane and the sprites over it (#1514).</summary>
    private static void VerifyBg4SubscreenAddition()
    {
        byte[] vram = new byte[SnesVram.ByteCount];
        // Color four depends on the third bitplane: a mistaken 2-bpp decode sees zero.
        for (int row = 0; row < 8; row++) vram[row * 2 + 16] = 255;
        for (int tile = 0; tile < 1024; tile++) vram[0xe000 + tile * 2 + 1] = 4;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[20] = 31 << 10;
        // The subscreen and its coverage picture the one-screen maps at a tilemap word, read as the fixture stands when drawn.
        PicturedPlane Plane(int tilemapWord) => VramPictureFixtures.Plane(vram, ToColors(colors), tilemapWord, 0, TileInkDepth.SixteenColours, AddedPicturedPlaneLayer.MapSize);
        var memory = FramePaint.FromNative(ToColors(colors), []);
        var packet = new DisplayPacket(0, new LayeredScene(memory,
            new DisplayLayer[] { new AddedPicturedPlaneLayer(Plane(0x7000)) }, 15));
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
            FramePaint sprites = FramePaint.FromOam(vram, ToColors(colors), oam, 1, 3);
            var shown = new SpritesLayer(sprites.SpriteDraws);
            var masked = new DisplayPacket(0, new LayeredScene(new FramePaint(sprites.Backdrop),
                new DisplayLayer[] { shown, new AddedPicturedPlaneLayer(Plane(0x7000),
                    new PicturedPlaneLayer(Plane(0x6800), new Position16<PlanePixels>(0, 0)), shown) }, 15));
            var pixels = DisplayRendering.Render(masked);
            AssertEqual(new RgbaPixel(255, 0, 0), pixels[0], "a glow over a plane does not add to the backdrop");
            AssertEqual(palette == 4 ? new RgbaPixel(0, 255, 255) : new RgbaPixel(0, 255, 0), pixels[8 * 256 + 8],
                "a glow over the sprites covers only those that blend");

        }
    }
}
