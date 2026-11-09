using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Ceres;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks Ceres escape VRAM descriptors and installed timer and Japanese overlay transfers.</summary>
internal static class CeresEscapeVramTransferDefinitionsVerification
{
    /// <summary>Compares nineteen native records and three terminators, then queues production transfers without descriptor reads.</summary>
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

        RoomCharacterAtlas[] escapePages = ImportCeresEscapeTilePages(rom);
        CeresEscapeOverlays overlay =
            CeresEscapeOverlays.Load(new MemoryStream(
                CeresEscapeOverlayTilemapFiles.Extract(rom), writable: false));
        var system = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance,
            services: new SuperMetroid.Verification.Enemies.StandaloneRoomEnemyServices(() => throw new InvalidOperationException("The warning fixture generates no random numbers.")))
        {
            TileArtwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>(), Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>() }), 
        };
        // Bringing up the warning takes one update for each ROM-checked record of the timer-sprite list, the warning look
        // showing on the last (#1538); where each record's page lands is the import's, which the Ceres escape tile check
        // compares with the cartridge's.
        TileTransferDefinition[] spriteList = [.. records
            .SkipWhile(record => record.Pointer != CeresEscapeVramTransferDefinitions.TimerSprites)
            .TakeWhile(record => record.Pointer < 0xc4fc)];
        AssertEqual(7, spriteList.Length, "Ceres timer-sprite list length");
        AssertEqual(spriteList.Length, CeresEscapeReadyingDefinitions.WarningReadyingDuration, "bringing up the warning takes one update a timer-sprite record");
        int timerPages = Enum.GetValues<EscapeTimerTilePage>().Length;
        var state = new RidleyEnemyState { CeresEscapeReadying = CeresEscapeReadying.Warning };
        for (int index = 0; index < spriteList.Length; index++)
        {
            bool finished = system.CeresRidleyBehavior.Getaway.ReadyCeresEscapeWarning(state);
            TileTransferDefinition expected = spriteList[index];
            AssertEqual(index == spriteList.Length - 1, finished, $"Ceres transfer ${expected.Pointer:X4} list completion");
            AssertEqual(index == spriteList.Length - 1, system.Presentation.Looks.Shows(CeresEscapeReadyingDefinitions.WarningLook),
                $"Ceres transfer ${expected.Pointer:X4}'s update shows the warning look only last");
            AssertTrue(expected.Pointer is 0xc4cb or 0xc4d2 ? index < timerPages
                    : CeresWarningTextPageConstants.ForTransfer(expected.SourceAddress, expected.ByteCount) == index - timerPages,
                $"Ceres transfer ${expected.Pointer:X4} is the copy of its page: the timer's two, then the warning's in order");
        }
        // The background list's records land in characters the Ridley arena does not show: it takes one update for each.
        TileTransferDefinition[] backgroundList = [.. records
            .SkipWhile(record => record.Pointer != CeresEscapeVramTransferDefinitions.TimerBackgrounds)];
        AssertEqual(8, backgroundList.Length, "Ceres timer-background list length");
        AssertEqual(backgroundList.Length, CeresEscapeReadyingDefinitions.BackgroundReadyingDuration, "readying the backgrounds takes one update a timer-background record");
        state.CeresEscapeReadying = CeresEscapeReadying.Backgrounds;
        for (int index = 0; index < backgroundList.Length; index++)
            AssertEqual(index == backgroundList.Length - 1, system.CeresRidleyBehavior.Getaway.ReadyCeresEscapeWarning(state),
                $"Ceres transfer ${backgroundList[index].Pointer:X4} update completes the list only last");
        AssertTrue(system.Presentation.Looks.Shows(CeresEscapeReadyingDefinitions.WarningLook),
            "readying the backgrounds shows nothing new over the sprites");

        // The Japanese subtitle rows are written over BG1 at exactly the four compiled records' destinations.
        system.CeresRidleyBehavior.Getaway.DrawCeresJapaneseOverlay();
        var written = new bool[TwoScreenTilemapCells.Count];
        for (int index = 0; index < 4; index++)
        {
            TileTransferDefinition expected = records[index];
            int first = expected.DestinationWord - GameplayVramLayout.Bg1TilemapWord;
            for (int word = 0; word < expected.ByteCount / sizeof(ushort); word++)
                written[first + word] = true;
        }
        ImmutableArray<int> shownWords = system.RidleyEncounter.CeresWarning.WordsIn(overlay);
        for (int word = 0; word < written.Length; word++)
            AssertEqual(written[word], shownWords[word] != RoomWrittenOverlay.Unwritten,
                $"Ceres Japanese overlay writes BG1 word {word} only inside its compiled records");
        Console.WriteLine(
            "Ceres escape transfers: 19 native records and three terminators match; " +
            "both installed timer lists show their pages and the Japanese overlay is written with descriptor reads blocked.");
    }


    /// <summary>Imports indexed character pages needed to run the installed Ceres escape transfer path.</summary>
    private static RoomCharacterAtlas[] ImportCeresEscapeTilePages(ISnesAddressSpace rom) =>
        CeresEscapeTileArtworkDefinitions.All.Select((page, index) =>
            RoomCharacterAtlasPng.Read(new MemoryStream(
                IndexedTilePageExtractor.Extract(rom, CeresEscapeNativeSources.PageSource(index),
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
