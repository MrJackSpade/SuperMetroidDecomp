using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A field and the operation that initialises it.</summary>
/// <param name="From">The from.</param>
/// <param name="To">The to.</param>
public readonly record struct FieldInitialization(IFieldSymbol From, IOperation To);
