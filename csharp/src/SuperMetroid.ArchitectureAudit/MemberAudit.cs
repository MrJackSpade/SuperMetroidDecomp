namespace SuperMetroid.ArchitectureAudit;

/// <summary>What the test-only-member check found: each unjustified finding, each justified one and each stale member review.</summary>
/// <param name="Problems">One line per finding with no reasoned review on its member, and per review without a reason.</param>
/// <param name="Justified">How many findings a reasoned review on their member justifies.</param>
/// <param name="Stale">The member reviews whose member no longer has the finding they justify.</param>
internal sealed record MemberAudit(IReadOnlyList<string> Problems, int Justified, IReadOnlyList<StaleReview> Stale);
