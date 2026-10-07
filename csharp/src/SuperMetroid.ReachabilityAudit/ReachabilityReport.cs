using System.Text;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Writes findings.tsv (one row per finding), the reflection review lists and summary.txt.
/// Members of an unreachable type are reported once, as the type.
/// </summary>
internal static class ReachabilityReport
{
    /// <summary>Assemblies that ship to players; symbols here must be reachable from the player hosts.</summary>
    private static readonly HashSet<string> ProductionAssemblies =
        ["SuperMetroid.Core", "SuperMetroid.AssetExtraction", "SuperMetroid.Diagnostics", "SuperMetroid.Desktop",
         "SuperMetroid.Rendering.Direct3D11", "SuperMetroid.Game", "SuperMetroid.Android"];

    public static void Write(ReachabilityResult result, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var declarations = result.Declarations;
        var unreachable = declarations.Values.Where(d => !result.Reachable.Contains(d.Key)).ToList();
        var unreachableTypes = unreachable.Where(d => d.Shape == DeclarationShape.Type).Select(d => d.Key).ToHashSet();
        bool InsideUnreachableType(Declaration d) => Containers(declarations, d).Any(unreachableTypes.Contains);
        var reported = unreachable.Where(d => !InsideUnreachableType(d)).ToList();

        var findings = new List<(string Category, Declaration Declaration, string Detail)>();
        foreach (var d in reported)
        {
            string category = d.Shape switch
            {
                DeclarationShape.Type => "unreachable-type",
                _ when result.Reflection.UnmarkedTargets.ContainsKey(d.Key) => "reflection-only-unmarked",
                _ when result.SerializerAccessed.Contains(d.Key) => "serialization-only",
                DeclarationShape.Field => "unreferenced-field",
                DeclarationShape.EnumMember => "",
                _ => "unreachable-member",
            };
            if (category.Length > 0)
                findings.Add((category, d, result.Reflection.UnmarkedTargets.TryGetValue(d.Key, out var site) ? site.ToString() : ""));
        }
        foreach (var d in declarations.Values.Where(d => d.Shape == DeclarationShape.EnumMember
                     && !result.Referenced.Contains(d.Key) && !InsideUnreachableType(d)))
            findings.Add(("unreferenced-enum-member", d, ""));

        var testOnly = declarations.Values.Where(d => result.Reachable.Contains(d.Key) && !result.ProductionReachable.Contains(d.Key)
            && d.Shape != DeclarationShape.EnumMember && d.Projects.All(ProductionAssemblies.Contains)).ToList();
        var testOnlyTypes = testOnly.Where(d => d.Shape == DeclarationShape.Type).Select(d => d.Key).ToHashSet();
        foreach (var d in testOnly.Where(d => !Containers(declarations, d).Any(testOnlyTypes.Contains)))
            findings.Add(("production-reached-only-by-tools", d, ""));

        var rows = new List<string> { "category\tkind\tsymbol\tfile\tline\tprojects\tdetail" };
        rows.AddRange(findings
            .OrderBy(f => f.Category, StringComparer.Ordinal).ThenBy(f => f.Declaration.File, StringComparer.Ordinal)
            .ThenBy(f => f.Declaration.Line)
            .Select(f => $"{f.Category}\t{f.Declaration.Kind}\t{f.Declaration.Display}\t{f.Declaration.File}\t{f.Declaration.Line}\t"
                + $"{string.Join(',', f.Declaration.Projects.Order(StringComparer.Ordinal))}\t{f.Detail}"));
        File.WriteAllLines(Path.Combine(outputDirectory, "findings.tsv"), rows, new UTF8Encoding(false));

        WriteLines(outputDirectory, "reflection-unresolved-sites.tsv", result.Reflection.UnresolvedSites.Select(s => s.ToString()));
        WriteLines(outputDirectory, "reflection-enumeration-sites.tsv", result.Reflection.EnumerationSites.Select(s => s.ToString()));
        // Unknown receiver type: unreachable declarations named by an identifier literal near the lookup.
        var unreachableByName = reported.ToLookup(d => SimpleName(d.Display));
        WriteLines(outputDirectory, "reflection-review-candidates.tsv", result.Reflection.CandidateNames
            .SelectMany(c => unreachableByName[c.Name].Select(d => $"{d.File}\t{d.Line}\t{d.Kind}\t{d.Display}\t{c.Site}")));

        var summary = new StringBuilder()
            .AppendLine($"declared\t{declarations.Count}")
            .AppendLine($"reachable\t{result.Reachable.Count(declarations.ContainsKey)}");
        foreach (var group in findings.GroupBy(f => f.Category).OrderBy(g => g.Key, StringComparer.Ordinal))
            summary.AppendLine($"{group.Key}\t{group.Count()}");
        summary.AppendLine($"reflection-unresolved-sites\t{result.Reflection.UnresolvedSites.Count}")
            .AppendLine($"reflection-enumeration-sites\t{result.Reflection.EnumerationSites.Count}");
        File.WriteAllText(Path.Combine(outputDirectory, "summary.txt"), summary.ToString());
        Console.Write(summary);
    }

    private static IEnumerable<string> Containers(IReadOnlyDictionary<string, Declaration> declarations, Declaration declaration)
    {
        for (string key = declaration.ContainingTypeKey; key.Length > 0;
             key = declarations.TryGetValue(key, out var container) ? container.ContainingTypeKey : "")
            yield return key;
    }

    private static string SimpleName(string display) => display.Split('(')[0].Split('.')[^1].Split('<')[0];

    private static void WriteLines(string directory, string name, IEnumerable<string> lines) =>
        File.WriteAllLines(Path.Combine(directory, name), lines.Distinct().Order(StringComparer.Ordinal), new UTF8Encoding(false));
}
