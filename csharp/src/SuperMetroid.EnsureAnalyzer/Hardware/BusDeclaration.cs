using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>A declared symbol, its type and what kind of declaration it is.</summary>
/// <param name="Symbol">The symbol.</param>
/// <param name="Type">The type.</param>
/// <param name="Kind">The kind.</param>
internal readonly record struct BusDeclaration(ISymbol? Symbol, ITypeSymbol? Type, string Kind);
