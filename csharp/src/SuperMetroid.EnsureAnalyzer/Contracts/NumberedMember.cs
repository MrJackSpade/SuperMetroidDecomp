using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A member and its ordinal in a numbered family.</summary>
/// <param name="Member">The member.</param>
/// <param name="Ordinal">The ordinal.</param>
public readonly record struct NumberedMember(ISymbol Member, int Ordinal);
