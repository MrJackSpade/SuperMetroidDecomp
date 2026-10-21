using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A subject compared with a catalog's members.</summary>
/// <param name="Subject">The subject.</param>
/// <param name="Catalog">The catalog.</param>
internal readonly record struct CatalogComparisonKey(ISymbol Subject, INamedTypeSymbol Catalog);
