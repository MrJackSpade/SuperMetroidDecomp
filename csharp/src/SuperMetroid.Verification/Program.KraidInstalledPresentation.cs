using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
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
