using SuperMetroid.Core.Rom;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static CeresEscapeTileArtworkVerification;

/// <summary>Checks installed Ceres emergency title and Japanese subtitle tilemaps against cartridge VRAM.</summary>
internal static class CeresEscapeOverlayTilemapsVerification
{
    /// <summary>Compares five native overlays, then checks live override, reload, and missing-resource behavior.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres escape overlay tilemaps")]
    internal static void VerifyCeresEscapeOverlayTilemaps(ISnesAddressSpace rom,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        CeresEscapeOverlayTilemapCatalog stockOverlay =
            stock.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
                "Installed Ceres escape overlay tilemaps are absent.");
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            AssertTrue(stockOverlay.TryResolve(page.SourceAddress,
                    page.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> data),
                $"Ceres overlay {page.Name} resolves completely");
            for (int offset = 0; offset < data.Length; offset++)
                AssertEqual(rom.ReadCartridgeByte(page.SourceAddress + offset),
                    data.Span[offset],
                    $"Ceres overlay {page.Name} native byte {offset:X2}");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var guarded = NewCeresEscapeArtworkReadGuard(rom);
        var installed = new RoomEnemySystem { TileArtwork = stock };
        installed.BindFixtureBus(guarded);
        var installedQueue = new VramWriteQueue();
        installed.CeresRidleyBehavior.QueueCeresEmergencyText(installedQueue);
        installed.CeresRidleyBehavior.QueueCeresTransferList(CeresEscapeVramTransferDefinitions.JapaneseOverlay, installedQueue);
        AssertEqual(5, installedQueue.Entries.Count,
            "Ceres production queues English title and four Japanese subtitle rows");
        var installedVram = new SnesVram();
        installedQueue.DrainTo(installedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeOverlayProvider(stock));

        var nativeQueue = new VramWriteQueue();
        CeresEscapeOverlayTilemapDefinition title =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        nativeQueue.Enqueue(checked((ushort)(title.WordCount * sizeof(ushort))),
            title.SourceAddress,
            VramDmaDestination.FromNative(CeresEscapeOverlayTilemapDefinitions.EmergencyDestination));
        foreach (CeresEscapeVramTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitionsTooling.All)
        {
            if (transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerSprites)
                break;
            int descriptor = 0xa60000 | transfer.Pointer;
            ushort byteCount = CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor);
            int source = (CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2));
            ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord((descriptor + 5));
            nativeQueue.Enqueue(byteCount, source, VramDmaDestination.FromNative(destination));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, rom);
        AssertTrue(nativeVram.ImageEquals(installedVram),
            "Ceres English/Japanese tilemap VRAM matches cartridge without source reads");

        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-overlay-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = CeresEscapeOverlayTilemapDefinitions.FileName;
        CeresEscapeOverlayTilemapDocument document =
            JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, fileName)),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidDataException("Stock Ceres overlay JSON is null.");
        document.Pages[title.Name][0] ^= 1;
        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName),
            CeresEscapeOverlayTilemapCatalog.Write(document));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(stockOverlay.TryResolve(title.SourceAddress,
                title.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> stockWords),
            "stock English Ceres title resolves");
        AssertTrue((edited.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException("Verification requires edited.CeresEscapeOverlayTilemaps.")).TryResolve(title.SourceAddress,
                title.WordCount * sizeof(ushort), out ReadOnlyMemory<byte> editedWords),
            "edited English Ceres title resolves");
        AssertEqual((byte)(stockWords.Span[0] ^ 1), editedWords.Span[0],
            "edited Ceres title changes exactly its first tile word");
        AssertTrue(stockWords.Span[1..].SequenceEqual(editedWords.Span[1..]),
            "edited Ceres title leaves other tilemap bytes unchanged");
        var editedEnemies = new RoomEnemySystem { TileArtwork = edited };
        editedEnemies.BindFixtureBus(guarded);
        var editedQueue = new VramWriteQueue();
        editedEnemies.CeresRidleyBehavior.QueueCeresEmergencyText(editedQueue);
        var editedVram = new SnesVram();
        editedQueue.DrainTo(editedVram, ReferenceMutableMemory.From(guarded),
            new CeresEscapeOverlayProvider(edited));
        AssertEqual((ushort)(nativeVram.ReadWord(
                CeresEscapeOverlayTilemapDefinitions.EmergencyDestination) ^ 1),
            editedVram.ReadWord(
                CeresEscapeOverlayTilemapDefinitions.EmergencyDestination),
            "edited Ceres title changes the live VRAM word");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .CeresEscapeOverlayTilemaps.")).TryResolve(title.SourceAddress,
                    title.WordCount * sizeof(ushort),
                    out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedWords.Span),
            "Ceres title tilemap edit survives catalog reload");

        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName), [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "corrupt Ceres overlay JSON fails loudly");
        var missing = new RoomEnemySystem
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }),
        };
        missing.BindFixtureBus(guarded);
        ExpectMissing(() => missing.CeresRidleyBehavior.QueueCeresEmergencyText(new VramWriteQueue()),
            "missing installed English title");
        ExpectMissing(() => missing.CeresRidleyBehavior.QueueCeresTransferList(CeresEscapeVramTransferDefinitions.JapaneseOverlay,
                    new VramWriteQueue()),
            "missing installed Japanese subtitle rows");
        Console.WriteLine(
            "Ceres escape overlays: five editable tilemaps match native bytes and guarded " +
            "VRAM; live edit, reload, and missing/corrupt resource rejection pass.");

        static void ExpectMissing(Action action, string context)
        {
            try
            {
                action();
                throw new InvalidOperationException(
                    $"Ceres escape accepted {context}.");
            }
            catch (InvalidDataException)
            {
                // An installed session cannot silently use ROM tilemap bytes.
            }
        }
    }

    /// <summary>Supplies installed Ceres tilemap bytes to a guarded VRAM transfer fixture.</summary>
    internal sealed class CeresEscapeOverlayProvider(EnemyTileArtworkCatalog art)
        : IVramAssetProvider, IInstalledArtworkTransferSource
    {
        /// <summary>Rejects unrelated asset requests from the overlay-only transfer fixture.</summary>
        public ReadOnlyMemory<byte> Resolve(VramAssetId asset) =>
            throw new InvalidOperationException(
                $"Ceres overlay fixture did not expect asset {asset}.");

        /// <summary>Resolves a native Ceres overlay source range from installed tilemap artwork.</summary>
        public bool TryResolve(int sourceAddress, int byteCount,
            out ReadOnlyMemory<byte> data) =>
            art.TryResolve(sourceAddress, byteCount, out data);
    }
}
