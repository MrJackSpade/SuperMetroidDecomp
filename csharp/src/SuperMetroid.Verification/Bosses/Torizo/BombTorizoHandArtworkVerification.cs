using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.TestSupport.VerificationOracles;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

/// <summary>Checks Bomb Torizo hand debris art from extraction through the installed page.</summary>
internal static class BombTorizoHandArtworkVerification
{
    /// <summary>Confirms the installed debris page matches the cartridge.</summary>
    internal static void VerifyBombTorizoHandArtwork()
    {
        var importer = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledEnemyArtwork stock = InstalledFixtures.EnemyArtwork;
        TorizoTileSheet page = TorizoSwapArtworkDefinitions.ChozoDebris;
        AssertTrue(new EnemyTileTransfers(stock).TryResolve(page.SourceAddress, page.ByteCount, out ReadOnlyMemory<byte> bytes),
            "installed Chozo debris page exists");
        for (int index = 0; index < bytes.Length; index++)
            AssertEqual(importer.ReadCartridgeByte(page.SourceAddress + index), bytes.Span[index],
                $"imported debris tile byte {index:X4} matches the pinned cartridge");
        Console.WriteLine("Bomb Torizo hand artwork: 1,024 native tile bytes pass.");
    }

    /// <summary>Runs the Bomb Torizo hand artwork check.</summary>
    [VerificationCommand("--bomb-torizo-hand-artwork")]
    internal static int BombTorizoHandArtworkCommand()
    {
        Suite(nameof(VerifyBombTorizoHandArtwork), () => VerifyBombTorizoHandArtwork());
        return 0;
    }
}
