using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A type switched on by kind, and the diagnostic for it.</summary>
/// <param name="Type">The type.</param>
/// <param name="Diagnostic">The diagnostic.</param>
internal readonly record struct KindSwitch(INamedTypeSymbol Type, Diagnostic Diagnostic);
