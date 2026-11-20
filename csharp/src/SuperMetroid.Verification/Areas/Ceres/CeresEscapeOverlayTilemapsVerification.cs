using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks installed Ceres emergency title and Japanese subtitle tilemaps against the cartridge's BG1 transfers.</summary>
internal static class CeresEscapeOverlayTilemapsVerification
{
    /// <summary>Compares the three installed overlays with the cartridge and the warning the getaway shows over BG1.</summary>
    internal static void VerifyCeresEscapeOverlayTilemaps(ISnesAddressSpace rom, ImportEnemyArtwork stock)
    {
        CeresEscapeOverlays stockOverlay =
            stock.Areas.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
                "Installed Ceres escape overlay tilemaps are absent.");
        foreach (CeresEscapeWarningPicture page in
                 CeresEscapeOverlayPages.All)
        {
            AssertTrue(stockOverlay.TryResolve(page, out ReadOnlyMemory<BackgroundTile> tiles) && tiles.Length == page.WordCount,
                $"Ceres overlay {page.Name} resolves completely");
            byte[] data = BackgroundTileWordBytes.Of(tiles.Span);
            for (int offset = 0; offset < data.Length; offset++)
                AssertEqual(rom.ReadCartridgeByte(CeresEscapeNativeSources.OverlaySource(page) + offset),
                    data[offset],
                    $"Ceres overlay {page.Name} native byte {offset:X2}");
        }

        var installed = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers.")), installedBosses: stock.Bosses);
        installed.CeresEscape.ShowWarningPicture(CeresEscapeWarningPictures.Emergency);
        foreach (CeresEscapeWarningPicture line in CeresEscapeWarningPictures.JapaneseSubtitle)
            installed.CeresEscape.ShowWarningPicture(line);

        // The cartridge's own transfers, read from its descriptors and source bytes, written over BG1. The getaway writes
        // each page's picture by name (#1466); its words are the installed page's.
        int[] native = [.. Enumerable.Repeat(RoomWarningText.Unwritten, TwoScreenTilemapCells.Count)];
        CeresEscapeWarningPicture title =
            CeresEscapeWarningPictures.Emergency;
        // $A6:C136 writes the title at BG1 tilemap word $50CB.
        WriteNativeWords(CeresEscapeNativeSources.OverlaySource(title), title.WordCount * sizeof(ushort), 0x50cb);
        int nativeTransfers = 1;
        foreach (TileTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerSprites)
                break;
            int descriptor = 0xa60000 | transfer.Pointer;
            WriteNativeWords(rom.ReadCartridgeLong(descriptor + 2),
                rom.ReadCartridgeWord(descriptor),
                rom.ReadCartridgeWord(descriptor + 5));
            nativeTransfers++;
        }
        AssertEqual(5, nativeTransfers, "the cartridge writes the English title and four Japanese subtitle rows");
        AssertTrue(native.SequenceEqual(installed.CeresEscape.Warning.Overlay.WordsIn(stockOverlay)),
            "Ceres English/Japanese BG1 overlay matches cartridge without source reads");

        void WriteNativeWords(int source, int byteCount, int destinationWord)
        {
            int first = destinationWord - GameplayVramLayout.Bg1TilemapWord;
            for (int index = 0; index < byteCount / sizeof(ushort); index++)
                native[first + index] = rom.ReadCartridgeWord(source + index * sizeof(ushort));
        }

        AssertTrue(stockOverlay.TryResolve(title, out ReadOnlyMemory<BackgroundTile> _),
            "stock English Ceres title resolves");
        var shownEnemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers.")), installedBosses: stock.Bosses);
        shownEnemies.CeresEscape.ShowWarningPicture(CeresEscapeWarningPictures.Emergency);
        int titleWord = TwoScreenTilemapCells.IndexOf(CeresEscapeWarningPictures.Emergency.Corner);
        AssertEqual(native[titleWord], shownEnemies.CeresEscape.Warning.Overlay.WordsIn(stock.Areas.CeresEscapeOverlayTilemaps)[titleWord],
            "the Ceres title shows its native word over BG1");
        // The getaway writes each page's picture by name (#1466) and holds no installed page of its own to miss.
        Console.WriteLine(
            "Ceres escape overlays: three installed tilemaps match native bytes and guarded " +
            "VRAM pass.");
    }
}
