using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Animation;

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
        var result = new SortedDictionary<string, InstructionLayout>(StringComparer.Ordinal);
        foreach (var catalog in InstructionProgramCatalog.All())
        {
            if (catalog.MechanicsWords is null)
            {
                result[catalog.Type.Name] = new InstructionLayout { Unsupported = true };
                continue;
            }
            int[][] words = [.. catalog.MechanicsWords.Select(word => new int[] { word.Address, word.Value })];
            int[] slots = [.. (catalog.PresentationOperands ?? []).Select(address => (int)address)];
            result[catalog.Type.Name] = new InstructionLayout { Mechanics = words, Presentation = slots };
        }
        File.WriteAllText(outputPath, JsonSerializer.Serialize(result, global::SuperMetroid.DebugRunner.DebugRunnerJson.Info<SortedDictionary<string, InstructionLayout>>(CompactJson)));
        Console.WriteLine($"Instruction layouts: {result.Count} program types written to {outputPath}.");
    }

    /// <summary>
    /// One program type's layout: that it has no compiled mechanics, or its mechanics words as address-value pairs and its
    /// presentation slot addresses. A part it does not have is left out of the dump.
    /// </summary>
    internal sealed record InstructionLayout
    {
        /// <summary>True when the type has no compiled mechanics words; otherwise left out.</summary>
        [JsonPropertyName("unsupported"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Unsupported { get; init; }

        /// <summary>Each mechanics word as its address and value.</summary>
        [JsonPropertyName("mechanics"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[][]? Mechanics { get; init; }

        /// <summary>The presentation slot addresses.</summary>
        [JsonPropertyName("presentation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int[]? Presentation { get; init; }
    }
}
