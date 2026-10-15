namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>A resource identity supplied directly by compiled definitions.</summary>
internal sealed record AuditCompiledDefinition(string Domain, string Resource);
