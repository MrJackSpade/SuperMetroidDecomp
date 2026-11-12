namespace SuperMetroid.ArchitectureAudit;

/// <summary>The solution the audit loads and what the run does with it.</summary>
/// <param name="SolutionPath">The solution path.</param>
/// <param name="Mode">Whether the run checks everything, rewrites the inventories, checks only the findings that need
/// judgment, or prunes stale reviews.</param>
internal readonly record struct AuditArguments(string SolutionPath, AuditMode Mode);
