using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Checks waterfall subscreen priority and subtraction against software and GPU rendering.</summary>
internal static class WaterfallTests
{
    /// <summary>Uses native TM=$11, TS=$06, CGADSUB=$B1 and distinct colors to expose priority and color-math errors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
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
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)2, 2048).ToArray(), GameplayVramLayout.Bg2TilemapWord, 1);
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)3, 1024).ToArray(), 0x5c00, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0x2003 }, 0x5c00 + 8 * 32 + 4, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0x2002 }, GameplayVramLayout.Bg2TilemapWord + 8 * 32 + 4, 1);
        vram.ExecuteWordTransfer(new ushort[] { 4 }, GameplayVramLayout.Bg2TilemapWord + 8 * 32 + 6, 1);
        vram.ExecuteWordTransfer(new ushort[] { 4 }, GameplayVramLayout.Bg2TilemapWord + 8 * 32 + 8, 1);
        vram.ExecuteWordTransfer(new ushort[] { 0 }, 0x5c00 + 8 * 32 + 8, 1);
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 80; oam[1] = 64; oam[3] = 0x36;
        oam[4] = 96; oam[5] = 64; oam[7] = 0x38;
        var memory = FramePaint.FromOam(vram.Bytes, cgram.Colors, oam, 2, 0);
        Layer3Cells plane = Layer3CellsFixtures.FromVram(vram.Bytes, cgram.Colors, 0x1000);
        var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
            WaterfallRoomDisplayRules.MainScreen);
        var bg3 = new FxPlaneBlendRenderLayer(plane, Layer3Plane.Rows / 2, 32, 32,
            ExpandedColorMathOperation.Subtract, new Position16<BgPlanePixels>[224]);
        var layer = new GameplayColorMathRenderLayer(new(registers, RoomBackgroundsFixtures.Uniform(vram.Bytes, cgram.Colors, new(1), new TilemapCell(14, 8, new(4))) with { Bg3 = plane }), Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false, new SceneBlend { Layers = WaterfallRoomDisplayRules.Blend, BlendsWithSubscreen = true, FixedColor = Bgr555.Black }, bg3, subscreenUsesBg2: true);
        var scene = new LayeredScene(memory, [layer], 15);
        var pixels = DisplayRendering.Render(scene);
        Check(16, 19, 13, 7, "BG2 beats low BG3 and subtracts through foreground");
        Check(32, 22, 17, 12, "high BG3 beats BG2");
        Check(48, 22, 17, 12, "transparent BG2 exposes low BG3");
        Check(64, 24, 20, 16, "transparent subscreen uses zero fixed color");
        Check(80, 24, 20, 16, "OBJ palette three is exempt");
        Check(96, 19, 13, 7, "OBJ palette four participates");
        Check(112, 19, 13, 7, "backdrop participates");
        var packet = new DisplayPacket(new(1, 1, 0), scene);
        PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "waterfall source/priority/color math");
        Console.WriteLine($"{device.Kind}: waterfall priority, colors and OBJ exemptions passed.");

        void Check(int x, int red, int green, int blue, string property)
        {
            static byte Expand(int value) => (byte)((value << 3) | (value >> 2));
            var expected = new Rgba32(Expand(red), Expand(green), Expand(blue), 255);
            if (pixels[64 * 256 + x] != expected)
                throw new InvalidDataException($"Waterfall {property}: expected {expected}, got {pixels[64 * 256 + x]}.");
        }
    }
}
