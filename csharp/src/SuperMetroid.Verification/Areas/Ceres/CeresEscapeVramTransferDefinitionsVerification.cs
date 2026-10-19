using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Ceres;
using SuperMetroid.Verification.Harness;

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
        var system = new RoomEnemySystem(new EnemySession())
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>(), CeresEscapeTiles = new CeresEscapeTileArtwork(escapePages), CeresEscapeOverlayTilemaps = overlay }),
            Presentation = { EscapeTimerArtwork = EscapeTimerTileAtlas.Load(timerPng) },
        };
        system.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        // Each typed list must queue exactly the ROM-checked records from its native list start to its terminator.
        foreach ((ushort start, CeresEscapeTransferCursor cursor) in new[]
                 {
                     (CeresEscapeVramTransferDefinitions.TimerSprites, CeresEscapeTransferCursor.TimerSprites),
                     (CeresEscapeVramTransferDefinitions.TimerBackgrounds, CeresEscapeTransferCursor.TimerBackgrounds),
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
                bool finished = (bool)system.CeresRidleyBehavior.QueueNextCeresEscapeTransfer(state, queue);
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
                else
                    AssertEqual(expected.SourceAddress, entry.SourceAddress,
                        $"Ceres transfer ${expected.Pointer:X4} queued source");
            }
        }

        var japaneseQueue = new VramWriteQueue();
        system.CeresRidleyBehavior.QueueCeresTransferList(CeresEscapeVramTransferDefinitions.JapaneseOverlay, japaneseQueue);
        AssertEqual(4, japaneseQueue.Entries.Count,
            "Ceres Japanese overlay queues four compiled transfers");
        for (int index = 0; index < 4; index++)
        {
            CeresEscapeVramTransferDefinition expected = records[index];
            VramWriteEntry actual = japaneseQueue.Entries[index];
            AssertEqual(expected.ByteCount, actual.SizeInBytes,
                $"Ceres Japanese transfer {index} byte count");
            AssertEqual(expected.SourceAddress, actual.SourceAddress,
                $"Ceres Japanese transfer {index} source");
            AssertEqual(expected.DestinationWord, actual.Destination.Encoded,
                $"Ceres Japanese transfer {index} destination");
        }
        system.Presentation.EscapeTimerArtwork = null;
        try
        {
            system.CeresRidleyBehavior.QueueNextCeresEscapeTransfer(new RidleyEnemyState
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
