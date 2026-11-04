using SuperMetroid.AssetExtraction.Artwork;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;

/// <summary>Checks installed standard OBJ characters and their override path (#1513: the game selects no sprite characters).</summary>
internal static class StandardObjectArtworkVerification
{
    /// <summary>The cartridge's standard sprite sheet at $9A:D200.</summary>
    private const int NativeStandardObjects = 0x9ad200;

    /// <summary>Compares every native OBJ byte, then checks the edited PNG and provenance.</summary>
    internal static void VerifyStandardObjectArtwork(string sourceRom)
    {
        // A private installation copy, whose standard OBJ override is written and whose stock sheet is damaged below.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.StandardObjectDirectory);
        string stock = copy.Installation.StandardObjectDirectory;
        string overrides = copy.Installation.StandardObjectOverrideDirectory;
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        RoomCharacterAtlas baseline = StandardObjectArtworkFiles.Load(stock, overrides);
        for (int offset = 0; offset < StandardObjectArtworkFormat.ByteCount; offset++)
            AssertEqual(native.ReadCartridgeByte(NativeStandardObjects + offset),
                baseline.Characters.Span[offset], $"standard OBJ native byte {offset:X4}");


        string stockPath = Path.Combine(stock, StandardObjectArtworkFormat.FileName);
        byte[] originalPng = File.ReadAllBytes(stockPath);
        int tiles = StandardObjectArtworkFormat.ByteCount /
            RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
        int rows = (tiles + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(originalPng),
            new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        image.Pixels[0] = (byte)(image.Pixels[0] == 1 ? 2 : 1);
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, StandardObjectArtworkFormat.FileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        RoomCharacterAtlas edited = StandardObjectArtworkFiles.Load(stock, overrides);
        AssertTrue(!edited.Characters.Span.SequenceEqual(baseline.Characters.Span),
            "selected indexed PNG changes the native standard OBJ transfer");
        AssertTrue(ReExtractStandardObjectStock(native, Path.Combine(copy.Root, "refreshed-stock"), overrides).Characters.Span
            .SequenceEqual(edited.Characters.Span),
            "stock re-extraction preserves the selected external OBJ override");

        File.WriteAllText(overridePath, "not an indexed PNG");
        AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
            "invalid OBJ override does not silently fall back");
        File.Delete(overridePath);
        AssertTrue(StandardObjectArtworkFiles.Load(stock, overrides).Characters.Span
            .SequenceEqual(baseline.Characters.Span),
            "removing override restores stock standard OBJ transfer");
        File.WriteAllText(stockPath, "damaged stock");
        AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(stock, overrides),
            "damaged stock OBJ provenance fails loudly");
        Console.WriteLine("Standard OBJ art: all $2E00 native bytes, visible PNG edit, invalid override and stock provenance pass.");
    }

    /// <summary>Extracts the stock standard OBJ sheet from the cartridge into <paramref name="refreshed"/> and loads it under <paramref name="overrides"/>.</summary>
    [PrivateInstallation("Runs StandardObjectArtworkFiles.Extract into a fresh stock directory beside the copy's standard OBJ override, to check that a stock re-extraction keeps the edited indexed PNG's OBJ characters selected; the copied stock is the installer's output and never re-runs the extractor.")]
    private static RoomCharacterAtlas ReExtractStandardObjectStock(ISnesAddressSpace native, string refreshed, string overrides)
    {
        StandardObjectArtworkFiles.Extract(native, refreshed, SupportedCartridge.Sha256);
        return StandardObjectArtworkFiles.Load(refreshed, overrides);
    }

    /// <summary>Runs the standard object-artwork check against the repository's retail ROM.</summary>
    [VerificationCommand("--standard-object-artwork")]
    internal static int StandardObjectArtworkCommand()
    {
        string standardObjectRom = RepositoryRomPath;
        Suite(nameof(VerifyStandardObjectArtwork), () => VerifyStandardObjectArtwork(standardObjectRom));
        return 0;
    }
}
