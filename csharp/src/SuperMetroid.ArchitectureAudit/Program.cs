using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// Architecture audit. Inventories every source type with a disposition and checks it against the checked-in
/// <c>docs/architecture-inventory.json</c>. Every structural finding is justified on the type itself with
/// <c>[ArchitectureReview]</c> or fails the audit, and a review whose finding is gone fails as stale; there is no
/// central list of accepted findings. It also writes and checks the per-file module inventory,
/// <c>docs/module-inventory.json</c>.
/// </summary>
internal static class Program
{
    /// <summary>Requires an explicit solution path and reserves --write for inventory regeneration.</summary>
    private const string Usage = "Usage: SuperMetroid.ArchitectureAudit --solution <SuperMetroid.Full.slnx> [--write]";

    /// <summary>Stable camel-case JSON settings shared by the inventory documents.</summary>
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Checks the type inventory or rewrites it while reporting failures through the guarded console boundary.</summary>
    private static async Task<int> Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            (string solutionPath, bool write) = args switch
            {
                ["--solution", var path] => (path, false),
                ["--solution", var path, "--write"] => (path, true),
                _ => throw new ArgumentException(Usage),
            };
            var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
            string inventoryPath = Path.Combine(solution.RepositoryRoot, "docs", "architecture-inventory.json");
            var entries = TypeInventory.Collect(solution);
            var problems = ReviewProblems(entries);
            string document = JsonSerializer.Serialize(new
            {
                issue = 621,
                regenerate = "dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write",
                types = entries.Select(Describe).ToList(),
            }, Json).ReplaceLineEndings("\n") + "\n";
            string modulePath = Path.Combine(solution.RepositoryRoot, "docs", "module-inventory.json");
            string modules = JsonSerializer.Serialize(ModuleInventory.Collect(solution, problems), Json).ReplaceLineEndings("\n") + "\n";
            foreach ((string path, string content) in new[] { (inventoryPath, document), (modulePath, modules) })
            {
                if (write)
                {
                    File.WriteAllText(path, content);
                    Console.WriteLine($"Wrote {path}.");
                }
                else if (!File.Exists(path) || File.ReadAllText(path).ReplaceLineEndings("\n") != content)
                {
                    problems.Add($"{path} is stale; regenerate it with --write and review the difference.");
                }
            }
            foreach (string problem in problems)
                Console.Error.WriteLine(problem);
            if (problems.Count != 0)
                return 1;
            Console.WriteLine($"Architecture audit passed: {entries.Count} types, {entries.Count(entry => entry.Flags.Count != 0)} findings justified at their sites.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>
    /// Each finding needs a review on its own type naming that finding with a reason; a review naming no current
    /// finding is stale. Nothing outside the type can justify its finding.
    /// </summary>
    private static List<string> ReviewProblems(IReadOnlyList<TypeEntry> entries)
    {
        var problems = new List<string>();
        foreach (TypeEntry entry in entries)
        {
            var findings = entry.Flags.Select(FindingKind).ToHashSet(StringComparer.Ordinal);
            foreach (ArchitectureReview review in entry.Reviews)
            {
                if (string.IsNullOrWhiteSpace(review.Reason))
                    problems.Add($"{entry.Type}: [ArchitectureReview(\"{review.Finding}\")] needs a reason specific to this type.");
                if (!findings.Contains(review.Finding))
                    problems.Add($"Stale review: {entry.Type} justifies \"{review.Finding}\" but has no such finding; remove the attribute.");
            }
            foreach (string flag in entry.Flags.Where(flag => !IsJustified(entry, flag)))
                problems.Add($"{entry.Type} is flagged [{flag}] with no [ArchitectureReview(\"{FindingKind(flag)}\", reason)] on the type. " +
                    "Fix the shape, or justify this one site.");
        }
        return problems;
    }

    /// <summary>The finding kind of a flag: <c>back-reference:&lt;creator&gt;</c> is justified as <c>back-reference</c>.</summary>
    private static string FindingKind(string flag) => flag.Split(':')[0];

    /// <summary>Whether the type declares a reasoned review for <paramref name="flag"/>.</summary>
    private static bool IsJustified(TypeEntry entry, string flag) =>
        entry.Reviews.Any(review => review.Finding == FindingKind(flag) && !string.IsNullOrWhiteSpace(review.Reason));

    /// <summary>Projects a discovered type with its computed disposition, its current flags and whether every flag is justified.</summary>
    private static object Describe(TypeEntry entry) => new
    {
        type = entry.Type,
        kind = entry.Kind,
        scope = entry.Scope,
        assemblies = entry.Assemblies,
        files = entry.Files,
        partial = entry.Partial,
        disposition = entry.Disposition,
        flags = entry.Flags.Count == 0 ? null : entry.Flags,
        justified = entry.Flags.Count != 0 && entry.Flags.All(flag => IsJustified(entry, flag)) ? true : (bool?)null,
    };
}
