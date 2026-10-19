using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Areas.Ceres.Artwork.CeresEscapeTileArtworkVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks installed Ceres emergency title and Japanese subtitle tilemaps against the cartridge's BG1 transfers.</summary>
internal static class CeresEscapeOverlayTilemapsVerification
{
    /// <summary>JSON options: Web defaults: camel-case names, case-insensitive matching, quoted numbers.</summary>
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

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

        var guarded = NewCeresEscapeArtworkReadGuard(rom);
        var installedOverlay = new RecordedBg1Overlay();
        var installed = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: installedOverlay.Services()) { TileArtwork = stock };
        installed.CeresRidleyBehavior.DrawCeresEmergencyText();
        installed.CeresRidleyBehavior.DrawCeresJapaneseOverlay(CeresEscapeVramTransferDefinitions.JapaneseOverlay);

        // The cartridge's own transfers, read from its descriptors and source bytes, written over BG1.
        RoomTilemapOverlay native = RoomTilemapOverlay.Empty;
        CeresEscapeOverlayTilemapDefinition title =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        native = WithNativeWords(native, title.SourceAddress, title.WordCount * sizeof(ushort),
            CeresEscapeOverlayTilemapDefinitions.EmergencyDestination);
        int nativeTransfers = 1;
        foreach (CeresEscapeVramTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitionsTooling.All)
        {
            if (transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerSprites)
                break;
            int descriptor = 0xa60000 | transfer.Pointer;
            native = WithNativeWords(native, CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor + 5));
            nativeTransfers++;
        }
        AssertEqual(5, nativeTransfers, "the cartridge writes the English title and four Japanese subtitle rows");
        AssertTrue(native.Words.SequenceEqual(installedOverlay.Overlay.Words),
            "Ceres English/Japanese BG1 overlay matches cartridge without source reads");

        RoomTilemapOverlay WithNativeWords(RoomTilemapOverlay overlay, int source, int byteCount, int destinationWord)
        {
            var words = new ushort[byteCount / sizeof(ushort)];
            for (int index = 0; index < words.Length; index++)
                words[index] = CartridgeImportSource.Require(rom).ReadCartridgeWord(source + index * sizeof(ushort));
            return overlay.With(destinationWord - GameplayVramLayout.Bg1TilemapWord, words);
        }

        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-overlay-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = CeresEscapeOverlayTilemapDefinitions.FileName;
        CeresEscapeOverlayTilemapDocument document =
            JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, fileName)),
                WebJson)
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
        var editedOverlay = new RecordedBg1Overlay();
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: editedOverlay.Services()) { TileArtwork = edited };
        editedEnemies.CeresRidleyBehavior.DrawCeresEmergencyText();
        int titleWord = CeresEscapeOverlayTilemapDefinitions.EmergencyDestination - GameplayVramLayout.Bg1TilemapWord;
        AssertEqual(native.Words[titleWord] ^ 1, editedOverlay.Overlay.Words[titleWord],
            "edited Ceres title changes the word shown over BG1");
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
        var missing = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }),
        };
        ExpectMissing(() => missing.CeresRidleyBehavior.DrawCeresEmergencyText(),
            "missing installed English title");
        ExpectMissing(() => missing.CeresRidleyBehavior.DrawCeresJapaneseOverlay(CeresEscapeVramTransferDefinitions.JapaneseOverlay),
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
