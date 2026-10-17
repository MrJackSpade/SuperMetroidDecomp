using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationHeadClockVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationOracleVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationResourcesVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidInstalledPresentationTailVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that extracted Kraid artwork supports the installed room background, head, and HUD presentation paths.</summary>
internal static class KraidInstalledPresentationVerification
{
    /// <summary>Validates the working map, stock artwork, background, control oracle, live head aliases, and required files as one Kraid presentation suite.</summary>
    internal static void VerifyKraidInstalledPresentation()
    {
        Suite(nameof(VerifyKraidWorkingMapTail), () => VerifyKraidWorkingMapTail());
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var temporary = new TestScratchDirectory("map-catalog");
        string stockPath = Path.Combine(temporary.Root, "stock");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(stockPath);
        EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(stockPath, null);
        Suite(nameof(VerifyInstalledKraidBackground), () => VerifyInstalledKraidBackground(source, stockPath, stock));
        Suite(nameof(VerifyKraidInstalledControlOracle), () => VerifyKraidInstalledControlOracle(source));
        Suite(nameof(VerifyKraidInstalledHeadIsolation), () => VerifyKraidInstalledHeadIsolation(source, stockPath, stock));
        Suite(nameof(VerifyKraidLiveHeadAlias), () => VerifyKraidLiveHeadAlias(stock));
        Suite(nameof(VerifyKraidRequiredFiles), () => VerifyKraidRequiredFiles(stockPath));
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
