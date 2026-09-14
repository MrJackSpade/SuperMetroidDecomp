using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
    {
        var files = BeamTileExtractor.Extract(bus);
        var catalog = BeamTileCatalog.Load(files);
        AssertEqual(12, files.Count, "Every legal beam combination has editable artwork");
        for (ushort selection = 0; selection < 12; selection++)
        {
            byte[] png = files[BeamTileAtlasDefinitions.FileName(selection)];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png));
            var native = new SnesVram(); var extracted = new SnesVram();
            SamusProjectileSystem.LoadBeamTilesAndPalette(bus, native, new SnesCgram(), selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.Bytes.SequenceEqual(extracted.Bytes), "PNG matches production beam upload across complete VRAM");
            var image = IndexedPng.Read(new MemoryStream(png), 64, 8);
            image.Pixels[0] ^= 1;
            using var editedPng = new MemoryStream();
            IndexedPng.Write(editedPng, image.Width, image.Height, image.Pixels, image.Palette);
            editedPng.Position = 0;
            var edited = BeamTileAtlas.Load(editedPng);
            edited.LoadTo(extracted);
            int firstByte = BeamTileAtlasDefinitions.DestinationWord * 2;
            AssertEqual((byte)(native.Bytes[firstByte] ^ 0x80), extracted.Bytes[firstByte], "Edited pixel changes the correct tile plane bit");
            for (int index = 0; index < native.Bytes.Length; index++)
                if (index != firstByte) AssertEqual(native.Bytes[index], extracted.Bytes[index], "Beam edit cannot change neighboring VRAM or other pixels");
            AssertEqual(BeamTileAtlasDefinitions.ByteCount, edited.Transfer.Length, "Edited upload retains native DMA size");
            var queue = new VramWriteQueue();
            var actualPalette = new SnesCgram(); var expectedPalette = new SnesCgram();
            var nativeQueue = new VramWriteQueue();
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(bus, nativeQueue, expectedPalette, selection);
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(new BeamArtworkReadGuard(bus), queue, actualPalette, selection, catalog);
            AssertEqual(nativeQueue.TailInBytes, queue.TailInBytes, "Beam PNG keeps native seven-byte queue size");
            AssertEqual(nativeQueue.Entries[0].SizeInBytes, queue.Entries[0].SizeInBytes, "Queued beam transfer retains byte count");
            AssertEqual(nativeQueue.Entries[0].EncodedVramDestination, queue.Entries[0].EncodedVramDestination, "Queued beam retains destination");
            AssertTrue(actualPalette.Colors.SequenceEqual(expectedPalette.Colors), "PNG selection leaves beam palette behavior unchanged");
            using var state = new MemoryStream();
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(state, queue);
            state.Position = 0;
            var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(state);
            var stockVram = new SnesVram();
            queue.DrainTo(stockVram, new ProjectileCompositionForbiddenBus(), catalog);
            AssertTrue(stockVram.Bytes.SequenceEqual(native.Bytes), "Queued PNG publishes native pixels only at drain");
            var replacements = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(selection)] = editedPng.ToArray() };
            var editedCatalog = BeamTileCatalog.Load(replacements);
            var reboundVram = new SnesVram();
            restored.DrainTo(reboundVram, new ProjectileCompositionForbiddenBus(), editedCatalog);
            AssertTrue(reboundVram.Bytes.SequenceEqual(extracted.Bytes), "Restored pending transfer resolves current PNG, not serialized stock bytes");
            AssertEqual(0, restored.TailInBytes, "NMI clears restored asset queue");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, 8, 8, new byte[64], new[] { new Rgba32(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new Rgba32((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, 64, 8, pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8])), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve production VRAM uploads, exact edited-pixel isolation and malformed resource rejection pass.");
    }

    private sealed class BeamArtworkReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if ((address >> 16) == 0x9a || address is >= 0x90c3b1 and < 0x90c3c9)
                throw new InvalidDataException("Authored beam queue still reads ROM graphics or selection pointers.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Artwork queue wrote the bus.");
    }
}
