using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A bool parameter and the literal passed to it, if any.</summary>
/// <param name="Parameter">The parameter.</param>
/// <param name="Literal">The literal.</param>
public readonly record struct BoolArgument(IParameterSymbol Parameter, bool? Literal);
