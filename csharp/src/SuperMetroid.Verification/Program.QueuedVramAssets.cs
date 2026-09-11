using System.Reflection;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyQueuedVramAssets()
    {
        foreach (ushort destination in new ushort[] { 0, 0x7fff, 0x8000, 0xffff })
        foreach (int length in new[] { 1, 2, 3, 8192 })
        {
            byte[] data = Enumerable.Range(0, length).Select(i => (byte)(i * 31 + 7)).ToArray();
            var bus = new TestAddressSpace();
            for (int i = 0; i < data.Length; i++) bus.WriteByte(0x7e0000 + i, data[i]);
            var expected = new SnesVram();
            expected.ExecuteQueuedWrite(bus, 0x7e0000, (ushort)length, destination);
            var actual = new SnesVram();
            var queue = new VramWriteQueue();
            queue.EnqueueAsset(VramAssetId.StandardHudTiles, (ushort)length, destination);
            AssertEqual(VramWriteQueue.EntryByteCount, queue.TailInBytes, "asset occupies the same native queue record budget");
            queue.DrainTo(actual, new ForbiddenMapBus(), new TestVramAssets(data));
            for (int i = 0; i < 65536; i++) AssertEqual(expected.ReadByte(i), actual.ReadByte(i), "asset DMA matches native byte ports, stride and wrap");
            AssertEqual(0, queue.TailInBytes, "asset queue clears at normal drain boundary");
        }

        var mixed = new VramWriteQueue();
        mixed.Enqueue(2, 0x7e0000, 0);
        mixed.EnqueueAsset(VramAssetId.StandardHudTiles, 3, 0);
        mixed.Enqueue(1, 0x7e0002, 1);
        var mixedBus = new TestAddressSpace();
        mixedBus.WriteByte(0x7e0000, 1); mixedBus.WriteByte(0x7e0001, 2); mixedBus.WriteByte(0x7e0002, 3);
        var mixedVram = new SnesVram();
        mixed.DrainTo(mixedVram, mixedBus, new TestVramAssets([7, 8, 9]));
        AssertEqual((byte)7, mixedVram.ReadByte(0), "asset follows earlier bus write");
        AssertEqual((byte)8, mixedVram.ReadByte(1), "asset high port retained");
        AssertEqual((byte)3, mixedVram.ReadByte(2), "later bus write follows asset in queue order");

        var pending = new VramWriteQueue();
        pending.EnqueueAsset(VramAssetId.StandardHudTiles, 3, 0);
        using var state = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(state, pending);
        state.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(state);
        AssertThrows<InvalidOperationException>(() => restored.DrainTo(new SnesVram(), new ForbiddenMapBus()), "missing asset provider fails loudly");
        AssertThrows<InvalidDataException>(() => restored.DrainTo(new SnesVram(), new ForbiddenMapBus(), new TestVramAssets([1])), "asset size mismatch fails loudly");
        var restoredVram = new SnesVram();
        restored.DrainTo(restoredVram, new ForbiddenMapBus(), new TestVramAssets([21, 22, 23]));
        AssertEqual((byte)21, restoredVram.ReadByte(0), "restored pending reference resolves current host artwork at drain");
        AssertEqual((byte)23, restoredVram.ReadByte(2), "odd final byte survives debugger round trip");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset(VramAssetId.None, 1, 0), "None is not an asset source");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset((VramAssetId)255, 1, 0), "unknown asset identifier rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => pending.EnqueueAsset(VramAssetId.StandardHudTiles, 0, 0), "empty asset cannot become queue terminator");
        var full = new VramWriteQueue();
        while (full.TailInBytes + VramWriteQueue.EntryByteCount + 2 <= VramWriteQueue.StorageByteCount)
            full.EnqueueAsset(VramAssetId.StandardHudTiles, 1, 0);
        int tail = full.TailInBytes;
        AssertThrows<InvalidOperationException>(() => full.EnqueueAsset(VramAssetId.StandardHudTiles, 1, 0), "asset enqueue respects original queue capacity");
        AssertEqual(tail, full.TailInBytes, "rejected asset leaves queue tail intact");
        FieldInfo[] fields = typeof(VramWriteEntry).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(field => field.MetadataToken).ToArray();
        var legacy = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(VramWriteEntry), fields, 3);
        AssertEqual(3, legacy.Length, "legacy queue record keeps its original three fields");
        AssertTrue(legacy.All(field => field.Name != "<AssetId>k__BackingField"), "legacy bus record receives default None, not an invented asset");
        Console.WriteLine("VRAM assets: native port/stride/wrap parity, mixed ordering, capacity, late-bound state restore and strict errors pass.");
    }

    private sealed class TestVramAssets(byte[] bytes) : IVramAssetProvider
    {
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset == VramAssetId.StandardHudTiles
            ? bytes : throw new InvalidDataException($"Unknown test asset {asset}.");
    }
}
