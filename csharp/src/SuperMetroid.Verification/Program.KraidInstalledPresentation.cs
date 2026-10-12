using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using static EnemyTileArtworkKraidVerification;
using static KraidInstalledPresentationHeadClockVerification;
using static KraidInstalledPresentationOracleVerification;
using static KraidInstalledPresentationResourcesVerification;
using static KraidInstalledPresentationTailVerification;
using static SuiteTimingVerification;

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
        Suite(nameof(VerifyKraidInstalledHeadIsolation), () => VerifyKraidInstalledHeadIsolation(stockPath, stock));
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
