using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class HardwareWindowTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var random = new Random(396);
        var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
        var oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
        var colors = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vram, colors, oam, 128);
        const SnesMainScreenLayers layers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;
        int sequence = 0;
        foreach (bool empty in new[] { false, true })
        for (int operation = 0; operation < 4; operation++)
        for (int mask = 0; mask < 16; mask++)
        {
            var admission = (SnesMainScreenLayers)((mask & 7) | ((mask & 8) << 1));
            var windows = new SnesWindowRegisters(0x3a, 0x0a, 0x0b, empty ? (byte)120 : (byte)32, 96,
                64, 128, (byte)(operation * 0x55), (byte)operation);
            var registers = new OrdinaryGameplayRegisters(3, 5, 7, 11, 64, 32,
                SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, SnesPpuLayout.GameplayHudCharacterBaseWord,
                layers, Windows: windows, MainScreenWindowMask: admission);
            var frame = new RenderFrameSnapshot(new(++sequence, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new OrdinaryGameplayRenderLayer(registers) }, 3, 15));
            frame = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(frame));
            PixelComparison.Verify(frame, SoftwareFrameSnapshotRenderer.Render(frame), renderer.RenderForReadback(frame),
                $"{device.Kind}: native windows operation={operation}, TMW={admission}, empty={empty}");
        }
        Console.WriteLine($"{device.Kind}: {sequence} hardware-window frames match software after packet round trip.");
    }
}
