using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using static SuperMetroid.ReachabilityAudit.Removal.RelocationSymbols;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The first step of a relocation: each selected declaration either moves, placed as <see cref="RelocationPlacement"/>
/// says, or stays for manual relocation with a <c>MANUAL</c> line saying why. A declaration that stays keeps in place
/// every selected declaration it uses.
/// </summary>
internal sealed class RelocationSelection
{
    /// <summary>A selected declaration that stays where it is, with the syntax and semantic context to read what it uses.</summary>
    /// <param name="Symbol">The declared symbol.</param>
    /// <param name="Node">Its declaration.</param>
    /// <param name="Tree">The tree that holds the declaration.</param>
    /// <param name="Model">The tree's semantic model.</param>
    private sealed record StayingDeclaration(ISymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model);

    /// <summary>The declarations that move.</summary>
    private readonly List<RelocationMove> found = [];

    /// <summary>The selected declarations that stay, each keeping in place what it uses.</summary>
    private readonly List<StayingDeclaration> staying = [];

    /// <summary>A line for each declaration left for manual relocation, saying why.</summary>
    private readonly List<string> manual = [];

    /// <summary>The declarations that move.</summary>
    public IReadOnlyList<RelocationMove> Found => found;

    /// <summary>A line for each declaration left for manual relocation, saying why.</summary>
    public IReadOnlyList<string> Manual => manual;

    /// <summary>Places every declaration whose key is among <paramref name="targets"/>.</summary>
    /// <param name="trees">Every repository tree, once.</param>
    /// <param name="targets">The keys of the declarations to move.</param>
    /// <param name="identity">Gives each declaration its key.</param>
    public static RelocationSelection Select(IReadOnlyList<ProjectTree> trees, IReadOnlySet<string> targets, SymbolIdentity identity)
    {
        var selection = new RelocationSelection();
        selection.Place(trees, targets, identity);
        return selection;
    }

    /// <summary>
    /// Keeps refused moves in place for manual relocation, each with a <c>MANUAL</c> line saying why, and then whatever a
    /// declaration that now stays uses. Every part of a refused declaration stays with it. Nothing has been edited yet: the
    /// relocation decides its moves again from what is left.
    /// </summary>
    /// <param name="refused">The moves whose promotions are refused, and why.</param>
    /// <param name="identity">Gives each declaration its key.</param>
    /// <exception cref="InvalidOperationException">A refused move is not among the moving declarations.</exception>
    public void Keep(IReadOnlyList<RefusedMove> refused, SymbolIdentity identity)
    {
        foreach (var (move, reason) in refused)
        {
            string? key = identity.Key(move.Symbol);
            var parts = found.Where(candidate => identity.Key(candidate.Symbol) == key).ToList();
            if (parts.Count == 0)
                throw new InvalidOperationException($"{move.Symbol.ToDisplayString()}, which a refused promotion keeps in place, does not move.");
            foreach (var part in parts)
            {
                found.Remove(part);
                staying.Add(new(part.Symbol, part.Node, part.Tree, part.Model));
            }
            manual.Add($"MANUAL {move.Symbol.Kind} {move.Symbol.ToDisplayString()} (kept in place: it uses {reason}, {Location(identity, move.Node)})");
        }
        KeepWhatStayingDeclarationsUse(identity, found, staying, manual);
    }

