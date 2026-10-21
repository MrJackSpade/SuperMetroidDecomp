using Microsoft.CodeAnalysis;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>A syntax node and the node that replaces it.</summary>
/// <param name="Old">The old.</param>
/// <param name="New">The new.</param>
public readonly record struct NodeReplacement(SyntaxNode Old, SyntaxNode New);
