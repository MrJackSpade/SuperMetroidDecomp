using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>One entry of an identity table: the member it is keyed by and the value Core runs or reads for that member.</summary>
/// <param name="Key">The identity member the entry is keyed by.</param>
/// <param name="Value">The entry's value: the code a dispatch runs for the member.</param>
/// <param name="Semantic">The semantic model the value binds in.</param>
internal readonly record struct IdentityTableEntry(IFieldSymbol Key, ExpressionSyntax Value, SemanticModel Semantic);

/// <summary>
/// Reads Core's dispatch through identity tables (#2553): a member typed <c>IdentityTable&lt;TIdentity, TValue&gt;</c> built over
/// a dictionary naming each identity member's own value, which Core indexes with the value deciding the member (a projectile
/// behaviour's <c>Move</c> indexing its motion handlers with the moving object's motion, a cue handler its cases with the
/// cue or the cue's kind). Each entry's value then runs only for its key, as a switch section runs only for its labels, so
/// the readers that split a switch by its sections split such a table by its entries.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class IdentityTableDispatch(CoreModel model)
{
    private List<(ISymbol Table, ExpressionSyntax Index, SemanticModel Semantic)>? _lookups;
    private readonly Dictionary<ISymbol, IReadOnlyList<IdentityTableEntry>> _entries = new(SymbolEqualityComparer.Default);

    /// <summary>Whether <paramref name="symbol"/> is a property or field holding an identity table.</summary>
    public bool IsTable(ISymbol? symbol) =>
        (symbol switch { IPropertySymbol property => property.Type, IFieldSymbol field => field.Type, _ => null }) is INamedTypeSymbol type
        && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.IdentityTable);

    /// <summary>The lookups of identity tables in <paramref name="root"/>: each table member indexed, with the index.</summary>
    public IEnumerable<(ISymbol Table, ExpressionSyntax Index)> LookupsIn(SemanticModel semantic, SyntaxNode root)
    {
        foreach (ElementAccessExpressionSyntax access in root.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>())
        {
            if (access.ArgumentList.Arguments is [var index] && semantic.GetSymbolInfo(access.Expression).Symbol is { } table && IsTable(table))
                yield return (table.OriginalDefinition, index.Expression);
        }
    }

    /// <summary>
    /// The entries of every identity table Core indexes with a cue of <paramref name="cueType"/> or with a member of one (its
    /// kind): each entry runs only for the cue its key names.
    /// </summary>
    public IEnumerable<IdentityTableEntry> CueEntries(INamedTypeSymbol cueType)
    {
        var seen = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach ((ISymbol table, ExpressionSyntax index, SemanticModel semantic) in Lookups())
        {
            bool byCue = IsCue(semantic, index, cueType) || (index is MemberAccessExpressionSyntax { Expression: var owner } && IsCue(semantic, owner, cueType));
            if (byCue && seen.Add(table))
            {
                foreach (IdentityTableEntry entry in Entries(table))
                    yield return entry;
            }
        }
    }

    /// <summary>
    /// The entries <paramref name="table"/> is built with, each keyed by a member of its identity; none when it is built other
    /// than from a dictionary naming its members (a function making every member's value alike).
    /// </summary>
    public IReadOnlyList<IdentityTableEntry> Entries(ISymbol table)
    {
        if (_entries.TryGetValue(table, out IReadOnlyList<IdentityTableEntry>? known))
            return known;
        var entries = new List<IdentityTableEntry>();
        INamedTypeSymbol type = (INamedTypeSymbol)(table switch { IPropertySymbol property => property.Type, IFieldSymbol field => field.Type, _ => throw new ArgumentException($"{table} holds no identity table.", nameof(table)) });
        ITypeSymbol identity = type.TypeArguments[0];
        foreach (SyntaxReference reference in table.DeclaringSyntaxReferences)
        {
            SyntaxNode declaration = reference.GetSyntax();
            SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
            // The first keyed initializer of the declaration is the dictionary the table is built over; the entries' own
            // values may hold further initializers.
            foreach (InitializerExpressionSyntax initializer in declaration.DescendantNodes().OfType<InitializerExpressionSyntax>())
            {
                var keyed = new List<IdentityTableEntry>();
                foreach (ExpressionSyntax expression in initializer.Expressions)
                {
                    if (expression is AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax { ArgumentList.Arguments: [var key] }, Right: var value }
                        && semantic.GetSymbolInfo(key.Expression).Symbol is IFieldSymbol member
                        && SymbolEqualityComparer.Default.Equals(member.ContainingType, identity))
                    {
                        keyed.Add(new IdentityTableEntry(member.OriginalDefinition, value, semantic));
                    }
                }
                if (keyed.Count > 0 && keyed.Count == initializer.Expressions.Count)
                {
                    entries.AddRange(keyed);
                    break;
                }
            }
        }
        _entries.Add(table, entries);
        return entries;
    }

    /// <summary>
    /// Whether <paramref name="name"/> is the key of an entry of a dictionary an identity table is built over: a label naming
    /// the member the entry is for, which neither starts nor gives that member.
    /// </summary>
    public bool IsEntryKey(SemanticModel semantic, ExpressionSyntax name)
    {
        SyntaxNode node = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        return node.Parent is ArgumentSyntax { Parent: BracketedArgumentListSyntax { Parent: ImplicitElementAccessSyntax key } }
            && key.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax { Parent: BaseObjectCreationExpressionSyntax dictionary } } entry
            && entry.Left == key
            && dictionary.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: BaseObjectCreationExpressionSyntax built } }
            && semantic.GetTypeInfo(built).Type is INamedTypeSymbol type
            && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, model.IdentityTable);
    }

    /// <summary>Every identity-table lookup in Core, read once.</summary>
    private List<(ISymbol Table, ExpressionSyntax Index, SemanticModel Semantic)> Lookups()
    {
        if (_lookups is not null)
            return _lookups;
        _lookups = [];
        foreach (SyntaxTree tree in model.Compilation.SyntaxTrees)
        {
            SyntaxNode root = tree.GetRoot();
            // Only a file indexing something can look a table up; the rest need no binding.
            if (!root.DescendantNodes().OfType<ElementAccessExpressionSyntax>().Any())
                continue;
            SemanticModel semantic = model.ModelOf(tree);
            foreach ((ISymbol table, ExpressionSyntax index) in LookupsIn(semantic, root))
                _lookups.Add((table, index, semantic));
        }
        return _lookups;
    }

    /// <summary>Whether <paramref name="expression"/> is a cue of <paramref name="cueType"/>: of that type or of a case deriving from it.</summary>
    private static bool IsCue(SemanticModel semantic, ExpressionSyntax expression, INamedTypeSymbol cueType)
    {
        for (ITypeSymbol? type = semantic.GetTypeInfo(expression).Type; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, cueType))
                return true;
        }
        return false;
    }
}
