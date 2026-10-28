using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Bosses;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks Mother Brain ascent's per-line layer mask, floor occlusion, and CPU/GPU packet parity.</summary>
internal static class MotherBrainAscentMaskTests
{
    /// <summary>Compares ascent and ordinary visibility at representative band and floor pixels.</summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        cgram.SetColor(1, new RgbColor(31, 0, 0)); cgram.SetColor(2, new RgbColor(0, 31, 0));
        cgram.SetColor(3, new RgbColor(31, 31, 0)); cgram.SetColor(129, new RgbColor(0, 0, 31));
        for (int row = 0; row < 8; row++)
        {
            vram.LoadBytes(32 + row * 2, new byte[] { 255, 0 });
            vram.LoadBytes(64 + row * 2, new byte[] { 0, 255 });
            vram.LoadBytes(96 + row * 2, new byte[] { 255, 0 });
        }
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x2002, 2048).ToArray(), GameplayVramLayout.Bg2TilemapWord, 1);
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount];
        oam[0] = 16; oam[1] = 212; oam[2] = 3; oam[3] = 0x30;
        var memory = FramePaint.FromOam(vram.Bytes, cgram.Colors, oam, 1, 0);
        var registers = new OrdinaryGameplayView(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
            ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites);
        ScreenLayers[] masks = Enumerable.Range(32, 192).Select(y => (ScreenLayers)(y < 56 ? 0x15 : y < 216 ? 0x13 : 0x05)).ToArray();
        foreach (bool active in new[] { true, false })
        {
            var layer = new OrdinaryGameplayRenderLayer(registers, RoomBackgroundsFixtures.Uniform(vram.Bytes, cgram.Colors, new(1)), mainScreenLayersByLine: active ? masks : []);
            var scene = new LayeredScene(memory, [layer], 15);
            var pixels = DisplayRendering.Render(scene);
            Check(16, 218, active ? 1 : 129, "leg hidden by floor only during ascent");
            Check(16, 215, 129, "leg remains visible immediately above floor band");
            Check(0, 40, active ? 1 : 2, "upper band omits BG2 only during ascent");
            Check(0, 80, 2, "middle band retains BG2");
            var packet = new DisplayPacket(new(1, 1, 0), scene);
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Mother Brain ascent mask");
            var math = new GameplayColorMathRenderLayer(layer, Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false, new SceneBlend { Layers = ColorBlend.None, BlendsWithSubscreen = false, FixedColor = RgbColor.Black });
            var mathScene = new LayeredScene(memory, [math], 15);
            var mathPacket = new DisplayPacket(new(2, 1, 0), mathScene);
            PixelComparison.Verify(mathPacket, pixels, DisplayRendering.Render(mathScene), "CPU source-aware ascent mask");
            PixelComparison.Verify(mathPacket, pixels, renderer.RenderForReadback(mathPacket), "GPU source-aware ascent mask");

            void Check(int x, int y, int color, string property)
            {
                if (pixels[y * 256 + x] != cgram.GetRgba(color))
                    throw new InvalidDataException($"Mother Brain {property} at {x},{y}: expected palette {color}.");
            }
        }
        Console.WriteLine($"{device.Kind}: Mother Brain ascent bands, floor occlusion and removal passed.");
    }
}
