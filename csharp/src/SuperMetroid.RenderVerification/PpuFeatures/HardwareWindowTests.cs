using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.PpuFeatures;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>Compares native PPU window geometry and admission masks across software and GPU rendering.</summary>
internal static class HardwareWindowTests
{
    /// <summary>Checks seeded window configurations after packet serialization against hardware-size frame readback.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(396);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        var oam = new byte[SnesPpuLayoutConstants.OamUploadByteCount]; random.NextBytes(oam);
        var colors = Enumerable.Range(0, 256).Select(_ => Bgr555.FromWord((ushort)random.Next(32768))).ToArray();
        var memory = PpuMemorySnapshot.FromOam(vram, colors, oam, 128, 3);
        var rooms = RoomBackgroundsFixtures.Seeded(random, new(3, 5), new(7, 11), door: true, overlay: true);
        const SnesMainScreenLayers layers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;
        int sequence = 0;
        foreach (bool empty in new[] { false, true })
        for (int operation = 0; operation < 4; operation++)
        for (int mask = 0; mask < 16; mask++)
        {
            var admission = (SnesMainScreenLayers)((mask & 7) | ((mask & 8) << 1));
            var windows = new SnesWindowRegisters((SnesWindowSelectionPair)0x3a, (SnesWindowSelectionPair)0x0a,
                (SnesWindowSelectionPair)0x0b, empty ? 120 : 32, 96, 64, 128, operation * 0x55, operation);
            var registers = new OrdinaryGameplayRegisters(new Position16<RoomPixels>(3, 5), new Position16<Bg2Pixels>(7, 11), new Extent16<TilemapCells>(64, 32),
                GameplayVramLayout.Bg2TilemapWord, 0, 0,
                layers, Windows: windows, MainScreenWindowMask: admission);
            var frame = new RenderFrameSnapshot(new(++sequence, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new OrdinaryGameplayRenderLayer(registers, rooms) }, 15));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: native windows operation={operation}, TMW={admission}, empty={empty}");
        }
        Console.WriteLine($"{device.Kind}: {sequence} hardware-window frames match software.");
    }
}
