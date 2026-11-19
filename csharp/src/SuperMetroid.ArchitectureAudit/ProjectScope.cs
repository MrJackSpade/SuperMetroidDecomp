namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// Which assemblies are checks and which are developer tools; every other assembly is shipped product. Shared by the type
/// inventory's scopes and the test-only-member check, so both draw the line between product and checks in one place.
/// </summary>
internal static class ProjectScope
{
    /// <summary>The repository-relative folder of the sources every verification project compiles in.</summary>
    internal const string TestSupportFolder = "csharp/test-support/";

    /// <summary>The developer tools that are neither shipped nor checks.</summary>
    private static readonly HashSet<string> Tools = new(StringComparer.Ordinal)
    {
        "SuperMetroid.DebugRunner", "SuperMetroid.Tooling", "SuperMetroid.AssetExtractor", "SuperMetroid.RoomViewer",
        "SuperMetroid.EnsureAnalyzer", "SuperMetroid.EnsureAnalyzer.CodeFixes",
    };

    /// <summary>
    /// Whether <paramref name="assembly"/> is a check: a verification project, which compiles the test-support access a
    /// private member is reached through. An audit is a developer tool that reaches members directly.
    /// </summary>
    internal static bool IsCheck(string assembly) => assembly.EndsWith("Verification", StringComparison.Ordinal);

    /// <summary>Whether <paramref name="assembly"/> is a developer tool: an audit, or one of the named tools.</summary>
    internal static bool IsTool(string assembly) =>
        Tools.Contains(assembly) || assembly.EndsWith("Audit", StringComparison.Ordinal);

    /// <summary>Whether <paramref name="assembly"/> ships: neither a verification project nor a developer tool.</summary>
    internal static bool IsProduct(string assembly) =>
        !assembly.EndsWith("Verification", StringComparison.Ordinal) && !IsTool(assembly);

    /// <summary>Whether the repository-relative <paramref name="file"/> is shared verification source.</summary>
    internal static bool IsTestSupport(string file) => file.StartsWith(TestSupportFolder, StringComparison.Ordinal);
}
