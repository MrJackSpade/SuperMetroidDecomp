using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.ReachabilityAudit;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// #621 architecture audit. Inventories every source type with a disposition and checks it against the
/// checked-in <c>docs/architecture-inventory.json</c>; every structural flag must be accepted by a reviewed
/// entry in <c>docs/architecture-reviews.json</c>, and no review may outlive its finding.
/// </summary>
internal static class Program
{
    /// <summary>Requires an explicit solution path and reserves --write for inventory regeneration.</summary>
    private const string Usage = "Usage: SuperMetroid.ArchitectureAudit --solution <SuperMetroid.Full.slnx> [--write]";

    /// <summary>Stable camel-case JSON settings shared by inventory and review documents.</summary>
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Checks the reviewed type inventory or rewrites it while reporting failures through the guarded console boundary.</summary>
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
            string reviewsPath = Path.Combine(solution.RepositoryRoot, "docs", "architecture-reviews.json");
            var reviews = JsonSerializer.Deserialize<ReviewDocument>(File.ReadAllText(reviewsPath), Json)
                ?? throw new InvalidDataException($"{reviewsPath} is empty.");
            var entries = TypeInventory.Collect(solution);
            var problems = ReviewProblems(entries, reviews.Reviews);
            string document = JsonSerializer.Serialize(new
            {
                issue = 621,
                regenerate = "dotnet run --project csharp/src/SuperMetroid.ArchitectureAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write",
                types = entries.Select(entry => Reviewed(entry, reviews.Reviews)).ToList(),
            }, Json).ReplaceLineEndings("\n") + "\n";
            if (write)
            {
                File.WriteAllText(inventoryPath, document);
                Console.WriteLine($"Wrote {inventoryPath}: {entries.Count} types.");
            }
            else if (!File.Exists(inventoryPath) || File.ReadAllText(inventoryPath).ReplaceLineEndings("\n") != document)
            {
                problems.Add($"{inventoryPath} is stale; regenerate it with --write and review the difference.");
            }
            foreach (string problem in problems)
                Console.Error.WriteLine(problem);
            if (problems.Count != 0)
                return 1;
            Console.WriteLine($"Architecture audit passed: {entries.Count} types, {entries.Count(entry => entry.Flags.Count != 0)} reviewed findings.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    /// <summary>Each flagged type needs a review naming exactly its flags; a review without a matching finding is stale.</summary>
    private static List<string> ReviewProblems(IReadOnlyList<TypeEntry> entries, IReadOnlyList<Review> reviews)
    {
        var problems = new List<string>();
        var byType = new Dictionary<string, Review>(StringComparer.Ordinal);
        foreach (Review review in reviews)
        {
            if (!byType.TryAdd(review.Type, review))
                problems.Add($"Duplicate review for {review.Type}.");
            if (string.IsNullOrWhiteSpace(review.Disposition) || string.IsNullOrWhiteSpace(review.Reason))
                problems.Add($"Review for {review.Type} needs a disposition and a reason.");
        }
        var flagged = entries.Where(entry => entry.Flags.Count != 0).ToDictionary(entry => entry.Type, StringComparer.Ordinal);
        foreach (TypeEntry entry in flagged.Values)
        {
            if (!byType.TryGetValue(entry.Type, out Review? review))
                problems.Add($"{entry.Type} is flagged [{string.Join(", ", entry.Flags)}] without a review in docs/architecture-reviews.json.");
            else if (!review.Flags.Order(StringComparer.Ordinal).SequenceEqual(entry.Flags.Order(StringComparer.Ordinal), StringComparer.Ordinal))
                problems.Add($"{entry.Type} is flagged [{string.Join(", ", entry.Flags)}] but its review accepts [{string.Join(", ", review.Flags)}].");
        }
        foreach (Review review in reviews.Where(review => !flagged.ContainsKey(review.Type)))
            problems.Add($"Stale review: {review.Type} has no current finding.");
        return problems;
    }

    /// <summary>Projects a discovered type with its reviewed disposition and only its current flags.</summary>
    private static object Reviewed(TypeEntry entry, IReadOnlyList<Review> reviews)
    {
        Review? review = reviews.FirstOrDefault(candidate => candidate.Type == entry.Type);
        return new
        {
            type = entry.Type,
            kind = entry.Kind,
            scope = entry.Scope,
            assemblies = entry.Assemblies,
            files = entry.Files,
            partial = entry.Partial,
            disposition = review?.Disposition ?? entry.Disposition,
            flags = entry.Flags.Count == 0 ? null : entry.Flags,
            review = review is null ? null : "reviewed",
        };
    }

    /// <summary>Root of the checked-in architecture review list.</summary>
    private sealed record ReviewDocument(IReadOnlyList<Review> Reviews);

    /// <summary>A reviewed acceptance of a type's flags.</summary>
    private sealed record Review(string Type, IReadOnlyList<string> Flags, string Disposition, string Reason);
}
