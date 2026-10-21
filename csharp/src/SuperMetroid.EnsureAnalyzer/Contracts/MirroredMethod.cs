using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A method, the sides its name mentions and its body.</summary>
/// <param name="Method">The method.</param>
/// <param name="Sides">The sides.</param>
/// <param name="Body">The body.</param>
internal readonly record struct MirroredMethod(IMethodSymbol Method, string Sides, SyntaxNode Body);
