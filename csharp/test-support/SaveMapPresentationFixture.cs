using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Explicit startup import for diagnostics that decode cartridge-compatible SRAM.</summary>
internal static class SaveMapPresentationFixture
{
    /// <summary>
    /// Extracts map presentation data into a temporary installation and loads the catalog used
    /// by save-RAM diagnostics, removing the temporary files before returning.
    /// </summary>
    /// <param name="nativeSource">Cartridge address space supplying the map presentation assets.</param>
    /// <returns>The loaded area-map presentation catalog for save-RAM decoding.</returns>
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
