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
            var layer = new XrayGameplayRenderLayer(gameplay, lines, frame % 2 == 0, (SnesColorMathControl)random.Next(256),
                frame % 3 != 0, (byte)random.Next(32), (byte)random.Next(32), (byte)random.Next(32), bg3);
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
