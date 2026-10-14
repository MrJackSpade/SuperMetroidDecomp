namespace SuperMetroid.ResourceAudit;

/// <summary>One production consumer and how the audit resolved its resource reference.</summary>
internal sealed record AuditConsumer(string Owner, string Source,     string Resolution);
