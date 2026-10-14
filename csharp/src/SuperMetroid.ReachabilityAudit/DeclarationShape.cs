namespace SuperMetroid.ReachabilityAudit;

/// <summary>How a declaration is reported; enum members and fields have their own categories.</summary>
internal enum DeclarationShape
{
    /// <summary>A named type declaration is reachable as a symbol root or reference target.</summary>
    Type,
    /// <summary>A callable, property, event, or indexer declaration.</summary>
    Member,
    /// <summary>A field whose reachability may also be implied by layout or containing type use.</summary>
    Field,
    /// <summary>An enum constant reported separately from ordinary fields.</summary>
    EnumMember,
}
