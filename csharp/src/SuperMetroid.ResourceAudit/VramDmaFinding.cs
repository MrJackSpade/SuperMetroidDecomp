namespace SuperMetroid.ResourceAudit;

/// <summary>One unresolved queue site, invalid transfer, or missing source ownership decision.</summary>
internal sealed record VramDmaFinding(string Code, string Owner, string Source, string Detail);
