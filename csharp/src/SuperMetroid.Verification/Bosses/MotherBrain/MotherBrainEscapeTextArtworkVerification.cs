using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Escape.Artwork;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks installed Mother Brain escape-text art and its five OBJ VRAM transfers.</summary>
internal static class MotherBrainEscapeTextArtworkVerification
{
    /// <summary>Compares stock art with the cartridge through the live VRAM transfer.</summary>
    internal static void VerifyInstalledMotherBrainEscapeTextArtwork(ImportEnemyArtwork stock)
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
        for (int page = 0; page < EscapeTextSheetFormat.PageCount; page++)
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

    /// <summary>Copies all five escape-text pages her escape message's look copies (#1538).</summary>
    private static SnesVram TransferMotherBrainEscapeTextPages(
        ImportEnemyArtwork artwork)
    {
        var resolver = new EnemyTileTransfers(new ImportEnemyArtwork(artwork.Enemies, artwork.Bosses, InstalledFixtures.AreaArtwork));
        int text = EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainEscapeTextLook);
        AssertEqual(EscapeTextSheetFormat.PageCount, EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain, text, resolver).Count,
            "her escape message's look copies every escape-text page");
        return ObjCharacterVramImage.Of(EncounterArtworkPlacements.Of(ComposedEncounter.MotherBrain, [text], resolver), resolver);
    }
}
