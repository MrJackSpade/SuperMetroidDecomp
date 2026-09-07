using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class XrayWindowTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        PpuMemorySnapshot Memory(ushort backdrop)
        {
            var palette = new ushort[256]; palette[0] = backdrop;
            return new(new byte[SnesPpuLayout.VramByteCount], palette,
                new byte[SnesPpuLayout.OamUploadByteCount], 0);
        }
        var child = new LayeredRenderSnapshot(Memory(0x6A93), Array.Empty<RenderLayer>(), 0, 15);
        int count = 0;
        // Cover every five-bit input, particularly bright components whose addition
        // overflows five bits. CPU/GPU parity alone cannot detect a shared wrong equation.
        foreach (int component in Enumerable.Range(0, 32))
        foreach (var endpoints in new[] { (0, 255), (255, 0), (0, 0), (255, 255), (31, 192) })
        {
            var parent = Memory((ushort)(component | component << 5 | component << 10));
            var lines = Enumerable.Repeat(new XrayWindowLine((byte)endpoints.Item1, (byte)endpoints.Item2), 224).ToArray();
            // A slanted interval exercises per-line copies, including both inclusive endpoints.
            lines[80] = new(17, 36);
            var layer = new XrayWindowRenderLayer(child, lines);
            lines[80] = new(255, 0);
            if (layer.Lines[80] != new XrayWindowLine(17, 36))
                throw new InvalidOperationException("Published X-ray window retained mutable caller memory.");
            var packet = new RenderFrameSnapshot(new(++count, 1, 0),
                new LayeredRenderSnapshot(parent, new RenderLayer[] { layer }, 0, 15));
            var baseline = SoftwareLayeredSnapshotRenderer.Render(new(parent, Array.Empty<RenderLayer>(), 0, 15));
            var revealed = SoftwareLayeredSnapshotRenderer.Render(child);
            var expected = baseline.ToArray();
            for (int y = 32; y < 224; y++)
            for (int x = 0; x < 256; x++)
            {
                int index = y * 256 + x;
                var line = layer.Lines[y];
                Rgba32 p = baseline[index];
                byte Dim(byte value)
                {
                    // The PPU retains the carry from addition for half-color math;
                    // saturation happens after division, not before it.
                    int component = Math.Min(31, ((value >> 3) + 7) / 2);
                    return (byte)(component * 8 + component / 4);
                }
                expected[index] = x >= line.Left && x <= line.Right ? revealed[index]
                    : new Rgba32(Dim(p.R), Dim(p.G), Dim(p.B), p.A);
            }
            PixelComparison.Verify(packet, expected, SoftwareFrameSnapshotRenderer.Render(packet), "X-ray reference equation/HUD/endpoints");
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: X-ray endpoints {endpoints}");
            var restored = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(packet));
            PixelComparison.Verify(restored, expected, renderer.RenderForReadback(restored), "X-ray packet round trip");
        }
        var random = new Random(348);
        PpuMemorySnapshot Pattern()
        {
            var vram = new byte[SnesPpuLayout.VramByteCount]; random.NextBytes(vram);
            var oam = new byte[SnesPpuLayout.OamUploadByteCount]; random.NextBytes(oam);
            var palette = Enumerable.Range(0, 256).Select(_ => (ushort)random.Next(32768)).ToArray();
            return new(vram, palette, oam, 128);
        }
        // Exercise actual child dispatches and restored parent bindings, not only backdrop copies.
        var patternedParent = Pattern(); var patternedChild = Pattern();
        for (byte brightness = 0; brightness <= 15; brightness++)
        {
            var lines = Enumerable.Range(0, 224).Select(y =>
                new XrayWindowLine((byte)(y % 127), (byte)(255 - y % 89))).ToArray();
            var reveal = new LayeredRenderSnapshot(patternedChild, new RenderLayer[] {
                new Bg4BppRenderLayer(0x4000, 0x6000, 3, 11, 32, 32, true), new ObjRenderLayer() }, 7, brightness);
            var xray = new XrayWindowRenderLayer(reveal, lines);
            var packet = new RenderFrameSnapshot(new(++count, 1, 0),
                new LayeredRenderSnapshot(patternedParent, new RenderLayer[] {
                    new ObjRenderLayer(), xray, new ObjPriorityRenderLayer(3), xray,
                    new FixedColorAddRenderLayer(1, 2, 3) }, 3, brightness));
            PixelComparison.Verify(packet, SoftwareFrameSnapshotRenderer.Render(packet), renderer.RenderForReadback(packet),
                $"{device.Kind}: X-ray nested memory, parent restoration, brightness {brightness}");
        }
        Console.WriteLine($"{device.Kind}: {count} X-ray windows, immutable endpoints, HUD exclusion, packet round trips and parent restoration passed.");
    }
}
