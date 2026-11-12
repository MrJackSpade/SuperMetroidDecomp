using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.TestSupport.WindowsConsole;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// Architecture audit. Inventories every source type with a disposition and checks it against the checked-in
/// <c>docs/architecture-inventory.json</c>. Every structural finding is justified on the type itself with
/// <c>[ArchitectureReview]</c> or fails the audit; there is no central list of accepted findings. It also writes and
/// checks the per-file module inventory, <c>docs/module-inventory.json</c>, and fails on every Core module cycle.
/// <para>
/// With no mode flag it runs the full check, which also fails on a stale inventory and on a stale review (one whose
/// finding its type no longer has). <c>--write</c> regenerates the inventories. <c>--findings</c> fails only on what
/// needs judgment: an unjustified finding, a review without a reason, a Core module cycle; CI runs it, because a stale
/// inventory or review is deterministic and the asynchronous cleanup fixes it after the push. <c>--prune-stale-reviews</c>
/// is that fix for reviews: it deletes every stale review's attribute from the source.
/// </para>
/// </summary>
internal static class Program
{
    /// <summary>Requires an explicit solution path and at most one mode flag.</summary>
    private const string Usage = "Usage: SuperMetroid.ArchitectureAudit --solution <SuperMetroid.Full.slnx> " +
        "[--write | --findings | --prune-stale-reviews]";

    /// <summary>Stable camel-case JSON settings shared by the inventory documents.</summary>
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Runs the chosen mode while reporting failures through the guarded console boundary.</summary>
    private static async Task<int> Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            (string solutionPath, AuditMode mode) = args switch
            {
                ["--solution", var path] => new AuditArguments(path, AuditMode.Check),
                ["--solution", var path, "--write"] => new AuditArguments(path, AuditMode.Write),
                ["--solution", var path, "--findings"] => new AuditArguments(path, AuditMode.Findings),
                ["--solution", var path, "--prune-stale-reviews"] => new AuditArguments(path, AuditMode.PruneStaleReviews),
                _ => throw new ArgumentException(Usage),
            };
            var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
            var entries = TypeInventory.Collect(solution);
            return mode switch
            {
                AuditMode.PruneStaleReviews => StaleReviewPruner.Prune(StaleReviews(entries), solution.RepositoryRoot),
                AuditMode.Findings => CheckFindings(solution, entries),
                AuditMode.Check => CheckInventories(solution, entries, write: false),
                AuditMode.Write => CheckInventories(solution, entries, write: true),
                _ => throw new InvalidOperationException($"Unknown audit mode {mode}."),
            };
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>
    /// The full check, or the regeneration: generates both inventories and either writes them or fails on a stale one,
    /// and fails on every judgment problem, every stale review and every Core module cycle.
    /// </summary>
    private static int CheckInventories(LoadedSolution solution, IReadOnlyList<TypeEntry> entries, bool write)
    {
        string inventoryPath = Path.Combine(solution.RepositoryRoot, "docs", "architecture-inventory.json");
        var problems = JudgmentProblems(entries);
        problems.AddRange(StaleReviews(entries).Select(stale =>
            $"Stale review: {stale.Type} justifies \"{stale.Review.Finding}\" but has no such finding; remove the attribute " +
            "(--prune-stale-reviews does)."));
        string document = JsonSerializer.Serialize(new
        {
            issue = 621,
            regenerate = "dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write",
            types = entries.Select(Describe).ToList(),
        }, Json).ReplaceLineEndings("\n") + "\n";
        string modulePath = Path.Combine(solution.RepositoryRoot, "docs", "module-inventory.json");
        string modules = JsonSerializer.Serialize(ModuleInventory.Collect(solution, problems), Json).ReplaceLineEndings("\n") + "\n";
        foreach ((string path, string content) in new[] { new GeneratedInventory(inventoryPath, document), new GeneratedInventory(modulePath, modules) })
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
        return Report(problems, $"Architecture audit passed: {entries.Count} types, " +
            $"{entries.Count(entry => entry.Flags.Count != 0)} findings justified at their sites.");
    }

    /// <summary>
    /// The push gate: fails only on what needs judgment. The module inventory is generated for its Core cycles but not
    /// compared, and stale reviews are not reported; the asynchronous cleanup regenerates and prunes both.
    /// </summary>
    private static int CheckFindings(LoadedSolution solution, IReadOnlyList<TypeEntry> entries)
    {
        var problems = JudgmentProblems(entries);
        _ = ModuleInventory.Collect(solution, problems);
        return Report(problems, $"Architecture findings passed: {entries.Count} types, " +
            $"{entries.Count(entry => entry.Flags.Count != 0)} findings justified at their sites, no Core module cycle.");
    }

    /// <summary>Prints every problem to standard error and fails, or prints the pass line.</summary>
    private static int Report(List<string> problems, string passed)
    {
        foreach (string problem in problems)
            Console.Error.WriteLine(problem);
        if (problems.Count != 0)
            return 1;
        Console.WriteLine(passed);
        return 0;
    }

    /// <summary>
    /// The problems only a person can resolve: each finding needs a review on its own type naming that finding, and
    /// every review needs a reason. Nothing outside the type can justify its finding.
    /// </summary>
    private static List<string> JudgmentProblems(IReadOnlyList<TypeEntry> entries)
    {
        var problems = new List<string>();
        foreach (TypeEntry entry in entries)
        {
            foreach (ArchitectureReview review in entry.Reviews.Where(review => string.IsNullOrWhiteSpace(review.Reason)))
                problems.Add($"{entry.Type}: [ArchitectureReview(\"{review.Finding}\")] needs a reason specific to this type.");
            foreach (string flag in entry.Flags.Where(flag => !IsJustified(entry, flag)))
                problems.Add($"{entry.Type} is flagged [{flag}] with no [ArchitectureReview(\"{FindingKind(flag)}\", reason)] on the type. " +
                    "Fix the shape, or justify this one site.");
        }
        return problems;
    }

    /// <summary>Every review naming a finding its type no longer has.</summary>
    private static List<StaleReview> StaleReviews(IReadOnlyList<TypeEntry> entries)
    {
        var stale = new List<StaleReview>();
        foreach (TypeEntry entry in entries)
        {
            var findings = entry.Flags.Select(FindingKind).ToHashSet(StringComparer.Ordinal);
            stale.AddRange(entry.Reviews.Where(review => !findings.Contains(review.Finding)).Select(review => new StaleReview(entry.Type, review)));
        }
        return stale;
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
