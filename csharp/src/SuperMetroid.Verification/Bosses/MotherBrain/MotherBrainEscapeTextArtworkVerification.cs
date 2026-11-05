using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Escape.Artwork;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks installed Mother Brain escape-text art and its five OBJ VRAM transfers.</summary>
internal static class MotherBrainEscapeTextArtworkVerification
{
    /// <summary>Compares stock art with the cartridge through the live VRAM transfer.</summary>
    internal static void VerifyInstalledMotherBrainEscapeTextArtwork(InstalledEnemyArtwork stock)
    {
        RoomCharacterAtlas artwork = stock.Bosses.MotherBrainEscapeText ??
            throw new InvalidDataException("Extracted Mother Brain escape-text PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            EscapeSourceDefinitions.TextCharacters,
            EscapeTextSheetFormat.ByteCount);
        AssertTrue(artwork.Characters.Span.SequenceEqual(native),
            "installed Mother Brain escape-text PNG preserves native characters");

        SnesVram installedVram = TransferMotherBrainEscapeTextPages(stock);
        // The oracle owns cartridge bytes; production has no uninstalled-ROM fallback.
        var cartridgeVram = new SnesVram();
        for (int page = 0; page < 5; page++)
        {
            int record = 0xa6c4d9 + 7 * page;
            int count = LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record), CpuBusRegionRead.Read(rom, record + 1));
            int source = (int)new SnesAddress(CpuBusRegionRead.Read(rom, record + 4), LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record + 2), CpuBusRegionRead.Read(rom, record + 3)));
            int destination = LittleEndianBytes.Word(CpuBusRegionRead.Read(rom, record + 5), CpuBusRegionRead.Read(rom, record + 6));
            cartridgeVram.LoadBytes(destination * 2, RomDataReader.ReadFixedBank(rom, source, count));
        }
        for (int page = 0; page < EscapeTextArtworkDefinitions.PageCount; page++)
        {
            int sourceOffset = EscapeTextPages.PageOffset(page);
            int destinationOffset =
                EscapeNativePageWords.Of(EscapeTextPages.PageFirstCharacter(page)) * 2;
            int byteCount = EscapeTextPages.PageByteCount(page);
            AssertTrue(installedVram.Bytes.Slice(destinationOffset, byteCount)
                .SequenceEqual(native.AsSpan(sourceOffset, byteCount)),
                $"installed Mother Brain escape-text VRAM page {page} matches native bytes");
            AssertTrue(installedVram.Bytes.Slice(destinationOffset, byteCount)
                .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, byteCount)),
                $"installed and cartridge Mother Brain escape-text VRAM page {page} agree");
        }

        Console.WriteLine(
            "  Mother Brain escape text art: five guarded OBJ pages and cartridge parity pass.");
    }

    /// <summary>Shows all five escape-text pages through the production Mother Brain path.</summary>
    private static SnesVram TransferMotherBrainEscapeTextPages(
        InstalledEnemyArtwork artwork)
    {
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
        var resolver = new EnemyTileTransfers(new InstalledEnemyArtwork(enemies.TileArtwork ?? InstalledFixtures.EnemyTiles,
            (BossSourceArtwork?)enemies.BossArtwork ?? InstalledFixtures.BossArtwork, InstalledFixtures.AreaArtwork));
        MotherBrainSpritePage[] pages = [.. Enumerable.Range(0, EscapeTextArtworkDefinitions.PageCount)
            .Select(page => MotherBrainSpritePage.Of(MotherBrainSpritePageSheet.EscapeText, page))];
        foreach (MotherBrainSpritePage page in pages)
            enemies.MotherBrainEncounter.ShowSpritePage(page);
        IReadOnlyList<int> steps = EncounterArtworkPlacements.MotherBrainSteps(pages);
        AssertEqual<int?>(steps[^1], enemies.Room.Session.ArtworkProgress.StepOf(EncounterArtworkSequence.MotherBrainPages),
            "the encounter reaches the last escape-text page's step");
        return ObjCharacterVramImage.Of(EncounterArtworkPlacements.Of(EncounterArtworkSequence.MotherBrainPages, steps, resolver), resolver);
    }
}
