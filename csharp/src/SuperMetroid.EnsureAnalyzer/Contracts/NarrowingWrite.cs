using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A symbol and the value written to it.</summary>
/// <param name="Target">The target.</param>
/// <param name="Value">The value.</param>
internal readonly record struct NarrowingWrite(ISymbol? Target, IOperation? Value);
