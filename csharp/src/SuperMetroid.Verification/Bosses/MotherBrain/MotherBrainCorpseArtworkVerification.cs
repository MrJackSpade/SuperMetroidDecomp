using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Enemies.Corpses;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
/// <summary>Checks Mother Brain corpse PNG import, VRAM transfers, and rotting buffer setup against native graphics.</summary>
internal static class MotherBrainCorpseArtworkVerification
{
    /// <summary>Runs the corpse sheet's installation checks from a validated stock catalog.</summary>
    internal static void VerifyMotherBrainCorpseStockArtwork()
    {
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(InstalledFixtures.EnemyArtwork));
    }

    /// <summary>Reads the banked native corpse character sheet solely as a verification oracle.</summary>
    private static byte[] ReadNativeMotherBrainCorpseArtwork(CartridgeImportAddressSpace rom) =>
        RomDataReader.ReadFixedBank(rom,
            MotherBrainSourceDefinitions.Corpse.Characters,
            MotherBrainCorpseSheet.ByteCount);

    /// <summary>Compares six installed VRAM pages and corpse staging with native bytes.</summary>
    internal static void VerifyInstalledMotherBrainCorpseArtwork(ImportEnemyArtwork stock)
    {
        RoomCharacterAtlas artwork = stock.Bosses.MotherBrainCorpse ??
            throw new InvalidDataException("Extracted Mother Brain corpse PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = ReadNativeMotherBrainCorpseArtwork(rom);
        AssertTrue(artwork.Characters.Span.SequenceEqual(native),
            "installed Mother Brain corpse PNG preserves all native source bytes");

        // Only the oracle/import above receives cartridge bytes. Production transfers and
        // staging run against the same cartridge-incapable memory type as installed games.
        byte[] stockCharacters = TransferMotherBrainCorpsePages(stock);
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
        {
            int sourceOffset = MotherBrainCorpseSheet.PageOffset(page);
            int shownOffset = MotherBrainCorpseSheet.PageFirstCharacter(page) * RoomCharacterAtlasFormat.BytesPerTile;
            AssertTrue(stockCharacters.AsSpan(shownOffset,
                    MotherBrainCorpseSheet.PageByteCount)
                .SequenceEqual(native.AsSpan(sourceOffset,
                    MotherBrainCorpseSheet.PageByteCount)),
                $"installed Mother Brain corpse page {page} shows the native page's characters");
        }

        var nativeStaging = new NativeEnemyStaging();
        // Independent $A9:E08B oracle: copy each right-hand frame's seven-tile row,
        // omitting the transparent last tile in the first four rows. No runtime fallback.
        StagingByteView nativeTiles = nativeStaging.MotherBrainCorpseTiles;
        for (int row = 0; row < 6; row++)
        {
            ReadOnlySpan<byte> rowBytes = native.AsSpan(row * 0x200 + 0xc0, row < 4 ? 0xc0 : 0xe0);
            for (int offset = 0; offset < rowBytes.Length; offset++)
                nativeTiles[row * 0xe0 + offset] = rowBytes[offset];
        }
        CorpseStaging installedCorpse = CorpseStaging.MotherBrain(artwork.Characters.Span);
        AssertMotherBrainCorpseStagingParity(nativeStaging, installedCorpse);

        // The runtime keeps only the final-death corpse's own rot schedule, which starts as the cartridge's $DC40 table
        // does: the bottom row first at the corpse's foot, each row above waiting two calls longer (#1472). The extraction
        // stages the corpse frame the schedule rots (#1392).
        CorpseRotScheduleChecks.AssertFreshSchedule(MotherBrainCorpseRottingState.EntryCount,
            new MotherBrainRainbowBeamAttackSequence().ThirdPhaseDeath.CorpseRotting.Schedule, "Mother Brain's final-death corpse");

        Console.WriteLine("  Mother Brain corpse art: indexed PNG import parity, six RAM-only sprite VRAM pages, independent staging oracle, room/sequence staging pass.");
    }

    /// <summary>Copies every corpse page her corpse's look copies and returns the sprite characters they show (#1538).</summary>
    private static byte[] TransferMotherBrainCorpsePages(
        ImportEnemyArtwork artwork)
    {
        int corpse = EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainCorpseLook);
        AssertEqual(MotherBrainCorpseArtworkDefinitions.RowCount,
            EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain, corpse, new EnemyTileTransfers(artwork)).Count,
            "her corpse's look copies one page a corpse row");
        return new ObjCharacterTablePage().Page(EncounterArtworkPlacements.Of(ComposedEncounter.MotherBrain, [corpse], new EnemyTileTransfers(artwork)),
                new EnemyTileTransfers(artwork))
            .Characters[..SpriteArtworkPlacements.TableByteCount].ToArray();
    }

    /// <summary>Compares the extraction's staged corpse rows with an independent native copy.</summary>
    private static void AssertMotherBrainCorpseStagingParity(NativeEnemyStaging expected, CorpseStaging actual)
    {
        StagingByteView expectedTiles = expected.MotherBrainCorpseTiles;
        for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
        {
            byte[] staged = actual.Upload(new CorpseRotRow(row, CorpseWorkMemory.CorpseRottingHeap + row * MotherBrainCorpseStaging.RowBytes,
                MotherBrainCorpseStaging.InitialCopyLength(row), 0));
            for (int offset = 0; offset < staged.Length; offset++)
                AssertEqual(expectedTiles[row * MotherBrainCorpseStaging.RowBytes + offset], staged[offset],
                    "installed Mother Brain corpse staging agrees with cartridge");
        }
    }

    /// <summary>Runs the installed Mother Brain corpse artwork verification command.</summary>
    [VerificationCommand("--mother-brain-corpse-stock-artwork")]
    internal static int MotherBrainCorpseStockArtworkCommand()
    {
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
        return 0;
    }
}
