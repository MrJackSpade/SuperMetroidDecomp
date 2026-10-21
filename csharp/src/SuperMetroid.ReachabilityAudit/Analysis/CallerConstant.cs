namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A constant a caller passes, keyed by the parameter that receives it.</summary>
/// <param name="Parameter">The parameter.</param>
/// <param name="Value">The value.</param>
internal readonly record struct CallerConstant(string Parameter, string Value);
