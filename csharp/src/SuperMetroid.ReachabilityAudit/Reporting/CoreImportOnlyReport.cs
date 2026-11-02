using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Reporting;

/// <summary>
/// Core declarations the game itself never reads (#1484): reachable from the product's entry points only through the
/// import pipeline, or only from tools and checks. The walk starts at the production roots and refuses to enter any
/// declaration the import pipeline compiles, so a Core type that only the import builds and reads is left unreached.
/// <para>
/// Framework-interface dispatch (#1502): the game often reads an object the import built through a framework interface,
/// indexing a colour band as an <c>IReadOnlyList&lt;RgbColor&gt;</c> or enumerating it as an <c>IEnumerable&lt;T&gt;</c>.
/// Such a call names only the framework member, and the object's constructor is reached only through the refused import,
/// so the walk would never enter the repository type that answers it. When game-reached code calls a member of a framework
/// interface, the walk therefore also reaches each repository member implementing that member, provided its type is
/// constructed somewhere in the product (a constructor of it, or the value type itself, is reached from the production
/// roots through the import). The match is by constructed contract: a call through <c>IReadOnlyList&lt;RgbColor&gt;</c>
/// reaches implementations of <c>IReadOnlyList&lt;RgbColor&gt;</c> and of contracts still open in a generic type, never
/// implementations of <c>IReadOnlyList&lt;int&gt;</c>; a call from generic code through an open contract reaches every
/// implementation of it. Calls the framework makes internally (a LINQ operator enumerating its argument) are not seen.
/// </para>
/// <para>
/// Constructors of types the game reads (#1502): the import builds what the game reads, so when the walk reaches a type
/// (the game names it, instantiates it or reaches any of its members) it also reaches the type's instance constructors and
/// what they call. The list then names only logic the game never runs. A constructor this rule marks reached that still
/// decodes or compresses import data remains a defect to move to the import; the rule does not excuse it.
/// </para>
/// </summary>
internal static class CoreImportOnlyReport
{
    /// <summary>The assembly whose declarations are inventoried.</summary>
    private const string CoreAssembly = "SuperMetroid.Core";

    /// <summary>The import pipeline, whose references into Core do not count as the game reading Core.</summary>
    private const string ImportAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>
    /// Writes one tab-separated line per finding: kind (type or member), whether anything outside the restricted walk
    /// reaches it (production through the import, or tools and checks only), the declaration and its source location.
    /// A member is listed only when its containing type is reached, so a whole import-only type appears once.
    /// </summary>
    public static int Write(ReachabilityResult result, string outputFile)
    {
        var declarations = result.Declarations;
        HashSet<string> gameReached = result.Graph.Reach(result.ProductionRoots,
            key => !declarations.TryGetValue(key, out Declaration? declaration) || !declaration.Projects.Contains(ImportAssembly),
            result.ProductionReachable);
        var lines = new List<string>();
        foreach (Declaration declaration in declarations.Values.OrderBy(d => d.File, StringComparer.Ordinal).ThenBy(d => d.Line))
        {
            if (!declaration.Projects.SetEquals([CoreAssembly]) || gameReached.Contains(declaration.Key) ||
                declaration.Shape == DeclarationShape.EnumMember)
                continue;
            bool isType = declaration.Shape == DeclarationShape.Type;
            if (!isType && !gameReached.Contains(declaration.ContainingTypeKey))
                continue;
            string reachedBy = result.ProductionReachable.Contains(declaration.Key) ? "import"
                : result.Reachable.Contains(declaration.Key) ? "tools-and-checks" : "unreached";
            lines.Add(string.Join('\t', isType ? "type" : "member", reachedBy, declaration.Display, $"{declaration.File}:{declaration.Line}"));
        }
        File.WriteAllLines(outputFile, lines);
        Console.WriteLine($"{lines.Count} Core declarations the game does not read; written to {outputFile}.");
        return 0;
    }
}
