using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>A source tree and the compilation that compiles it.</summary>
/// <param name="Tree">The tree.</param>
/// <param name="Compilation">The compilation.</param>
internal readonly record struct CompiledTree(SyntaxTree Tree, Compilation Compilation);
