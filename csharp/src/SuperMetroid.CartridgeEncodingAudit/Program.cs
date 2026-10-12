using SuperMetroid.EnsureAnalyzer;
using SuperMetroid.ReachabilityAudit;

namespace SuperMetroid.CartridgeEncodingAudit;

/// <summary>
/// Writes the cartridge-encoding ratchet baseline and the #641 inventory from the same scan the
/// build analyzer runs. A project is audited exactly when it carries the baseline as an additional file.
/// </summary>
internal static class Program
{
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
                default:
                    throw new ArgumentException(
                        "Usage: SuperMetroid.CartridgeEncodingAudit --solution <SuperMetroid.Full.slnx> --write");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }
}
