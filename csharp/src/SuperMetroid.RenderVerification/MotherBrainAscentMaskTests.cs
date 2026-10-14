using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
namespace SuperMetroid.RenderVerification;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Checks Mother Brain ascent's per-line layer mask, floor occlusion, and CPU/GPU packet parity.</summary>
internal static class MotherBrainAscentMaskTests
{
    /// <summary>Compares ascent and ordinary visibility at representative band and floor pixels after packet round-trip.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        cgram.SetColor(1, new Bgr555(31, 0, 0)); cgram.SetColor(2, new Bgr555(0, 31, 0));
        cgram.SetColor(3, new Bgr555(31, 31, 0)); cgram.SetColor(129, new Bgr555(0, 0, 31));
        for (int row = 0; row < 8; row++)
        {
            vram.LoadBytes(32 + row * 2, new byte[] { 255, 0 });
            vram.LoadBytes(64 + row * 2, new byte[] { 0, 255 });
            vram.LoadBytes(96 + row * 2, new byte[] { 255, 0 });
        }
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)1, 2048).ToArray(), GameplayVramLayout.Bg1TilemapWord, 1);
        vram.ExecuteWordTransfer(Enumerable.Repeat((ushort)0x2002, 2048).ToArray(), GameplayVramLayout.Bg2TilemapWord, 1);
        var oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 16; oam[1] = 212; oam[2] = 3; oam[3] = 0x30;
        var memory = new PpuMemorySnapshot(vram.Bytes, cgram.Colors, oam, 1);
        var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(0, 0), new Position16<Bg2Pixels>(0, 0), new Extent16<TilemapCells>(64, 32),
            GameplayVramLayout.Bg2TilemapWord, 0, 0, 0x1000,
            SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj);
        ushort[] masks = Enumerable.Range(32, 192).Select(y => (ushort)(y < 56 ? 0x15 : y < 216 ? 0x13 : 0x05)).ToArray();
        foreach (bool active in new[] { true, false })
        {
            var layer = new OrdinaryGameplayRenderLayer(registers, mainScreenLayersByLine: active ? masks : []);
            var scene = new LayeredRenderSnapshot(memory, [layer], 0, 15);
            var pixels = SoftwareLayeredSnapshotRenderer.Render(scene);
            Check(16, 218, active ? 1 : 129, "leg hidden by floor only during ascent");
            Check(16, 215, 129, "leg remains visible immediately above floor band");
            Check(0, 40, active ? 1 : 2, "upper band omits BG2 only during ascent");
            Check(0, 80, 2, "middle band retains BG2");
            var packet = new RenderFrameSnapshot(new(1, 1, 0), scene);
            PixelComparison.Verify(packet, pixels, renderer.RenderForReadback(packet), "Mother Brain ascent mask");
            var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotWriter.Serialize(packet));
            PixelComparison.Verify(restored, pixels, renderer.RenderForReadback(restored), "ascent mask packet round trip");
            var math = new GameplayColorMathRenderLayer(layer, Enumerable.Repeat(new XrayWindowLine(255, 0), 224).ToArray(), false, new ColorMathRegisters { ColorMath = SnesColorMathControl.None, AddSubscreen = false, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 });
            var mathScene = new LayeredRenderSnapshot(memory, [math], 0, 15);
            var mathPacket = new RenderFrameSnapshot(new(2, 1, 0), mathScene);
            PixelComparison.Verify(mathPacket, pixels, SoftwareLayeredSnapshotRenderer.Render(mathScene), "CPU source-aware ascent mask");
            PixelComparison.Verify(mathPacket, pixels, renderer.RenderForReadback(mathPacket), "GPU source-aware ascent mask");

            void Check(int x, int y, int color, string property)
            {
                if (pixels[y * 256 + x] != cgram.GetRgba(color))
                    throw new InvalidDataException($"Mother Brain {property} at {x},{y}: expected palette {color}.");
            }
        }
        Console.WriteLine($"{device.Kind}: Mother Brain ascent bands, floor occlusion, removal and packet round trip passed.");
    }
}
