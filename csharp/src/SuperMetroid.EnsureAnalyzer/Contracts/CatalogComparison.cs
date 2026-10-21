using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>The catalog members compared and where the first comparison is.</summary>
/// <param name="Names">The names.</param>
/// <param name="First">The first.</param>
internal readonly record struct CatalogComparison(HashSet<string> Names, Location First);
