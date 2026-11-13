namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>Which of an owner's two relocation classes a holder is.</summary>
internal enum RelocationHolderKind
{
    /// <summary>The class holding the owner's moved static members and nested types.</summary>
    Members,
    /// <summary>The class holding the owner's moved instance members in extension blocks.</summary>
    Extensions,
}
