namespace SuperMetroid.SourceAudit;

/// <summary>Every production finding; each must move to a named owner or carry a reasoned same-line allowance.</summary>
internal sealed record MagicNumberAuditResult(IReadOnlyList<MagicNumberFinding> Findings)
{
    /// <summary>Whether the production scan found no unowned literal.</summary>
    public bool Passed => Findings.Count == 0;
}
