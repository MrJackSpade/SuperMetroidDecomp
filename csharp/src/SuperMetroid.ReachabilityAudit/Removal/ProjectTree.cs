using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>A source tree, the compilation that compiles it and that project's assembly name.</summary>
/// <param name="Tree">The tree.</param>
/// <param name="Compilation">The compilation.</param>
/// <param name="Project">The project.</param>
internal readonly record struct ProjectTree(SyntaxTree Tree, Compilation Compilation, string Project);
