namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>One audited API parameter and its finite production-loader name domain.</summary>
internal sealed record NamedPresentationSelection(string Type, string Method, string Parameter,
    string Domain, IEnumerable<string> Names);
