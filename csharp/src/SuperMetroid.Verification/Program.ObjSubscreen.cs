using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyObjSubscreenAddition()
    {
        VerifyMode7ObjSubtraction();
        VerifyBg4SubscreenAddition();
        VerifyObjFixedColor();
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int row = 0; row < 8; row++) vram[row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = new PpuMemorySnapshot(vram, colors, oam, 1);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer(true) }, 0, 15));
        var restored = RoundTripRenderPacket(packet);
        Directory.CreateDirectory("csharp/test-temp/ending-504");
        File.WriteAllBytes("csharp/test-temp/ending-504/obj-subscreen.smframe", RenderFrameSnapshotCodec.Serialize(restored));
        var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
        AssertEqual(new Rgba32(255, 0, 255), pixels[8 * 256 + 8], "OBJ subscreen adds blue to red backdrop");
        AssertEqual(new Rgba32(255, 0, 0), pixels[0], "transparent OBJ preserves backdrop");
        byte[] unsupported = RenderFrameSnapshotCodec.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(RenderPacketFormat.Signature.Length), 13);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(unsupported),
            "version 13 cannot claim additive OBJ composition");
        var ordinary = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer() }, 0, 15));
        // Version 13's final OBJ descriptor has no version-19 fixed-color presence byte.
        byte[] oldPacket = RenderFrameSnapshotCodec.Serialize(ordinary)[..^1];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldPacket.AsSpan(RenderPacketFormat.Signature.Length), 13);
        AssertEqual(new Rgba32(57, 0, 255), SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(oldPacket))[8 * 256 + 8],
            "legacy packets retain replacing OBJ composition");
    }

    private static void VerifyMode7ObjSubtraction()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
        for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[1] = 31 | (12 << 5);
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = new PpuMemorySnapshot(vram, colors, oam, 1);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Mode7RenderLayer(new(256, 0, 0, 256, 0, 0, 0, 0), true) }, 3, 15));
        var restored = RoundTripRenderPacket(packet);
        var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
        AssertEqual(new Rgba32(198, 99, 0), pixels[8 * 256 + 8], "Mode7 minus OBJ clamps each five-bit component without halving");
        AssertEqual(new Rgba32(255, 99, 0), pixels[0], "transparent OBJ leaves Mode7 color unchanged");
        Directory.CreateDirectory("csharp/test-temp/ending-504");
        File.WriteAllBytes("csharp/test-temp/ending-504/mode7-obj-subtract.smframe", RenderFrameSnapshotCodec.Serialize(restored));
        byte[] unsupported = RenderFrameSnapshotCodec.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(RenderPacketFormat.Signature.Length), 14);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(unsupported), "older packet cannot claim Mode7 OBJ subtraction");
    }

    private static void VerifyObjFixedColor()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int y = 0; y < 8; y++) vram[y * 2] = 255;
        ushort[] palette = new ushort[256];
        palette[0] = 31 << 10;
        palette[129] = palette[193] = 7;
        foreach (byte objPalette in new byte[] { 0, 4 })
        {
            byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(objPalette * 2);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(
                new PpuMemorySnapshot(vram, palette, oam, 1),
                new RenderLayer[] { new ObjRenderLayer(FixedColor: new(8, 4, 2)) }, 0, 15));
            var restored = RoundTripRenderPacket(packet);
            var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
            AssertEqual(objPalette == 4 ? new Rgba32(123, 33, 16) : new Rgba32(57, 0, 0), pixels[8 * 256 + 8],
                "OBJ fixed RGB addition respects palette eligibility and five-bit expansion");
            AssertEqual(new Rgba32(0, 0, 255), pixels[0], "OBJ-only fixed color does not alter uncovered backdrop");
            Directory.CreateDirectory("csharp/test-temp/ending-504");
            File.WriteAllBytes($"csharp/test-temp/ending-504/obj-fixed-{objPalette}.smframe", RenderFrameSnapshotCodec.Serialize(restored));
        }
    }

    private static void VerifyBg4SubscreenAddition()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        // Color four depends on the third bitplane: a mistaken 2-bpp decode sees zero.
        for (int row = 0; row < 8; row++) vram[row * 2 + 16] = 255;
        for (int tile = 0; tile < 1024; tile++) vram[0xe000 + tile * 2 + 1] = 4;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[20] = 31 << 10;
        var memory = new PpuMemorySnapshot(vram, colors, new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new BgSubscreenAddRenderLayer(0x7000, 0, FourBpp: true) }, 0, 15));
        var restored = RoundTripRenderPacket(packet);
        AssertTrue(SoftwareFrameSnapshotRenderer.Render(restored).All(pixel => pixel == new Rgba32(255, 0, 255)),
            "four-bit subscreen resolves upper bitplanes and sixteen-color palette stride before addition");
        Directory.CreateDirectory("csharp/test-temp/ending-504");
        File.WriteAllBytes("csharp/test-temp/ending-504/bg4-subscreen.smframe", RenderFrameSnapshotCodec.Serialize(restored));
        byte[] unsupported = RenderFrameSnapshotCodec.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(RenderPacketFormat.Signature.Length), 15);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(unsupported), "older packets cannot claim four-bit subscreen layers");
        for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
        colors[129] = 31 << 5;
        colors[193] = 31 << 5;
        for (int tile = 0; tile < 1024; tile++) vram[0xd000 + tile * 2] = 1;
        foreach (byte palette in new byte[] { 0, 4 })
        {
            byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(palette * 2);
            var masked = new RenderFrameSnapshot(new(3, 1, 0), new LayeredRenderSnapshot(
                new PpuMemorySnapshot(vram, colors, oam, 1),
                new RenderLayer[] { new ObjRenderLayer(), new BgSubscreenAddRenderLayer(0x7000, 0,
                    new Bg4BppRenderLayer(0x6800, 0, 0, 0, 32, 32, null), FourBpp: true, MainObjects: true) }, 3, 15));
            var decoded = RoundTripRenderPacket(masked);
            var pixels = SoftwareFrameSnapshotRenderer.Render(decoded);
            AssertEqual(new Rgba32(255, 0, 0), pixels[0], "masked subscreen does not add to backdrop");
            AssertEqual(palette == 4 ? new Rgba32(0, 255, 255) : new Rgba32(0, 255, 0), pixels[8 * 256 + 8],
                "only winning OBJ palettes four through seven participate in main-screen color math");
            File.WriteAllBytes($"csharp/test-temp/ending-504/bg4-main-obj-{palette}.smframe", RenderFrameSnapshotCodec.Serialize(decoded));
        }
        foreach (byte priority in new byte[] { 1, 2, 3 })
        foreach (bool highBackground in new[] { false, true })
        {
            byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(priority << 4);
            vram[0xe000 + (32 + 1) * 2 + 1] = (byte)(4 | (highBackground ? 32 : 0));
            var composite = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(
                new PpuMemorySnapshot(vram, colors, oam, 1),
                new RenderLayer[] { new BgSubscreenAddRenderLayer(0x7000, 0, FourBpp: true, IncludeObjects: true) }, 3, 15));
            var decoded = RoundTripRenderPacket(composite);
            bool objectWins = priority >= (highBackground ? 3 : 2);
            AssertEqual(objectWins ? new Rgba32(255, 255, 0) : new Rgba32(255, 0, 255),
                SoftwareFrameSnapshotRenderer.Render(decoded)[8 * 256 + 8],
                "subscreen selects BG2 or OBJ by priority before addition, never sums both");
            File.WriteAllBytes($"csharp/test-temp/ending-504/bg4-obj-{priority}-{highBackground}.smframe", RenderFrameSnapshotCodec.Serialize(decoded));
        }
    }
}
