using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>A loaded project and its compilation.</summary>
/// <param name="Project">The project.</param>
/// <param name="Compilation">The compilation.</param>
public readonly record struct ProjectCompilation(Project Project, Compilation Compilation);
