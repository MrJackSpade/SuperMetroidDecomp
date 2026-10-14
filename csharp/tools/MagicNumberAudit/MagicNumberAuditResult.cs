namespace SuperMetroid.SourceAudit;

/// <summary>Current audit occurrences, reviewed baseline size, and additions requiring remediation.</summary>
internal sealed record MagicNumberAuditResult(
    int CurrentFindingCount,
    int BaselineCount,
    int RetiredBaselineEntries,
    IReadOnlyList<MagicNumberFinding> CurrentFindings,
    IReadOnlyList<MagicNumberFinding> NewFindings)
{
    /// <summary>Whether the production scan introduced no findings outside the reviewed baseline.</summary>
    public bool Passed => NewFindings.Count == 0;
}
