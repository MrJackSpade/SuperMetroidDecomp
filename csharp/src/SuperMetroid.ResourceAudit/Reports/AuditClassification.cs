namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>Recorded classification of a production owner at a source location.</summary>
internal sealed record AuditClassification(string Owner, string Source);
