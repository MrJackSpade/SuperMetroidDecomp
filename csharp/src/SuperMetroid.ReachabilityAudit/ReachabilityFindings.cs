namespace SuperMetroid.ReachabilityAudit;

/// <summary>One reported declaration and why it is reported.</summary>
internal sealed record Finding(string Category, Declaration Declaration);

/// <summary>
/// Classifies declarations into report categories. Members of an unreachable type are reported
/// once, as the type; shipped code reached only by tools is listed separately from dead code.
/// </summary>
internal static class ReachabilityFindings
{
    public const string UnreachableType = "unreachable-type";
    public const string UnreachableMember = "unreachable-member";
    public const string UnreferencedField = "unreferenced-field";
    public const string UnreferencedEnumMember = "unreferenced-enum-member";
    public const string SerializationOnly = "serialization-only";
    public const string ReachedOnlyByTools = "production-reached-only-by-tools";
    /// <summary>Unused by tools in a source file verification projects also compile: unlink it from the tool.</summary>
    public const string TestSupportUnusedByTools = "test-support-unused-by-tools";

    /// <summary>Assemblies that ship to players; symbols here must be reachable from the player hosts.</summary>
    internal static readonly HashSet<string> ProductionAssemblies =
        ["SuperMetroid.Core", "SuperMetroid.AssetExtraction", "SuperMetroid.Diagnostics", "SuperMetroid.Desktop",
         "SuperMetroid.Rendering.Direct3D11", "SuperMetroid.Game", "SuperMetroid.Android"];

    public static IReadOnlyList<Finding> Classify(ReachabilityResult result)
    {
        var declarations = result.Declarations;
        var unreachableTypes = declarations.Values
            .Where(d => d.Shape == DeclarationShape.Type && !result.Reachable.Contains(d.Key)).Select(d => d.Key).ToHashSet();
        bool InsideUnreachableType(Declaration d) => Containers(declarations, d).Any(unreachableTypes.Contains);

        var findings = new List<Finding>();
        foreach (var d in declarations.Values.Where(d => !InsideUnreachableType(d)))
        {
            string? category = d.Shape == DeclarationShape.EnumMember
                ? result.Referenced.Contains(d.Key) ? null : UnreferencedEnumMember
                : result.Reachable.Contains(d.Key) ? null
                : d.Shape switch
                {
                    DeclarationShape.Type => UnreachableType,
                    DeclarationShape.Member or DeclarationShape.Field
                        when result.SerializerAccessed.Contains(d.Key) => SerializationOnly,
                    DeclarationShape.Field => UnreferencedField,
                    DeclarationShape.Member => UnreachableMember,
                    DeclarationShape.EnumMember =>
                        throw new InvalidOperationException("Enum members are classified by reference above."),
                    _ => throw new InvalidOperationException($"Undefined declaration shape {d.Shape}."),
                };
            if (category is not null)
                findings.Add(new(result.VerificationSourceFiles.Contains(d.File) ? TestSupportUnusedByTools : category, d));
        }

        var toolOnly = declarations.Values.Where(d => result.Reachable.Contains(d.Key) && !result.ProductionReachable.Contains(d.Key)
            && d.Shape != DeclarationShape.EnumMember && d.Projects.All(ProductionAssemblies.Contains)).ToList();
        var toolOnlyTypes = toolOnly.Where(d => d.Shape == DeclarationShape.Type).Select(d => d.Key).ToHashSet();
        findings.AddRange(toolOnly.Where(d => !Containers(declarations, d).Any(toolOnlyTypes.Contains))
            .Select(d => new Finding(ReachedOnlyByTools, d)));
        return findings;
    }

    private static IEnumerable<string> Containers(IReadOnlyDictionary<string, Declaration> declarations, Declaration declaration)
    {
        for (string key = declaration.ContainingTypeKey; key.Length > 0;
             key = declarations.TryGetValue(key, out var container) ? container.ContainingTypeKey : "")
            yield return key;
    }
}
