using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static BeamPaletteArtworkVerification;
using static ProjectileCompositionsVerification;
using static ProjectileTrailArtworkVerification;
using static ProjectileTrailDefinitionsVerification;
using static RetailRuntimeFixtureVerification;
using static SuiteTimingVerification;

/// <summary>Represents beam tile artwork verification.</summary>
internal static class BeamTileArtworkVerification
{
    /// <summary>verify beam tile artwork.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyBeamTileArtwork(ISnesAddressSpace bus)
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
            byte[] png = files[BeamTileAtlasDefinitions.FileName(SamusBeamCombinations.FromTableIndex(selection))];
            var atlas = BeamTileAtlas.Load(new MemoryStream(png), SamusBeamCombinations.FromTableIndex(selection));
            var native = new SnesVram(); var extracted = new SnesVram();
            LoadNativeBeamFixture(bus, native, new SnesCgram(), null, selection);
            atlas.LoadTo(extracted);
            AssertTrue(native.ImageEquals(extracted), "PNG matches production beam upload across complete VRAM");
            var image = IndexedPng.Read(new MemoryStream(png), new(64, 8));
            image.Pixels[0] ^= 1;
            using var editedPng = new MemoryStream();
            IndexedPng.Write(editedPng, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
            editedPng.Position = 0;
            var edited = BeamTileAtlas.Load(editedPng, SamusBeamCombinations.FromTableIndex(selection));
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
            SamusProjectileSystem.QueueBeamTilesAndLoadPalette(NewBeamArtworkReadGuard(bus), queue, actualPalette, selection, catalog);
            var legacyVram = new SnesVram();
            nativeQueue.DrainTo(legacyVram, ReferenceMutableMemory.From(NewProjectileCompositionForbiddenBus()), catalog);
            AssertTrue(legacyVram.ImageEquals(native), "Legacy beam source resolves exact native pixels without ROM DMA");
            // Recreate the consumed reference queue for the native record-layout assertions.
            LoadNativeBeamFixture(bus, null, expectedPalette, nativeQueue, selection);
            AssertEqual(nativeQueue.TailInBytes, queue.TailInBytes, "Beam PNG keeps native seven-byte queue size");
            AssertEqual(nativeQueue.Entries[0].SizeInBytes, queue.Entries[0].SizeInBytes, "Queued beam transfer retains byte count");
            AssertEqual(nativeQueue.Entries[0].EncodedVramDestination, queue.Entries[0].EncodedVramDestination, "Queued beam retains destination");
            AssertTrue(new CgramPalettes(actualPalette).ImageEquals(new CgramPalettes(expectedPalette)), "PNG selection leaves beam palette behavior unchanged");
            using var state = new MemoryStream();
            SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(state, queue);
            state.Position = 0;
            var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<VramWriteQueue>(state);
            var stockVram = new SnesVram();
            queue.DrainTo(stockVram, ReferenceMutableMemory.From(NewProjectileCompositionForbiddenBus()), catalog);
            AssertTrue(stockVram.ImageEquals(native), "Queued PNG publishes native pixels only at drain");
            var replacements = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(SamusBeamCombinations.FromTableIndex(selection))] = editedPng.ToArray() };
            var editedCatalog = BeamTileCatalog.Load(replacements);
            var reboundVram = new SnesVram();
            restored.DrainTo(reboundVram, ReferenceMutableMemory.From(NewProjectileCompositionForbiddenBus()), editedCatalog);
            AssertTrue(reboundVram.ImageEquals(extracted), "Restored pending transfer resolves current PNG, not serialized stock bytes");
            AssertEqual(0, restored.TailInBytes, "NMI clears restored asset queue");
        }
        using var wrongSize = new MemoryStream();
        IndexedPng.Write(wrongSize, new(8, 8), new byte[64], new[] { new Rgba32(0, 0, 0, 255) });
        wrongSize.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(wrongSize, 0), "Wrong beam atlas dimensions rejected");
        using var invalidIndex = new MemoryStream();
        var colors = Enumerable.Range(0, 17).Select(i => new Rgba32((byte)i, 0, 0, 255)).ToArray();
        var pixels = new byte[512]; pixels[0] = 16;
        IndexedPng.Write(invalidIndex, new(64, 8), pixels, colors);
        invalidIndex.Position = 0;
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(invalidIndex, 0), "Beam index exceeds four bit hardware palette");
        AssertThrows<InvalidDataException>(() => BeamTileAtlas.Load(new MemoryStream(new byte[8]), 0), "Malformed beam PNG rejected");
        Console.WriteLine("Beam PNG artwork: twelve production VRAM uploads, exact edited-pixel isolation and malformed resource rejection pass.");
    }

    /// <summary>load native beam fixture.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the native beam tile artwork fixture")]
    internal static void LoadNativeBeamFixture(ISnesAddressSpace bus, SnesVram? vram,
        SnesCgram cgram, VramWriteQueue? queue, ushort selection)
    {
        int tileSource = SamusProjectileRomData.Banks.CharacterData |
            CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusProjectileRomData.Beams.TilePointers + selection * 2));
        if (vram is not null)
        {
            var bytes = new byte[BeamTileAtlasDefinitions.ByteCount];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = bus.ReadCartridgeByte(tileSource + i);
            vram.LoadBytes(BeamTileAtlasDefinitions.DestinationWord * 2, bytes);
        }
        queue?.Enqueue(BeamTileAtlasDefinitions.ByteCount, tileSource, VramDmaDestination.FromNative(BeamTileAtlasDefinitions.DestinationWord));
        int colors = SamusProjectileRomData.Banks.Movement |
            CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusProjectileRomData.Beams.PalettePointers + selection * 2));
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + i, Bgr555.FromWord(checked((ushort)(CartridgeImportSource.Require(bus).ReadCartridgeWord((colors + i * 2))))));
    }
    /// <summary>new beam artwork read guard.</summary>
    private static MutableCartridgeReadGuard NewBeamArtworkReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => (SnesAddressMath.BankNumber(address)) == 0x9a || address is >= 0x90c3b1 and < 0x90c3c9,
                _ => new InvalidDataException("Authored beam queue still reads ROM graphics or selection pointers."))
            .RejectWrites(_ => "Artwork queue wrote the bus.");
        return guard;
    }

    /// <summary>verify runtime beam artwork.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyRuntimeBeamArtwork(ISnesAddressSpace bus, Dictionary<string, byte[]> files, BeamTileCatalog stock)
    {
        var image = IndexedPng.Read(new MemoryStream(files[BeamTileAtlasDefinitions.FileName(0)]), new(64, 8));
        image.Pixels[0] ^= 1;
        using var png = new MemoryStream();
        IndexedPng.Write(png, new(64, 8), image.Pixels, image.Palette);
        var changed = new Dictionary<string, byte[]>(files) { [BeamTileAtlasDefinitions.FileName(0)] = png.ToArray() };
        var edited = BeamTileCatalog.Load(changed, stock.Palettes);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        byte[] previous = runtime.Vram.Bytes.ToArray();
        // Simulate an old snapshot with a queued ROM beam upload before host rebind.
        LoadNativeBeamFixture(bus, null, runtime.Cgram, runtime.VramWrites, 0);
        runtime.Artwork.BeamArtwork = edited;
        AssertTrue(previous.AsSpan().SequenceEqual(runtime.Vram.Bytes), "Binding beam PNG leaves retained VRAM unchanged");
        runtime.NmiHandler.RunNmi(0, false);
        AssertTrue(previous.AsSpan().SequenceEqual(runtime.Vram.Bytes), "Lag NMI does not publish rebound artwork");
        runtime.NmiHandler.RunNmi(0, true);
        int start = BeamTileAtlasDefinitions.DestinationWord * 2;
        AssertTrue(runtime.Vram.Bytes.Slice(start, 256).SequenceEqual(edited.Resolve(BeamTileCatalog.AssetFor(0)).Span),
            "Accepted NMI uses current PNG after legacy queued writes");
        runtime.SamusPhases.QueueGameplayBeamTilesAndLoadPalette(1);
        AssertEqual(BeamTileCatalog.AssetFor(SamusBeamCombination.Wave), runtime.VramWrites.Entries[0].AssetId, "Runtime equipment upload uses typed beam selection");
        using var saved = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(saved, runtime);
        saved.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Runtime.SuperMetroidRuntime>(saved);
        AssertTrue(restored.Artwork.BeamArtwork is null, "Beam artwork is not embedded in runtime state");
        BindRetailRuntimeFixture(restored);
        restored.Player.Samus!.EquippedBeams = 1;
        restored.Artwork.BeamArtwork = stock;
        restored.NmiHandler.RunNmi(0, true);
        AssertTrue(restored.Vram.Bytes.Slice(start, 256).SequenceEqual(stock.Resolve(BeamTileCatalog.AssetFor(SamusBeamCombination.Wave)).Span),
            "Restored runtime resolves queued beam selection through its bound provider");
        Console.WriteLine("Runtime beam PNG: retained/lag display stability, accepted-NMI refresh after legacy writes, equipment queue and restored provider pass.");
    }

    /// <summary>beam tile artwork fixture command.</summary>
    [VerificationCommand("--beam-tile-artwork-fixture")]
    internal static int BeamTileArtworkFixtureCommand()
    {
        Suite(nameof(VerifyBeamTileArtwork), () => VerifyBeamTileArtwork(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
