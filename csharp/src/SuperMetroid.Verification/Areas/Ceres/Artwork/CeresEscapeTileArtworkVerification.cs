using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
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
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Areas.Ceres.Artwork;

/// <summary>Checks installed Ceres warning and door tiles through production VRAM transfers.</summary>
internal static class CeresEscapeTileArtworkVerification
{
    /// <summary>Compares native transfer bytes and the installed pages without cartridge reads.</summary>
    internal static void VerifyCeresEscapeTileArtwork(ISnesAddressSpace rom, ImportEnemyArtwork stock)
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
        var installed = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // The station's escape runs its warning until the escape timer and warning show (#2228, #2469).
        installed.CeresEscape.BeginSelfDestruct();
        for (int update = 0; update < CeresSelfDestructWarningDefinitions.BeforeWarningUpdates; update++)
            installed.CeresEscape.RunRidleysStep(0);
        AssertTrue(installed.Presentation.Looks.Shows(CeresEscapeWarningLookDefinitions.WarningLook),
            "Ceres escape production shows the warning look once its wait runs out");
        IReadOnlyList<EncounterArtworkUpload> installedPages = EncounterLookUploads.Uploads(ComposedEncounter.CeresEscape,
            EncounterLookUploadsConstants.IndexOf(ComposedEncounter.CeresEscape, CeresEscapeWarningLookDefinitions.WarningLook), new EnemyTileTransfers(stock, timer: timer));
        AssertEqual(7, installedPages.Count, "the warning look copies the timer-sprite list's seven pages");

        // The cartridge's timer-sprite records, read from its descriptors and source bytes.
        var nativeQueue = new VramWriteQueue();
        foreach (TileTransferDefinition transfer in
                 CeresEscapeVramTransferDefinitions.All)
        {
            if (transfer.Pointer < CeresEscapeVramTransferDefinitions.TimerSprites ||
                transfer.Pointer >= CeresEscapeVramTransferDefinitions.TimerBackgrounds)
                continue;
            int descriptor = 0xa60000 | transfer.Pointer;
            ushort byteCount = rom.ReadCartridgeWord(descriptor);
            int source = (rom.ReadCartridgeLong(descriptor + 2));
            ushort destination = rom.ReadCartridgeWord((descriptor + 5));
            nativeQueue.Enqueue(byteCount, source, VramDmaDestination.FromNative(destination));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, rom);
        for (int index = 0; index < installedPages.Count; index++)
        {
            EncounterArtworkUpload page = installedPages[index];
            AssertTrue(nativeVram.Bytes.Slice(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort) + page.FirstByte, page.Characters.Length)
                    .SequenceEqual(page.Characters.Span),
                $"the Ceres escape page {index} the warning look copies matches the cartridge's OBJ characters without source reads");
        }

        TileSheetDefinition warning =
            CeresEscapeTileArtworkDefinitions.WarningText;
        CeresEscapeTileArtwork stockArt = stock.Areas.CeresEscapeTiles
            ?? throw new InvalidOperationException("Stock Ceres escape art was not installed.");
        AssertTrue(stockArt.TryResolve(0, 0,
                0x0200, out ReadOnlyMemory<byte> stockChunk),
            "Ceres warning transfer resolves in stock artwork");
        // The warning text's first page follows the timer-sprite list's timer pages.
        int firstWarningPage = CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer) -
            CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.TimerSprites);
        EncounterArtworkUpload shownWarning = installedPages[firstWarningPage];
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
        var missing = new EnemyTileTransfers(new ImportEnemyArtwork(stock.Enemies, stock.Bosses, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.AreaArtwork), timer: timer);
        AssertThrows<InvalidDataException>(() => EncounterLookUploads.Steps(ComposedEncounter.CeresEscape, missing),
            "Ceres warning pictures reject missing installed character artwork");
        Console.WriteLine(
            "Ceres escape tiles: both indexed PNGs match native bytes; the seven OBJ " +
            "pages match the cartridge without ROM art reads.");
    }

    /// <summary>Rejects live descriptor, overlay, warning-tile, and timer-tile source reads.</summary>
    internal static CartridgeReadGuard NewCeresEscapeArtworkReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => CeresEscapeVramTransferDefinitions.IsDescriptorByteAddress(address) ||
                CeresEscapeWarningPictures.ContainsByteAddress(address) ||
                CeresEscapeTileArtworkDefinitionsTooling.Contains(address, 1) ||
                address >= EscapeTimerTileSourceConstants.FirstSourceAddress &&
                address < EscapeTimerTileSourceConstants.FirstSourceAddress + EscapeTimerTileAtlasFormat.TotalByteCount,
                address => $"Installed Ceres escape reread source ${address:X6}.");
}
