using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyObjSubscreenAddition()
    {
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
        byte[] oldPacket = RenderFrameSnapshotCodec.Serialize(ordinary);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(oldPacket.AsSpan(RenderPacketFormat.Signature.Length), 13);
        AssertEqual(new Rgba32(57, 0, 255), SoftwareFrameSnapshotRenderer.Render(RenderFrameSnapshotCodec.Deserialize(oldPacket))[8 * 256 + 8],
            "legacy packets retain replacing OBJ composition");
    }
}
