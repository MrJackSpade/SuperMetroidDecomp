using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks gameplay color math and X-ray source selection against the WARP reference and D3D11 pixels.</summary>
internal static class XrayGameplayTests
{
    /// <summary>Exercises window reveal, OBJ palette exemptions, subscreen math, old packet decoding, and patterned frames.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        int count = 0;
        foreach (bool reveal in new[] { false, true })
        foreach (bool sub in new[] { false, true })
        foreach (bool half in new[] { false, true })
        foreach (bool subtract in new[] { false, true })
        foreach (int palette in Enumerable.Range(0, 8))
        {
            var vram = new SnesVram();
            var cgram = new SnesCgram();
            cgram.SetColor(0, XrayRoomDisplayRules.ActiveBackdrop);
            cgram.SetColor(1, new RgbColor(31, 0, 0)); cgram.SetColor(2, new RgbColor(0, 20, 0)); cgram.SetColor(3, new RgbColor(0, 0, 10));
            cgram.SetColor(128 + palette * 16 + 1, new RgbColor(31, 0, 0));
            for (int y = 0; y < 8; y++)
            {
                vram.LoadBytes(y * 2, new byte[] { 255, 0 }); // OBJ character zero, red.
                vram.LoadBytes(32 + y * 2, new byte[] { 255, 0 }); // BG1 character one, red.
                vram.LoadBytes(64 + y * 2, new byte[] { 0, 255 }); // BG2 character two, green.
                vram.LoadBytes(0x2000 + 48 + y * 2, new byte[] { 255, 255 }); // BG3 character three, blue.
            }
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)2, 2048).ToArray(), GameplayVramLayout.Bg2TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5C00, 1);
            // BG2 shows the tilemap just loaded, as its library background's image (#1403).
            var bg1 = RoomBackgroundsFixtures.Uniform(vram.Bytes, cgram.Colors, new(1));
            var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
            oam[0] = 16; oam[1] = 64; oam[3] = (byte)(0x30 | palette << 1);
            var memory = FramePaint.FromOam(vram.Bytes, cgram.Colors, oam, 1, 0);
            Layer3Cells plane = Layer3CellsFixtures.FromVram(vram.Bytes, cgram.Colors, 0x1000);
            bg1 = bg1 with { Bg3 = plane };
            var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
                ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites);
            var lines = Enumerable.Repeat(new XrayWindowLine(128, 255), 224).ToArray();
            var controls = ColorBlend.Bg1 | ColorBlend.Bg2 | ColorBlend.Sprites | ColorBlend.Backdrop
                | (half ? ColorBlend.Half : 0) | (subtract ? ColorBlend.Subtract : 0);
            var bg3 = sub ? new FxPlaneBlendRenderLayer(plane, Layer3Plane.Rows / 2, 32, 32,
                ExpandedColorMathOperation.Add, new Position16<BgPlanePixels>[224]) : null;
            var layer = new GameplayColorMathRenderLayer(new(registers, bg1), lines, reveal, new SceneBlend { Layers = controls, BlendsWithSubscreen = true, FixedColor = new RgbColor(7, 7, 7) }, bg3);
            var scene = new LayeredScene(memory, new RenderLayer[] { layer }, 15);
            var packet = new DisplayPacket(new(++count, 1, 0), scene);
            var pixels = DisplayRendering.Render(scene);
            // Independent scalar expectations, not just parity between two implementations.
            RgbaPixel MathPixel(bool exempt)
            {
                if (exempt) return new(255, 0, 0, 255);
                byte Channel(int main, int operand)
                {
                    int value = subtract ? main - operand : main + operand;
                    if (half && sub) value = (int)Math.Floor(value / 2.0);
                    value = Math.Clamp(value, 0, 31);
                    return (byte)(value * 8 + value / 4);
                }
                return new(Channel(31, sub ? 0 : 7), Channel(0, sub ? 0 : 7), Channel(0, sub ? 10 : 7), 255);
            }
            Check(pixels[64 * 256 + 64], MathPixel(false), "outside BG1 math");
            Check(pixels[64 * 256 + 16], MathPixel(palette < 4), "OBJ palette exemption");
            Check(pixels[64 * 256 + 160], reveal ? new(0, 165, 0, 255) : new(255, 0, 0, 255), "inside layer selection/no math");
            Check(pixels[0], new(24, 24, 24, 255), "unchanged HUD");
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), $"{device.Kind}: source-aware X-ray {count}");
            if (!reveal && !sub && !half && !subtract)
            {
                // Native Phantoon configuration adds BG2 to main BG1/eligible OBJ,
                // without putting the body in the main-screen priority contest. His body is the boss's own BG2
                // image (#1353); the VRAM tilemap holds the same words, so both sources show the same body (#1426).
                foreach (bool bossImage in new[] { false, true })
                foreach (bool blackBody in new[] { false, true })
                {
                    var bodyRooms = bossImage
                        ? bg1 with { BossBg2 = RoomBackgroundsFixtures.UniformBody(new(2), new(64, 32), vram.Bytes, cgram.Colors) }
                        : bg1;
                    var additive = new GameplayColorMathRenderLayer(new(registers with
                    {
                        MainScreenLayers = ScreenLayers.Foreground | ScreenLayers.Sprites,
                    }, bodyRooms), Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false, new SceneBlend { Layers = ColorBlend.Bg1 | ColorBlend.Bg3 | ColorBlend.Sprites | ColorBlend.Backdrop, BlendsWithSubscreen = true, FixedColor = RgbColor.Black }, subscreenUsesBg2: true);
                    var bodyColors = cgram.Colors.ToArray();
                    if (blackBody) bodyColors[2] = RgbColor.Black;
                    var addScene = new LayeredScene(FramePaint.FromOam(vram.Bytes, bodyColors, oam, 1, 0), [additive], 15);
                    var addPixels = DisplayRendering.Render(addScene);
                    var yellow = new RgbaPixel(255, blackBody ? (byte)0 : (byte)165, 0);
                    Check(addPixels[64 * 256 + 64], yellow, "BG2 additive color contribution / black identity");
                    Check(addPixels[64 * 256 + 16], palette < 4 ? new(255, 0, 0) : yellow, "BG2 subscreen OBJ palette eligibility");
                    Check(addPixels[0], pixels[0], "BG2 subscreen preserves HUD");
                    var addPacket = new DisplayPacket(new(++count, 1, 0), addScene);
                    PixelComparison.Verify(addPacket, addPixels, renderer.RenderForReadback(addPacket), "BG2 subscreen positive colors and black identity");
                }
            }
            if (!reveal && !sub && !half && !subtract && palette == 0)
            {
                // Expose BG2 at x64 and the backdrop at x72 using transparent tile
                // four. Keep foreground at x80 and the OBJ at x16. This distinguishes
                // native Fireflea source selection from a whole-frame dark overlay.
                vram.ExecuteWordTransfer(new ushort[] { 4 }, GameplayVramLayout.Bg2TilemapWord + 8 * 32 + 9, 1);
                var dark = new GameplayColorMathRenderLayer(new(registers, RoomBackgroundsFixtures.Uniform(vram.Bytes, cgram.Colors, new(1), new TilemapCell(8, 8, new(4)), new TilemapCell(9, 8, new(4))) with { Bg3 = Layer3CellsFixtures.FromVram(vram.Bytes, cgram.Colors, 0x1000) }), Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false, new SceneBlend { Layers = ColorBlend.Bg2 | ColorBlend.Backdrop | ColorBlend.Subtract, BlendsWithSubscreen = false, FixedColor = new RgbColor(18, 18, 18) });
                var darkScene = new LayeredScene(FramePaint.FromOam(vram.Bytes, cgram.Colors, oam, 1, 0), new RenderLayer[] { dark }, 15);
                var darkPixels = DisplayRendering.Render(darkScene);
                Check(darkPixels[64 * 256 + 64], new(0, 16, 0, 255), "Fireflea BG2 subtraction");
                Check(darkPixels[64 * 256 + 72], new(0, 0, 0, 255), "Fireflea backdrop subtraction");
                Check(darkPixels[64 * 256 + 80], new(255, 0, 0, 255), "Fireflea foreground exemption");
                Check(darkPixels[64 * 256 + 16], new(255, 0, 0, 255), "Fireflea OBJ exemption");
                var darkPacket = new DisplayPacket(new(++count, 1, 0), darkScene);
                PixelComparison.Verify(darkPacket, darkPixels, renderer.RenderForReadback(darkPacket), "normal Fireflea sources");
            }
        }
        var random = new Random(34813);
        for (int frame = 0; frame < 32; frame++)
        {
            var bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes);
            var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
            var colors = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
            // The OBSEL is drawn after the layer below, keeping the seeded stream; the OAM converts under it then.
            ushort Word() => (ushort)random.Next(65536);
            var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(Word(), Word()), new Position16<Bg2Pixels>(Word(), Word()),
                frame % 2 == 0 ? new Extent16<TilemapCells>(64, 32) : new Extent16<TilemapCells>(32, 64),
                (ScreenLayers)(random.Next(32) & 0x13));
            // Every fifth frame's planes draw from block pictures (#1392).
            var gameplay = new OrdinaryGameplayRenderLayer(registers, RoomBackgroundsFixtures.Seeded(random, registers.Bg1Scroll.AsBgPlaneScroll(),
                    frame % 2 == 0 ? registers.Bg2Scroll.AsBgPlaneScroll() : null, door: frame % 3 == 0, overlay: frame % 4 == 0,
                    pictureColours: frame % 5 == 0 ? colors : null),
                new Bg2LineScrolls(
                Enumerable.Range(0, 192).Select(_ => Word()).ToArray(),
                Enumerable.Range(0, 192).Select(_ => Word()).ToArray()));
            var lines = Enumerable.Range(0, 224).Select(_ => new XrayWindowLine((byte)random.Next(256), (byte)random.Next(256))).ToArray();
            int bg3Rows = frame % 2 == 0 ? 32 : 64;
            var bg3 = new FxPlaneBlendRenderLayer(gameplay.Rooms.ShownBg3, random.Next(Layer3Plane.Rows - bg3Rows + 1), bg3Rows, 32,
                ExpandedColorMathOperation.Add, Enumerable.Range(0, 224).Select(_ => new Position16<BgPlanePixels>(Word(), Word())).ToArray());
            bool bg2Subscreen = frame % 4 == 0;
            var layer = new GameplayColorMathRenderLayer(gameplay, lines, frame % 2 == 0, new SceneBlend { Layers = (ColorBlend)random.Next(256), BlendsWithSubscreen = bg2Subscreen || frame % 3 != 0, FixedColor = new RgbColor((byte)random.Next(32), (byte)random.Next(32), (byte)random.Next(32)) }, bg2Subscreen ? null : bg3, bg2Subscreen);
            byte obsel = (byte)random.Next(256);
            var memory = FramePaint.FromOam(bytes, colors, oam, 128, obsel);
            var scene = new LayeredScene(memory, new RenderLayer[] { layer }, (byte)(frame % 16));
            var packet = new DisplayPacket(new(++count, 1, 0), scene);
            var expected = DisplayRendering.Render(scene);
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: patterned X-ray {frame}");
        }
        // The reveal on BG2 drawn from block pictures in every arrangement (#1392), at the fine scrolls X-ray gives BG2.
        for (int frame = 0; frame < 8; frame++)
        {
            var bytes = new byte[SnesVram.ByteCount]; random.NextBytes(bytes);
            var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
            var colors = Enumerable.Range(0, 256).Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
            ushort Fine() => (ushort)random.Next(16);
            var registers = new OrdinaryGameplayView(new Position16<RoomPixels>((ushort)random.Next(65536), (ushort)random.Next(65536)),
                new Position16<Bg2Pixels>(Fine(), Fine()), new Extent16<TilemapCells>(64, 32),
                ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites);
            SuperMetroid.Core.Gameplay.RoomBackgrounds seeded = RoomBackgroundsFixtures.Seeded(random, registers.Bg1Scroll.AsBgPlaneScroll(), pictureColours: colors);
            var gameplay = new OrdinaryGameplayRenderLayer(registers, seeded with { XrayReveal = RoomBackgroundsFixtures.SeededReveal(random, colors) });
            var lines = Enumerable.Range(0, 224).Select(_ => new XrayWindowLine((byte)random.Next(256), (byte)random.Next(256))).ToArray();
            var layer = new GameplayColorMathRenderLayer(gameplay, lines, true, new SceneBlend { Layers = (ColorBlend)random.Next(256), BlendsWithSubscreen = false, FixedColor = new RgbColor((byte)random.Next(32), (byte)random.Next(32), (byte)random.Next(32)) }, null);
            var memory = FramePaint.FromOam(bytes, colors, oam, 128, 3);
            var scene = new LayeredScene(memory, new RenderLayer[] { layer }, 15);
            var packet = new DisplayPacket(new(++count, 1, 0), scene);
            PixelComparison.Verify(packet, DisplayRendering.Render(scene), renderer.RenderForReadback(packet),
                $"{device.Kind}: X-ray reveal pictures {frame}");
        }
        Console.WriteLine($"{device.Kind}: {count} source-aware X-ray cases pass BG masking, OBJ palette exemptions, subscreen fallback, arithmetic, and priority/scroll/flip patterns.");

        static void Check(RgbaPixel actual, RgbaPixel expected, string context)
        {
            if (actual != expected) throw new InvalidOperationException($"{context}: expected {expected}, got {actual}.");
        }
    }
}
