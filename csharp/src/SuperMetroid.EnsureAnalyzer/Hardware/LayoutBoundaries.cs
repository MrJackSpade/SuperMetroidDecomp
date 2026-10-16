using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Shared boundary tests for the hardware layout analyzers: whether a symbol lies inside a declared boundary, and
/// attribute matching by short type name so fixtures and production declare their own markers.
/// </summary>
internal static class LayoutBoundaries
{
    /// <summary>
    /// Whether <paramref name="symbol"/> lies inside a declaration carrying <paramref name="boundaryAttributeName"/>
    /// (itself, an accessor's property, or any containing member or type) or inside a raw API type's own
    /// implementation (a type carrying <paramref name="accessAttributeName"/>).
    /// </summary>
    internal static bool InBoundary(ISymbol? symbol, string boundaryAttributeName, string accessAttributeName)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingSymbol)
        {
            if (current is INamespaceSymbol)
                return false;
            if (HasAttribute(current, boundaryAttributeName) ||
                current is IMethodSymbol { AssociatedSymbol: { } property } && HasAttribute(property, boundaryAttributeName) ||
                current is INamedTypeSymbol && HasAttribute(current, accessAttributeName))
                return true;
        }
        return false;
    }

    /// <summary>Matches an analyzer marker by its short attribute type name.</summary>
    internal static bool HasAttribute(ISymbol symbol, string name) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == name);
}
