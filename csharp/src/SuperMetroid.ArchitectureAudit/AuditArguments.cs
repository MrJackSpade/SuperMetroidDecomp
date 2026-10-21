namespace SuperMetroid.ArchitectureAudit;

/// <summary>The solution the audit loads and whether it rewrites the inventories.</summary>
/// <param name="SolutionPath">The solution path.</param>
/// <param name="Write">The write.</param>
internal readonly record struct AuditArguments(string SolutionPath, bool Write);
