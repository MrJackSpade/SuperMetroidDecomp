using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Moves check-only helper code, dead top-level types tests still use, whole into test-support: each declaration, every
/// part of a partial type, goes to the same path under the target with its namespace and its file's using directives
/// (<see cref="RelocationOutput"/>). Its name and namespace do not change, so every check naming it binds to it as before.
/// <para>
/// A member of a production type is never moved: nothing in production exists for checks alone, so a dead one is removed
/// (<see cref="SymbolRemover"/>), and the checks and moved code reading it fail to build (owner ruling, #2599). Nothing
/// is reached through reflection, and no production member is widened for moved code: moved code that reaches what
/// test-support cannot see fails to build there. The private-access members of #2570 and the promotions, renames and
/// refusals before them are gone with the member moves they served.
/// </para>
/// <para>
/// The references a move changes are written with it. Documentation staying in a shipped assembly, which cannot see the
/// target, that cites a moved type becomes text: a labelled cref its label, an unlabelled one the name as code, which
/// SME6454 rejects in Core where the sentence must be reworded. Moved documentation citing a member test-support cannot
/// see (private, or private protected) becomes text the same way. A staying type compiled by a project that cannot see the
/// target no longer implements a moved interface; a type every project compiling it sees the target from keeps it.
/// </para>
/// </summary>
internal static class SymbolRelocator
{
    /// <summary>Moves the top-level types with the given keys into the target and writes every changed file.</summary>
    /// <param name="solution">The loaded solution.</param>
    /// <param name="targets">The keys of the types to move.</param>
    /// <param name="targetDirectory">The test-support directory moved code goes into.</param>
    /// <param name="shippedAssemblies">The assemblies that ship to players, whose documentation cannot cite the target.</param>
    /// <exception cref="InvalidOperationException">A key names no top-level type of repository source.</exception>
    public static void Relocate(LoadedSolution solution, IReadOnlySet<string> targets, string targetDirectory, IReadOnlySet<string> shippedAssemblies)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource)
                .Select(t => new ProjectTree(Tree: t, p.Compilation, Project: p.Project.AssemblyName)))
            .GroupBy(entry => Path.GetFullPath(entry.Tree.FilePath), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        var moves = Select(trees, targets, identity);
        var seesTarget = TargetVisibility(solution, targetDirectory);
        var edits = References(trees, moves, identity, shippedAssemblies, seesTarget);
        var placed = RelocationOutput.Place(moves, edits, trees, targetDirectory, identity);
        Console.WriteLine($"Relocated {targets.Count} types ({moves.Count} declarations) into {placed.Written} files; " +
            $"deleted {placed.Deleted} emptied files.");
    }

    /// <summary>Every declaration of every type to move, each part of a partial type among them.</summary>
    /// <exception cref="InvalidOperationException">A key names a nested type, a member, or nothing in repository source.</exception>
    private static List<RelocationMove> Select(IReadOnlyList<ProjectTree> trees, IReadOnlySet<string> targets, SymbolIdentity identity)
    {
        var moves = new List<RelocationMove>();
        foreach (var (tree, compilation, _) in trees)
        {
            var model = SemanticModels.Of(compilation, tree);
            foreach (var node in tree.GetRoot().DescendantNodes(node => node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
                         .Where(node => node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
                if (SymbolIdentity.Normalize(model.GetDeclaredSymbol(node)) is INamedTypeSymbol type && identity.Key(type) is { } key &&
                    targets.Contains(key))
                    moves.Add(new RelocationMove(type, node, tree, model));
        }
        var matched = moves.Select(move => identity.Key(move.Symbol)).ToHashSet(StringComparer.Ordinal);
        if (targets.Where(key => !matched.Contains(key)).Order(StringComparer.Ordinal).ToList() is [_, ..] unmatched)
            throw new InvalidOperationException("The relocation moves top-level types only, and these keys name none in repository source: " +
                string.Join(", ", unmatched));
        return moves;
    }

    /// <summary>
    /// Whether every project compiling a file compiles the target too, so code in it sees what moves there. Test-support
    /// shared through <c>.projitems</c> and the verification projects do; a shipped project does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">No project compiles a file in the target directory.</exception>
    private static Func<string, bool> TargetVisibility(LoadedSolution solution, string targetDirectory)
    {
        string prefix = Path.GetFullPath(targetDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        var targetProjects = solution.Projects.Where(project => project.Compilation.SyntaxTrees.Any(tree =>
            tree.FilePath.Length > 0 && Path.GetFullPath(tree.FilePath).StartsWith(prefix, StringComparison.OrdinalIgnoreCase))).ToList();
        if (targetProjects.Count == 0)
            throw new InvalidOperationException($"No project compiles a file in {targetDirectory}, so code moved there would compile nowhere.");
        var compiledOutsideTarget = solution.Projects.Except(targetProjects)
            .SelectMany(project => project.Compilation.SyntaxTrees.Where(tree => tree.FilePath.Length > 0).Select(tree => Path.GetFullPath(tree.FilePath)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return path => !compiledOutsideTarget.Contains(Path.GetFullPath(path));
    }

    /// <summary>The edits that keep documentation and base lists valid once the types move, by tree.</summary>
    private static Dictionary<SyntaxTree, List<TextEdit>> References(IReadOnlyList<ProjectTree> trees, List<RelocationMove> moves,
        SymbolIdentity identity, IReadOnlySet<string> shippedAssemblies, Func<string, bool> seesTarget)
    {
        var movedKeys = moves.Select(move => identity.Key(move.Symbol)).OfType<string>().ToHashSet(StringComparer.Ordinal);
        bool Moved(ISymbol symbol)
        {
            for (ISymbol? current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
                if (identity.Key(SymbolIdentity.Normalize(current)) is { } key && movedKeys.Contains(key))
                    return true;
            return false;
        }
        bool InMove(SyntaxTree tree, SyntaxNode node) => moves.Any(move => move.Tree == tree && move.Node.FullSpan.Contains(node.Span));
        var edits = new Dictionary<SyntaxTree, List<TextEdit>>();
        void Edit(SyntaxTree tree, TextEdit edit)
        {
            if (!edits.TryGetValue(tree, out var list))
                edits[tree] = list = [];
            list.Add(edit);
        }
        foreach (var (tree, compilation, assembly) in trees)
        {
            bool shipped = shippedAssemblies.Contains(assembly);
            foreach (var cref in tree.GetRoot().DescendantNodes(descendIntoTrivia: true).OfType<CrefSyntax>())
            {
                if (cref.Parent is not XmlCrefAttributeSyntax attribute ||
                    SymbolIdentity.Normalize(SemanticModels.At(compilation, tree, cref).GetSymbolInfo(cref).Symbol) is not { } bound)
                    continue;
                bool inMove = InMove(tree, cref);
                if (Moved(bound) ? shipped && !inMove : inMove && HiddenFromTarget(bound))
                    Edit(tree, AsText(attribute, cref.ToString()));
            }
            if (seesTarget(tree.FilePath))
                continue;
            foreach (var baseList in tree.GetRoot().DescendantNodes().OfType<BaseListSyntax>().Where(baseList => !InMove(tree, baseList)))
            {
                var kept = baseList.Types.Where(entry => SemanticModels.At(compilation, tree, entry.Type).GetTypeInfo(entry.Type).Type is not
                    INamedTypeSymbol { TypeKind: TypeKind.Interface } implemented || !Moved(implemented.OriginalDefinition)).ToList();
                if (kept.Count == baseList.Types.Count)
                    continue;
                var previous = baseList.GetFirstToken().GetPreviousToken();
                Edit(tree, kept.Count == 0 ? new TextEdit(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(previous.Span.End, baseList.Span.End), "")
                    : new TextEdit(baseList.Span, ": " + string.Join(", ", kept.Select(entry => entry.ToString()))));
            }
        }
        return edits;
    }

    /// <summary>Whether test-support cannot see a staying symbol: it, or a type containing it, is private or private protected.</summary>
    private static bool HiddenFromTarget(ISymbol symbol) =>
        symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal ||
        symbol.ContainingType?.OriginalDefinition is { } container && HiddenFromTarget(container);

    /// <summary>A cref element written as text: an empty element's reference as code, a labelled one as its label.</summary>
    /// <param name="attribute">The cref's attribute.</param>
    /// <param name="written">The reference as the text should spell it.</param>
    /// <exception cref="InvalidOperationException">The cref is in an element that is neither empty nor labelled.</exception>
    private static TextEdit AsText(XmlCrefAttributeSyntax attribute, string written) => attribute.Parent switch
    {
        XmlEmptyElementSyntax element => new TextEdit(element.Span, $"<c>{written}</c>"),
        XmlElementStartTagSyntax { Parent: XmlElementSyntax labelled } => new TextEdit(labelled.Span, labelled.Content.ToFullString()),
        _ => throw new InvalidOperationException($"The cref at {attribute.GetLocation().GetLineSpan()} is in an unexpected element."),
    };
}
