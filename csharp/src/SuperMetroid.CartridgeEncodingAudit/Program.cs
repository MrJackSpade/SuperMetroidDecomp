using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.CartridgeEncodingAudit;

/// <summary>
/// Writes the cartridge-encoding ratchet baseline and the #641 inventory from the same scan the
/// build analyzer runs. A project is audited exactly when it carries the baseline as an additional file.
/// </summary>
internal static class Program
{
    /// <summary>Writes the encoding ratchet baseline and inventory from one
    /// solution measurement, reporting exceptions through the console boundary.</summary>
    private static async Task<int> Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            switch (args)
            {
                case ["--solution", var solutionPath, "--write"]:
                {
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    var measurement = await EncodingMeasurement.MeasureAsync(solution);
                    string baselinePath = Path.Combine(solution.RepositoryRoot, "csharp", CartridgeEncodingScanner.BaselineFileName);
                    File.WriteAllText(baselinePath, CartridgeEncodingBaseline.Format(measurement.RatchetCounts()));
                    string inventoryPath = Path.Combine(solution.RepositoryRoot, "docs", "cartridge-encoding-inventory.json");
                    EncodingInventory.Write(measurement, inventoryPath);
                    Console.WriteLine($"Wrote {baselinePath} and {inventoryPath}.");
                    Console.WriteLine(measurement.Summary());
                    return 0;
                }
                case ["--solution", var solutionPath, "--list", .. var ruleIds] when ruleIds.Length > 0:
                {
                    // Prints each ratcheted occurrence of the named rules with its location, for migrating a rule's
                    // remaining entries; the baseline only records per-file counts.
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    var measurement = await EncodingMeasurement.MeasureAsync(solution);
                    foreach (EncodedFile file in measurement.Files)
                    foreach (CartridgeEncodingOccurrence occurrence in file.Occurrences)
                    {
                        string? rule = CartridgeEncodingScanner.RuleId(occurrence.Rule);
                        if (rule is null || !ruleIds.Contains(rule, StringComparer.Ordinal))
                            continue;
                        int line = occurrence.Location.GetLineSpan().StartLinePosition.Line + 1;
                        Console.WriteLine($"{file.Path}:{line}	{rule}	{occurrence.Basis}");
                    }
                    return 0;
                }
                default:
                    throw new ArgumentException(
                        "Usage: SuperMetroid.CartridgeEncodingAudit --solution <SuperMetroid.Full.slnx> (--write | --list <rule id>...)");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
