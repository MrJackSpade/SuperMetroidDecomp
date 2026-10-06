using System.Reflection;
using System.Text.Json;

internal static partial class Program
{
    /// <summary>
    /// #1165 conversion probe (<c>--dump-instruction-layouts &lt;file&gt;</c>): records every compiled
    /// instruction program's mechanics words and presentation slot addresses, so a semantic rewrite
    /// can be shown identical to the definition it replaces by comparing dumps taken before and after.
    /// </summary>
    private static void DumpInstructionLayouts(string outputPath)
    {
        Assembly core = typeof(SuperMetroid.Core.Game.CommonEnemyInstructionCodes).Assembly;
        var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
        foreach (Type type in core.GetTypes().Where(type => type.Name.EndsWith("InstructionProgramDefinitions", StringComparison.Ordinal)))
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            MethodInfo? mechanics = type.GetMethod("MechanicsWord", flags, [typeof(int)]);
            object? mechanicsCount = type.GetProperty("MechanicsWordCount", flags)?.GetValue(null) ??
                type.GetField("MechanicsWordCount", flags)?.GetValue(null);
            MethodInfo? presentation = type.GetMethod("PresentationWordAddress", flags, [typeof(int)]);
            object? presentationCount = type.GetProperty("PresentationWordCount", flags)?.GetValue(null) ??
                type.GetField("PresentationWordCount", flags)?.GetValue(null);
            if (mechanics is null || mechanicsCount is null)
            {
                result[type.Name] = new { unsupported = true };
                continue;
            }
            var words = new List<int[]>();
            for (int index = 0; index < Convert.ToInt32(mechanicsCount); index++)
            {
                object word = mechanics.Invoke(null, [index])!;
                Type wordType = word.GetType();
                int address = Convert.ToInt32(wordType.GetProperty("Address")!.GetValue(word));
                int value = Convert.ToInt32(wordType.GetProperty("Value")!.GetValue(word));
                words.Add([address, value]);
            }
            var slots = new List<int>();
            if (presentation is not null && presentationCount is not null)
                for (int index = 0; index < Convert.ToInt32(presentationCount); index++)
                    slots.Add(Convert.ToInt32(presentation.Invoke(null, [index])));
            result[type.Name] = new { mechanics = words, presentation = slots };
        }
        File.WriteAllText(outputPath, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false }));
        Console.WriteLine($"Instruction layouts: {result.Count} program types written to {outputPath}.");
    }
}
