using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks queued installed-artwork DMA against native VRAM write behavior and debugger state restoration.</summary>
internal static class QueuedVramAssetsVerification
{
    /// <summary>Places <paramref name="data"/> at the $7E:0000 A-bus source the queued DMA records name.</summary>
    private static TestAddressSpace CreateWorkRamDmaSource(ReadOnlySpan<byte> data)
    {
        var bus = new TestAddressSpace();
        bus.WriteBytes(0x7e0000, data);
        return bus;
    }

    /// <summary>Compares byte-port wrap and stride, mixed queue order, capacity, and late asset resolution after serialization.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyQueuedVramAssets()
    {
        foreach (ushort destination in new ushort[] { 0, 0x7fff, 0x8000, 0xffff })
        foreach (int length in new[] { 1, 2, 3, 8192 })
        {
            byte[] data = Enumerable.Range(0, length).Select(i => (byte)(i * 31 + 7)).ToArray();
            TestAddressSpace bus = CreateWorkRamDmaSource(data);
            var expected = new SnesVram();
            expected.ExecuteQueuedMemoryWrite(bus, 0x7e0000, (ushort)length, destination);
            var actual = new SnesVram();
            var queue = new VramWriteQueue();
            queue.EnqueueAsset(VramAssetId.StandardHudTiles, (ushort)length, VramDmaDestination.FromNative(destination));
            AssertEqual(VramWriteQueue.EntryByteCount, queue.TailInBytes, "asset occupies the same native queue record budget");
            queue.DrainTo(actual, new TestVramAssets(data), null, BgArtworkFixtures.For(actual));
            for (int i = 0; i < 65536; i++) AssertEqual(expected.ReadByte(i), actual.ReadByte(i), "asset DMA matches native byte ports, stride and wrap");
            AssertEqual(0, queue.TailInBytes, "asset queue clears at normal drain boundary");
        }

        var mixedStaging = new VramStagingBuffer(3);
        ((byte[])[1, 2, 3]).CopyTo(mixedStaging.WritableBytes());
        var mixed = new VramWriteQueue();
        mixed.EnqueueStaging(2, 0x7e0000, mixedStaging, 0, VramDmaDestination.FromNative(0));
        mixed.EnqueueAsset(VramAssetId.StandardHudTiles, 3, VramDmaDestination.FromNative(0));
        mixed.EnqueueStaging(1, 0x7e0002, mixedStaging, 2, VramDmaDestination.FromNative(1));
        var mixedVram = new SnesVram();
        mixed.DrainTo(mixedVram, new TestVramAssets([7, 8, 9]), null, BgArtworkFixtures.For(mixedVram));
        AssertEqual((byte)7, mixedVram.ReadByte(0), "asset follows earlier staged write");
        AssertEqual((byte)8, mixedVram.ReadByte(1), "asset high port retained");
        AssertEqual((byte)3, mixedVram.ReadByte(2), "later staged write follows asset in queue order");

        var pending = new VramWriteQueue();
        pending.EnqueueAsset(VramAssetId.StandardHudTiles, 3, VramDmaDestination.FromNative(0));
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, pending);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(state);
        AssertThrows<InvalidOperationException>(() => restored.DrainTo(new SnesVram(), null, null, BgArtworkFixtures.For(new SnesVram())), "missing asset provider fails loudly");
        AssertThrows<InvalidDataException>(() => restored.DrainTo(new SnesVram(), new TestVramAssets([1]), null, BgArtworkFixtures.For(new SnesVram())), "asset size mismatch fails loudly");
        var restoredVram = new SnesVram();
        restored.DrainTo(restoredVram, new TestVramAssets([21, 22, 23]), null, BgArtworkFixtures.For(restoredVram));
        AssertEqual((byte)21, restoredVram.ReadByte(0), "restored pending reference resolves current host artwork at drain");
        AssertEqual((byte)23, restoredVram.ReadByte(2), "odd final byte survives debugger round trip");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset(VramAssetId.None, 1, VramDmaDestination.FromNative(0)), "None is not an asset source");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset((VramAssetId)255, 1, VramDmaDestination.FromNative(0)), "unknown asset identifier rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset(VramAssetId.StandardHudTiles, 0, VramDmaDestination.FromNative(0)), "empty asset cannot become queue terminator");
        var full = new VramWriteQueue();
        while (full.TailInBytes + VramWriteQueue.EntryByteCount + 2 <= VramWriteQueue.StorageByteCount)
            full.EnqueueAsset(VramAssetId.StandardHudTiles, 1, VramDmaDestination.FromNative(0));
        int tail = full.TailInBytes;
        AssertThrows<InvalidOperationException>(() => full.EnqueueAsset(VramAssetId.StandardHudTiles, 1, VramDmaDestination.FromNative(0)), "asset enqueue respects original queue capacity");
        AssertEqual(tail, full.TailInBytes, "rejected asset leaves queue tail intact");
        Console.WriteLine("VRAM assets: native port/stride/wrap parity, mixed ordering, capacity, late-bound state restore and strict errors pass.");
    }

    /// <summary>Supplies a single HUD tile byte sequence to isolate queued asset writes from filesystem loading.</summary>
    internal sealed class TestVramAssets(byte[] bytes) : IVramAssetProvider
    {
        /// <summary>Provides the fixture's HUD bytes and rejects any other asset identity.</summary>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset == VramAssetId.StandardHudTiles
            ? bytes : throw new InvalidDataException($"Unknown test asset {asset}.");
    }
}