    /// <summary>Places each selected declaration, keeps in place what a staying one uses, and reports keys matching none.</summary>
    private void Place(IReadOnlyList<ProjectTree> trees, IReadOnlySet<string> targets, SymbolIdentity identity)
    {
        foreach (var (tree, compilation, _) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is ParameterSyntax || SymbolIdentity.Normalize(DeclarationCollector.DeclaredSymbol(model, node)) is not { } symbol
                    || identity.Key(symbol) is not { } key || !targets.Contains(key))
                    continue;
                if (node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: > 1 } })
                {
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} (shared declaration, {Location(identity, node)})");
                    staying.Add(new(symbol, DeclarationNode(node), tree, model));
                }
                else if (Classify(symbol, node) is { } placement)
                {
                    found.Add(new(symbol, DeclarationNode(node), tree, model, placement));
                }
                else
                {
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} ({Location(identity, node)})");
                    staying.Add(new(symbol, DeclarationNode(node), tree, model));
                }
            }
        }
        KeepWhatStayingDeclarationsUse(identity, found, staying, manual);
        var seen = found.Select(m => identity.Key(m.Symbol)).ToHashSet();
        foreach (string key in targets.Where(k => !seen.Contains(k) && !manual.Any(m => m.Contains(k.Split('@')[0][2..], StringComparison.Ordinal))))
            manual.Add($"MANUAL unmatched declaration {key}");
    }

    /// <summary>
    /// Keeps in place every selected declaration that a declaration staying in place uses, until none is left: a selected
    /// declaration left for manual relocation stays in its assembly, so moving a declaration it uses would leave it
    /// referring to code its assembly no longer holds. The 2026-11-12 cleanup at d44b823793 moved
    /// InstalledImagePlanesConstants out of AssetExtraction while InstalledImagePlanes.CopyTo, which uses it, stayed for
    /// manual relocation, and the tree no longer compiled. A staying declaration inside a type that moves whole moves with
    /// that type, so it holds nothing back while that type moves.
    /// </summary>
    private static void KeepWhatStayingDeclarationsUse(SymbolIdentity identity, List<RelocationMove> found, List<StayingDeclaration> staying,
        List<string> manual)
    {
        bool Holds(RelocationMove move, ISymbol used) => identity.Key(move.Symbol) is { } key &&
            (identity.Key(used) == key || (move.Symbol is INamedTypeSymbol && Containers(used).Any(type => identity.Key(type) == key)));
        bool MovesWithType(StayingDeclaration declaration) => found.Any(move => move.Symbol is INamedTypeSymbol &&
            move.Tree == declaration.Tree && move.Node.FullSpan.Contains(declaration.Node.Span));
        bool kept = true;
        while (kept)
        {
            kept = false;
            foreach (StayingDeclaration declaration in staying.ToList())
            {
                if (MovesWithType(declaration))
                    continue;
                foreach (SyntaxNode use in declaration.Node.DescendantNodes())
                {
                    if (SymbolIdentity.Normalize(declaration.Model.GetSymbolInfo(use).Symbol) is not { } used)
                        continue;
                    foreach (RelocationMove move in found.Where(move => Holds(move, used)).ToList())
                    {
                        found.Remove(move);
                        manual.Add($"MANUAL {move.Symbol.Kind} {move.Symbol.ToDisplayString()} (kept in place: " +
                            $"{declaration.Symbol.ToDisplayString()} stays and uses it, {Location(identity, move.Node)})");
                        staying.Add(new(move.Symbol, move.Node, move.Tree, move.Model));
                        kept = true;
                    }
                }
            }
        }
    }

    /// <summary>Places supported declarations and leaves unsafe shapes for manual relocation.</summary>
    private static RelocationPlacement? Classify(ISymbol symbol, SyntaxNode node)
    {
        if (symbol is INamedTypeSymbol type)
            return type.ContainingType is { IsGenericType: true } ? null : RelocationPlacement.WholeType;
        if (symbol.ContainingType is not { } owner || owner.IsGenericType) return null;
        // A member of an extension block keeps its receiver: it moves into a block with that same receiver.
        if (owner.IsExtension)
            return symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } or IPropertySymbol { IsIndexer: false }
                ? RelocationPlacement.ExtensionMember : null;
        // An enum value cannot leave its enum.
        if (owner.TypeKind == TypeKind.Enum) return null;
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: true }:
            case IPropertySymbol { IsStatic: true, IsIndexer: false }:
            case IFieldSymbol { IsStatic: true }:
                return RelocationPlacement.AdapterMember;
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false, IsAbstract: false, IsVirtual: false, IsOverride: false } method
                when method.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node) && !ReadsPrimaryConstructorParameter(owner, node):
            case IPropertySymbol { IsStatic: false, IsIndexer: false, IsAbstract: false, IsVirtual: false, IsOverride: false, SetMethod: null } property
                when property.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node)
                    && !HasBackingField(property) && !ReadsPrimaryConstructorParameter(owner, node):
                return RelocationPlacement.ExtensionMember;
            default:
                return null;
        }
    }

    /// <summary>Rejects auto-properties whose compiler-generated storage cannot move as an extension.</summary>
    private static bool HasBackingField(IPropertySymbol property) => property.ContainingType.GetMembers()
        .OfType<IFieldSymbol>().Any(f => Comparer.Equals(f.AssociatedSymbol, property));

    /// <summary>
    /// Rejects members that read a primary-constructor parameter the owner captures: an extension outside the type cannot
    /// reach it. A same-named local is rejected too, which only leaves the member for manual relocation.
    /// </summary>
    private static bool ReadsPrimaryConstructorParameter(INamedTypeSymbol owner, SyntaxNode node)
    {
        HashSet<string> parameters = owner.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>().SelectMany(type => type.ParameterList?.Parameters ?? [])
            .Select(parameter => parameter.Identifier.ValueText).ToHashSet(StringComparer.Ordinal);
        return parameters.Count != 0 && node.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Any(name => parameters.Contains(name.Identifier.ValueText));
    }

    /// <summary>Rejects struct members that assign into state because an extension receiver would copy it.</summary>
    private static bool MutatesStruct(INamedTypeSymbol owner, SyntaxNode node) =>
        owner.TypeKind == TypeKind.Struct && node.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any();
}
