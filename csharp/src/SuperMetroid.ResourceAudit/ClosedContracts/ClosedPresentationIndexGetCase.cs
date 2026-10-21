namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>The type, method and parameter of one closed presentation index get case.</summary>
/// <param name="Type">The type.</param>
/// <param name="Method">The method.</param>
/// <param name="Parameter">The parameter.</param>
internal readonly record struct ClosedPresentationIndexGetCase(string Type, string Method, string Parameter);
