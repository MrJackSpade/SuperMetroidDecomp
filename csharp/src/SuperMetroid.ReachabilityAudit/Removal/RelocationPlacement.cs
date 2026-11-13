namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>Whether a declaration moves whole, into an owner adapter, or as an instance extension.</summary>
internal enum RelocationPlacement
{
    /// <summary>Moves a selected top-level type to the tool project while retaining its namespace.</summary>
    WholeType,
    /// <summary>Moves a static or nested member into the staying owner's tooling adapter.</summary>
    AdapterMember,
    /// <summary>Moves an instance method or property into an extension block for its owner.</summary>
    ExtensionMember,
}
