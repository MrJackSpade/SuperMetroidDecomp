using Microsoft.CodeAnalysis;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A parameter and the constant passed to it.</summary>
/// <param name="Parameter">The parameter.</param>
/// <param name="Value">The value.</param>
/// <param name="Type">The parameter's type, which decides the number the constant selects.</param>
internal readonly record struct ConstantSelection(string Parameter, object Value, ITypeSymbol Type);
