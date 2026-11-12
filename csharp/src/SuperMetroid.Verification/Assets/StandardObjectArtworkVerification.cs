using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Assets;

/// <summary>Checks the extracted standard OBJ characters against the cartridge (#1513: the game selects no sprite characters).</summary>
internal static class StandardObjectArtworkVerification
{
    /// <summary>The cartridge's standard sprite sheet at $9A:D200.</summary>
    private const int NativeStandardObjects = 0x9ad200;

    /// <summary>Compares every extracted OBJ byte with the cartridge's.</summary>
    internal static void VerifyStandardObjectArtwork(string sourceRom)
    {
        var native = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        // The characters are import data no installation holds (#1636): the cartridge-derived extraction's are compared.
        RoomCharacterAtlas extracted = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.StandardObjects;
        for (int offset = 0; offset < StandardObjectArtworkFormat.ByteCount; offset++)
            AssertEqual(native.ReadCartridgeByte(NativeStandardObjects + offset),
                extracted.Characters.Span[offset], $"standard OBJ native byte {offset:X4}");
        Console.WriteLine("Standard OBJ art: all $2E00 native bytes match.");
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
