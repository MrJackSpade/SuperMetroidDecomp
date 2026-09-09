using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using Hardware = SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRenderPacketCodec()
    {
        var frame = new RenderFrameSnapshot(new(9876543210, 42, ushort.MaxValue),
            new Rgba32(123, 231, 76, 255), new byte[] { 9, 7 });
        byte[] bytes = RenderFrameSnapshotCodec.Serialize(frame);
        RenderFrameSnapshot restored = RoundTripRenderPacket(frame);
        AssertEqual(frame.Identity, restored.Identity, "portable fixture host/cartridge identity");
        byte[] firstVersion = (byte[])bytes.Clone();
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            firstVersion.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FirstSupportedVersion);
        AssertEqual(frame.SolidColor, RenderFrameSnapshotCodec.Deserialize(firstVersion).SolidColor,
            "version-one fixture remains readable after format extension");
        AssertTrue(SoftwareFrameSnapshotRenderer.Render(frame).AsSpan().SequenceEqual(
            SoftwareFrameSnapshotRenderer.Render(restored)), "fixture ordered-fade pixel parity");
        for (int length = 0; length < bytes.Length; length++)
        {
            byte[] truncated = bytes[..length];
            AssertThrows<IOException>(() => RenderFrameSnapshotCodec.Deserialize(truncated), "reject truncated solid fixture");
        }
        byte[] corrupt = (byte[])bytes.Clone();
        corrupt[0] ^= byte.MaxValue;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(corrupt), "reject signature mismatch");
        byte[] version = (byte[])bytes.Clone();
        version[RenderPacketFormat.Signature.Length]++;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(version), "reject unsupported format version");
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize([.. bytes, 0]), "reject trailing data");
        byte[] huge = new byte[RenderPacketFormat.MaximumPacketBytes + 1];
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(huge), "reject oversized fixture");
        byte[] badKind = (byte[])bytes.Clone();
        badKind[^5] = byte.MaxValue; // Kind directly precedes solid RGBA.
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badKind), "reject unknown composition kind");
        byte[] badCount = (byte[])bytes.Clone();
        int fadeCountOffset = RenderPacketFormat.Signature.Length + sizeof(ushort) + 2 * sizeof(long) + sizeof(ushort);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(badCount.AsSpan(fadeCountOffset), int.MaxValue);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badCount), "reject count before operation allocation");

        var memory = new PpuMemorySnapshot(new byte[Hardware.SnesPpuLayout.VramByteCount],
            new ushort[Hardware.SnesPpuLayout.CgramColorCount], new byte[Hardware.SnesPpuLayout.OamUploadByteCount], 0);
        var registers = new Mode7RenderRegisters(short.MinValue, short.MaxValue, -1, 0,
            -123, 456, -789, 1023, true);
        var mode7 = new RenderFrameSnapshot(new(1, 1, 0), new Mode7ObjRenderSnapshot(memory, registers, 3, 15));
        AssertEqual(registers, RoundTripRenderPacket(mode7).Mode7!.Background!.Value, "signed matrix and outside policy preserved");
        var wrapping = registers with { FillOutsideWithCharacterZero = false, WrapOutsideMap = true };
        var wrapPacket = new RenderFrameSnapshot(new(1, 1, 0), new Mode7ObjRenderSnapshot(memory, wrapping, 3, 15));
        AssertEqual(wrapping, RoundTripRenderPacket(wrapPacket).Mode7!.Background!.Value, "Mode 7 wrap policy survives packet round trip");
        byte[] oldWrap = RenderFrameSnapshotCodec.Serialize(wrapPacket);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldWrap.AsSpan(RenderPacketFormat.Signature.Length), 21);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(oldWrap), "version 21 cannot claim Mode 7 wrapping");
        byte[] legacyFill = RenderFrameSnapshotCodec.Serialize(mode7);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(legacyFill.AsSpan(RenderPacketFormat.Signature.Length), 21);
        AssertEqual(registers, RenderFrameSnapshotCodec.Deserialize(legacyFill).Mode7!.Background!.Value, "legacy boolean overflow retains character-zero semantics");
        var insertedMode7 = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Mode7RenderLayer(registers) }, 3, 15));
        byte[] invalidOldVersion = RenderFrameSnapshotCodec.Serialize(insertedMode7);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            invalidOldVersion.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FirstSupportedVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(invalidOldVersion),
            "version-one cannot claim a version-two layer operation");
        byte[] badBool = RenderFrameSnapshotCodec.Serialize(mode7);
        badBool[^1] = 2;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badBool), "reject noncanonical boolean");
        var layered = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer() }, 3, 15));
        byte[] badLayer = RenderFrameSnapshotCodec.Serialize(layered);
        badLayer[^1] = byte.MaxValue;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badLayer), "reject unknown layer kind");
        var fixedColor = new RenderFrameSnapshot(new(3, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new FixedColorAddRenderLayer(31, 15, 1) }, 3, 15));
        byte[] versionTwo = RenderFrameSnapshotCodec.Serialize(fixedColor);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            versionTwo.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.Mode7LayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(versionTwo),
            "version-two cannot claim a version-three color operation");
        var viewport = new RenderFrameSnapshot(new(4, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new Bg2BppViewportRenderLayer(0, 0, 255, true, null) }, 3, 15));
        byte[] versionThree = RenderFrameSnapshotCodec.Serialize(viewport);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
            versionThree.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.FixedColorLayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(versionThree),
            "version-three cannot claim a version-four viewport operation");
        VerifyBg2ViewportPackets();
        Console.WriteLine("  Display fixture codec: stable round trip, identity, fades, truncation, signature/version and trailing-data rejection agree.");
    }

    private static RenderFrameSnapshot RoundTripRenderPacket(RenderFrameSnapshot frame)
    {
        byte[] bytes = RenderFrameSnapshotCodec.Serialize(frame);
        RenderFrameSnapshot restored = RenderFrameSnapshotCodec.Deserialize(bytes);
        AssertEqual(frame.Identity, restored.Identity, "render fixture preserves frame identity");
        AssertTrue(bytes.AsSpan().SequenceEqual(RenderFrameSnapshotCodec.Serialize(restored)),
            "canonical fixture round trip preserves every encoded field");
        return restored;
    }

    private static void VerifyBg2ViewportPackets()
    {
        var random = new Random(321);
        byte[] vramBytes = new byte[Hardware.SnesPpuLayout.VramByteCount];
        random.NextBytes(vramBytes);
        ushort[] colors = Enumerable.Range(0, Hardware.SnesPpuLayout.CgramColorCount)
            .Select(_ => (ushort)random.Next(32768)).ToArray();
        var memory = new PpuMemorySnapshot(vramBytes, colors,
            new byte[Hardware.SnesPpuLayout.OamUploadByteCount], 0);
        var scratch = new SoftwarePpuSnapshotMemory(memory);
        foreach (ushort scroll in new ushort[] { 0, 8, 255, ushort.MaxValue })
        foreach (bool transparent in new[] { false, true })
        foreach (bool? priority in new bool?[] { null, false, true })
        {
            var layer = new Bg2BppViewportRenderLayer(0, 0x4000, scroll, transparent, priority);
            var frame = new RenderFrameSnapshot(new(1, 1, 0),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            Rgba32[] full = SnesBgTilemapRenderer.Render2Bpp(scratch.Vram, scratch.Cgram,
                layer.TilemapWord, layer.CharacterWord, rowCount: 32,
                transparentColorZero: transparent, priority: priority);
            Rgba32[] expected = SnesLayerCompositor.CreateBackdrop(scratch.Cgram, 256 * 224);
            for (int y = 0; y < 224; y++)
                for (int x = 0; x < 256; x++)
                {
                    Rgba32 pixel = full[((y + scroll) % 256) * 256 + x];
                    if (pixel.A != 0) expected[y * 256 + x] = pixel;
                }
            AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(frame))),
                $"2bpp viewport scroll={scroll}, transparent={transparent}, priority={priority}");
        }
    }
}
