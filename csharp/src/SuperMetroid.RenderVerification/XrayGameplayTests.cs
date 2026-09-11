using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class XrayGameplayTests
{
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
            cgram.SetColor(1, 31); cgram.SetColor(2, 20 << 5); cgram.SetColor(3, 10 << 10);
            cgram.SetColor(128 + palette * 16 + 1, 31);
            for (int y = 0; y < 8; y++)
            {
                vram.LoadBytes(y * 2, new byte[] { 255, 0 }); // OBJ character zero, red.
                vram.LoadBytes(32 + y * 2, new byte[] { 255, 0 }); // BG1 character one, red.
                vram.LoadBytes(64 + y * 2, new byte[] { 0, 255 }); // BG2 character two, green.
                vram.LoadBytes(0x2000 + 48 + y * 2, new byte[] { 255, 255 }); // BG3 character three, blue.
            }
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)1, 2048).ToArray(), SnesPpuLayout.GameplayBg1TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)2, 2048).ToArray(), SnesPpuLayout.GameplayBg2TilemapWord, 1);
            vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5C00, 1);
            var oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 16; oam[1] = 64; oam[3] = (byte)(0x30 | palette << 1);
            var memory = new PpuMemorySnapshot(vram.Bytes, cgram.Colors, oam, 1);
            var registers = new OrdinaryGameplayRegisters(0, 0, 0, 0, 64, 32,
                SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, 0x1000,
                SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
            var lines = Enumerable.Repeat(new XrayWindowLine(128, 255), 224).ToArray();
            var controls = SnesColorMathControl.Bg1 | SnesColorMathControl.Bg2 | SnesColorMathControl.Obj | SnesColorMathControl.Backdrop
                | (half ? SnesColorMathControl.Half : 0) | (subtract ? SnesColorMathControl.Subtract : 0);
            var bg3 = sub ? new Bg2BppColorMathRenderLayer(0x5C00, 0x1000, 32, 32,
                ExpandedColorMathOperation.Add, new BackgroundLineScroll[224]) : null;
            var layer = new XrayGameplayRenderLayer(new(registers), lines, reveal, controls, true, 7, 7, 7, bg3);
            var scene = new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 0, 15);
            var packet = new RenderFrameSnapshot(new(++count, 1, 0), scene);
            var pixels = SoftwareLayeredSnapshotRenderer.Render(scene);
            // Independent scalar expectations, not just parity between two implementations.
            Rgba32 MathPixel(bool exempt)
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
            var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(restored, pixels, renderer.RenderForReadback(restored), "source-aware X-ray packet round trip");
            // The version-24 single-X-ray-layer packet ends after its optional BG3
            // child. Remove only version 25's trailing BG2 selector and restore the
            // historical header, checking that old packets still select no BG2 operand.
            byte[] legacyBytes = RenderFrameSnapshotCodec.Serialize(packet)[..^1];
            legacyBytes[8] = 24; legacyBytes[9] = 0;
            var legacy = RenderFrameSnapshotCodec.Deserialize(legacyBytes);
            PixelComparison.Verify(legacy, pixels, renderer.RenderForReadback(legacy), "version-24 subscreen compatibility");
            if (!reveal && !sub && !half && !subtract)
            {
                // Native Phantoon configuration adds BG2 to main BG1/eligible OBJ,
                // without putting the body in the main-screen priority contest.
                var additive = new XrayGameplayRenderLayer(new(registers with
                {
                    MainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj,
                }), Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false,
                    SnesColorMathControl.Bg1 | SnesColorMathControl.Bg3 | SnesColorMathControl.Obj | SnesColorMathControl.Backdrop,
                    true, 0, 0, 0, subscreenUsesBg2: true);
                foreach (bool blackBody in new[] { false, true })
                {
                    var bodyColors = cgram.Colors.ToArray();
                    if (blackBody) bodyColors[2] = 0;
                    var addScene = new LayeredRenderSnapshot(new(vram.Bytes, bodyColors, oam, 1), [additive], 0, 15);
                    var addPixels = SoftwareLayeredSnapshotRenderer.Render(addScene);
                    var yellow = new Rgba32(255, blackBody ? (byte)0 : (byte)165, 0);
                    Check(addPixels[64 * 256 + 64], yellow, "BG2 additive color contribution / black identity");
                    Check(addPixels[64 * 256 + 16], palette < 4 ? new(255, 0, 0) : yellow, "BG2 subscreen OBJ palette eligibility");
                    Check(addPixels[0], pixels[0], "BG2 subscreen preserves HUD");
                    var addPacket = new RenderFrameSnapshot(new(++count, 1, 0), addScene);
                    var addRestored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(addPacket));
                    PixelComparison.Verify(addRestored, addPixels, renderer.RenderForReadback(addRestored), "BG2 subscreen positive colors and black identity");
                }
            }
            if (!reveal && !sub && !half && !subtract && palette == 0)
            {
                // Expose BG2 at x64 and the backdrop at x72 using transparent tile
                // four. Keep foreground at x80 and the OBJ at x16. This distinguishes
                // native Fireflea source selection from a whole-frame dark overlay.
                vram.ExecuteWordTransfer(new ushort[] { 4, 4 }, SnesPpuLayout.GameplayBg1TilemapWord + 8 * 32 + 8, 1);
                vram.ExecuteWordTransfer(new ushort[] { 4 }, SnesPpuLayout.GameplayBg2TilemapWord + 8 * 32 + 9, 1);
                var dark = new XrayGameplayRenderLayer(new(registers),
                    Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false,
                    SnesColorMathControl.Bg2 | SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract,
                    false, 18, 18, 18);
                var darkScene = new LayeredRenderSnapshot(new(vram.Bytes, cgram.Colors, oam, 1), new RenderLayer[] { dark }, 0, 15);
                var darkPixels = SoftwareLayeredSnapshotRenderer.Render(darkScene);
                Check(darkPixels[64 * 256 + 64], new(0, 16, 0, 255), "Fireflea BG2 subtraction");
                Check(darkPixels[64 * 256 + 72], new(0, 0, 0, 255), "Fireflea backdrop subtraction");
                Check(darkPixels[64 * 256 + 80], new(255, 0, 0, 255), "Fireflea foreground exemption");
                Check(darkPixels[64 * 256 + 16], new(255, 0, 0, 255), "Fireflea OBJ exemption");
                var darkPacket = new RenderFrameSnapshot(new(++count, 1, 0), darkScene);
                PixelComparison.Verify(darkPacket, darkPixels, renderer.RenderForReadback(darkPacket), "normal Fireflea sources");
            }
        }
        var random = new Random(34813);
        for (int frame = 0; frame < 32; frame++)
        {
            var bytes = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(bytes);
            var oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
            var colors = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
            var memory = new PpuMemorySnapshot(bytes, colors, oam, 128);
            ushort Word() => (ushort)random.Next(65536);
            var registers = new OrdinaryGameplayRegisters(Word(), Word(), Word(), Word(),
                frame % 2 == 0 ? 64 : 32, frame % 2 == 0 ? 32 : 64, Word(), Word(), Word(), Word(),
                (SnesMainScreenLayers)(random.Next(32) & 0x13));
            var gameplay = new OrdinaryGameplayRenderLayer(registers,
                Enumerable.Range(0, 192).Select(_ => Word()).ToArray(),
                Enumerable.Range(0, 192).Select(_ => Word()).ToArray());
            var lines = Enumerable.Range(0, 224).Select(_ => new XrayWindowLine((byte)random.Next(256), (byte)random.Next(256))).ToArray();
            var bg3 = new Bg2BppColorMathRenderLayer(Word(), Word(), frame % 2 == 0 ? 32 : 64, 32,
                ExpandedColorMathOperation.Add, Enumerable.Range(0, 224).Select(_ => new BackgroundLineScroll(Word(), Word())).ToArray());
            bool bg2Subscreen = frame % 4 == 0;
            var layer = new XrayGameplayRenderLayer(gameplay, lines, frame % 2 == 0, (SnesColorMathControl)random.Next(256),
                bg2Subscreen || frame % 3 != 0, (byte)random.Next(32), (byte)random.Next(32), (byte)random.Next(32),
                bg2Subscreen ? null : bg3, bg2Subscreen);
            var scene = new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, (byte)random.Next(256), (byte)(frame % 16));
            var packet = new RenderFrameSnapshot(new(++count, 1, 0), scene);
            var expected = SoftwareLayeredSnapshotRenderer.Render(scene);
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: patterned X-ray {frame}");
            var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(restored, expected, renderer.RenderForReadback(restored), "patterned X-ray round trip");
        }
        Console.WriteLine($"{device.Kind}: {count} source-aware X-ray cases pass BG masking, OBJ palette exemptions, subscreen fallback, arithmetic, priority/scroll/flip patterns and packet round trips.");

        static void Check(Rgba32 actual, Rgba32 expected, string context)
        {
            if (actual != expected) throw new InvalidOperationException($"{context}: expected {expected}, got {actual}.");
        }
    }
}
