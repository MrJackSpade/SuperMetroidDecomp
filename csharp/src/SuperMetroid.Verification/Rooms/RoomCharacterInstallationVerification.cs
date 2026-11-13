using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Rooms.RoomArtworkRenderParityVerification;
using static SuperMetroid.Verification.Rooms.RoomVisualLayoutsVerification;
using static SuperMetroid.Verification.Samus.Abilities.XrayRevealVisualInstallationVerification;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Verification.Rooms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Checks stock room characters, palettes, blocks, backgrounds, and sky artwork.</summary>
internal static class RoomCharacterInstallationVerification
{
    /// <summary>Stock import contract for room art in the shared installation, with a private copy of its background directory whose manifest is altered.</summary>
    internal static void VerifyRoomArtworkInstallation(GameInstallation installed, CartridgeImportAddressSpace bus, SelectedRoom landing)
    {
        AssertTrue(File.Exists(Path.Combine(installed.RoomCharacterDirectory,
                RoomCharacterArtworkFiles.ManifestFileName)),
            "room-character stock manifest is installed");
        AssertTrue(File.Exists(Path.Combine(installed.EnemyTileDirectory,
                EnemyTileArtworkFormat.ManifestFileName)),
            "enemy-tile stock manifest is installed");
        // The enemies' palettes only pre-colour their sheets: they are import data (#1637), which the installation never holds.
        AssertEqual(0, Directory.GetFiles(installed.EnemyTileDirectory, "*-colors.json").Length,
            "the installation holds no enemy, boss or area colour document");
        AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount,
            Directory.GetFiles(CartridgeImportContent.ImportData(GameInstallationLayout.EnemyTileDirectoryName), "enemy-????-colors.json").Length,
            "the import extracts every retail enemy palette into its import data");
        _ = installed.LoadEnemyTiles();
        // Room characters are the import's input (#1466) and import data no installation holds (#1636): they are read
        // from the cartridge-derived extraction, and rooms show pre-composed pictures, which a player edits in the
        // installed block sheets.
        RoomCharacterAtlases stock = FixtureRoomCharacters;
        LoadedRoomAssets native = LoadFixtureRoomAssets(landing);

