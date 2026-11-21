using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>One declaration of a top-level type that moves whole into test-support.</summary>
/// <param name="Symbol">The declared type.</param>
/// <param name="Node">Its declaration: one part of a partial type.</param>
/// <param name="Tree">The tree that holds the declaration.</param>
/// <param name="Model">The tree's semantic model.</param>
internal sealed record RelocationMove(INamedTypeSymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model);
