using System.Text;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

internal static partial class AssetTools
{
    /// <summary>Regenerates the checked-in compiled enemy visual-selector catalog from the inventory.</summary>
    internal static int GenerateEnemyVisualSelectors()
    {
        var inventory = EnemyVisualSelectorInventory.Collect(LoadRepositoryRom());
        // These catalogs keep hand-written selector handling; list them so a new one is noticed.
        foreach (string skipped in inventory.Unresolved)
            Console.WriteLine($"SKIPPED {skipped}");
        GenerateCompiledEnemyVisualSelectorCatalog(inventory.Keyed);
        return 0;
    }

    /// <summary>
    /// Development-only mechanical source generation. The checked-in result contains
    /// only sparse, fixed pointer selectors, not an executable ROM or hidden raw bank.
    /// </summary>
    private static void GenerateCompiledEnemyVisualSelectorCatalog(
        Dictionary<int, ushort> selectors)
    {
        IGrouping<int, KeyValuePair<int, ushort>>[] banks = selectors
            .Where(pair => !CompiledEnemyVisualSelectors.IsCalculatedSelector(pair.Key))
            .GroupBy(pair => pair.Key >> 16)
            .OrderBy(group => group.Key)
            .ToArray();
        foreach (IGrouping<int, KeyValuePair<int, ushort>> bank in banks)
        {
            var bankSource = new StringBuilder(bank.Count() * 40);
            bankSource.AppendLine("// Generated from the pinned retail cartridge by --generate-enemy-visual-selectors.");
            bankSource.AppendLine("namespace SuperMetroid.Core.Assets;");
            bankSource.AppendLine();
            bankSource.AppendLine("internal static partial class CompiledEnemyVisualSelectors");
            bankSource.AppendLine("{");
            bankSource.AppendLine($"    private static CompiledEnemyVisualSelector[] Bank{bank.Key:X2} =>");
            bankSource.AppendLine("    [");
            foreach ((int address, ushort pointer) in bank.OrderBy(pair => pair.Key))
                bankSource.AppendLine($"        new(0x{address:X6}, 0x{pointer:X4}),");
            bankSource.AppendLine("    ];");
            bankSource.AppendLine("}");
            string bankPath =
                $"csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank{bank.Key:X2}.Definitions.cs";
            File.WriteAllText(bankPath, bankSource.ToString(), new UTF8Encoding(false));
        }

        // The calculated-family dispatcher is maintained separately from literal bank generation.
        Console.WriteLine(
            $"Generated {banks.Sum(bank => bank.Count())} remaining literal visual selectors in {banks.Length} bank files; calculated families stay in their dispatcher.");
    }
}
