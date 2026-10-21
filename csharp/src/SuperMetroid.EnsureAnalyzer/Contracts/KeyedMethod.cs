using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A method and its structural key.</summary>
/// <param name="Key">The key.</param>
/// <param name="Method">The method.</param>
public readonly record struct KeyedMethod(string Key, IMethodSymbol Method);
