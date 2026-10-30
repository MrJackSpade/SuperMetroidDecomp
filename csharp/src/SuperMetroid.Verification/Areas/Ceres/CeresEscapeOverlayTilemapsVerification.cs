using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using System.Text.Json;
using static SuperMetroid.Verification.Areas.Ceres.Artwork.CeresEscapeTileArtworkVerification;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks installed Ceres emergency title and Japanese subtitle tilemaps against the cartridge's BG1 transfers.</summary>
internal static class CeresEscapeOverlayTilemapsVerification
{
    /// <summary>JSON options: Web defaults: camel-case names, case-insensitive matching, quoted numbers.</summary>
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    /// <summary>Compares five native overlays, then checks live override, reload, and missing-resource behavior.</summary>
    internal static void VerifyCeresEscapeOverlayTilemaps(ISnesAddressSpace rom,
        string stockDirectory, InstalledEnemyArtwork stock)
    {
        CeresEscapeOverlays stockOverlay =
            stock.Areas.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
                "Installed Ceres escape overlay tilemaps are absent.");
        foreach (CeresEscapeOverlayTilemapDefinition page in
                 CeresEscapeOverlayTilemapDefinitions.All)
        {
            AssertTrue(stockOverlay.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles) && tiles.Length == page.WordCount,
                $"Ceres overlay {page.Name} resolves completely");
            byte[] data = BackgroundTileWordBytes.Of(tiles.Span);
            for (int offset = 0; offset < data.Length; offset++)
                AssertEqual(rom.ReadCartridgeByte(CeresEscapeNativeSources.OverlaySource(page) + offset),
                    data[offset],
                    $"Ceres overlay {page.Name} native byte {offset:X2}");
        }

        var guarded = NewCeresEscapeArtworkReadGuard(rom);
        var installedOverlay = new RecordedBg1Overlay();
        var installed = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: installedOverlay.Services()) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas };
        installed.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
        installed.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay();

        // The cartridge's own transfers, read from its descriptors and source bytes, written over BG1. The getaway writes
        // each page's picture by name (#1466); its words are the installed page's.
        int[] native = [.. Enumerable.Repeat(RoomWrittenOverlay.Unwritten, RoomWrittenOverlay.CellCount)];
        CeresEscapeOverlayTilemapDefinition title =
            CeresEscapeOverlayTilemapDefinitions.Emergency;
        // $A6:C136 writes the title at BG1 tilemap word $50CB.
        WriteNativeWords(CeresEscapeNativeSources.OverlaySource(title), title.WordCount * sizeof(ushort), 0x50cb);
        int nativeTransfers = 1;
        foreach (TileTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerSprites)
                break;
            int descriptor = 0xa60000 | transfer.Pointer;
            WriteNativeWords(CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor + 5));
            nativeTransfers++;
        }
        AssertEqual(5, nativeTransfers, "the cartridge writes the English title and four Japanese subtitle rows");
        AssertTrue(native.SequenceEqual(installedOverlay.Overlay.WordsIn(stockOverlay)),
            "Ceres English/Japanese BG1 overlay matches cartridge without source reads");

        void WriteNativeWords(int source, int byteCount, int destinationWord)
        {
            int first = destinationWord - GameplayVramLayout.Bg1TilemapWord;
            for (int index = 0; index < byteCount / sizeof(ushort); index++)
                native[first + index] = CartridgeImportSource.Require(rom).ReadCartridgeWord(source + index * sizeof(ushort));
        }

        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-overlay-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = CeresWarningGlyphDefinitions.FileName;
        CeresEscapeOverlayTilemapDocument document =
            JsonSerializer.Deserialize<CeresEscapeOverlayTilemapDocument>(
                File.ReadAllBytes(Path.Combine(stockDirectory, fileName)),
                WebJson)
            ?? throw new InvalidDataException("Stock Ceres overlay JSON is null.");
        document.Pages[title.Name][0] = BackgroundTileWordBytes.Tile(document.Pages[title.Name][0].PackedWord ^ 1);
        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName),
            CeresEscapeOverlays.Write(document));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(stockOverlay.TryResolve(title, out ReadOnlyMemory<BackgroundTile> stockTiles),
            "stock English Ceres title resolves");
        AssertTrue((edited.Areas.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException("Verification requires edited.CeresEscapeOverlayTilemaps.")).TryResolve(title, out ReadOnlyMemory<BackgroundTile> editedTiles),
            "edited English Ceres title resolves");
        byte[] stockWords = BackgroundTileWordBytes.Of(stockTiles.Span), editedWords = BackgroundTileWordBytes.Of(editedTiles.Span);
        AssertEqual((byte)(stockWords[0] ^ 1), editedWords[0],
            "edited Ceres title changes exactly its first tile word");
        AssertTrue(stockWords.AsSpan(1).SequenceEqual(editedWords.AsSpan(1)),
            "edited Ceres title leaves other tilemap bytes unchanged");
        var editedOverlay = new RecordedBg1Overlay();
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: editedOverlay.Services()) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        editedEnemies.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
        int titleWord = CeresEscapeOverlayTilemapDefinitions.Emergency.Cell;
        AssertEqual(native.Words[titleWord] ^ 1, editedOverlay.Overlay.Words[titleWord],
            "edited Ceres title changes the word shown over BG1");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Areas.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory) .CeresEscapeOverlayTilemaps.")).TryResolve(title,
                    out ReadOnlyMemory<BackgroundTile> reloaded) &&
                   BackgroundTileWordBytes.Of(reloaded.Span).AsSpan().SequenceEqual(editedWords),
            "Ceres title tilemap edit survives catalog reload");

        File.WriteAllBytes(Path.Combine(overrideDirectory, fileName), [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "corrupt Ceres overlay JSON fails loudly");
        var missing = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }),
        };
        ExpectMissing(() => missing.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText(),
            "missing installed English title");
        ExpectMissing(() => missing.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay(),
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

}
