namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A method of a presentation type.</summary>
/// <param name="Type">The type.</param>
/// <param name="Method">The method.</param>
internal readonly record struct PresentationMethod(string Type, string Method);
