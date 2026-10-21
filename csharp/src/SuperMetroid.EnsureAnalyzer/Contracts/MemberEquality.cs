using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A member compared for equality with a value.</summary>
/// <param name="Member">The member.</param>
/// <param name="Value">The value.</param>
public readonly record struct MemberEquality(ISymbol Member, long Value);
