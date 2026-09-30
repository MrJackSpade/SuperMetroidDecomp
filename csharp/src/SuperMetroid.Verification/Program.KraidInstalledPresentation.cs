using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyKraidInstalledPresentation()
    {
        VerifyKraidWorkingMapTail();
        var source = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        using var temporary = new MapCatalogTestDirectory();
        string stockPath = Path.Combine(temporary.Root, "stock");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(stockPath);
        EnemyTileArtworkCatalog stock = EnemyTileArtworkFiles.Load(stockPath, null);
        VerifyInstalledKraidBackground(source, stockPath, stock);
        VerifyKraidInstalledControlOracle(source);
        VerifyKraidInstalledHeadIsolation(stockPath, stock);
        VerifyKraidLiveHeadAlias(stock);
        VerifyKraidRequiredFiles(stockPath);
        Console.WriteLine("PASS installed Kraid maps, head frames, room characters and HUD restoration.");
    }
}
