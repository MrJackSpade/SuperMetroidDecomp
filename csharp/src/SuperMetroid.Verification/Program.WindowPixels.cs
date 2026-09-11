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
        var capturedMemory = new PpuMemorySnapshot(bytes, palette, objects, 128);
        var memory = new SoftwarePpuSnapshotMemory(capturedMemory);
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
            var registers = new OrdinaryGameplayRegisters(3, 5, 7, 11, 64, 32,
                SnesPpuLayout.GameplayBg2TilemapWord, 0, 0, SnesPpuLayout.GameplayHudCharacterBaseWord,
                layers, Windows: windows, MainScreenWindowMask: admission);
            var packet = new RenderFrameSnapshot(new(1, 1, 1),
                new LayeredRenderSnapshot(capturedMemory, new RenderLayer[] { new OrdinaryGameplayRenderLayer(registers) }, 3, 15));
            var restored = RoundTripRenderPacket(packet);
            var restoredRegisters = ((OrdinaryGameplayRenderLayer)restored.Layers!.Layers[0]).Registers;
            AssertEqual(windows, restoredRegisters.Windows, "window register bytes survive packet round trip");
            AssertEqual(admission, restoredRegisters.MainScreenWindowMask, "TMW survives packet round trip");
            AssertTrue(actual.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(restored)),
                "windowed packet renders exact direct-composition pixels");
            if (operation == 0 && mask == 0)
            {
                // A single-layer v20 packet is the identical prefix without v21's
                // nine window bytes, TMW, and v26's mosaic byte. It must decode
                // with windowing and mosaic disabled.
                byte[] modern = RenderFrameSnapshotCodec.Serialize(packet);
                byte[] old = modern.AsSpan(0, modern.Length - 11).ToArray();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(
                    old.AsSpan(RenderPacketFormat.Signature.Length), RenderPacketFormat.ObjPriorityFixedColorVersion);
                var legacy = RenderFrameSnapshotCodec.Deserialize(old);
                AssertEqual(default(SnesWindowRegisters), ((OrdinaryGameplayRenderLayer)legacy.Layers!.Layers[0]).Registers.Windows,
                    "v20 gameplay has no invented window registers");
                AssertTrue(reference[layers].AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(legacy)),
                    "v20 gameplay pixels remain unchanged");
                AssertThrows<EndOfStreamException>(() => RenderFrameSnapshotCodec.Deserialize(modern.AsSpan(0, modern.Length - 1)),
                    "truncated current gameplay data remains a loud error");
            }
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
