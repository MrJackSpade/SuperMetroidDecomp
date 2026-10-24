using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Verification.Areas.Ceres.Artwork;

/// <summary>Checks installed Ceres warning and door tiles through production VRAM transfers.</summary>
internal static class CeresEscapeTileArtworkVerification
{
    /// <summary>Compares native transfer bytes, then checks a PNG edit reaches VRAM without cartridge reads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres escape tile artwork")]
    internal static void VerifyCeresEscapeTileArtwork(ISnesAddressSpace rom,
        string stockDirectory, InstalledEnemyArtwork stock)
    {
        AssertTrue(stock.Areas.CeresEscapeTiles is not null,
            "installed Ceres escape warning and door PNGs exist");
        foreach (TileSheetDefinition page in
                 CeresEscapeTileArtworkDefinitions.All)
        {
            AssertTrue((stock.Areas.CeresEscapeTiles ?? throw new InvalidOperationException("Verification requires stock.Areas.CeresEscapeTiles.")).TryResolve(page.SourceAddress,
                    page.ByteCount, out ReadOnlyMemory<byte> nativePage),
                $"Ceres escape page {page.FileName} resolves completely");
            for (int offset = 0; offset < page.ByteCount; offset++)
                AssertEqual(rom.ReadCartridgeByte(page.SourceAddress + offset),
                    nativePage.Span[offset],
                    $"Ceres escape page {page.FileName} byte {offset:X4}");
        }

        using var timerPng = new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom),
            writable: false);
        EscapeTimerTileAtlas timer = EscapeTimerTileAtlas.Load(timerPng);
        var guarded = NewCeresEscapeArtworkReadGuard(rom);
        var installed = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = stock.Enemies,
            AreaArtwork = stock.Areas,
            BossArtworkBinding = { EscapeTimerArtwork = timer },
        };
        foreach (CeresEscapePageCursor start in new[] { CeresEscapePageCursor.Objects, CeresEscapePageCursor.Backgrounds })
        {
            var state = new RidleyEnemyState { CeresEscapePages = start };
            while (!installed.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(state))
            {
            }
        }
        ObjCharacterUploads shown = installed.Presentation.ObjCharacters;
        AssertEqual(7, shown.Uploads.Count, "Ceres escape production shows the timer-sprite list's seven pages");
        ReadOnlySpan<byte> installedPage = new ObjCharacterTablePage().Page(shown, new EnemyTileTransfers(stock, timer: timer)).Characters;

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
        foreach (ObjCharacterUpload page in shown.Uploads)
            AssertTrue(nativeVram.Bytes.Slice(GameplayVramLayout.ObjectCharacters.Offset * sizeof(ushort) + page.FirstByte, page.ByteCount)
                    .SequenceEqual(installedPage.Slice(page.FirstByte, page.ByteCount)),
                $"the Ceres page {page.Source} matches the cartridge's OBJ characters without source reads");

        TileSheetDefinition warning =
            CeresEscapeTileArtworkDefinitions.WarningText;
        string overrideDirectory = Path.Combine(stockDirectory,
            "ceres-escape-tile-overrides");
        Directory.CreateDirectory(overrideDirectory);
        int tileCount = warning.ByteCount / RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        using (var input = new MemoryStream(File.ReadAllBytes(
                   Path.Combine(stockDirectory, warning.FileName)), writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            using var output = File.Create(Path.Combine(overrideDirectory,
                warning.FileName));
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        CeresEscapeTileArtwork stockArt = stock.Areas.CeresEscapeTiles
            ?? throw new InvalidOperationException("Stock Ceres escape art was not installed.");
        CeresEscapeTileArtwork editedArt = edited.Areas.CeresEscapeTiles
            ?? throw new InvalidOperationException("Edited Ceres escape art was not installed.");
        AssertTrue(stockArt.TryResolve(warning.SourceAddress,
                0x0200, out ReadOnlyMemory<byte> stockChunk),
            "Ceres warning transfer resolves in stock artwork");
        AssertTrue(editedArt.TryResolve(warning.SourceAddress,
                0x0200, out ReadOnlyMemory<byte> editedChunk),
            "Ceres warning transfer resolves in edited artwork");
        AssertEqual((byte)(stockChunk.Span[0] ^ 0x80), editedChunk.Span[0],
            "Ceres warning PNG edit changes the first tile-plane byte");
        AssertTrue(stockChunk.Span[1..].SequenceEqual(editedChunk.Span[1..]),
            "Ceres warning PNG edit leaves other transfer bytes unchanged");
        var editedEnemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = edited.Enemies,
            AreaArtwork = edited.Areas,
            BossArtworkBinding = { EscapeTimerArtwork = timer },
        };
        // The cursor starts at the first warning-text page, part-way through the timer-sprite list.
        int firstWarningPage = CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer) -
            CeresEscapeVramTransferDefinitions.Records.ToList().FindIndex(record =>
                record.Pointer == CeresEscapeVramTransferDefinitions.TimerSprites);
        var editedState = new RidleyEnemyState { CeresEscapePages = CeresEscapePageCursor.Objects with { Shown = firstWarningPage } };
        _ = editedEnemies.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(editedState);
        ReadOnlySpan<byte> editedPage = new ObjCharacterTablePage().Page(editedEnemies.Presentation.ObjCharacters,
            new EnemyTileTransfers(edited, timer: timer)).Characters;
        AssertTrue(CeresEscapeVramTransferDefinitions.TryGet(
                CeresEscapeVramTransferDefinitions.WarningTextFirstTransfer,
                out TileTransferDefinition warningTransfer),
            "first warning transfer is compiled");
        AssertEqual(editedChunk.Span[0],
            editedPage[ObjCharacterUploads.TableByte(VramDmaDestination.FromNative(warningTransfer.DestinationWord))],
            "edited Ceres warning PNG changes the page production shows");
        AssertTrue((EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Areas.CeresEscapeTiles ?? throw new InvalidOperationException("Verification requires the reloaded Ceres escape tiles.")).TryResolve(warning.SourceAddress, 0x0200,
                    out ReadOnlyMemory<byte> reloaded) &&
                   reloaded.Span.SequenceEqual(editedChunk.Span),
            "Ceres warning tile override survives catalog reload");

        File.WriteAllBytes(Path.Combine(overrideDirectory, warning.FileName),
            [1, 2, 3]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "invalid Ceres warning PNG fails loudly");

        var missing = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }),
            BossArtworkBinding = { EscapeTimerArtwork = timer },
        };
        try
        {
            missing.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(new RidleyEnemyState
            {
                CeresEscapePages = CeresEscapePageCursor.Objects with { Shown = firstWarningPage },
            });
            throw new InvalidOperationException(
                "Installed Ceres transfer accepted missing character artwork.");
        }
        catch (InvalidDataException)
        {
            // Missing installed art must not silently fall back to cartridge bytes.
        }
        Console.WriteLine(
            "Ceres escape tiles: both indexed PNGs match native bytes; the seven OBJ " +
            "pages match the cartridge without ROM art reads; edit, reload, and rejection pass.");
    }

    /// <summary>Rejects live descriptor, overlay, warning-tile, and timer-tile source reads.</summary>
    internal static CartridgeReadGuard NewCeresEscapeArtworkReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => CeresEscapeVramTransferDefinitions.IsDescriptorByteAddress(address) ||
                CeresEscapeOverlayTilemapDefinitions.ContainsByteAddress(address) ||
                CeresEscapeTileArtworkDefinitionsTooling.Contains(address, 1) ||
                address >= EscapeTimerTileRomData.FirstSourceAddress &&
                address < EscapeTimerTileRomData.FirstSourceAddress + EscapeTimerTileAtlasFormat.TotalByteCount,
                address => $"Installed Ceres escape reread source ${address:X6}.");
}
