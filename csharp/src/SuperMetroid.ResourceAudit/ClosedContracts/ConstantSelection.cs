namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A parameter and the constant passed to it.</summary>
/// <param name="Parameter">The parameter.</param>
/// <param name="Value">The value.</param>
internal readonly record struct ConstantSelection(string Parameter, object Value);
