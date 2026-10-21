using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A generic type's or method's type arguments and the ordinals of those whose identity reaches its hash.</summary>
/// <param name="TypeArguments">The type arguments.</param>
/// <param name="Ordinals">The ordinals.</param>
internal readonly record struct HashedTypeArguments(ImmutableArray<ITypeSymbol> TypeArguments, IEnumerable<int> Ordinals);
