namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>A positional record parameter, named by its record and its own name.</summary>
/// <param name="Record">The record.</param>
/// <param name="Parameter">The parameter.</param>
internal readonly record struct RecordParameter(string Record, string Parameter);
