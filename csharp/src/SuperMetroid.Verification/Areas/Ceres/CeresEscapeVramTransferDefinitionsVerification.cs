using SuperMetroid.Core.Areas;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Areas.Ceres;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>Checks Ceres escape VRAM descriptors and installed timer and Japanese overlay transfers.</summary>
internal static class CeresEscapeVramTransferDefinitionsVerification
{
    /// <summary>Compares nineteen native records and three terminators, then queues production transfers without descriptor reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres escape VRAM transfer definitions")]
    internal static void VerifyCeresEscapeVramTransferDefinitions(ISnesAddressSpace rom)
    {
        IReadOnlyList<CeresEscapeVramTransferDefinition> records =
            CeresEscapeVramTransferDefinitionsTooling.All;
        AssertEqual(19, records.Count, "Ceres transfer metadata record count");
        foreach (CeresEscapeVramTransferDefinition record in records)
        {
            int address = 0xa60000 | record.Pointer;
            AssertEqual(record.ByteCount, CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                $"Ceres transfer ${record.Pointer:X4} byte count");
            AssertEqual(SnesAddressMath.OffsetInBank(record.SourceAddress), CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)),
                $"Ceres transfer ${record.Pointer:X4} source offset");
            AssertEqual(SnesAddressMath.BankNumber(record.SourceAddress), rom.ReadCartridgeByte(address + 4),
                $"Ceres transfer ${record.Pointer:X4} source bank");
            AssertEqual(record.DestinationWord, CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 5)),
                $"Ceres transfer ${record.Pointer:X4} VRAM destination");
            AssertTrue(CeresEscapeVramTransferDefinitions.TryGet(record.Pointer,
                    out var selected) && selected == record,
                $"Ceres transfer ${record.Pointer:X4} lookup");
        }
        foreach (ushort terminator in new ushort[] { 0xc3d4, 0xc4fc, 0xc536 })
        {
            AssertEqual(0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa60000 | terminator)),
                $"Ceres transfer ${terminator:X4} native terminator");
            AssertTrue(CeresEscapeVramTransferDefinitions.IsTerminator(terminator),
                $"Ceres transfer ${terminator:X4} compiled terminator");
        }
        AssertTrue(!CeresEscapeVramTransferDefinitions.TryGet(0xc4ca, out _) &&
                   !CeresEscapeVramTransferDefinitions.IsTerminator(0xc4ca),
            "non-record Ceres transfer pointer is not classified");

        using var timerPng = new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom),
            writable: false);
        RoomCharacterAtlas[] escapePages = ImportCeresEscapeTilePages(rom);
        CeresEscapeOverlayTilemapCatalog overlay =
            CeresEscapeOverlayTilemapCatalog.Load(new MemoryStream(
                CeresEscapeOverlayTilemapFiles.Extract(rom), writable: false));
        var japaneseOverlay = new RecordedBg1Overlay();
        var system = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: japaneseOverlay.Services())
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), AreaArtwork = new AreaArtworkCatalog { CeresEscapeTiles = new CeresEscapeTileArtwork(escapePages), CeresEscapeOverlayTilemaps = overlay },
            BossArtworkBinding = { EscapeTimerArtwork = EscapeTimerTileAtlas.Load(timerPng) },
        };
        // Each typed list must queue exactly the ROM-checked records from its native list start to its terminator.
        foreach ((ushort start, CeresEscapeTransferCursor cursor) in new[]
                 {
                     new VerifyCeresEscapeVramTransferDefinitionsCase(CeresEscapeVramTransferDefinitions.TimerSprites, CeresEscapeTransferCursor.TimerSprites),
                     new VerifyCeresEscapeVramTransferDefinitionsCase(CeresEscapeVramTransferDefinitions.TimerBackgrounds, CeresEscapeTransferCursor.TimerBackgrounds),
                 })
        {
            var state = new RidleyEnemyState { CeresEscapeTransfers = cursor };
            var queue = new VramWriteQueue();
            CeresEscapeVramTransferDefinition[] list = records
                .SkipWhile(record => record.Pointer != start)
                .TakeWhile(record => start == CeresEscapeVramTransferDefinitions.TimerSprites
                    ? record.Pointer < 0xc4fc : record.Pointer < 0xc536)
                .ToArray();
            AssertTrue(list.Length is 7 or 8, "Ceres installed transfer list length");
            for (int index = 0; index < list.Length; index++)
            {
                bool finished = system.CeresRidleyBehavior.Getaway.QueueNextCeresEscapeTransfer(state, queue);
                CeresEscapeVramTransferDefinition expected = list[index];
                AssertEqual(index == list.Length - 1, finished,
                    $"Ceres transfer ${expected.Pointer:X4} list completion");
                AssertEqual(index + 1, queue.Entries.Count,
                    $"Ceres transfer ${expected.Pointer:X4} one record per call");
                VramWriteEntry entry = queue.Entries[index];
                AssertEqual(expected.ByteCount, entry.SizeInBytes,
                    $"Ceres transfer ${expected.Pointer:X4} queued byte count");
                AssertEqual(expected.DestinationWord, entry.Destination.Encoded,
                    $"Ceres transfer ${expected.Pointer:X4} queued destination");
                if (expected.Pointer is 0xc4cb or 0xc4d2)
                {
                    AssertEqual(expected.Pointer == 0xc4cb
                            ? VramAssetId.EscapeTimerFirstTiles
                            : VramAssetId.EscapeTimerSecondTiles,
                        entry.AssetId,
                        $"Ceres transfer ${expected.Pointer:X4} editable timer page");
                }
                else if (CeresEscapeTileCharacters.ForTransfer(expected.SourceAddress, expected.ByteCount) is { } characters)
                    AssertEqual<ObjCharacterSource?>(characters, entry.OwnedSource,
                        $"Ceres transfer ${expected.Pointer:X4} names its installed character page");
                else
                    AssertEqual(expected.SourceAddress, entry.SourceAddress,
                        $"Ceres transfer ${expected.Pointer:X4} queued source");
            }
        }

        // The Japanese subtitle rows are written over BG1 at exactly the four compiled records' destinations.
        system.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay(CeresEscapeVramTransferDefinitions.JapaneseOverlay);
        var written = new bool[RoomTilemapOverlay.WordCount];
        for (int index = 0; index < 4; index++)
        {
            CeresEscapeVramTransferDefinition expected = records[index];
            int first = expected.DestinationWord - GameplayVramLayout.Bg1TilemapWord;
            for (int word = 0; word < expected.ByteCount / sizeof(ushort); word++)
                written[first + word] = true;
        }
        for (int word = 0; word < written.Length; word++)
            AssertEqual(written[word], japaneseOverlay.Overlay.Words[word] != RoomTilemapOverlay.Unwritten,
                $"Ceres Japanese overlay writes BG1 word {word} only inside its compiled records");
        system.BossArtworkBinding.EscapeTimerArtwork = null;
        try
        {
            system.CeresRidleyBehavior.Getaway.QueueNextCeresEscapeTransfer(new RidleyEnemyState
                    {
                        CeresEscapeTransfers = CeresEscapeTransferCursor.TimerSprites,
                    }, new VramWriteQueue());
            throw new InvalidOperationException(
                "Installed Ceres escape accepted missing timer PNG artwork.");
        }
        catch (InvalidDataException)
        {
            // Installed presentation loss is explicit, never a ROM-art fallback.
        }
        Console.WriteLine(
            "Ceres escape transfers: 19 native records and three terminators match; " +
            "both installed timer lists and Japanese overlay execute with descriptor reads blocked.");
    }


    /// <summary>Imports indexed character pages needed to run the installed Ceres escape transfer path.</summary>
    [CpuBusBoundary("exact bus test: imports the Ceres escape tile pages from their retail ROM source addresses")]
    private static RoomCharacterAtlas[] ImportCeresEscapeTilePages(ISnesAddressSpace rom) =>
        CeresEscapeTileArtworkDefinitions.All.Select(page =>
            RoomCharacterAtlas.Load(new MemoryStream(
                IndexedTilePageExtractor.Extract(rom, page.SourceAddress,
                    page.ByteCount, page.FileName), writable: false),
                page.ByteCount)).ToArray();

    /// <summary>Runs the focused Ceres escape descriptor check against the retail ROM.</summary>
    [VerificationCommand("--ceres-escape-transfers")]
    internal static int CeresEscapeTransfersCommand()
    {
        Suite(nameof(VerifyCeresEscapeVramTransferDefinitions), () => VerifyCeresEscapeVramTransferDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
