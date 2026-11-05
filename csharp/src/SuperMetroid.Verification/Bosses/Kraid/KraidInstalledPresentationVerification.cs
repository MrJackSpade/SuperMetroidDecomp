using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationOracleVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationTailVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that extracted Kraid artwork supports the installed room background, head, and HUD presentation paths.</summary>
internal static class KraidInstalledPresentationVerification
{
    /// <summary>Validates the working map, stock artwork, background and control oracle as one Kraid presentation suite.</summary>
    internal static void VerifyKraidInstalledPresentation()
    {
        Suite(nameof(VerifyKraidWorkingMapTail), () => VerifyKraidWorkingMapTail());
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledEnemyArtwork stock = InstalledFixtures.EnemyArtwork;
        Suite(nameof(VerifyInstalledKraidBackground), () => VerifyInstalledKraidBackground(source, stock));
        Suite(nameof(VerifyKraidInstalledControlOracle), () => VerifyKraidInstalledControlOracle(source));
        Console.WriteLine("PASS installed Kraid maps, head frames, room characters and HUD restoration.");
    }

    /// <summary>Runs the installed Kraid presentation suite from the verification command line.</summary>
    [VerificationCommand("--kraid-installed-presentation")]
    internal static int KraidInstalledPresentationCommand()
    {
        Suite(nameof(VerifyKraidInstalledPresentation), () => VerifyKraidInstalledPresentation());
        return 0;
    }
}
