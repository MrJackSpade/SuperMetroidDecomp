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

/// <summary>Checks installed standard OBJ characters and a player's in-place edit of them (#1513: the game selects no sprite characters).</summary>
internal static class StandardObjectArtworkVerification
{
    /// <summary>The cartridge's standard sprite sheet at $9A:D200.</summary>
    private const int NativeStandardObjects = 0x9ad200;

    /// <summary>Compares every native OBJ byte, then checks an in-place PNG edit and a malformed one.</summary>
    internal static void VerifyStandardObjectArtwork(string sourceRom)
    {
        // A private installation copy, whose installed standard OBJ sheet is edited and damaged in place below.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.StandardObjectDirectory);
        string directory = copy.Installation.StandardObjectDirectory;
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        RoomCharacterAtlas baseline = StandardObjectArtworkFiles.Load(directory);
        for (int offset = 0; offset < StandardObjectArtworkFormat.ByteCount; offset++)
            AssertEqual(native.ReadCartridgeByte(NativeStandardObjects + offset),
                baseline.Characters.Span[offset], $"standard OBJ native byte {offset:X4}");


        string installedPath = Path.Combine(directory, StandardObjectArtworkFormat.FileName);
        byte[] originalPng = File.ReadAllBytes(installedPath);
        int tiles = StandardObjectArtworkFormat.ByteCount /
            RoomCharacterAtlasFormat.BytesPerTile;
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tiles);
        int rows = (tiles + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(originalPng),
            new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        image.Pixels[0] = (byte)(image.Pixels[0] == 1 ? 2 : 1);
        using (var output = File.Create(installedPath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        RoomCharacterAtlas edited = StandardObjectArtworkFiles.Load(directory);
        AssertTrue(!edited.Characters.Span.SequenceEqual(baseline.Characters.Span),
            "an in-place edit of the installed indexed PNG changes the standard OBJ transfer");

        File.WriteAllText(installedPath, "not an indexed PNG");
        AssertThrows<InvalidDataException>(() => StandardObjectArtworkFiles.Load(directory),
            "an installed OBJ sheet edited into a malformed PNG fails loudly");
        Console.WriteLine("Standard OBJ art: all $2E00 native bytes, in-place PNG edit loaded, malformed edit rejected.");
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
