using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static ColorWordsVerification;
using static GraphDigestVerification;
using static RenderPacketCodecTestsVerification;
using static SuiteTimingVerification;

/// <summary>Checks PPU color math and render-packet compatibility for OBJ and BG subscreen layers.</summary>
internal static class ObjSubscreenVerification
{
    /// <summary>Checks additive OBJ composition and delegates the related Mode 7, BG, and fixed-color cases.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    internal static void VerifyObjSubscreenAddition()
    {
        Suite(nameof(VerifyMode7ObjSubtraction), () => VerifyMode7ObjSubtraction());
        Suite(nameof(VerifyMode7Bg1Addition), () => VerifyMode7Bg1Addition());
        Suite(nameof(VerifyBg4SubscreenAddition), () => VerifyBg4SubscreenAddition());
        Suite(nameof(VerifyObjFixedColor), () => VerifyObjFixedColor());
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        for (int row = 0; row < 8; row++) vram[row * 2] = 255;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[129] = 7 | (31 << 10);
        byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
        oam[0] = 8; oam[1] = 8;
        var memory = new PpuMemorySnapshot(vram, ToColors(colors), oam, 1);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer(true) }, 0, 15));
        var restored = RoundTripRenderPacket(packet);

        var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
        AssertEqual(new Rgba32(255, 0, 255), pixels[8 * 256 + 8], "OBJ subscreen adds blue to red backdrop");
        AssertEqual(new Rgba32(255, 0, 0), pixels[0], "transparent OBJ preserves backdrop");
        byte[] unsupported = RenderFrameSnapshotWriter.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(RenderPacketFormat.Signature.Length), 13);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(unsupported),
            "version 13 cannot claim additive OBJ composition");
        var ordinary = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer() }, 0, 15));
        // Version 13's final OBJ descriptor has no version-19 fixed-color presence byte.
        byte[] oldPacket = RenderFrameSnapshotWriter.Serialize(ordinary)[..^1];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldPacket.AsSpan(RenderPacketFormat.Signature.Length), 13);
        AssertEqual(new Rgba32(57, 0, 255), SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(oldPacket))[8 * 256 + 8],
            "legacy packets retain replacing OBJ composition");
    }

    /// <summary>Checks Mode 7 BG1 self-addition and OBJ palette eligibility after main-screen priority selection.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    private static void VerifyMode7Bg1Addition()
    {
        foreach (bool opaqueBg in new[] { false, true })
        foreach (byte palette in new byte[] { 0, 4 })
        foreach (byte priority in new byte[] { 0, 1, 2, 3 })
        {
            byte[] vram = new byte[SnesPpuLayout.VramByteCount];
            if (opaqueBg) for (int pixel = 0; pixel < 64; pixel++) vram[pixel * 2 + 1] = 1;
            for (int row = 0; row < 8; row++) vram[0xc000 + row * 2] = 255;
            ushort[] colors = new ushort[256];
            colors[0] = 3 << 10; colors[1] = 8 << 5; colors[129 + palette * 16] = 12;
            byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(palette * 2 + priority * 16);
            var registers = new Mode7RenderRegisters(256, 0, 0, 256, new Offset16<Mode7Pixels>(0, 0), new Offset16<Mode7Pixels>(0, 0));
            var memory = new PpuMemorySnapshot(vram, ToColors(colors), oam, 1);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Mode7RenderLayer(registers, AddBg1Subscreen: true) }, 3, 15));
            var pixels = SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(packet));
            Rgba32 expected = opaqueBg && priority == 0 ? new(0, 132, 0)
                : opaqueBg && palette == 4 ? new(99, 66, 0) : new(99, 0, 0);
            AssertEqual(expected, pixels[8 * 256 + 8], "Mode7 main priority and OBJ palette eligibility before addition");
            AssertEqual(opaqueBg ? new Rgba32(0, 132, 0) : new Rgba32(0, 0, 24), pixels[0],
                "BG1 doubles itself but transparent BG1 preserves unmodified backdrop");
            byte[] old = RenderFrameSnapshotWriter.Serialize(packet);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(old.AsSpan(RenderPacketFormat.Signature.Length), 22);
            AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(old), "old packet rejects unrepresentable BG1 addition");
            AssertThrows<ArgumentException>(() => new LayeredRenderSnapshot(memory,
                new RenderLayer[] { new Mode7RenderLayer(registers, true, true) }, 3, 15), "reject contradictory Mode7 math modes");
        }
    }

    /// <summary>Checks per-channel Mode 7 minus OBJ subtraction and rejection of old packets that cannot encode it.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
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
        var memory = new PpuMemorySnapshot(vram, ToColors(colors), oam, 1);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Mode7RenderLayer(new(256, 0, 0, 256, default, default), true) }, 3, 15));
        var restored = RoundTripRenderPacket(packet);
        var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
        AssertEqual(new Rgba32(198, 99, 0), pixels[8 * 256 + 8], "Mode7 minus OBJ clamps each five-bit component without halving");
        AssertEqual(new Rgba32(255, 99, 0), pixels[0], "transparent OBJ leaves Mode7 color unchanged");

        byte[] unsupported = RenderFrameSnapshotWriter.Serialize(packet);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(RenderPacketFormat.Signature.Length), 14);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(unsupported), "older packet cannot claim Mode7 OBJ subtraction");
    }

    /// <summary>Checks OBJ fixed-color addition, palette and priority filters, and legacy priority-packet decoding.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
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
                new PpuMemorySnapshot(vram, ToColors(palette), oam, 1),
                new RenderLayer[] { new ObjRenderLayer(FixedColor: new(8, 4, 2)) }, 0, 15));
            var restored = RoundTripRenderPacket(packet);
            var pixels = SoftwareFrameSnapshotRenderer.Render(restored);
            AssertEqual(objPalette == 4 ? new Rgba32(123, 33, 16) : new Rgba32(57, 0, 0), pixels[8 * 256 + 8],
                "OBJ fixed RGB addition respects palette eligibility and five-bit expansion");
            AssertEqual(new Rgba32(0, 0, 255), pixels[0], "OBJ-only fixed color does not alter uncovered backdrop");

            foreach (byte selectedPriority in new byte[] { 0, 1 })
            {
                var filtered = RoundTripRenderPacket(new(new(1, 1, 0), new LayeredRenderSnapshot(
                    new PpuMemorySnapshot(vram, ToColors(palette), oam, 1),
                    new RenderLayer[] { new ObjPriorityRenderLayer(selectedPriority, new(8, 4, 2)) }, 0, 15)));
                var filteredPixels = SoftwareFrameSnapshotRenderer.Render(filtered);
                AssertEqual(selectedPriority == 0 ? pixels[8 * 256 + 8] : new Rgba32(0, 0, 255),
                    filteredPixels[8 * 256 + 8], "fixed OBJ color obeys both priority filtering and palette eligibility");

            }
        }
        var legacyPriority = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(
            new PpuMemorySnapshot(vram, ToColors(palette), new byte[SnesPpuLayout.OamUploadByteCount], 0),
            new RenderLayer[] { new ObjPriorityRenderLayer(2) }, 0, 15));
        // Version 19 ends this layer after its priority byte; version 20 adds color presence.
        byte[] oldPriority = RenderFrameSnapshotWriter.Serialize(legacyPriority)[..^1];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldPriority.AsSpan(RenderPacketFormat.Signature.Length), 19);
        AssertTrue(SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(oldPriority)).AsSpan()
            .SequenceEqual(RenderForComparison(legacyPriority)), "legacy priority packets remain readable without color payload");
    }

    /// <summary>Checks four-bit BG subscreen decoding, object masking, and BG-versus-OBJ priority before addition.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native PPU geometry, native VRAM words")]
    private static void VerifyBg4SubscreenAddition()
    {
        byte[] vram = new byte[SnesPpuLayout.VramByteCount];
        // Color four depends on the third bitplane: a mistaken 2-bpp decode sees zero.
        for (int row = 0; row < 8; row++) vram[row * 2 + 16] = 255;
        for (int tile = 0; tile < 1024; tile++) vram[0xe000 + tile * 2 + 1] = 4;
        ushort[] colors = new ushort[256];
        colors[0] = 31;
        colors[20] = 31 << 10;
        var memory = new PpuMemorySnapshot(vram, ToColors(colors), new byte[SnesPpuLayout.OamUploadByteCount], 0);
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new BgSubscreenAddRenderLayer(0x7000, 0, FourBpp: true) }, 0, 15));
        var restored = RoundTripRenderPacket(packet);
        AssertTrue(SoftwareFrameSnapshotRenderer.Render(restored).All(pixel => pixel == new Rgba32(255, 0, 255)),
            "four-bit subscreen resolves upper bitplanes and sixteen-color palette stride before addition");

        byte[] unsupported = RenderFrameSnapshotWriter.Serialize(packet);
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
                new PpuMemorySnapshot(vram, ToColors(colors), oam, 1),
                new RenderLayer[] { new ObjRenderLayer(), new BgSubscreenAddRenderLayer(0x7000, 0,
                    new Bg4BppRenderLayer(0x6800, 0, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null), FourBpp: true, MainObjects: true) }, 3, 15));
            var decoded = RoundTripRenderPacket(masked);
            var pixels = SoftwareFrameSnapshotRenderer.Render(decoded);
            AssertEqual(new Rgba32(255, 0, 0), pixels[0], "masked subscreen does not add to backdrop");
            AssertEqual(palette == 4 ? new Rgba32(0, 255, 255) : new Rgba32(0, 255, 0), pixels[8 * 256 + 8],
                "only winning OBJ palettes four through seven participate in main-screen color math");

        }
        foreach (byte priority in new byte[] { 1, 2, 3 })
        foreach (bool highBackground in new[] { false, true })
        {
            byte[] oam = new byte[SnesPpuLayout.OamUploadByteCount];
            oam[0] = 8; oam[1] = 8; oam[3] = (byte)(priority << 4);
            vram[0xe000 + (32 + 1) * 2 + 1] = (byte)(4 | (highBackground ? 32 : 0));
            var composite = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(
                new PpuMemorySnapshot(vram, ToColors(colors), oam, 1),
                new RenderLayer[] { new BgSubscreenAddRenderLayer(0x7000, 0, FourBpp: true, IncludeObjects: true) }, 3, 15));
            var decoded = RoundTripRenderPacket(composite);
            bool objectWins = priority >= (highBackground ? 3 : 2);
            AssertEqual(objectWins ? new Rgba32(255, 255, 0) : new Rgba32(255, 0, 255),
                SoftwareFrameSnapshotRenderer.Render(decoded)[8 * 256 + 8],
                "subscreen selects BG2 or OBJ by priority before addition, never sums both");

        }
    }
}
