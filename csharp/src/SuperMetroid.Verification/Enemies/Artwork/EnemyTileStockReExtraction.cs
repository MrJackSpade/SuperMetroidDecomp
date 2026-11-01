using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Re-extracts the stock enemy artwork over an edited enemy colour override's private copy, for the override checks that
/// confirm a stock replacement neither rewrites nor outranks the user's file.
/// </summary>
internal static class EnemyTileStockReExtraction
{
    /// <summary>
    /// Extracts the stock enemy artwork from <paramref name="bus"/> into <paramref name="stockDirectory"/>, the private
    /// copy's stock directory beside its override, so the caller can compare the override's bytes with those it held
    /// before the extraction and load the replacement stock under the override.
    /// </summary>
    /// <param name="bus">Cartridge the stock enemy artwork is extracted from.</param>
    /// <param name="stockDirectory">The private copy's stock enemy-tile directory, rewritten in place.</param>
    [PrivateInstallation("Runs EnemyTileArtworkFiles.Extract in place over a private copy's stock enemy artwork beside an edited Norfair Ridley, Tourian statue, Zebetite or Ninja Pirate colour override, so its callers check that a stock extraction into that directory leaves the override's bytes alone and the override still outranks the newly written stock; reading the installed enemy artwork never runs the extractor.")]
    internal static void ReExtractStock(ISnesAddressSpace bus, string stockDirectory) =>
        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
}
