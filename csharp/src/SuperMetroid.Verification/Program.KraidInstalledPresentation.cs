using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    /// <summary>Extracts and validates the stock Kraid artwork catalog, then runs the installed-content checks for backgrounds, controls, head frames, and required assets.</summary>
    private static void VerifyKraidInstalledPresentation()
    {
        Suite(nameof(VerifyKraidWorkingMapTail), () => VerifyKraidWorkingMapTail());
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var temporary = new TestTempDirectory("map-catalog");
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
}
