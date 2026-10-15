using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Reporting;

/// <summary>One reported declaration and why it is reported.</summary>
internal sealed record Finding(string Category, Declaration Declaration);
