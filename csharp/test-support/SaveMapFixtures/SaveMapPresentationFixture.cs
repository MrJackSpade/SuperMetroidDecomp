using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.SaveMapFixtures;

/// <summary>Explicit startup import for diagnostics that decode cartridge-compatible SRAM.</summary>
internal static class SaveMapPresentationFixture
{
    /// <summary>
    /// Extracts the source's map presentation into an isolated temporary installation,
    /// loads the typed catalog for verification, and removes the temporary files afterward.
    /// </summary>
    public static AreaMapPresentationCatalog Create(ISnesAddressSpace nativeSource)
    {
        string parent = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        Directory.CreateDirectory(parent);
        string root = Directory.CreateDirectory(Path.Combine(parent, "save-map-fixture-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            MapPresentationExtractor.Extract(nativeSource, Path.Combine(root, "game", "maps"), "test-provenance");
            return new GameInstallation(root).LoadMaps();
        }
        finally
        {
            string resolved = Path.GetFullPath(root);
            if (!resolved.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Save-map fixture cleanup escaped its temporary parent.");
            Directory.Delete(resolved, recursive: true);
        }
    }
}
