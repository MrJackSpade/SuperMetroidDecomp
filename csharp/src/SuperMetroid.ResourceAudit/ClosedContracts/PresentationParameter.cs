namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A parameter of a presentation method.</summary>
/// <param name="Type">The type.</param>
/// <param name="Method">The method.</param>
/// <param name="Parameter">The parameter.</param>
internal readonly record struct PresentationParameter(string Type, string Method, string Parameter);
