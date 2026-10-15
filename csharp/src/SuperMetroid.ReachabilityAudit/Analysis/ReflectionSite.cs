namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A reflection lookup site, formatted as repository file, line and source text.</summary>
internal sealed record ReflectionSite(string File, int Line, string Text)
{
    /// <summary>Formats a lookup site as a tab-separated path, line, and compact source expression.</summary>
    public override string ToString() => $"{File}\t{Line}\t{Text}";
}
