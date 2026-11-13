using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>Captures a selected symbol with the syntax and semantic context needed to rewrite it.</summary>
/// <param name="Symbol">The declared symbol.</param>
/// <param name="Node">Its declaration.</param>
/// <param name="Tree">The tree that holds the declaration.</param>
/// <param name="Model">The tree's semantic model.</param>
/// <param name="Placement">Whether it moves whole, into its owner's adapter, or as an instance extension.</param>
internal sealed record RelocationMove(ISymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model, RelocationPlacement Placement)
{
    /// <summary>
    /// The staying type that receives an adapter for a moved member. A member of an extension block belongs to
    /// the static class declaring the block, not to the block's synthesized extension type.
    /// </summary>
    public INamedTypeSymbol? Owner => Placement == RelocationPlacement.WholeType ? null
        : Symbol.ContainingType is { IsExtension: true, ContainingType: { } declaring } ? declaring : Symbol.ContainingType;

    /// <summary>The receiver parameter of the extension block that declared this member, or null for a type member.</summary>
    public string? ExtensionReceiver => Node.Parent is ExtensionBlockDeclarationSyntax { ParameterList.Parameters: [var receiver] }
        ? receiver.ToString() : null;

    /// <summary>The adapter owner of a member move; whole-type moves have none.</summary>
    public INamedTypeSymbol AdapterOwner => Owner ??
        throw new InvalidOperationException($"{Symbol.Name} moves as a whole type and has no adapter owner.");

    /// <summary>The kind of holder class a member move goes into: instance extensions or static members.</summary>
    public RelocationHolderKind HolderKind =>
        Placement == RelocationPlacement.ExtensionMember ? RelocationHolderKind.Extensions : RelocationHolderKind.Members;
}
