using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Whether a symbol belongs to this repository. A command-line build sees another project's types as compiled
/// metadata while the IDE sees them as source, so a source-location test alone would enforce a rule across projects
/// in the IDE and only within one project in the build. Repository assemblies share one name prefix.
/// </summary>
internal static class RepositoryOwnership
{
    /// <summary>The name prefix of every assembly this repository builds.</summary>
    internal const string AssemblyPrefix = "SuperMetroid.";

    /// <summary>Whether <paramref name="symbol"/> is declared in source or in a repository assembly.</summary>
    internal static bool Owns(ISymbol symbol) =>
        symbol.Locations.Any(location => location.IsInSource) ||
        symbol.ContainingAssembly?.Name.StartsWith(AssemblyPrefix, StringComparison.Ordinal) == true;
}
