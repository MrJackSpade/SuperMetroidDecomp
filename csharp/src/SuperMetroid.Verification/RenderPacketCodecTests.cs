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
        byte[] badBool = RenderFrameSnapshotCodec.Serialize(mode7);
        badBool[^1] = 2;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badBool), "reject noncanonical boolean");
        var layered = new RenderFrameSnapshot(new(2, 1, 0), new LayeredRenderSnapshot(memory,
            new RenderLayer[] { new ObjRenderLayer() }, 3, 15));
        byte[] badLayer = RenderFrameSnapshotCodec.Serialize(layered);
        badLayer[^1] = byte.MaxValue;
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(badLayer), "reject unknown layer kind");
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
}
