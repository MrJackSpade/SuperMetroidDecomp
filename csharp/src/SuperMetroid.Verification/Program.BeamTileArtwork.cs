using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyProjectileTrailDefinitions), () => VerifyProjectileTrailDefinitions(bus));
        Suite(nameof(VerifyProjectileTrailArtwork), () => VerifyProjectileTrailArtwork(bus));
        var files = BeamTileExtractor.Extract(bus);
        var palettes = BeamPaletteCatalog.Load(new MemoryStream(BeamPaletteExtractor.Extract(bus)));
        var catalog = BeamTileCatalog.Load(files, palettes);
        Suite(nameof(VerifyBeamPaletteArtwork), () => VerifyBeamPaletteArtwork(bus, catalog));
        Suite(nameof(VerifyRuntimeBeamArtwork), () => VerifyRuntimeBeamArtwork(bus, files, catalog));
        AssertEqual(14, files.Count, "Every legal beam combination and both bounded invalid uploads have editable artwork");
        for (ushort selection = 0; selection < 12; selection++)
        {
            byte[] png = files[BeamTileAtlasDefinitions.FileName(selection)];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), selection);
            var native = new SnesVram(); var extracted = new SnesVram();
            LoadNativeBeamFixture(bus, native, new SnesCgram(), null, selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.Bytes.SequenceEqual(extracted.Bytes), "PNG matches production beam upload across complete VRAM");
            var image = IndexedPng.Read(new MemoryStream(png), 64, 8);
            image.Pixels[0] ^= 1;
            using var editedPng = new MemoryStream();
            IndexedPng.Write(editedPng, image.Width, image.Height, image.Pixels, image.Palette);
            editedPng.Position = 0;
            var edited = BeamTileAtlas.Load(editedPng, selection);
            edited.LoadTo(extracted);
            int firstByte = BeamTileAtlasDefinitions.DestinationWord * 2;
            AssertEqual((byte)(native.Bytes[firstByte] ^ 0x80), extracted.Bytes[firstByte], "Edited pixel changes the correct tile plane bit");
            for (int index = 0; index < native.Bytes.Length; index++)
                if (index != firstByte) AssertEqual(native.Bytes[index], extracted.Bytes[index], "Beam edit cannot change neighboring VRAM or other pixels");
            AssertEqual(BeamTileAtlasDefinitions.ByteCount, edited.Transfer.Length, "Edited upload retains native DMA size");
            var queue = new VramWriteQueue();
            var actualPalette = new SnesCgram(); var expectedPalette = new SnesCgram();
            var nativeQueue = new VramWriteQueue();
            LoadNativeBeamFixture(bus, null, expectedPalette, nativeQueue, selection);
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(new BeamArtworkReadGuard(bus), queue, actualPalette, selection, catalog);
            var legacyVram = new SnesVram();
            nativeQueue.DrainTo(legacyVram, ReferenceMutableMemory.From(new ProjectileCompositionForbiddenBus()), catalog);
            AssertTrue(legacyVram.Bytes.SequenceEqual(native.Bytes), "Legacy beam source resolves exact native pixels without ROM DMA");
            // Recreate the consumed reference queue for the native record-layout assertions.
            LoadNativeBeamFixture(bus, null, expectedPalette, nativeQueue, selection);
            AssertEqual(nativeQueue.TailInBytes, queue.TailInBytes, "Beam PNG keeps native seven-byte queue size");
            AssertEqual(nativeQueue.Entries[0].SizeInBytes, queue.Entries[0].SizeInBytes, "Queued beam transfer retains byte count");
            AssertEqual(nativeQueue.Entries[0].EncodedVramDestination, queue.Entries[0].EncodedVramDestination, "Queued beam retains destination");
            AssertTrue(actualPalette.Colors.SequenceEqual(expectedPalette.Colors), "PNG selection leaves beam palette behavior unchanged");
            using var state = new MemoryStream();
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(state, queue);
            state.Position = 0;
            var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(state);
            var stockVram = new SnesVram();
            queue.DrainTo(stockVram, ReferenceMutableMemory.From(new ProjectileCompositionForbiddenBus()), catalog);
            AssertTrue(stockVram.Bytes.SequenceEqual(native.Bytes), "Queued PNG publishes native pixels only at drain");
            var replacements = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(selection)] = editedPng.ToArray() };
            var editedCatalog = BeamTileCatalog.Load(replacements);
            var reboundVram = new SnesVram();
            restored.DrainTo(reboundVram, ReferenceMutableMemory.From(new ProjectileCompositionForbiddenBus()), editedCatalog);
            AssertTrue(reboundVram.Bytes.SequenceEqual(extracted.Bytes), "Restored pending transfer resolves current PNG, not serialized stock bytes");
            AssertEqual(0, restored.TailInBytes, "NMI clears restored asset queue");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, 8, 8, new byte[64], new[] { new Rgba32(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize, 0), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new Rgba32((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, 64, 8, pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex, 0), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8]), 0), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve production VRAM uploads, exact edited-pixel isolation and malformed resource rejection pass.");
    }

    private static void LoadNativeBeamFixture(ISnesAddressSpace bus, SnesVram? vram,
        SnesCgram cgram, VramWriteQueue? queue, ushort selection)
    {
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        int tileSource = SamusProjectileRomData.Banks.CharacterData |
            Word(SamusProjectileRomData.Beams.TilePointers + selection * 2);
        if (vram is not null)
        {
            var bytes = new byte[BeamTileAtlasDefinitions.ByteCount];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = bus.ReadByte(tileSource + i);
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, bytes);
        }
        queue?.Enqueue(BeamTileAtlasDefinitions.ByteCount, tileSource, BeamTileAtlasDefinitions.DestinationWord);
        int colors = SamusProjectileRomData.Banks.Movement |
            Word(SamusProjectileRomData.Beams.PalettePointers + selection * 2);
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + i, Bgr555.FromWord(checked((ushort)(Word(colors + i * 2)))));
    }
    private sealed class BeamArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadWorkRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        public byte ReadByte(int address)
        {
            if ((address >> 16) == 0x9a || address is >= 0x90c3b1 and < 0x90c3c9)
                throw new InvalidDataException("Authored beam queue still reads ROM graphics or selection pointers.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Artwork queue wrote the bus.");
    }

    private static void VerifyRuntimeBeamArtwork(ISnesAddressSpace bus, Dictionary<string, byte[]> files, BeamTileCatalog stock)
    {
        var image = IndexedPng.Read(new MemoryStream(files[BeamTileAtlasDefinitions.FileName(0)]), 64, 8);
        image.Pixels[0] ^= 1;
        using var png = new MemoryStream();
        IndexedPng.Write(png, 64, 8, image.Pixels, image.Palette);
        var changed = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(0)] = png.ToArray() };
        var edited = BeamTileCatalog.Load(changed, stock.Palettes);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        byte[] previous = runtime.Vram.Bytes.ToArray();
        // Simulate an old snapshot with a queued ROM beam upload before host rebind.
        LoadNativeBeamFixture(bus, null, runtime.Cgram, runtime.VramWrites, 0);
        runtime.BeamArtwork = edited;
        AssertTrue(previous.AsSpan().SequenceEqual(runtime.Vram.Bytes), "Binding beam PNG leaves retained VRAM unchanged");
        runtime.RunNmi(0, false);
        AssertTrue(previous.AsSpan().SequenceEqual(runtime.Vram.Bytes), "Lag NMI does not publish rebound artwork");
        runtime.RunNmi(0, true);
        int start = BeamTileAtlasDefinitions.DestinationWord * 2;
        AssertTrue(runtime.Vram.Bytes.Slice(start, 256).SequenceEqual(edited.Resolve(BeamTileCatalog.AssetFor(0)).Span),
            "Accepted NMI uses current PNG after legacy queued writes");
        runtime.QueueGameplayBeamTilesAndLoadPalette(1);
        AssertEqual(BeamTileCatalog.AssetFor(1), runtime.VramWrites.Entries[0].AssetId, "Runtime equipment upload uses typed beam selection");
        using var saved = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, runtime);
        saved.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Runtime.SuperMetroidRuntime>(saved);
        AssertTrue(restored.BeamArtwork is null, "Beam artwork is not embedded in runtime state");
        BindRetailRuntimeFixture(restored);
        restored.Samus!.EquippedBeams = 1;
        restored.BeamArtwork = stock;
        restored.RunNmi(0, true);
        AssertTrue(restored.Vram.Bytes.Slice(start, 256).SequenceEqual(stock.Resolve(BeamTileCatalog.AssetFor(1)).Span),
            "Restored runtime resolves queued beam selection through its bound provider");
        Console.WriteLine("Runtime beam PNG: retained/lag display stability, accepted-NMI refresh after legacy writes, equipment queue and restored provider pass.");
    }
}
