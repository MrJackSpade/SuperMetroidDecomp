using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A record and its structural key.</summary>
/// <param name="Key">The key.</param>
/// <param name="Record">The record.</param>
public readonly record struct KeyedRecord(string Key, INamedTypeSymbol Record);
