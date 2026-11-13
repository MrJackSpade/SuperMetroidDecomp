using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.Declarations;
using static SuperMetroid.ReachabilityAudit.Removal.RelocationSymbols;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The declarations that move, as the later steps of a relocation ask about them: a member moves with its type when the
/// type itself moves, every part of a partial type moves once, and a nested type of a staying owner goes into its adapter.
/// </summary>
internal sealed class RelocationMoves
{
    /// <summary>Gives each symbol its repository key.</summary>
    private readonly SymbolIdentity identity;

    /// <summary>Each move by its symbol's key.</summary>
    private readonly Dictionary<string, RelocationMove> moveBySymbol;

    /// <summary>The keys of the types that move whole.</summary>
    private readonly HashSet<string> movingTypeKeys;

    /// <summary>The moves of <paramref name="found"/>, the selection's moving declarations.</summary>
    /// <param name="found">The declarations the selection moves.</param>
    /// <param name="identity">Gives each symbol its repository key.</param>
    /// <exception cref="InvalidOperationException">A staying extension block would be emptied member by member.</exception>
    public RelocationMoves(IReadOnlyList<RelocationMove> found, SymbolIdentity identity)
    {
        this.identity = identity;
        var movingTypes = found.Where(m => m.Symbol is INamedTypeSymbol).Select(m => (INamedTypeSymbol)m.Symbol)
            .ToHashSet<INamedTypeSymbol>(Comparer);
        // Every part of a partial type moves; other declarations are distinct per symbol.
        var moves = found.Where(m => !Containers(m.Symbol).Any(movingTypes.Contains))
            .GroupBy(m => m.Node).Select(g => g.First()).ToList();
        // Nested types of a staying owner go into its adapter.
        All = [.. moves.Select(m => m.Symbol is INamedTypeSymbol { ContainingType: not null } ? m with { Placement = RelocationPlacement.AdapterMember } : m)];
        moveBySymbol = All.GroupBy(m => identity.Key(m.Symbol) ??
                throw new InvalidOperationException($"Selected symbol {m.Symbol} has no repository identity."))
            .ToDictionary(g => g.Key, g => g.First());
        movingTypeKeys = movingTypes.Select(identity.Key).OfType<string>().ToHashSet();
        RejectEmptiedBlocks(All);
        MovedInterfaces = All.Where(m => m is { Placement: RelocationPlacement.WholeType, Symbol: INamedTypeSymbol { TypeKind: TypeKind.Interface } })
            .Select(m => (INamedTypeSymbol)m.Symbol).ToHashSet<INamedTypeSymbol>(Comparer);
        MovedInterfaceKeys = MovedInterfaces.Select(identity.Key).OfType<string>().ToHashSet();
        MovedSpans = All.GroupBy(m => m.Tree).ToDictionary(g => g.Key, g => g.Select(m => m.Node.FullSpan).ToList());
    }

    /// <summary>Every move, in the order the selection found them.</summary>
    public IReadOnlyList<RelocationMove> All { get; }

    /// <summary>The interfaces that move whole; their staying implementations' adapters implement them.</summary>
    public HashSet<INamedTypeSymbol> MovedInterfaces { get; }

    /// <summary>The keys of <see cref="MovedInterfaces"/>.</summary>
    public HashSet<string> MovedInterfaceKeys { get; }

    /// <summary>The full span of each moved declaration, by tree.</summary>
    public IReadOnlyDictionary<SyntaxTree, List<TextSpan>> MovedSpans { get; }

    /// <summary>The move of a symbol, or null when it stays.</summary>
    public RelocationMove? MoveOf(ISymbol? symbol) => identity.Key(symbol) is { } key && moveBySymbol.TryGetValue(key, out var m) ? m : null;

    /// <summary>Whether a symbol moves, itself or with the type containing it.</summary>
    public bool Moved(ISymbol symbol) => MoveOf(symbol) is not null || Containers(symbol).Any(c => identity.Key(c) is { } k && movingTypeKeys.Contains(k));

    /// <summary>Whether two owners are the same type.</summary>
    public bool SameOwner(INamedTypeSymbol? a, INamedTypeSymbol? b) => a is not null && b is not null && identity.Key(a) == identity.Key(b);

    /// <summary>Whether a span lies inside a moved declaration.</summary>
    public bool InMovedSpan(SyntaxTree tree, TextSpan span) =>
        MovedSpans.TryGetValue(tree, out var spans) && spans.Any(s => s.Contains(span));

    /// <summary>The move whose declaration holds a node, or null.</summary>
    public RelocationMove? EnclosingMove(SyntaxTree tree, SyntaxNode node) =>
        All.FirstOrDefault(m => m.Tree == tree && m.Node.FullSpan.Contains(node.Span));

    /// <summary>Whether an owner implements an interface that moves whole.</summary>
    public bool ImplementsMovedInterface(INamedTypeSymbol owner) => owner.Interfaces.Any(i => MovedInterfaces.Contains(i.OriginalDefinition));

    /// <summary>An owner's repository key.</summary>
    /// <exception cref="InvalidOperationException">The owner is not repository source.</exception>
    public string OwnerKey(INamedTypeSymbol owner) => identity.Key(owner) ??
        throw new InvalidOperationException($"Owner {owner} has no repository identity.");

    /// <summary>
    /// Fails when every member of an extension block that stays would move out of it one by one. A block is reached only
    /// through its members, so a block none of whose members stays is itself selected and moves whole; one emptied member by
    /// member means the audit took it for reachable through another declaration. The 2026-11-13 fixture work found blocks of
    /// one class taking the same receiver sharing one key, so a verification-only block left an empty block in shipped code
    /// (<see cref="SymbolIdentity"/> keys each block by where it is declared).
    /// </summary>
    /// <exception cref="InvalidOperationException">A staying block would be left with no members.</exception>
    private static void RejectEmptiedBlocks(IReadOnlyList<RelocationMove> moves)
    {
        foreach (var group in moves.Where(m => m.Placement == RelocationPlacement.ExtensionMember && m.Node.Parent is ExtensionBlockDeclarationSyntax)
                     .GroupBy(m => (ExtensionBlockDeclarationSyntax)m.Node.Parent!))
            if (group.Key.Members.All(member => group.Any(m => m.Node == member)) &&
                !moves.Any(m => m.Tree == group.First().Tree && m.Node.FullSpan.Contains(group.Key.Span)))
                throw new InvalidOperationException($"Every member of the extension block at {group.Key.GetLocation().GetLineSpan()} " +
                    "would move out of it one by one, leaving the block empty, while the block itself is not selected to move.");
    }
}
