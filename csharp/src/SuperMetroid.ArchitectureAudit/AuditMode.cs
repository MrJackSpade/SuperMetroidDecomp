namespace SuperMetroid.ArchitectureAudit;

/// <summary>What one audit run does.</summary>
internal enum AuditMode
{
    /// <summary>The full check: fails on a stale inventory, a stale review and every finding that needs judgment.</summary>
    Check,

    /// <summary>Regenerates both inventories, then fails on every other problem the full check reports, stale reviews included.</summary>
    Write,

    /// <summary>
    /// Fails only on what needs judgment: an unjustified finding, a review without a reason and a Core module cycle.
    /// A stale inventory or review is deterministic and left to the asynchronous cleanup.
    /// </summary>
    Findings,

    /// <summary>Deletes from the source every review whose finding its type no longer has.</summary>
    PruneStaleReviews,
}
