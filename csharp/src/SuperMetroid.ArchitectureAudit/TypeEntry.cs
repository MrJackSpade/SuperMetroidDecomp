namespace SuperMetroid.ArchitectureAudit;

/// <summary>One source-declared type: where it lives, what it is, its architecture findings and the reviews it declares for them.</summary>
internal sealed record TypeEntry(
    string Type,
    string Kind,
    string Scope,
    IReadOnlyList<string> Assemblies,
    IReadOnlyList<string> Files,
    string Partial,
    string Disposition,
    IReadOnlyList<string> Flags,
    IReadOnlyList<ArchitectureReview> Reviews);
