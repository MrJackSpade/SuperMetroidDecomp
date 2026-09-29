using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static AreaMapPresentationCatalog? retailPresentationFixture;

    /// <summary>
    /// Installs the same extracted content contract used by the playable host in
    /// frontend verifier fixtures. Reusing one catalog avoids repeated full-map
    /// extraction while independent game instances still own their mutable state.
    /// </summary>
    private static AreaMapPresentationCatalog RetailPresentationFixture()
    {
        if (retailPresentationFixture is not null)
            return retailPresentationFixture;

        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "retail-presentation-fixture-" + Guid.NewGuid().ToString("N")));
        MapPresentationExtractor.Extract(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")),
            Path.Combine(root, "game", "maps"), "test-provenance");
        return retailPresentationFixture = new GameInstallation(root).LoadMaps();
    }
}
