using SuperMetroid.Core.Hardware;

internal static partial class AssetTools
{
    /// <summary>
    /// Regenerates <c>docs/ppu-layout-inventory.json</c> from the production PPU layout catalogs
    /// (#630), refusing to write an inventory whose catalogs break the identity or alias rules.
    /// Run from the repository root.
    /// </summary>
    private static int GeneratePpuLayoutInventory()
    {
        string root = Directory.GetCurrentDirectory();
        IReadOnlyList<PpuLayoutCatalogEntry> entries = PpuLayoutCatalogInventory.Enumerate(PpuLayoutCatalogInventory.ProductionAssemblies);
        IReadOnlyList<string> violations = PpuLayoutCatalogInventory.Violations(entries);
        if (violations.Count > 0)
            throw new InvalidDataException("PPU layout catalog rules are broken:" + Environment.NewLine +
                string.Join(Environment.NewLine, violations));
        string path = Path.Combine(root, PpuLayoutCatalogInventory.InventoryPath);
        File.WriteAllText(path, PpuLayoutCatalogInventory.RenderJson(entries, root));
        Console.WriteLine($"Wrote {entries.Count} PPU layout cells to {path}.");
        return 0;
    }
}
