using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks installed Ceres emergency title and Japanese subtitle tilemaps against the cartridge's BG1 transfers.</summary>
internal static class CeresEscapeOverlayTilemapsVerification
{
    /// <summary>Compares five native overlays and the title the getaway shows over BG1.</summary>
    internal static void VerifyCeresEscapeOverlayTilemaps(ISnesAddressSpace rom, InstalledEnemyArtwork stock)
    {
        CeresEscapeOverlays stockOverlay =
            stock.Areas.CeresEscapeOverlayTilemaps ?? throw new InvalidOperationException(
                "Installed Ceres escape overlay tilemaps are absent.");
        foreach (CeresEscapeOverlayTilemapDefinition page in
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

        var installed = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers."))) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        installed.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
        installed.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay();

        // The cartridge's own transfers, read from its descriptors and source bytes, written over BG1. The getaway writes
        // each page's picture by name (#1466); its words are the installed page's.
        int[] native = [.. Enumerable.Repeat(RoomWrittenOverlay.Unwritten, TwoScreenTilemapCells.Count)];
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
        AssertTrue(native.SequenceEqual(installed.RidleyEncounter.CeresWarning.WordsIn(stockOverlay)),
            "Ceres English/Japanese BG1 overlay matches cartridge without source reads");

        void WriteNativeWords(int source, int byteCount, int destinationWord)
        {
            int first = destinationWord - GameplayVramLayout.Bg1TilemapWord;
            for (int index = 0; index < byteCount / sizeof(ushort); index++)
                native[first + index] = CartridgeImportSource.Require(rom).ReadCartridgeWord(source + index * sizeof(ushort));
        }

        AssertTrue(stockOverlay.TryResolve(title, out ReadOnlyMemory<BackgroundTile> _),
            "stock English Ceres title resolves");
        var shownEnemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers."))) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        shownEnemies.CeresRidleyBehavior.Getaway.DrawCeresEmergencyText();
        int titleWord = TwoScreenTilemapCells.IndexOf(CeresEscapeOverlayTilemapDefinitions.Emergency.Cell);
        AssertEqual(native[titleWord], shownEnemies.RidleyEncounter.CeresWarning.WordsIn(stock.Areas.CeresEscapeOverlayTilemaps)[titleWord],
            "the Ceres title shows its native word over BG1");
        // The getaway writes each page's picture by name (#1466) and holds no installed page of its own to miss.
        Console.WriteLine(
            "Ceres escape overlays: five installed tilemaps match native bytes and guarded " +
            "VRAM pass.");
    }
}
