using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.Verification.Areas.Ceres.Artwork;

/// <summary>Checks installed Ceres warning and door tiles through production VRAM transfers.</summary>
internal static class CeresEscapeTileArtworkVerification
{
    /// <summary>Compares native transfer bytes and the installed pages without cartridge reads.</summary>
    internal static void VerifyCeresEscapeTileArtwork(ISnesAddressSpace rom, InstalledEnemyArtwork stock)
    {
        AssertTrue(stock.Areas.CeresEscapeTiles is not null,
            "installed Ceres escape warning and door PNGs exist");
        for (int pageIndex = 0; pageIndex < CeresEscapeTileArtworkDefinitions.All.Length; pageIndex++)
        {
            TileSheetDefinition page = CeresEscapeTileArtworkDefinitions.All[pageIndex];
            AssertTrue((stock.Areas.CeresEscapeTiles ?? throw new InvalidOperationException("Verification requires stock.Areas.CeresEscapeTiles.")).TryResolve(pageIndex, 0,
                    page.ByteCount, out ReadOnlyMemory<byte> nativePage),
                $"Ceres escape page {page.FileName} resolves completely");
            for (int offset = 0; offset < page.ByteCount; offset++)
                AssertEqual(rom.ReadCartridgeByte(CeresEscapeNativeSources.PageSource(pageIndex) + offset),
                    nativePage.Span[offset],
                    $"Ceres escape page {page.FileName} byte {offset:X4}");
        }

        using var timerPng = new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom),
            writable: false);
        EscapeTimerCharacters timer = EscapeTimerCharacters.Read(timerPng);
        var guarded = NewCeresEscapeArtworkReadGuard(rom);
        var installed = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = stock.Enemies,
            
        };
        foreach (CeresEscapePageCursor start in new[] { CeresEscapePageCursor.Objects, CeresEscapePageCursor.Backgrounds })
        {
            var state = new RidleyEnemyState { CeresEscapePages = start };
            while (!installed.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(state))
            {
            }
        }
        AssertEqual<int?>(CeresEscapePageDefinitions.ObjectPageCount - 1, installed.Presentation.ArtworkProgress.StepOf(EncounterArtworkSequence.CeresEscapePages),
            "Ceres escape production reaches the last of the timer-sprite list's seven pages, and the background list's show nothing");
        IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> installedSteps =
            EncounterArtworkUploads.Steps(EncounterArtworkSequence.CeresEscapePages, new EnemyTileTransfers(stock, timer: timer));
        AssertEqual(CeresEscapePageDefinitions.ObjectPageCount, installedSteps.Count, "the escape's artwork sequence has the timer-sprite list's seven pages");

        // The cartridge's timer-sprite records, read from its descriptors and source bytes.
        var nativeQueue = new VramWriteQueue();
        foreach (TileTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer < CeresEscapeVramTransferDefinitions.TimerSprites ||
                transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerBackgrounds)
                continue;
            int descriptor = 0xa60000 | transfer.Pointer;
            ushort byteCount = CartridgeImportSource.Require(rom).ReadCartridgeWord(descriptor);
            int source = (CartridgeImportSource.Require(rom).ReadCartridgeLong(descriptor + 2));
            ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord((descriptor + 5));
            nativeQueue.Enqueue(byteCount, source, VramDmaDestination.FromNative(destination));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, rom);
        for (int step = 0; step < installedSteps.Count; step++)
        {
            foreach (EncounterArtworkUpload page in installedSteps[step])
                AssertTrue(nativeVram.Bytes.Slice(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort) + page.FirstByte, page.Characters.Length)
                        .SequenceEqual(page.Characters.Span),
                    $"the Ceres escape page of step {step} matches the cartridge's OBJ characters without source reads");
        }

        TileSheetDefinition warning =
            CeresEscapeTileArtworkDefinitions.WarningText;
        CeresEscapeTileArtwork stockArt = stock.Areas.CeresEscapeTiles
            ?? throw new InvalidOperationException("Stock Ceres escape art was not installed.");
        AssertTrue(stockArt.TryResolve(0, 0,
                0x0200, out ReadOnlyMemory<byte> stockChunk),
            "Ceres warning transfer resolves in stock artwork");
        var warningEnemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = stock.Enemies,
        };
        // The cursor starts at the first warning-text page, part-way through the timer-sprite list.
        int firstWarningPage = CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer) -
            CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.TimerSprites);
        var warningState = new RidleyEnemyState { CeresEscapePages = CeresEscapePageCursor.Objects with { Shown = firstWarningPage } };
        _ = warningEnemies.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(warningState);
        AssertEqual<int?>(firstWarningPage, warningEnemies.Presentation.ArtworkProgress.StepOf(EncounterArtworkSequence.CeresEscapePages),
            "the getaway reaches the warning text's first page");
        EncounterArtworkUpload shownWarning = EncounterArtworkUploads.Steps(EncounterArtworkSequence.CeresEscapePages,
            new EnemyTileTransfers(stock, timer: timer))[firstWarningPage].Single();
        AssertTrue(CeresEscapeVramTransferDefinitions.TryGet(
                CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer,
                out TileTransferDefinition warningTransfer),
            "first warning transfer is compiled");
        AssertEqual(SpriteArtworkPlacements.TableByte(VramDmaDestination.FromNative(warningTransfer.DestinationWord)), shownWarning.FirstByte,
            "the warning text's first page lands where the cartridge copies it");
        AssertTrue(shownWarning.Characters.Span.SequenceEqual(stockChunk.Span[..shownWarning.Characters.Length]),
            "the warning text's first page pictures the installed characters");

        // The pictures of the warning pages are composed from the installed characters; composing them without those
        // characters fails rather than falling back to cartridge bytes.
        var missing = new EnemyTileTransfers(new InstalledEnemyArtwork(stock.Enemies, stock.Bosses, new AreaSourceArtwork()), timer: timer);
        AssertThrows<InvalidDataException>(() => EncounterArtworkUploads.Steps(EncounterArtworkSequence.CeresEscapePages, missing),
            "Ceres warning pictures reject missing installed character artwork");
        Console.WriteLine(
            "Ceres escape tiles: both indexed PNGs match native bytes; the seven OBJ " +
            "pages match the cartridge without ROM art reads.");
    }

    /// <summary>Rejects live descriptor, overlay, warning-tile, and timer-tile source reads.</summary>
    internal static CartridgeReadGuard NewCeresEscapeArtworkReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => CeresEscapeVramTransferDefinitions.IsDescriptorByteAddress(address) ||
                CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(address) ||
                CeresEscapeTileArtworkDefinitionsTooling.Contains(address, 1) ||
                address >= EscapeTimerTileSourceConstants.FirstSourceAddress &&
                address < EscapeTimerTileSourceConstants.FirstSourceAddress + EscapeTimerTileAtlasFormat.TotalByteCount,
                address => $"Installed Ceres escape reread source ${address:X6}.");
}