        int ghostSource = RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters;
        byte[] nativeGhost = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            ghostSource, LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount);
        AssertTrue(stock.GetAt(ghostSource).Characters.Span.SequenceEqual(nativeGhost),
            "the extracted statue-ghost characters preserve every native character byte");
        // The cartridge's library background transfers, scanned once for the statue ghost's and Ceres's.
        IReadOnlyList<LibraryBackgroundNativeOperand> libraryTransfers = LibraryBackgroundSourceInventory.Scan(bus);
        LibraryBackgroundNativeOperand ghostTransfer = libraryTransfers
            .Single(entry => entry.SourceAddress == ghostSource);
        AssertEqual(LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount,
            (ghostTransfer.TransferByteCount ?? throw new InvalidOperationException("Verification requires ghostTransfer.TransferByteCount.")), "statue-ghost library upload length");
        AssertEqual(LibraryBackgroundOperands.DestinationWord(LibraryBackgroundDestination.StatueGhostSpriteCharacters),
            (ghostTransfer.VramDestination ?? throw new InvalidOperationException("Verification requires ghostTransfer.VramDestination.")), "statue-ghost library VRAM destination");
        var nativeGhostVram = new SnesVram();
        SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeGhostVram,
            ghostTransfer.ListPointer, activeDoorPointer: 0);
        SnesVram installedGhostVram = ShownVram(LibraryBackgroundProgramDefinitions.Get(ghostTransfer.ListPointer), new LibraryBackgroundArtwork { TilemapArt = InstalledFixtures.BackgroundTilemaps, CharacterArt = stock });
        // The ghost's characters reach sprite characters only the statue ghost would draw, and it draws nothing, so the
        // installed replay writes them nowhere (#1466); the check above compares the installed characters byte for byte. The rest
        // of the upload matches the native one with that range left as the fresh video memory holds it.
        nativeGhostVram.ExecuteQueuedAssetWrite(
            new byte[LibraryBackgroundStagingDefinitions.TourianStatueGhostByteCount],
            LibraryBackgroundOperands.DestinationWord(LibraryBackgroundDestination.StatueGhostSpriteCharacters));
        AssertTrue(nativeGhostVram.ImageEquals(installedGhostVram),
            "installed statue-ghost library upload matches the native one apart from the characters only the ghost would draw");

        // The room palettes are import data (#1637): the installation holds none, and the import reads them where it extracted them.
        AssertTrue(!Directory.Exists(Path.Combine(installed.ContentDirectory, GameInstallationLayout.RoomPaletteDirectoryName)),
            "the installation holds no room palette");
        _ = RoomStaticPaletteArtworkFiles.LoadForImport(CartridgeImportContent.ImportData(GameInstallationLayout.RoomPaletteDirectoryName));

        // The stock block compositions are extraction input (#1431): the block sheets are composed from them and each
        // tileset records the appearance inputs its blocks depend on beside its sheets (#1466). Rooms load no composition,
        // so the compositions are import data (#1638): the installation holds none, and the import reads them where it
        // extracted them.
        AssertTrue(!Directory.Exists(Path.Combine(installed.ContentDirectory, GameInstallationLayout.RoomMetatileDirectoryName)),
            "the installation holds no block composition");
        RoomMetatileCatalog stockBlocks = RoomMetatileArtworkFiles.LoadForImport(CartridgeImportContent.ImportData(GameInstallationLayout.RoomMetatileDirectoryName));
        InstalledRoomArtwork installedSheets = installed.LoadRoomBlockSheets();
        AssertTrue(File.Exists(Path.Combine(installed.RoomBlockSheetDirectory, RoomBlockAppearanceFiles.FileNameOf(native.Tileset))),
            "the Landing Site tileset's block appearance record is installed beside its block sheets");
        AssertEqual((stockBlocks.Cre.BlockTiles.Length + stockBlocks.Get(native.Tileset.Blocks).BlockTiles.Length) / RoomMetatileFormat.TilesPerBlock,
            installedSheets.Get(native.Tileset).Inputs.Count,
            "the installed appearance record covers every block a Landing Site room loads");

        // The background tilemaps and sky pages only the import reads, to picture the backgrounds and the sky, are import
        // data (#1638): the installation holds none, and the import reads them where it extracted them.
        AssertTrue(!Directory.Exists(Path.Combine(installed.ContentDirectory, GameInstallationLayout.RoomBackgroundTilemapDirectoryName)),
            "the installation holds no background tilemap or sky page");
        string backgroundImportData = CartridgeImportContent.ImportData(GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
        RoomBackgroundTilemaps stockBackgrounds = InstalledFixtures.BackgroundTilemaps;
        // A manifest entry that names no background tilemap (#1414) is rejected. The extraction is shared and never
        // changes, so the altered manifest is written over a private copy of the extracted background directory.
        using var backgroundScratch = new TestScratchDirectory("room-background-manifest-rejection");
        Directory.CreateDirectory(backgroundScratch.Root);
        foreach (string file in Directory.GetFiles(backgroundImportData))
            File.Copy(file, Path.Combine(backgroundScratch.Root, Path.GetFileName(file)));
        string backgroundManifestPath = Path.Combine(backgroundScratch.Root, RoomBackgroundTilemapArtworkFiles.ManifestFileName);
        byte[] originalBackgroundManifest = File.ReadAllBytes(backgroundManifestPath);
        const string substitutedTilemap = "NotABackgroundTilemap";
        AssertTrue(!Enum.TryParse(substitutedTilemap, out RoomBackgroundTilemap _),
            "manifest substitution fixture names no background tilemap");
        JsonNode alteredManifest = JsonNode.Parse(originalBackgroundManifest)
            ?? throw new InvalidDataException("Extracted background manifest is empty.");
        JsonObject manifestEntries = alteredManifest.Required("entries").AsObject();
        string stockName = RoomBackgroundTilemapArtworkFiles.FileName(Enum.GetValues<RoomBackgroundTilemap>()[0]);
        string substitutedName = "room-background-not-a-background-tilemap.json";
        JsonNode alteredEntry = manifestEntries.Required(stockName).DeepClone();
        alteredEntry["tilemap"] = substitutedTilemap;
        manifestEntries.Remove(stockName);
        manifestEntries[substitutedName] = alteredEntry;
        File.WriteAllText(backgroundManifestPath, alteredManifest.ToJsonString());
        try
        {
            _ = RoomBackgroundTilemapArtworkFiles.LoadForImport(backgroundScratch.Root);
            throw new InvalidOperationException(
                "Substituted background tilemap was accepted by the import's loader.");
        }
        catch (InvalidDataException error)
        {
            AssertTrue(error.Message.Contains("invalid tilemap entry",
                    StringComparison.Ordinal),
                "substituted background tilemap fails the named identity check");
        }
        SelectedRoom ceres = SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(bus, 0xdf8d);
        int backgroundSource = libraryTransfers
            .Single(source => source.ListPointer == ceres.State.BackgroundDataPointer &&
                source.Command == LibraryBackgroundCommand.Decompress).SourceAddress;
        string backgroundName = RoomBackgroundTilemapArtworkFiles.FileName(
            LibraryBackgroundOperandDecoding.DecompressedSource(backgroundSource).Tilemap);
        var nativeBackgroundVram = new SnesVram();
        var stockBackgroundVram = new SnesVram();
        SuperMetroid.TestSupport.VerificationImporters.LibraryBackgroundProgramImporter.ExecuteReference(bus, nativeBackgroundVram,
            ceres.State.BackgroundDataPointer, activeDoorPointer: 0);
        LibraryBackgroundReplay.Execute(stockBackgroundVram, LibraryBackgroundProgramDefinitions.Get(ceres.State.BackgroundDataPointer), activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: new LibraryBackgroundArtwork { TilemapArt = stockBackgrounds });
        AssertTrue(nativeBackgroundVram.ImageEquals(stockBackgroundVram),
            "extracted Ceres background tilemap retains native VRAM words");

        RoomSkyTilemaps stockSky = InstalledFixtures.SkyTilemaps;
        // The landing-cutscene door's command-E record selects the uploaded sky page.
        LibraryBackgroundInstruction landingSky = LibraryBackgroundProgramDefinitions.GetDoorTransfer(
            unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
            LandingSiteRomData.LandingCutsceneDoorPointer);
        int skyPage = landingSky.Source is LibraryBackgroundSource.SkyPage sky ? sky.Page
            : throw new InvalidOperationException($"The landing-cutscene door transfer reads {landingSky.Source}, not a sky page.");
        string skyName = RoomSkyTilemapFormat.FileName(skyPage);
        string skyStockPath = Path.Combine(backgroundImportData, skyName);
        AssertTrue(File.Exists(skyStockPath), "selected scrolling-sky page is extracted into the import data");
        byte[] nativeSky = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            LibraryBackgroundOperands.SkyPageAddress(skyPage), landingSky.ByteCount);
        AssertTrue(BackgroundTileWordBytes.Of(stockSky.Page(skyPage, landingSky.ByteCount / sizeof(ushort)).Span).AsSpan().SequenceEqual(nativeSky),
            "extracted scrolling-sky page retains native door-selected bytes");

        Console.WriteLine("  Room artwork installation: statue ghost, RGB5, block, BG and sky stock import verified.");
    }

    /// <summary>
    /// Runs a library background program with installed character art and returns the VRAM it leaves: the statue ghost's
    /// characters are written nowhere, as only the statue ghost would draw them and it draws nothing.
    /// </summary>
    private static SnesVram ShownVram(LibraryBackgroundProgram program, LibraryBackgroundArtwork artwork)
    {
        var vram = new SnesVram();
        LibraryBackgroundReplay.Execute(vram, program, activeDoor: FixtureRoomHeaders.UnmatchedDoor, artwork: artwork);
        return vram;
    }

    /// <summary>The retail cartridge the room-art installation contract reads, loaded once for every part.</summary>
    private static readonly Lazy<CartridgeImportAddressSpace> roomArtworkBus =
        new(() => CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath));

    /// <summary>The landing site's room header, imported once from <see cref="roomArtworkBus"/>.</summary>
    private static readonly Lazy<SelectedRoom> landingSite =
        new(() => SuperMetroid.TestSupport.RoomHeaderImport.CartridgeRoomHeaderImporter.Load(roomArtworkBus.Value, 0x91f8));

    /// <summary>
    /// The room-art installation contract's parts, each a suite of its own over the shared repository installation: the
    /// parts only read it, and the one that alters a manifest does so in its own copy of that directory.
    /// </summary>
    internal static RegisteredSuite[] RoomArtworkInstallationSuites() =>
    [
        new(nameof(VerifyRoomArtworkRenderParity), () => VerifyRoomArtworkRenderParity(RepositoryInstallation.Installation, roomArtworkBus.Value)),
        new(nameof(VerifyRoomVisualLayouts), () => VerifyRoomVisualLayouts(RepositoryInstallation.Installation, roomArtworkBus.Value, landingSite.Value)),
        new(nameof(VerifyXrayRevealVisualInstallation), () => VerifyXrayRevealVisualInstallation(RepositoryInstallation.Installation)),
        new(nameof(VerifyRoomArtworkInstallation), () => VerifyRoomArtworkInstallation(RepositoryInstallation.Installation, roomArtworkBus.Value, landingSite.Value)),
    ];

    /// <summary>Runs every part of the room-art installation contract (<see cref="RoomArtworkInstallationSuites"/>).</summary>
    [VerificationCommand("--room-artwork-installation")]
    internal static int RoomArtworkInstallationCommand()
    {
        foreach (RegisteredSuite suite in RoomArtworkInstallationSuites())
            Suite(suite.Name, suite.Body);
        return 0;
    }
}
