using System.Text.Json;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.DebugRunner.Tools;

/// <summary>Exports a stable mechanics-and-presentation snapshot of compiled instruction catalogs.</summary>
internal static class InstructionLayoutDump
{
    /// <summary>JSON options: Unindented output.</summary>
    private static readonly JsonSerializerOptions CompactJson = new() { WriteIndented = false };

    /// <summary>
    /// Conversion probe (<c>--dump-instruction-layouts &lt;file&gt;</c>): records every compiled
    /// instruction program's mechanics words and presentation slot addresses, so a semantic rewrite
    /// can be shown identical to the definition it replaces by comparing dumps taken before and after.
    /// </summary>
    internal static void DumpInstructionLayouts(string outputPath)
    {
        var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
        foreach (var catalog in InstructionProgramCatalog.All())
        {
            if (catalog.MechanicsWords is null)
            {
                result[catalog.Type.Name] = new { unsupported = true };
                continue;
            }
            int[][] words = [.. catalog.MechanicsWords.Select(word => new int[] { word.Address, word.Value })];
            int[] slots = [.. (catalog.PresentationOperands ?? []).Select(address => (int)address)];
            result[catalog.Type.Name] = new { mechanics = words, presentation = slots };
        }
        File.WriteAllText(outputPath, JsonSerializer.Serialize(result, CompactJson));
        Console.WriteLine($"Instruction layouts: {result.Count} program types written to {outputPath}.");
    }
}
