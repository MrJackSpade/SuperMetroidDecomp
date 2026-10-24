using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
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
        IReadOnlyList<TileTransferDefinition> records =
            CeresEscapeVramTransferDefinitions.All;
        AssertEqual(19, records.Count, "Ceres transfer metadata record count");
        foreach (TileTransferDefinition record in records)
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
        // The timer-sprite list shows exactly the ROM-checked records' characters over the OBJ characters, one an update.
        TileTransferDefinition[] spriteList = [.. records
            .SkipWhile(record => record.Pointer != CeresEscapeVramTransferDefinitions.TimerSprites)
            .TakeWhile(record => record.Pointer < 0xc4fc)];
        AssertEqual(7, spriteList.Length, "Ceres timer-sprite list length");
        SpriteCharacters shown = system.Presentation.SpriteCharacters;
        var state = new RidleyEnemyState { CeresEscapePages = CeresEscapePageCursor.Objects };
        for (int index = 0; index < spriteList.Length; index++)
        {
            bool finished = system.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(state);
            TileTransferDefinition expected = spriteList[index];
            AssertEqual(index == spriteList.Length - 1, finished, $"Ceres transfer ${expected.Pointer:X4} list completion");
            AssertEqual(index + 1, shown.Uploads.Count, $"Ceres transfer ${expected.Pointer:X4} one page per update");
            SpriteCharacterPlacement page = shown.Uploads[index];
            AssertEqual(expected.ByteCount, page.ByteCount, $"Ceres transfer ${expected.Pointer:X4} page size");
            AssertEqual(SpriteCharacters.TableByte(VramDmaDestination.FromNative(expected.DestinationWord)), page.FirstByte,
                $"Ceres transfer ${expected.Pointer:X4} page's place among the OBJ characters");
            AssertEqual<ObjCharacterSource?>(expected.Pointer is 0xc4cb or 0xc4d2
                    ? new AssetObjCharacters(expected.Pointer == 0xc4cb ? VramAssetId.EscapeTimerFirstTiles : VramAssetId.EscapeTimerSecondTiles)
                    : CeresEscapeTileCharactersConstants.ForTransfer(expected.SourceAddress, expected.ByteCount),
                page.Source, $"Ceres transfer ${expected.Pointer:X4} names its installed page");
        }
        // The background list's records land in characters the Ridley arena does not show: it takes one update for each.
        TileTransferDefinition[] backgroundList = [.. records
            .SkipWhile(record => record.Pointer != CeresEscapeVramTransferDefinitions.TimerBackgrounds)];
        AssertEqual(8, backgroundList.Length, "Ceres timer-background list length");
        state.CeresEscapePages = CeresEscapePageCursor.Backgrounds;
        for (int index = 0; index < backgroundList.Length; index++)
            AssertEqual(index == backgroundList.Length - 1, system.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(state),
                $"Ceres transfer ${backgroundList[index].Pointer:X4} update completes the list only last");
        AssertEqual(spriteList.Length, shown.Uploads.Count, "the timer-background list shows nothing over the OBJ characters");

        // The Japanese subtitle rows are written over BG1 at exactly the four compiled records' destinations.
        system.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay();
        var written = new bool[RoomTilemapOverlay.WordCount];
        for (int index = 0; index < 4; index++)
        {
            TileTransferDefinition expected = records[index];
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
            system.CeresRidleyBehavior.Getaway.ShowNextCeresEscapePage(new RidleyEnemyState
            {
                CeresEscapePages = CeresEscapePageCursor.Objects,
            });
            throw new InvalidOperationException(
                "Installed Ceres escape accepted missing timer PNG artwork.");
        }
        catch (InvalidDataException)
        {
            // Installed presentation loss is explicit, never a ROM-art fallback.
        }
        Console.WriteLine(
            "Ceres escape transfers: 19 native records and three terminators match; " +
            "both installed timer lists show their pages and the Japanese overlay is written with descriptor reads blocked.");
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
