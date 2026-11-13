using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;
using static SuperMetroid.ReachabilityAudit.Removal.RelocationSymbols;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Moves shipped-assembly symbols into a development-only project or test-support, per <see cref="RelocationStyle"/>.
/// <list type="bullet">
/// <item>Top-level types move whole, keeping their namespace, to the same relative path under the target.</item>
/// <item>Static members and nested types of a type that stays move into a per-owner adapter class,
/// <c>&lt;Owner&gt;Tooling</c>, in the owner's namespace. It implements any moved interface the owner
/// implemented and carries <c>[ToolingFor(typeof(Owner))]</c>.</item>
/// <item>Instance methods and properties move into <c>extension(Owner self)</c> blocks.</item>
/// <item>An owner's adapter and extension classes are chosen by <see cref="RelocationHolders"/>, among every class the
/// target's projects already compile: a class of the style's name is merged into or passed over, never declared twice.</item>
/// </list>
/// Moved bodies are rewritten so that references back to the owner resolve; owner members they
/// use that were private become internal, and every modifier the move writes is one its declaration
/// kind takes (<see cref="RelocationModifiers"/>). A member that becomes internal keeps every rule a promotion can break
/// (<see cref="PromotionRules"/>), and an underscore-prefixed one takes its name without the underscore, with every
/// reference rewritten (<see cref="PromotedNames"/>). Every reference and documentation cref in the solution
/// is retargeted, and crefs in shipped assemblies, which cannot see the target, become plain text, as do crefs in moved
/// documentation to members the target cannot see.
/// Anything that cannot move mechanically (instance fields, constructors, indexers, operators,
/// generic owners, mutating struct members) is reported and left in place.
/// <para>
/// A relocation runs in steps, each taking what the earlier ones decided: <see cref="RelocationSelection"/> places each
/// selected declaration or leaves it for manual relocation; <see cref="RelocationMoves"/> settles what moves;
/// <see cref="RelocationHolders.ForMoves"/> chooses each owner's holder classes; the promotion step collects every
/// reference edit (<see cref="ReferenceRewriter"/>), decides what becomes internal and under which name, and collects
/// again with the new names; the accessibility step writes the promoted declarations; and <see cref="RelocationOutput"/>
/// renders the moved members into their files and writes every changed file.
/// </para>
/// </summary>
internal static class SymbolRelocator
{
    /// <summary>The promotion step's outcome: the reference edits, collected with the names promoted members take.</summary>
    /// <param name="Collected">The edits and the members that become internal.</param>
    /// <param name="Renames">The new name of each promoted underscore-prefixed member.</param>
    private sealed record PromotionDecision(ReferenceEdits Collected, RenameMap Renames);

    /// <summary>Moves the selected reachability category into tool source, rewrites references, and reports manual cases.</summary>
    public static void Relocate(LoadedSolution solution, ReachabilityResult result, string category,
        string targetDirectory, IReadOnlySet<string> shippedAssemblies) =>
        Relocate(solution, ReachabilityFindings.Classify(result).Where(f => f.Category == category)
            .Select(f => f.Declaration.Key).ToHashSet(), targetDirectory, shippedAssemblies, RelocationStyle.Tooling);

