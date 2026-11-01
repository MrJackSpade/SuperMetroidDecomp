using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Re-extracts the stock enemy artwork beside an edited enemy colour override, for the override checks that confirm a
/// stock replacement neither rewrites nor outranks the user's file.
/// </summary>
internal static class EnemyTileStockReExtraction
{
    /// <summary>
    /// Extracts the stock enemy artwork from <paramref name="bus"/> into a fresh directory named
    /// <paramref name="name"/> beside <paramref name="stockDirectory"/>, so the caller can compare the override's bytes
    /// with those it held before the extraction and load the replacement stock under the override.
    /// </summary>
    /// <param name="bus">Cartridge the stock enemy artwork is extracted from.</param>
    /// <param name="stockDirectory">The private copy's stock enemy-tile directory the replacement is written beside.</param>
    /// <param name="name">Name of the fresh directory, distinct per caller sharing one copy.</param>
    /// <returns>The directory holding the replacement stock.</returns>
    [PrivateInstallation("Runs EnemyTileArtworkFiles.Extract into a fresh stock directory beside an edited Norfair Ridley, Tourian statue, Zebetite or Ninja Pirate colour override, so its callers check that a stock extraction leaves the override's bytes alone and the override still outranks the newly written stock; reading the installed enemy artwork never runs the extractor.")]
    internal static string ReExtractStock(ISnesAddressSpace bus, string stockDirectory, string name)
    {
        string rebuilt = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))
            ?? throw new InvalidOperationException($"Stock enemy artwork {stockDirectory} has no parent directory."), name);
        EnemyTileArtworkFiles.Extract(bus, rebuilt, SupportedCartridge.Sha256);
        return rebuilt;
    }
}
