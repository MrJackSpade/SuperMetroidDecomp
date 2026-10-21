using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A domain type written at a site.</summary>
/// <param name="Domain">The domain.</param>
/// <param name="Site">The site.</param>
internal readonly record struct DomainSite(ITypeSymbol Domain, Location Site);
