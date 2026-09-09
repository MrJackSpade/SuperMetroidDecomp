using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyWindowPixels()
    {
        var random = new Random(396);
        byte[] bytes = new byte[SnesPpuLayout.VramByteCount];
        random.NextBytes(bytes);
        ushort[] palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
        byte[] objects = new byte[SnesPpuLayout.OamUploadByteCount];
        random.NextBytes(objects);
        var memory = new SoftwarePpuSnapshotMemory(new PpuMemorySnapshot(bytes, palette, objects, 128));
        const SnesMainScreenLayers layers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj;
        var reference = new Dictionary<SnesMainScreenLayers, Rgba32[]>();
        for (int mask = 0; mask < 8; mask++)
        {
            var enabled = (SnesMainScreenLayers)((mask & 3) | ((mask & 4) << 2));
            reference[enabled] = Draw(enabled);
        }
        Rgba32 backdrop = memory.Cgram.GetRgba(0);
        int differences = 0;
        for (int operation = 0; operation < 4; operation++)
        for (int mask = 0; mask < 16; mask++)
        {
            var windows = new SnesWindowRegisters(0x3a, 0x0a, 0x0b, 32, 96, 64, 128,
                (byte)(operation * 0x55), (byte)operation);
            var admission = (SnesMainScreenLayers)((mask & 7) | ((mask & 8) << 1));
            Rgba32[] actual = Draw(layers, windows, admission);
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                // Each reference image was composed with entire layers disabled. Select
                // the correct independent reference column, rather than painting black
                // over the final image (which would lose the newly exposed lower layer).
                var hidden = windows.MaskedLayers((byte)x, admission);
                Rgba32 expected = y < 32 && (hidden & SnesMainScreenLayers.Bg3) != 0 ? backdrop :
                    reference[layers & ~hidden][y * 256 + x];
                if (actual[y * 256 + x] != expected)
                    throw new InvalidOperationException($"Window pixel mismatch operation={operation}, TMW={admission}, xy={x}/{y}: expected {expected}, got {actual[y * 256 + x]}.");
                if (expected != reference[layers][y * 256 + x]) differences++;
            }
        }
        AssertTrue(differences > 0, "window fixture visibly changes pixels rather than testing transparent layers");
        Console.WriteLine($"Hardware window pixels: 64 full frames match independent layer-removal compositions ({differences} changed pixels).");

        Rgba32[] Draw(SnesMainScreenLayers enabled, SnesWindowRegisters windows = default,
            SnesMainScreenLayers admission = SnesMainScreenLayers.None) =>
            SnesGameplayFrameRenderer.RenderHudOrdinaryBackgroundsAndObjs(memory.Vram, memory.Cgram, memory.Oam,
                3, 5, 7, 11, mainScreenLayers: enabled, windowRegisters: windows, mainScreenWindowMask: admission);
    }
}
