namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>One missing resource or unresolved audit boundary with its owner and source location.</summary>
internal sealed record AuditFinding(string Code, string Domain, string Owner, string Resource,
    string Source, string Message);
