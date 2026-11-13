using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>What <see cref="ReferenceRewriter"/> collected: every edit by tree, and the staying members that become internal.</summary>
/// <param name="byTree">The edits, by tree.</param>
/// <param name="promotions">The staying members moved code reaches from outside their owners.</param>
/// <param name="setterPromotions">The staying properties whose private or protected setter moved code writes.</param>
/// <param name="widened">The promoted members and every field sharing a declaration with one.</param>
/// <param name="reachedBy">The moves that reach each member of <paramref name="widened"/>.</param>
internal sealed class ReferenceEdits(Dictionary<SyntaxTree, List<TextEdit>> byTree, HashSet<ISymbol> promotions, HashSet<ISymbol> setterPromotions,
    HashSet<ISymbol> widened, Dictionary<ISymbol, HashSet<RelocationMove>> reachedBy)
{
    /// <summary>The moves that reach a member that becomes internal: those a refusal of that promotion keeps in place.</summary>
    /// <exception cref="InvalidOperationException">No move reaches the member.</exception>
    public IReadOnlyCollection<RelocationMove> MovesReaching(ISymbol member) =>
        reachedBy.TryGetValue(member, out var moves) && moves.Count > 0 ? moves
            : throw new InvalidOperationException($"No moved code reaches {member.ToDisplayString()}, which becomes internal.");

    /// <summary>The edits, by tree: outside moved spans they apply in place; inside, to the moved text.</summary>
    public Dictionary<SyntaxTree, List<TextEdit>> ByTree => byTree;

    /// <summary>The staying members moved code reaches from outside their owners.</summary>
    public HashSet<ISymbol> Promotions => promotions;

    /// <summary>The staying properties whose private or protected setter moved code writes.</summary>
    public HashSet<ISymbol> SetterPromotions => setterPromotions;

    /// <summary>The promoted members and every field sharing a declaration with one, all of which become internal.</summary>
    public HashSet<ISymbol> Widened => widened;

    /// <summary>Adds an edit to <paramref name="tree"/>.</summary>
    public void Add(SyntaxTree tree, TextSpan span, string text)
    {
        if (!byTree.TryGetValue(tree, out var list)) byTree[tree] = list = [];
        list.Add(new TextEdit(span, text));
    }
}
