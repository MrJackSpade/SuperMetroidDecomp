namespace SuperMetroid.ArchitectureAudit;

/// <summary>An inventory file and the content the audit generates for it.</summary>
/// <param name="Path">The path.</param>
/// <param name="Content">The content.</param>
internal readonly record struct GeneratedInventory(string Path, string Content);
