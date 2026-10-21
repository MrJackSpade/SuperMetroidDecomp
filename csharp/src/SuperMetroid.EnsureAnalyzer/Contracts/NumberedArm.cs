using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A switch arm's index and the member it selects.</summary>
/// <param name="Index">The index.</param>
/// <param name="Member">The member.</param>
public readonly record struct NumberedArm(long Index, ISymbol Member);
