namespace SuperMetroid.ReachabilityAudit;

/// <summary>One reported declaration and why it is reported.</summary>
internal sealed record Finding(string Category, Declaration Declaration);
