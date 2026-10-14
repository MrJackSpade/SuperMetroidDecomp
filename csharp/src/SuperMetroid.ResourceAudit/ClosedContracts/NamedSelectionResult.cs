namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Whether a named presentation call resolves to installed identities.</summary>
internal enum NamedSelectionResult
{
    /// <summary>Every finite candidate belongs to the reviewed installed domain.</summary>
    Resolved,
    /// <summary>A finite candidate names an absent presentation resource.</summary>
    Missing,
    /// <summary>Source flow cannot bound the possible names.</summary>
    Unresolved
}
