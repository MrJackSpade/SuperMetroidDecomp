using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>Checks installed Mother Brain sprite pages, the native leg-loading records and the page selection bounds.</summary>
internal static class MotherBrainSpecialSpriteArtworkVerification
{
    /// <summary>
    /// Refuses a page none of Mother Brain's sheets has (#1406): the encounter selects pages by sheet and number, so a
    /// selection past either end of a sheet fails where it is made.
    /// </summary>
    internal static void VerifyMotherBrainSpritePageBounds()
    {
        foreach (MotherBrainSpritePageSheet sheet in Enum.GetValues<MotherBrainSpritePageSheet>())
        {
            int pages = MotherBrainSpritePage.PageCount(sheet);
            AssertEqual(pages - 1, MotherBrainSpritePage.Of(sheet, pages - 1).Page, $"{sheet}'s last page is selectable");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainSpritePage.Of(sheet, pages), $"{sheet} has no page past its last");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainSpritePage.Of(sheet, -1), $"{sheet} has no page before its first");
        }
    }

    /// <summary>Compares installed legs, Baby, attack and exploded-door sheets with native records and guarded VRAM uploads.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain special sprite sheets and Baby tile records")]
    internal static void VerifyInstalledMotherBrainSpecialSpriteArtwork(
        string directory, InstalledEnemyArtwork stock)
    {
        MotherBrainSpecialSpriteArtworkCatalog installed =
            stock.Bosses.MotherBrainSpecialSprites ?? throw new InvalidDataException(
                "Extracted Mother Brain special sprite PNGs were not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                sheet.SourceAddress, sheet.ByteCount);
            AssertTrue(installed.Get(sheet.SourceAddress).Characters.Span.SequenceEqual(native),
                $"installed {sheet.FileName} preserves native characters");
            SnesVram installedVram = ShowMotherBrainSpecialPages(stock,
                sheet);
            SnesVram cartridgeVram = ReferenceMotherBrainSpecialPages(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), sheet);
            for (int page = 0; page < sheet.PageCount; page++)
            {
                int sourceOffset = page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                int destinationOffset = ShownByte(sheet.PageFirstCharacter(page));
                int count = MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(native.AsSpan(sourceOffset, count)),
                    $"installed {sheet.FileName} page {page} matches native source");
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, count)),
                    $"installed {sheet.FileName} page {page} matches the independent cartridge transfer");
            }

            string filePath = Path.Combine(directory, sheet.FileName);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(sheet.ByteCount);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            using var input = new MemoryStream(File.ReadAllBytes(filePath));
            IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory,
                "mother-brain-special-" + sheet.SourceAddress.ToString("X6"));
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, sheet.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            SnesVram editedVram = ShowMotherBrainSpecialPages(edited,
                sheet);
            int firstDestination = ShownByte(sheet.FirstCharacter);
            AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
                editedVram.ReadByte(firstDestination),
                $"{sheet.FileName} PNG edit changes live OBJ VRAM");
            AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)
                .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)),
                $"{sheet.FileName} edit leaves neighboring bytes unchanged");
            RoomCharacterAtlas reloaded = (EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .Bosses.MotherBrainSpecialSprites ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(directory, overrideDirectory) .MotherBrainSpecialSprites.")).Get(sheet.SourceAddress);
            AssertTrue(reloaded.Characters.Span.SequenceEqual(
                    (edited.Bosses.MotherBrainSpecialSprites ?? throw new InvalidOperationException("Verification requires edited.MotherBrainSpecialSprites.")).Get(sheet.SourceAddress).Characters.Span),
                $"{sheet.FileName} override survives catalog reload");

            File.WriteAllBytes(overridePath, [0]);
            AssertThrows<InvalidDataException>(
                () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                $"malformed {sheet.FileName} override fails explicitly");
        }

        // Unlike the other two lists, Baby loading reads its four native records
        // from $A9:8FE5. Check every compiled sheet coordinate against that list.
        MotherBrainSpecialSpriteSheetDefinition baby =
            MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroid;
        for (int page = 0; page < baby.PageCount; page++)
        {
            int record = MotherBrainBabyPageListConstants.BabyTileList +
                page * MotherBrainBabyPageListConstants.RecordSize;
            AssertEqual(MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                $"Baby tile record {page} size");
            uint source = (uint)(int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            AssertEqual((uint)(baby.SourceAddress +
                page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount), source,
                $"Baby tile record {page} source");
            AssertEqual(ShownByte(baby.PageFirstCharacter(page)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5) * 2,
                $"Baby page {page} shows where the cartridge's record places it");
            RuntimeCharacterUpload shown = PlacedUploads.Of(MotherBrainCharacterUploads.Of(MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.BabyMetroid, page)));
            AssertEqual(ShownByte(baby.PageFirstCharacter(page)) - ObjectCharactersByte, shown.FirstByte,
                $"Baby page {page} selection shows at its record's characters");
            AssertEqual((int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record), shown.ByteCount,
                $"Baby page {page} selection shows its record's bytes");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.BabyMetroid, baby.PageCount),
            "the Baby Metroid has no page past the terminator");
        Suite(nameof(VerifyMotherBrainLegLoadPages), () => VerifyMotherBrainLegLoadPages(stock, rom));
        Suite(nameof(VerifyMotherBrainEscapeTimerPages), () => VerifyMotherBrainEscapeTimerPages(stock, rom));
        Console.WriteLine(
            "  Mother Brain special sprites: legs, Baby, attack and exploded-door pages match cartridge records, guarded live uploads, PNG edits, reload, invalid override and strict installed-source boundary pass.");
    }

    /// <summary>
    /// Checks every page the ascent shows against the native leg-loading list, one per call: where each shows, its bytes,
    /// the call that completes the list and the produced characters.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain leg-loading records and pixels")]
    private static void VerifyMotherBrainLegLoadPages(
        InstalledEnemyArtwork stock, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas };
        SnesVram ObjVram() => ObjCharacterVramImage.Of(enemies.Room.Session.ObjCharacters,
            new EnemyTileTransfers(new InstalledEnemyArtwork(enemies.TileArtwork ?? InstalledFixtures.EnemyTiles, enemies.BossArtwork ?? InstalledFixtures.BossArtwork, enemies.AreaArtwork ?? InstalledFixtures.AreaArtwork)));
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);

        for (int page = 0; page < MotherBrainEnemyState.LegLoadPageCount; page++)
        {
            int record = MotherBrainLegLoadListConstants.NativeListAddress +
                page * MotherBrainLegLoadListConstants.RecordByteCount;
            int nativeSize = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record);
            int nativeSource = (int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            int nativeDestination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5);

            bool completed = enemies.MotherBrainBehavior.Ascent.ShowNextLegPage(state);
            AssertEqual(page + 1 == MotherBrainEnemyState.LegLoadPageCount,
                completed, $"Mother Brain leg-loading page {page} completion frame");
            AssertEqual(completed ? 0 : page + 1, state.NextLegTilePage,
                $"Mother Brain leg-loading page {page} next page, back to the first after the last");
            int nativeTableByte = nativeDestination * 2 - ObjectCharactersByte;
            ObjCharacterUpload shown = enemies.Room.Session.ObjCharacters.LastOver(nativeTableByte, nativeSize)
                ?? throw new InvalidOperationException($"Mother Brain leg-loading page {page} shows nothing.");
            AssertEqual(nativeTableByte, shown.FirstByte,
                $"Mother Brain leg-loading page {page} shows where the cartridge's record places it");
            AssertEqual(nativeSize, shown.ByteCount, $"Mother Brain leg-loading page {page} shows its record's bytes");
            byte[] nativePixels = RomDataReader.ReadFixedBank(rom, nativeSource, nativeSize);
            AssertTrue(ObjVram().Bytes.Slice(nativeDestination * 2, nativeSize)
                .SequenceEqual(nativePixels),
                $"Mother Brain leg-loading page {page} characters match cartridge");
        }
        int terminator = MotherBrainLegLoadListConstants.NativeListAddress +
            MotherBrainEnemyState.LegLoadPageCount *
            MotherBrainLegLoadListConstants.RecordByteCount;
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), terminator),
            "Mother Brain leg-loading page eleven is followed by native zero terminator");

        AssertThrows<ArgumentOutOfRangeException>(
            () => state.NextLegTilePage = MotherBrainEnemyState.LegLoadPageCount,
            "Mother Brain's ascent has no page past the twelfth");
        AssertThrows<InvalidDataException>(
            () => NativeSnapshotImportMotherBrainVerification.DecodeLegTilePage(unchecked(
                MotherBrainLegLoadListConstants.NativeListPointer + 1)),
            "a misaligned snapshot leg-loading pointer is rejected at the import");
    }

    /// <summary>Shows both escape-timer digit pages and compares the characters they show with the cartridge's pages.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyMotherBrainEscapeTimerPages(
        InstalledEnemyArtwork stock, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        EscapeTimerTileAtlas timer = EscapeTimerTileAtlas.Load(new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom)));
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance)
        {
            TileArtwork = stock.Enemies, BossArtwork = stock.Bosses, AreaArtwork = stock.Areas,
            BossArtworkBinding = { EscapeTimerArtwork = timer },
        };
        SnesVram ObjVram() => ObjCharacterVramImage.Of(enemies.Room.Session.ObjCharacters, new EnemyTileTransfers(stock, timer: timer));

        EscapeTimerTilePage[] pages = Enum.GetValues<EscapeTimerTilePage>();
        for (int page = 0; page < pages.Length; page++)
        {
            enemies.MotherBrainEncounter.ShowSpritePage(MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.EscapeTimer, page));
            byte[] native = ReadNativeMotherBrainPageSource(rom, EscapeTimerTilePages.SourceAddressOf(pages[page]), EscapeTimerTilePages.ByteCountOf(pages[page]));
            AssertTrue(ObjVram().Bytes.Slice(EscapeNativePageWords.Of(EscapeTimerTilePages.FirstCharacterOf(pages[page])) * 2, native.Length)
                .SequenceEqual(native),
                "installed Mother Brain timer page matches cartridge without visual ROM reads");
        }
    }

    /// <summary>Reads source bytes from retail data for independent comparison with a shown page.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for a Mother Brain sprite page source")]
    private static byte[] ReadNativeMotherBrainPageSource(
        CartridgeImportAddressSpace rom, int sourceAddress, int byteCount) =>
        RomDataReader.ReadFixedBank(rom, sourceAddress, byteCount);

    /// <summary>The OBJ character table's first byte in the VRAM image the oracle compares.</summary>
    private static int ObjectCharactersByte => GameplayVramLayout.ObjectCharacters.Offset * 2;

    /// <summary>The VRAM image byte sprite character <paramref name="character"/> begins at.</summary>
    private static int ShownByte(int character) => ObjectCharactersByte + character * RoomCharacterAtlasFormat.BytesPerTile;

    /// <summary>The page sheet each of <see cref="MotherBrainSpecialSpriteArtworkDefinitions.All"/> is, in the same order.</summary>
    private static readonly MotherBrainSpritePageSheet[] SpecialSheets =
        [MotherBrainSpritePageSheet.Legs, MotherBrainSpritePageSheet.BabyMetroid, MotherBrainSpritePageSheet.Attack, MotherBrainSpritePageSheet.ExplodedDoor];

    /// <summary>Copies each native sheet page directly from retail data into its authored VRAM destination for the oracle.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle: independent cartridge transfer of a Mother Brain special sprite sheet")]
    private static SnesVram ReferenceMotherBrainSpecialPages(
        ISnesAddressSpace bus, MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        var vram = new SnesVram();
        for (int page = 0; page < sheet.PageCount; page++)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                sheet.SourceAddress + page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
            int destination = ShownByte(sheet.PageFirstCharacter(page));
            vram.LoadBytes(destination, bytes);
        }
        return vram;
    }
    /// <summary>Shows every page of <paramref name="sheet"/> through the installed artwork and the production encounter.</summary>
    private static SnesVram ShowMotherBrainSpecialPages(
        InstalledEnemyArtwork artwork, MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses, AreaArtwork = artwork.Areas };
        SnesVram ObjVram() => ObjCharacterVramImage.Of(enemies.Room.Session.ObjCharacters,
            new EnemyTileTransfers(new InstalledEnemyArtwork(enemies.TileArtwork ?? InstalledFixtures.EnemyTiles, enemies.BossArtwork ?? InstalledFixtures.BossArtwork, enemies.AreaArtwork ?? InstalledFixtures.AreaArtwork)));
        MotherBrainSpritePageSheet shown = SpecialSheets[MotherBrainSpecialSpriteArtworkDefinitions.All.ToList().IndexOf(sheet)];
        for (int page = 0; page < sheet.PageCount; page++)
            enemies.MotherBrainEncounter.ShowSpritePage(MotherBrainSpritePage.Of(shown, page));
        return ObjVram();
    }


    /// <summary>Runs the Mother Brain sprite page bounds check from the verification command.</summary>
    [VerificationCommand("--mother-brain-transfer-sources")]
    internal static int MotherBrainTransferSourcesCommand()
    {
        Suite(nameof(VerifyMotherBrainSpritePageBounds), () => VerifyMotherBrainSpritePageBounds());
        return 0;
    }
}
