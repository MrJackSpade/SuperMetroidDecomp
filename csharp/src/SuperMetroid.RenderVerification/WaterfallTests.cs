using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class WaterfallTests
{
    // GR-3: native TM=$11, TS=$06, CGADSUB=$B1. Distinct colors make
    // an omitted plane, wrong priority, opaque overlay or halving observable.
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        Bgr555 main = new(24, 20, 16);
        cgram.SetColor(0, main);
        cgram.SetColor(1, main);
        cgram.SetColor(2, new Bgr555(5, 7, 9));
        cgram.SetColor(3, new Bgr555(2, 3, 4));
        cgram.SetColor(128 + 3 * 16 + 1, main);
        cgram.SetColor(128 + 4 * 16 + 1, main);
        for (int row = 0; row < 8; row++)
        {
            vram.LoadBytes(row * 2, new byte[] { 255, 0 });
            vram.LoadBytes(32 + row * 2, new byte[] { 255, 0 });
            vram.LoadBytes(64 + row * 2, new byte[] { 0, 255 });
            vram.LoadBytes(0x2000 + 48 + row * 2, new byte[] { 255, 255 });
        }
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)1, 2048).ToArray(), SnesPpuLayout.GameplayBg1TilemapWord, 1);
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)2, 2048).ToArray(), SnesPpuLayout.GameplayBg2TilemapWord, 1);
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5c00, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0x2003 }, 0x5c00 + 8 * 32 + 4, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0x2002 }, SnesPpuLayout.GameplayBg2TilemapWord + 8 * 32 + 4, 1);
        vram.ExecuteWordTransfer(new ushort[] { 4 }, SnesPpuLayout.GameplayBg2TilemapWord + 8 * 32 + 6, 1);
        vram.ExecuteWordTransfer(new ushort[] { 4 }, SnesPpuLayout.GameplayBg2TilemapWord + 8 * 32 + 8, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0 }, 0x5c00 + 8 * 32 + 8, 1);
        vram.ExecuteWordTransfer(new ushort[] { 4 }, SnesPpuLayout.GameplayBg1TilemapWord + 8 * 32 + 14, 1);
        var oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 80; oam[1] = 64; oam[3] = 0x36;
        oam[4] = 96; oam[5] = 64; oam[7] = 0x38;
        var memory = new PpuMemorySnapshot(vram.Bytes, cgram.Colors, oam, 2);
        var registers = new OrdinaryGameplayRegisters(0, 0, 0, 0, 64, 32,
            SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, 0x1000, WaterfallRoomDisplayRules.MainScreen);
        var bg3 = new Bg2BppColorMathRenderLayer(0x5c00, 0x1000, 32, 32,
            ExpandedColorMathOperation.Subtract, new BackgroundLineScroll[224]);
        var layer = new GameplayColorMathRenderLayer(new(registers),
            Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false,
            WaterfallRoomDisplayRules.ColorMath, true, 0, 0, 0, bg3, subscreenUsesBg2: true);
        var scene = new LayeredRenderSnapshot(memory, [layer], 0, 15);
        var pixels = SoftwareLayeredSnapshotRenderer.Render(scene);
        Check(16, 19, 13, 7, "BG2 beats low BG3 and subtracts through foreground");
        Check(32, 22, 17, 12, "high BG3 beats BG2");
        Check(48, 22, 17, 12, "transparent BG2 exposes low BG3");
        Check(64, 24, 20, 16, "transparent subscreen uses zero fixed color");
        Check(80, 24, 20, 16, "OBJ palette three is exempt");
        Check(96, 19, 13, 7, "OBJ palette four participates");
        Check(112, 19, 13, 7, "backdrop participates");
        var packet = new RenderFrameSnapshot(new(1, 1, 0), scene);
        PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "waterfall source/priority/color math");
        var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
        PixelComparison.Verify(restored, pixels, renderer.RenderForReadback(restored), "combined subscreen packet round trip");
        Console.WriteLine($"{device.Kind}: waterfall priority, colors, OBJ exemptions and packet round trip passed.");

        void Check(int x, int red, int green, int blue, string property)
        {
            static byte Expand(int value) => (byte)((value << 3) | (value >> 2));
            var expected = new Rgba32(Expand(red), Expand(green), Expand(blue), 255);
            if (pixels[64 * 256 + x] != expected)
                throw new InvalidDataException($"Waterfall {property}: expected {expected}, got {pixels[64 * 256 + x]}.");
        }
    }
}