    /// <summary>Moves the declarations with the given keys into the target, rewrites references, and reports manual cases.</summary>
    public static void Relocate(LoadedSolution solution, IReadOnlySet<string> targets, string targetDirectory,
        IReadOnlySet<string> shippedAssemblies, RelocationStyle style)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource)
                .Select(t => new ProjectTree(Tree:t, p.Compilation, Project:p.Project.AssemblyName)))
            .GroupBy(entry => Path.GetFullPath(entry.Tree.FilePath), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();

        var selection = RelocationSelection.Select(trees, targets, identity);
        var moves = new RelocationMoves(selection.Found, identity);
        var holders = RelocationHolders.ForMoves(solution, moves, targetDirectory, style, identity);
        var decision = DecidePromotions(trees, moves, holders, identity, shippedAssemblies);
        WritePromotions(decision.Collected, decision.Renames, moves, identity);
        var placed = RelocationOutput.Place(moves, decision.Collected, holders, trees, style, targetDirectory, identity);

        foreach (string line in selection.Manual) Console.WriteLine(line);
        Console.WriteLine($"Relocated {moves.All.Count} declarations into {placed.Written} files; deleted {placed.Deleted} emptied files; " +
            $"{decision.Collected.Promotions.Count} members made internal; {decision.Renames.Count} renamed without their underscore; " +
            $"{selection.Manual.Count} left for manual relocation.");
    }

    /// <summary>
    /// The promotion step: what becomes internal decides the names it takes (<see cref="PromotedNames"/>) and must keep every
    /// rule a promotion can break (<see cref="PromotionRules"/>); once names change, the edits are collected again with every
    /// reference written with its new name, and every reference must be.
    /// </summary>
    /// <exception cref="InvalidOperationException">A promotion breaks a rule, or the rename changes what moved code reaches or misses a reference.</exception>
    private static PromotionDecision DecidePromotions(IReadOnlyList<ProjectTree> trees, RelocationMoves moves, RelocationHolders holders,
        SymbolIdentity identity, IReadOnlySet<string> shippedAssemblies)
    {
        var collected = new ReferenceRewriter(trees, moves, holders, identity, shippedAssemblies,
            new RenameMap(new Dictionary<string, string>(StringComparer.Ordinal), identity)).Collect();
        var types = new SourceTypes(identity, trees.Select(entry => entry.Compilation));
        var compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);
        var renames = new RenameMap(PromotedNames.Plan(collected.Widened, trees, types, identity), identity);
        PromotionRules.Reject(collected.Widened, [.. collected.SetterPromotions.OfType<IPropertySymbol>()], symbol => renames.NewName(symbol) ?? symbol.Name,
            types, tree => compilationOf[tree].GetSemanticModel(tree), identity);
        if (renames.Count == 0)
            return new PromotionDecision(collected, renames);
        var rewriter = new ReferenceRewriter(trees, moves, holders, identity, shippedAssemblies, renames);
        var renamed = rewriter.Collect();
        if (!renamed.Widened.SetEquals(collected.Widened))
            throw new InvalidOperationException("Renaming the promoted underscore-prefixed members changed which members moved code reaches.");
        rewriter.RejectUnwrittenRenames();
        return new PromotionDecision(renamed, renames);
    }

    /// <summary>
    /// The accessibility step: staying members used by moved code become internal (<see cref="RelocationModifiers"/>), an
    /// underscore-prefixed one under its name without the underscore (<see cref="PromotedNames"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">A promoted declaration takes no accessibility modifier or cannot be made internal.</exception>
    private static void WritePromotions(ReferenceEdits collected, RenameMap renames, RelocationMoves moves, SymbolIdentity identity)
    {
        foreach (var symbol in collected.SetterPromotions)
            foreach (var syntax in DeclarationsOf(identity, symbol))
                if (syntax is PropertyDeclarationSyntax { AccessorList: { } accessors } property &&
                    !moves.InMovedSpan(property.SyntaxTree, property.Span) &&
                    accessors.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) is { } setter)
                    foreach (var (span, text) in RelocationModifiers.PromotedSetter(setter,
                                 symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.ProtectedOrInternal)))
                        collected.Add(property.SyntaxTree, span, text);
        foreach (var symbol in collected.Promotions)
        {
            if (symbol.DeclaredAccessibility is Accessibility.Internal or Accessibility.Public or Accessibility.ProtectedOrInternal) continue;
            // An accessibility modifier would require the method to be implemented (CS8795), and whether its calls run at all
            // depends on that.
            if (Unimplemented(symbol))
                throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, a partial method without an " +
                    "implementation, from outside its owner, and it cannot be made internal without one.");
            foreach (var syntax in DeclarationsOf(identity, symbol))
            {
                var declaration = syntax is VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax sharedField } ? sharedField : DeclarationNode(syntax);
                if (moves.InMovedSpan(declaration.SyntaxTree, declaration.Span)) continue;
                if (declaration is not MemberDeclarationSyntax member)
                    throw new InvalidOperationException($"{symbol.ToDisplayString()}, which moved code uses, is declared by a " +
                        $"{declaration.Kind()} at {declaration.GetLocation().GetLineSpan()}, which takes no accessibility modifier.");
                foreach (var (span, text) in RelocationModifiers.Promoted(member))
                    collected.Add(declaration.SyntaxTree, span, text);
            }
        }
        foreach (var symbol in collected.Widened)
            if (renames.NewName(symbol) is { } name)
                foreach (var syntax in DeclarationsOf(identity, symbol))
                    collected.Add(syntax.SyntaxTree, syntax switch
                    {
                        VariableDeclaratorSyntax declarator => declarator.Identifier.Span,
                        PropertyDeclarationSyntax property => property.Identifier.Span,
                        _ => throw new InvalidOperationException($"{symbol.ToDisplayString()} is declared by a {syntax.Kind()} at " +
                            $"{syntax.GetLocation().GetLineSpan()}, whose name the rename cannot find."),
                    }, name);
    }
}
