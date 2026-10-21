using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A generic serialization helper and the ordinal of the type argument it serializes.</summary>
/// <param name="Helper">The helper.</param>
/// <param name="Ordinal">The ordinal.</param>
internal readonly record struct GenericHelperArgument(IMethodSymbol Helper, int Ordinal);
