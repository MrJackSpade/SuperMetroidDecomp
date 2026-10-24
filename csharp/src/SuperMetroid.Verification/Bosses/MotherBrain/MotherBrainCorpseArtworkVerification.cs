using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;

/// <summary>Checks Mother Brain corpse PNG import, VRAM transfers, and rotting buffer setup against native graphics.</summary>
internal static class MotherBrainCorpseArtworkVerification
{
    /// <summary>Extracts the retail corpse sheet and runs its installation and edit checks from a validated stock catalog.</summary>
    internal static void VerifyMotherBrainCorpseStockArtwork()
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        EnemyTileArtworkFiles.Extract(source, temporary.Root, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(temporary.Root);
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(temporary.Root, EnemyTileArtworkFiles.Load(temporary.Root, null)));
    }

    /// <summary>Reads the banked native corpse character sheet solely as a verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain corpse character sheet")]
    private static byte[] ReadNativeMotherBrainCorpseArtwork(CartridgeImportAddressSpace rom) =>
        RomDataReader.ReadFixedBank(rom,
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);

    /// <summary>Compares six installed VRAM pages and corpse staging with native bytes, then checks one PNG pixel edit and reload.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyInstalledMotherBrainCorpseArtwork(
        string directory, InstalledEnemyArtwork stock)
    {
        RoomCharacterAtlas artwork = stock.Bosses.MotherBrainCorpse ??
            throw new InvalidDataException("Extracted Mother Brain corpse PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = ReadNativeMotherBrainCorpseArtwork(rom);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain corpse PNG preserves all native source bytes");

        // Only the oracle/import above receives cartridge bytes. Production transfers and
        // staging run against the same cartridge-incapable memory type as installed games.
        byte[] stockCharacters = TransferMotherBrainCorpsePages(stock);
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
        {
            int sourceOffset = checked((int)MotherBrainCorpseArtworkDefinitions.PageSource(page) -
                MotherBrainCorpseArtworkDefinitions.SourceAddress);
            int shownOffset = MotherBrainCorpseArtworkDefinitions.PageFirstCharacter(page) * RoomCharacterAtlasFormat.BytesPerTile;
            AssertTrue(stockCharacters.AsSpan(shownOffset,
                    MotherBrainCorpseArtworkDefinitions.PageByteCount)
                .SequenceEqual(native.AsSpan(sourceOffset,
                    MotherBrainCorpseArtworkDefinitions.PageByteCount)),
                $"installed Mother Brain corpse page {page} shows the native page's characters");
        }

        var nativeStaging = new EnemyStagingSurfaces();
        var installedStaging = new EnemyStagingSurfaces();
        // Independent $A9:E08B oracle: copy each right-hand frame's seven-tile row,
        // omitting the transparent last tile in the first four rows. No runtime fallback.
        StagingByteView nativeTiles = nativeStaging.MotherBrainCorpseTiles;
        for (int row = 0; row < 6; row++)
        {
            ReadOnlySpan<byte> rowBytes = native.AsSpan(row * 0x200 + 0xc0, row < 4 ? 0xc0 : 0xe0);
            for (int offset = 0; offset < rowBytes.Length; offset++)
                nativeTiles[row * 0xe0 + offset] = rowBytes[offset];
        }
        CorpseRotTable nativeRotTable = nativeStaging.MotherBrainRotTable;
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
        {
            nativeRotTable.SetYOffset(index, (ushort)(47 - index));
            nativeRotTable.SetDelay(index, (ushort)(index * 2));
        }
        // The runtime builds only the rot table; the extraction stages the corpse frame the table rots (#1392).
        new MotherBrainCorpseRottingState().Initialize(installedStaging);
        AssertMotherBrainRotTableParity(nativeStaging, installedStaging);
        CorpseStaging installedCorpse = CorpseStaging.MotherBrain(artwork.Transfer.Span);
        AssertMotherBrainCorpseStagingParity(nativeStaging, installedCorpse);

        // The phase-three state machine owns a separate corpse processor. Verify
        // its explicit setup route as well as the room-entry processor above.
        var sequenceStaging = new EnemyStagingSurfaces();
        new MotherBrainRainbowBeamAttackSequence(sequenceStaging).ThirdPhaseDeath.InitializeCorpseRotting();
        AssertMotherBrainRotTableParity(nativeStaging, sequenceStaging);

        string fileName = MotherBrainCorpseArtworkDefinitions.FileName;
        using var input = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        // The first native copy starts $C0 bytes into this sheet: tile six,
        // first pixel. That pixel becomes the high bit of $7E:9000.
        image.Pixels[6 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "mother-brain-corpse-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        byte[] editedCharacters = TransferMotherBrainCorpsePages(edited);
        int editedVramOffset = MotherBrainCorpseArtworkDefinitions.PageFirstCharacter(0) * RoomCharacterAtlasFormat.BytesPerTile +
            6 * RoomCharacterAtlasFormat.BytesPerTile;
        AssertEqual((byte)(stockCharacters[editedVramOffset] ^ 0x80),
            editedCharacters[editedVramOffset],
            "Mother Brain corpse PNG edit changes live sprite VRAM");
        byte[] installedTiles = StagedCorpseRows(installedCorpse);
        byte[] editedTiles = StagedCorpseRows(CorpseStaging.MotherBrain((edited.Bosses.MotherBrainCorpse
            ?? throw new InvalidOperationException("Verification requires edited.MotherBrainCorpse.")).Transfer.Span));
        AssertEqual((byte)(installedTiles[0] ^ 0x80),
            editedTiles[0],
            "Mother Brain corpse PNG pixel edit changes the corpse staging");
        for (int offset = 1; offset < installedTiles.Length; offset++)
            AssertEqual(installedTiles[offset],
                editedTiles[offset],
                "Mother Brain corpse PNG edit leaves other staged pixels unchanged");
        RoomCharacterAtlas reloaded = (EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .Bosses.MotherBrainCorpse ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(directory, overrideDirectory) .MotherBrainCorpse."));
        AssertEqual(editedTiles[0], StagedCorpseRows(CorpseStaging.MotherBrain(reloaded.Transfer.Span))[0],
            "Mother Brain corpse override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain corpse override is rejected explicitly");

        Console.WriteLine("  Mother Brain corpse art: indexed PNG import parity, six RAM-only sprite VRAM pages, independent staging oracle, room/sequence staging, edit/reload and invalid override pass.");
    }

    /// <summary>Shows every corpse page through the production encounter and returns the sprite characters they show.</summary>
    private static byte[] TransferMotherBrainCorpsePages(
        InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
            enemies.MotherBrainEncounter.ShowSpritePage(MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.Corpse, page));
        return new ObjCharacterTablePage().Page(enemies.Room.Session.ObjCharacters, new EnemyTileTransfers(artwork))
            .Characters[..ObjCharacterUploads.TableByteCount].ToArray();
    }

    /// <summary>The six staged rows of the corpse frame, as the extraction stages them, in heap order.</summary>
    private static byte[] StagedCorpseRows(CorpseStaging staged) =>
        [.. Enumerable.Range(0, MotherBrainCorpseArtworkDefinitions.RowCount).SelectMany(row => staged.Upload(new CorpseRotRow(row,
            EnemyStagingDefinitions.RottingHeapNativeAddress + row * MotherBrainCorpseArtworkDefinitions.RowBytes,
            MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row), 0)))];

    /// <summary>Compares the extraction's staged corpse rows with an independent native copy.</summary>
    private static void AssertMotherBrainCorpseStagingParity(EnemyStagingSurfaces expected, CorpseStaging actual)
    {
        StagingByteView expectedTiles = expected.MotherBrainCorpseTiles;
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
        {
            byte[] staged = actual.Upload(new CorpseRotRow(row, EnemyStagingDefinitions.RottingHeapNativeAddress + row * MotherBrainCorpseArtworkDefinitions.RowBytes,
                MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row), 0));
            for (int offset = 0; offset < staged.Length; offset++)
                AssertEqual(expectedTiles[row * MotherBrainCorpseArtworkDefinitions.RowBytes + offset], staged[offset],
                    "installed Mother Brain corpse staging agrees with cartridge");
        }
    }

    /// <summary>Compares all rotting-table entries with an independent native copy.</summary>
    private static void AssertMotherBrainRotTableParity(EnemyStagingSurfaces expected, EnemyStagingSurfaces actual)
    {
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
            AssertEqual(MotherBrainCorpseRottingState.ReadEntry(expected, index),
                MotherBrainCorpseRottingState.ReadEntry(actual, index), "installed native rot-table initialization");
    }

    /// <summary>Runs the installed Mother Brain corpse artwork verification command.</summary>
    [VerificationCommand("--mother-brain-corpse-stock-artwork")]
    internal static int MotherBrainCorpseStockArtworkCommand()
    {
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
        return 0;
    }
}
