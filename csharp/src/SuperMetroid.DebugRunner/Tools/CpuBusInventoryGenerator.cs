using SuperMetroid.Core.Hardware;

internal static partial class AssetTools
{
    /// <summary>
    /// Regenerates <c>docs/cpu-bus-inventory.json</c> from the production CPU-bus catalogs (#629),
    /// refusing to write an inventory whose catalogs break the identity or alias rules.
    /// Run from the repository root.
    /// </summary>
    private static int GenerateCpuBusInventory()
    {
        string root = Directory.GetCurrentDirectory();
        IReadOnlyList<CpuBusCatalogEntry> entries = CpuBusCatalogInventory.Enumerate(CpuBusCatalogInventory.ProductionAssemblies);
        IReadOnlyList<string> violations = CpuBusCatalogInventory.Violations(entries);
        if (violations.Count > 0)
            throw new InvalidDataException("CPU-bus catalog rules are broken:" + Environment.NewLine +
                string.Join(Environment.NewLine, violations));
        string path = Path.Combine(root, CpuBusCatalogInventory.InventoryPath);
        File.WriteAllText(path, CpuBusCatalogInventory.RenderJson(entries, root));
        Console.WriteLine($"Wrote {entries.Count} CPU-bus cells to {path}.");
        return 0;
    }
}
