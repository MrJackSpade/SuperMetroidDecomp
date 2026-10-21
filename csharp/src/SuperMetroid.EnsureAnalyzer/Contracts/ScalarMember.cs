using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A member and its scalar type.</summary>
/// <param name="Member">The member.</param>
/// <param name="Type">The type.</param>
public readonly record struct ScalarMember(ISymbol Member, ITypeSymbol? Type);
