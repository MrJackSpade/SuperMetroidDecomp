using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>
/// Re-extracts the stock map presentation beside an edited override, for the override checks that confirm a stock
/// replacement neither rewrites nor outranks the user's file.
/// </summary>
internal static class MapStockReExtraction
{
    /// <summary>
    /// Extracts the stock map presentation from <paramref name="bus"/> into <paramref name="rebuiltDirectory"/> and loads
    /// it under <paramref name="overrideDirectory"/>, so the caller can compare the selected content and the override's
    /// bytes with those it held before the extraction.
    /// </summary>
    /// <param name="bus">Cartridge the stock map presentation is extracted from.</param>
    /// <param name="rebuiltDirectory">Fresh directory the extractor writes the replacement stock into.</param>
    /// <param name="overrideDirectory">Override directory holding the edit under test.</param>
    /// <returns>The map presentation loaded from the replacement stock and the override directory.</returns>
    [PrivateInstallation("Runs MapPresentationExtractor into a fresh stock directory beside an edited map override, so its callers check that a stock extraction leaves the override's bytes alone and the override still outranks the newly written stock; reading the installed maps never runs the extractor.")]
    internal static AreaMapPresentation ReExtractStock(ISnesAddressSpace bus, string rebuiltDirectory, string overrideDirectory)
    {
        MapPresentationExtractor.Extract(bus, rebuiltDirectory, "test-provenance");
        return MapPresentationFiles.Load(rebuiltDirectory, overrideDirectory);
    }
}
