using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A write of a domain value at a site.</summary>
/// <param name="Domain">The domain.</param>
/// <param name="Site">The site.</param>
/// <param name="Stepped">The stepped.</param>
/// <param name="Constant">The constant.</param>
internal readonly record struct DomainWrite(ITypeSymbol? Domain, Location Site, bool Stepped, object? Constant);
