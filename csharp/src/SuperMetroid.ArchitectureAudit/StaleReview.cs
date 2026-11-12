namespace SuperMetroid.ArchitectureAudit;

/// <summary>A review whose finding its type no longer has.</summary>
/// <param name="Type">The reviewed type's inventory key.</param>
/// <param name="Review">The review that justifies a finding the type does not have.</param>
internal sealed record StaleReview(string Type, ArchitectureReview Review);
