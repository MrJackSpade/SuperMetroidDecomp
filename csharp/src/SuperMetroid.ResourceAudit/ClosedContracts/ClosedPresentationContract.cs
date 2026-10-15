using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A reviewed presentation provider, its qualifying operations,
/// closure rule, and source files whose declarations must remain present.</summary>
internal sealed record ClosedPresentationContract(string Type, string Rule, string[] Methods,
    ReviewedSource[] Sources);
