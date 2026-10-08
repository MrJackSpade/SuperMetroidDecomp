using System.Text;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>Writes findings.tsv (one row per finding), the reflection review lists and summary.txt.</summary>
internal static class ReachabilityReport
{
    public static void Write(ReachabilityResult result, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var findings = ReachabilityFindings.Classify(result);

        var rows = new List<string> { "category\tkind\tsymbol\tfile\tline\tprojects" };
        rows.AddRange(findings
            .OrderBy(f => f.Category, StringComparer.Ordinal).ThenBy(f => f.Declaration.File, StringComparer.Ordinal)
            .ThenBy(f => f.Declaration.Line)
            .Select(f => $"{f.Category}\t{f.Declaration.Kind}\t{f.Declaration.Display}\t{f.Declaration.File}\t{f.Declaration.Line}\t"
                + string.Join(',', f.Declaration.Projects.Order(StringComparer.Ordinal))));
        File.WriteAllLines(Path.Combine(outputDirectory, "findings.tsv"), rows, new UTF8Encoding(false));

        WriteLines(outputDirectory, "reflection-unresolved-sites.tsv", result.Reflection.UnresolvedSites.Select(s => s.ToString()));
        WriteLines(outputDirectory, "reflection-enumeration-sites.tsv", result.Reflection.EnumerationSites.Select(s => s.ToString()));
        // Unknown receiver type: dead declarations named by an identifier literal near the lookup.
        var deadByName = findings.Where(f => f.Category != ReachabilityFindings.ReachedOnlyByTools)
            .ToLookup(f => SimpleName(f.Declaration.Display));
        WriteLines(outputDirectory, "reflection-review-candidates.tsv", result.Reflection.CandidateNames
            .SelectMany(c => deadByName[c.Name].Select(f =>
                $"{f.Declaration.File}\t{f.Declaration.Line}\t{f.Declaration.Kind}\t{f.Declaration.Display}\t{c.Site}")));

        var summary = new StringBuilder()
            .AppendLine($"declared\t{result.Declarations.Count}")
            .AppendLine($"reachable\t{result.Reachable.Count(result.Declarations.ContainsKey)}");
        foreach (var group in findings.GroupBy(f => f.Category).OrderBy(g => g.Key, StringComparer.Ordinal))
            summary.AppendLine($"{group.Key}\t{group.Count()}");
        summary.AppendLine($"reflection-unresolved-sites\t{result.Reflection.UnresolvedSites.Count}")
            .AppendLine($"reflection-enumeration-sites\t{result.Reflection.EnumerationSites.Count}");
        File.WriteAllText(Path.Combine(outputDirectory, "summary.txt"), summary.ToString());
        Console.Write(summary);
    }

    /// <summary>
    /// The CI gate: writes the report, then fails on any finding the allow-list does not name and on
    /// any allow-list entry that no longer matches a finding. An entry is a findings.tsv row's
    /// category, kind, symbol and file, tab-separated; blank lines and <c>#</c> comments are ignored.
    /// </summary>
    public static int Check(ReachabilityResult result, string outputDirectory, string allowListPath)
    {
        Write(result, outputDirectory);
        var allowed = File.ReadAllLines(allowListPath)
            .Where(line => line.Length > 0 && !line.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);
        var found = ReachabilityFindings.Classify(result)
            .Select(f => $"{f.Category}\t{f.Declaration.Kind}\t{f.Declaration.Display}\t{f.Declaration.File}")
            .ToHashSet(StringComparer.Ordinal);
        var unexpected = found.Where(f => !allowed.Contains(f)).Order(StringComparer.Ordinal).ToList();
        var stale = allowed.Where(a => !found.Contains(a)).Order(StringComparer.Ordinal).ToList();
        foreach (string finding in unexpected)
            Console.Error.WriteLine("DEAD " + finding);
        foreach (string entry in stale)
            Console.Error.WriteLine("STALE ALLOW-LIST ENTRY " + entry);
        if (unexpected.Count == 0 && stale.Count == 0)
        {
            Console.WriteLine("Reachability gate passed: no unallowed findings.");
            return 0;
        }
        Console.Error.WriteLine($"Reachability gate failed: {unexpected.Count} unallowed findings, {stale.Count} stale allow-list entries. " +
            "Delete dead code, or move code only tools reach into SuperMetroid.Tooling.");
        return 1;
    }

    private static string SimpleName(string display) => display.Split('(')[0].Split('.')[^1].Split('<')[0];

    private static void WriteLines(string directory, string name, IEnumerable<string> lines) =>
        File.WriteAllLines(Path.Combine(directory, name), lines.Distinct().Order(StringComparer.Ordinal), new UTF8Encoding(false));
}
