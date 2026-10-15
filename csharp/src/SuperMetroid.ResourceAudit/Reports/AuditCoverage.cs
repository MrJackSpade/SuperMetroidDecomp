namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>Reference and export totals for one resource identity domain.</summary>
internal sealed record AuditCoverage(string Domain, int References, int Exports);
