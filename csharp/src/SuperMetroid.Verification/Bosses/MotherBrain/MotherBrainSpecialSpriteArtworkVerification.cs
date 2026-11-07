using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
/// <summary>Checks installed Mother Brain sprite pages, the native leg-loading records and the looks that copy them.</summary>
internal static class MotherBrainSpecialSpriteArtworkVerification
{
    /// <summary>Compares installed legs, Baby, attack and exploded-door sheets with native records and guarded VRAM uploads.</summary>
    internal static void VerifyInstalledMotherBrainSpecialSpriteArtwork(InstalledEnemyArtwork stock)
    {
        MotherBrainSpecialSpriteArtworkCatalog installed =
            stock.Bosses.MotherBrainSpecialSprites ?? throw new InvalidDataException(
                "Extracted Mother Brain special sprite PNGs were not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteSheetDefinitions.All)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet], sheet.ByteCount);
            AssertTrue(installed.Get(sheet.Sheet).Characters.Span.SequenceEqual(native),
                $"installed {sheet.FileName} preserves native characters");
            SnesVram installedVram = ShowMotherBrainSpecialPages(stock,
                sheet);
            SnesVram cartridgeVram = ReferenceMotherBrainSpecialPages(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), sheet);
            for (int page = 0; page < sheet.PageCount; page++)
            {
                int sourceOffset = page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                int destinationOffset = ShownByte(MotherBrainSpecialSprites.PageFirstCharacter(sheet, page));
                int count = MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(native.AsSpan(sourceOffset, count)),
                    $"installed {sheet.FileName} page {page} matches native source");
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, count)),
                    $"installed {sheet.FileName} page {page} matches the independent cartridge transfer");
            }
        }

        // Unlike the other two lists, Baby loading reads its four native records
        // from $A9:8FE5. Check every compiled sheet coordinate against that list.
        MotherBrainSpecialSpriteSheetDefinition baby =
            MotherBrainSpecialSpriteSheetDefinitions.BabyMetroid;
        for (int page = 0; page < baby.PageCount; page++)
        {
            int record = MotherBrainBabyPageListConstants.BabyTileList +
                page * MotherBrainBabyPageListConstants.RecordSize;
            AssertEqual(MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record),
                $"Baby tile record {page} size");
            uint source = (uint)(int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            AssertEqual((uint)(MotherBrainSourceDefinitions.SpecialSprites.Sources[baby.Sheet] +
                page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount), source,
                $"Baby tile record {page} source");
            AssertEqual(ShownByte(MotherBrainSpecialSprites.PageFirstCharacter(baby, page)),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5) * 2,
                $"Baby page {page} shows where the cartridge's record places it");
            EncounterArtworkUpload shown = EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain,
                EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainBabyLook), new EnemyTileTransfers(stock))[page];
            AssertEqual(ShownByte(MotherBrainSpecialSprites.PageFirstCharacter(baby, page)) - ObjectCharactersByte, shown.FirstByte,
                $"the Baby's look copies page {page} at its record's characters");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record), shown.Characters.Length,
                $"the Baby's look copies page {page}'s record's bytes");
        }
        Suite(nameof(VerifyMotherBrainLegLoadPages), () => VerifyMotherBrainLegLoadPages(stock, rom));
        Suite(nameof(VerifyMotherBrainEscapeTimerPages), () => VerifyMotherBrainEscapeTimerPages(stock, rom));
        Console.WriteLine(
            "  Mother Brain special sprites: legs, Baby, attack and exploded-door pages match cartridge records, guarded live uploads and strict installed-source boundary pass.");
    }

    /// <summary>
    /// Checks every page her standing body's looks copy, her legs' then her attacks', against the native leg-loading list,
    /// one an update of the rise's readying: where each lands, its bytes and the produced characters; and that the readying
    /// completes on the list's last record, as the cartridge's list ends there (#1538).
    /// </summary>
    private static void VerifyMotherBrainLegLoadPages(
        InstalledEnemyArtwork stock, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = stock.Enemies, BossArtwork = stock.Bosses };
        var resolver = new EnemyTileTransfers(new InstalledEnemyArtwork(stock.Enemies, stock.Bosses, InstalledFixtures.AreaArtwork));
        EncounterArtworkUpload[] standing = [.. new[] { EncounterLookUploads.MotherBrainLegsLook, EncounterLookUploads.MotherBrainAttackLook }
            .SelectMany(look => EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain, EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, look), resolver))];
        AssertEqual(MotherBrainReadyingDefinitions.StandingBodyDuration, standing.Length, "her rise readies her standing body one update a page her looks copy");
        SnesVram ObjVram(int copied) => ObjCharacterVramImage.Of(EncounterArtworkPlacements.Of(standing.Take(copied)), resolver);
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);

        for (int page = 0; page < MotherBrainReadyingDefinitions.StandingBodyDuration; page++)
        {
            int record = MotherBrainLegLoadListConstants.NativeListAddress +
                page * MotherBrainLegLoadListConstants.RecordByteCount;
            int nativeSize = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record);
            int nativeSource = (int)new SnesAddress(rom.ReadCartridgeByte(record + 4),
                RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 2));
            int nativeDestination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), record + 5);

            bool completed = MotherBrainAscent.ReadyStandingBodyOnce(state);
            AssertEqual(page + 1 == MotherBrainReadyingDefinitions.StandingBodyDuration,
                completed, $"Mother Brain's standing body readying update {page} completion frame");
            AssertEqual(completed ? 0 : page + 1, state.StandingBodyReadyingElapsed,
                $"Mother Brain's standing body readying update {page} count, back to the first after the last");
            int nativeTableByte = nativeDestination * 2 - ObjectCharactersByte;
            EncounterArtworkUpload shown = standing[page];
            AssertEqual(nativeTableByte, shown.FirstByte,
                $"Mother Brain leg-loading page {page} lands where the cartridge's record places it");
            AssertEqual(nativeSize, shown.Characters.Length, $"Mother Brain leg-loading page {page} copies its record's bytes");
            byte[] nativePixels = RomDataReader.ReadFixedBank(rom, nativeSource, nativeSize);
            AssertTrue(ObjVram(page + 1).Bytes.Slice(nativeDestination * 2, nativeSize)
                .SequenceEqual(nativePixels),
                $"Mother Brain leg-loading page {page} characters match cartridge");
        }
        int terminator = MotherBrainLegLoadListConstants.NativeListAddress +
            MotherBrainReadyingDefinitions.StandingBodyDuration *
            MotherBrainLegLoadListConstants.RecordByteCount;
        AssertEqual((ushort)0, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), terminator),
            "Mother Brain leg-loading page eleven is followed by native zero terminator");

        AssertThrows<ArgumentOutOfRangeException>(
            () => state.StandingBodyReadyingElapsed = MotherBrainReadyingDefinitions.StandingBodyDuration,
            "Mother Brain's rise has no readying update past the twelfth");
        AssertThrows<InvalidDataException>(
            () => NativeSnapshotMotherBrainImport.DecodeLegTilePage(unchecked(
                MotherBrainLegLoadListConstants.NativeListPointer + 1)),
            "a misaligned snapshot leg-loading pointer is rejected at the import");
    }

    /// <summary>Copies both escape-timer digit pages her escape timer's look copies and compares the characters with the cartridge's pages.</summary>
    private static void VerifyMotherBrainEscapeTimerPages(
        InstalledEnemyArtwork stock, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom)
    {
        EscapeTimerCharacters timer = EscapeTimerCharacters.Read(new MemoryStream(EscapeTimerTileAtlasExtractor.Extract(rom)));
        var resolver = new EnemyTileTransfers(stock, timer: timer);
        IReadOnlyList<EncounterArtworkUpload> copies = EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain,
            EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainEscapeTimerLook), resolver);
        SnesVram ObjVram(int copied) => ObjCharacterVramImage.Of(EncounterArtworkPlacements.Of(copies.Take(copied)), resolver);

        EscapeTimerTilePage[] pages = Enum.GetValues<EscapeTimerTilePage>();
        AssertEqual(pages.Length, copies.Count, "her escape timer's look copies both timer pages");
        for (int page = 0; page < pages.Length; page++)
        {
            byte[] native = ReadNativeMotherBrainPageSource(rom, EscapeTimerTilePagesConstants.SourceAddressOf(pages[page]), EscapeTimerTilePageTransfers.ByteCountOf(pages[page]));
            AssertTrue(ObjVram(page + 1).Bytes.Slice(EscapeNativePageWords.Of(EscapeTimerTilePageTransfers.FirstCharacterOf(pages[page])) * 2, native.Length)
                .SequenceEqual(native),
                "installed Mother Brain timer page matches cartridge without visual ROM reads");
        }
    }

    /// <summary>Reads source bytes from retail data for independent comparison with a shown page.</summary>
    private static byte[] ReadNativeMotherBrainPageSource(
        CartridgeImportAddressSpace rom, int sourceAddress, int byteCount) =>
        RomDataReader.ReadFixedBank(rom, sourceAddress, byteCount);

    /// <summary>The OBJ character table's first byte in the VRAM image the oracle compares.</summary>
    private static int ObjectCharactersByte => GameplayVramLayoutConstants.ObjectCharacters.Offset * 2;

    /// <summary>The VRAM image byte sprite character <paramref name="character"/> begins at.</summary>
    private static int ShownByte(int character) => ObjectCharactersByte + character * RoomCharacterAtlasFormat.BytesPerTile;

    /// <summary>Copies each native sheet page directly from retail data into its authored VRAM destination for the oracle.</summary>
    private static SnesVram ReferenceMotherBrainSpecialPages(
        ISnesAddressSpace bus, MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        var vram = new SnesVram();
        for (int page = 0; page < sheet.PageCount; page++)
        {
            byte[] bytes = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
                MotherBrainSourceDefinitions.SpecialSprites.Sources[sheet.Sheet] + page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
            int destination = ShownByte(MotherBrainSpecialSprites.PageFirstCharacter(sheet, page));
            vram.LoadBytes(destination, bytes);
        }
        return vram;
    }
    /// <summary>Copies every page of <paramref name="sheet"/> her first look that copies it copies, through the installed artwork (#1538).</summary>
    private static SnesVram ShowMotherBrainSpecialPages(
        InstalledEnemyArtwork artwork, MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        var resolver = new EnemyTileTransfers(new InstalledEnemyArtwork(artwork.Enemies, artwork.Bosses, InstalledFixtures.AreaArtwork));
        string look = sheet.Sheet switch
        {
            MotherBrainSpriteSheet.Legs => EncounterLookUploads.MotherBrainLegsLook,
            MotherBrainSpriteSheet.BabyMetroid => EncounterLookUploads.MotherBrainBabyLook,
            MotherBrainSpriteSheet.Attack => EncounterLookUploads.MotherBrainAttackLook,
            MotherBrainSpriteSheet.ExplodedDoor => EncounterLookUploads.MotherBrainExplodedDoorLook,
            MotherBrainSpriteSheet.Corpse or MotherBrainSpriteSheet.EscapeTimer or MotherBrainSpriteSheet.EscapeText =>
                throw new InvalidOperationException($"{sheet.Sheet} is not one of her special sheets."),
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainSpriteSheet)} {sheet.Sheet}."),
        };
        int place = EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, look);
        AssertEqual(sheet.PageCount, EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain, place, resolver).Count,
            $"her {look} look copies every page of {sheet.FileName}");
        return ObjCharacterVramImage.Of(EncounterArtworkPlacements.Of(ComposedEncounter.MotherBrain, [place], resolver), resolver);
    }
}
