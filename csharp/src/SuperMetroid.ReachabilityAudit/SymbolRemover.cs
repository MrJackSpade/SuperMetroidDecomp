using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Deletes the declarations of the selected finding categories from repository source. Every
/// part of a partial declaration is removed; a field declaration only when all of its variables
/// are removed. A positional record parameter is removed together with the matching argument of
/// every construction, record base call and <c>with</c> initializer. Removed instance fields, including auto-property backing fields, are
/// listed so persisted debugger-state layouts can retire them.
/// </summary>
internal static class SymbolRemover
{
    /// <summary>Removes declarations for selected finding categories, optionally limited to repository-relative files.</summary>
    public static void Remove(LoadedSolution solution, ReachabilityResult result, IReadOnlySet<string> categories,
        string? pathPrefix, string retiredFieldsPath) =>
        Remove(solution, ReachabilityFindings.Classify(result)
            .Where(f => categories.Contains(f.Category) && f.Category != ReachabilityFindings.TestSupportUnusedByTools
                && (pathPrefix is null || f.Declaration.File.StartsWith(pathPrefix, StringComparison.Ordinal)))
            .Select(f => f.Declaration.Key).ToHashSet(), retiredFieldsPath);

    /// <summary>Deletes the named declarations, given by their symbol keys.</summary>
    /// <summary>Removes declarations identified by stable symbol keys and records retired instance fields.</summary>
    public static void Remove(LoadedSolution solution, HashSet<string> targets, string retiredFieldsPath)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var retired = new SortedSet<string>(StringComparer.Ordinal);
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int removedNodes = 0, deletedFiles = 0;
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource).Select(t => (Tree: t, p.Compilation)))
            .GroupBy(entry => Path.GetFullPath(entry.Tree.FilePath), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();
        // Positional record properties are constructor parameters: removing one edits the record
        // and every construction of it, so collect those edits across the whole solution first.
        var parameterEdits = PositionalParameterEdits(trees, identity, targets, retired);
        foreach (var (tree, compilation) in trees)
        {
            {
                string path = Path.GetFullPath(tree.FilePath);
                if (!processed.Add(path))
                    continue;
                var model = compilation.GetSemanticModel(tree);
                var root = tree.GetRoot();
                var removals = new List<SyntaxNode>();
                if (parameterEdits.TryGetValue(tree, out var positional))
                    removals.AddRange(positional.Nodes);
                foreach (var node in root.DescendantNodes())
                {
                    if (node is ParameterSyntax
                        || SymbolIdentity.Normalize(DeclarationCollector.DeclaredSymbol(model, node)) is not { } symbol
                        || identity.Key(symbol) is not { } key || !targets.Contains(key))
                        continue;
                    removals.Add(RemovalNode(node, model, identity, targets));
                    if (RetiredField(symbol) is { } field)
                        retired.Add($"{RuntimeName(field.ContainingType)}\t{field.Name}");
                }
                var outermost = removals.Distinct().Where(n => !removals.Any(other => other != n && other.Span.Contains(n.Span) && other.Contains(n))).ToList();
                if (outermost.Count == 0)
                    continue;
                // Removing an enum member must not renumber the members after it: each surviving
                // implicitly valued member that follows a removed one gets its value written out.
                var renumbered = ImplicitlyValuedSurvivors(model, outermost);
                var marked = root.ReplaceNodes(outermost.Concat(renumbered.Keys), (original, rewritten) =>
                    renumbered.TryGetValue(original, out var value)
                        ? ((EnumMemberDeclarationSyntax)rewritten).WithIdentifier(((EnumMemberDeclarationSyntax)rewritten).Identifier.WithTrailingTrivia())
                            .WithEqualsValue(SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space),
                                SyntaxFactory.ParseExpression(value)).WithTrailingTrivia(((EnumMemberDeclarationSyntax)rewritten).Identifier.TrailingTrivia))
                        : rewritten.WithAdditionalAnnotations(Removal));
                var edited = marked.RemoveNodes(marked.GetAnnotatedNodes(Removal), SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepDirectives)!;
                removedNodes += outermost.Count;
                if (edited is CompilationUnitSyntax unit && !unit.DescendantNodes().OfType<MemberDeclarationSyntax>()
                        .Any(m => m is not BaseNamespaceDeclarationSyntax))
                {
                    File.Delete(path);
                    deletedFiles++;
                    continue;
                }
                bool bom = File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];
                string text = edited.ToFullString();
                if (positional is not null)
                    text = RemoveParameterDocumentation(text, positional.DocumentedParameters);
                File.WriteAllText(path, text, new UTF8Encoding(bom));
            }
        }
        File.WriteAllLines(retiredFieldsPath, retired, new UTF8Encoding(false));
        Console.WriteLine($"Removed {removedNodes} declarations of {targets.Count} findings; deleted {deletedFiles} emptied files; " +
            $"{retired.Count} retired instance fields written to {retiredFieldsPath}.");
    }

    /// <summary>The syntax a set of positional-parameter removals takes out of one file.</summary>
    /// <summary>Syntax removals for a tree plus record parameters whose XML entries must be removed.</summary>
    private sealed record PositionalEdits(List<SyntaxNode> Nodes, List<(string Record, string Parameter)> DocumentedParameters);

    /// <summary>
    /// For every targeted positional record parameter: the parameter itself and the matching
    /// argument of each primary-constructor call, record base call and <c>with</c> initializer.
    /// </summary>
    /// <summary>Collects edits for positional record properties and the corresponding constructor arguments.</summary>
    private static Dictionary<SyntaxTree, PositionalEdits> PositionalParameterEdits(
        (SyntaxTree Tree, Compilation Compilation)[] trees, SymbolIdentity identity, HashSet<string> targets,
        SortedSet<string> retired)
    {
        var edits = new Dictionary<SyntaxTree, PositionalEdits>();
        PositionalEdits For(SyntaxTree tree) => edits.TryGetValue(tree, out var e) ? e : edits[tree] = new([], []);

        // Primary constructor documentation id -> its removed parameter ordinals.
        var removedByConstructor = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        foreach (var (tree, compilation) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var parameter in tree.GetRoot().DescendantNodes().OfType<ParameterSyntax>())
            {
                if (parameter.Parent?.Parent is not RecordDeclarationSyntax record ||
                    model.GetDeclaredSymbol(parameter) is not IParameterSymbol symbol ||
                    symbol.ContainingSymbol is not IMethodSymbol constructor)
                    continue;
                var property = constructor.ContainingType.GetMembers(symbol.Name).OfType<IPropertySymbol>().FirstOrDefault();
                if (property is null || identity.Key(property) is not { } key || !targets.Contains(key))
                    continue;
                For(tree).Nodes.Add(parameter);
                For(tree).DocumentedParameters.Add((record.Identifier.Text, symbol.Name));
                string id = constructor.GetDocumentationCommentId()
                    ?? throw new InvalidOperationException($"{constructor.ToDisplayString()} has no documentation id.");
                if (!removedByConstructor.TryGetValue(id, out var ordinals))
                    removedByConstructor[id] = ordinals = [];
                ordinals.Add(symbol.Ordinal);
                if (RetiredField(property) is { } field)
                    retired.Add($"{RuntimeName(field.ContainingType)}\t{field.Name}");
            }
        }
        if (removedByConstructor.Count == 0)
            return edits;

        foreach (var (tree, compilation) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is WithExpressionSyntax with)
                {
                    // `record with { P = value }` writes a removed property: drop that assignment.
                    foreach (var assignment in with.Initializer.Expressions.OfType<AssignmentExpressionSyntax>())
                        if (model.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol property &&
                            identity.Key(property) is { } key && targets.Contains(key))
                            For(tree).Nodes.Add(assignment);
                    continue;
                }
                (ArgumentListSyntax? arguments, ISymbol? called) = node switch
                {
                    BaseObjectCreationExpressionSyntax creation => (creation.ArgumentList, model.GetSymbolInfo(creation).Symbol),
                    PrimaryConstructorBaseTypeSyntax baseType => (baseType.ArgumentList, model.GetSymbolInfo(baseType).Symbol),
                    ConstructorInitializerSyntax initializer => (initializer.ArgumentList, model.GetSymbolInfo(initializer).Symbol),
                    _ => (null, null),
                };
                if (arguments is null || called is not IMethodSymbol constructor ||
                    constructor.OriginalDefinition.GetDocumentationCommentId() is not { } id ||
                    !removedByConstructor.TryGetValue(id, out var removed))
                    continue;
                for (int index = 0; index < arguments.Arguments.Count; index++)
                {
                    var argument = arguments.Arguments[index];
                    int ordinal = argument.NameColon is { } name
                        ? constructor.Parameters.Single(p => p.Name == name.Name.Identifier.Text).Ordinal
                        : index;
                    if (!removed.Contains(ordinal))
                        continue;
                    if (argument.Expression.DescendantNodesAndSelf().Any(HasEffect))
                        Console.WriteLine($"REVIEW removed argument with a call or write: {argument} " +
                            $"({identity.Relative(Path.GetFullPath(tree.FilePath))}:{argument.GetLocation().GetLineSpan().StartLinePosition.Line + 1})");
                    For(tree).Nodes.Add(argument);
                }
            }
        }
        return edits;
    }

    /// <summary>Whether evaluating this expression node could call code or write state.</summary>
    private static bool HasEffect(SyntaxNode node) => node is InvocationExpressionSyntax or AssignmentExpressionSyntax
        or AwaitExpressionSyntax or BaseObjectCreationExpressionSyntax
        || node is PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax &&
            node.RawKind is (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreIncrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreDecrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression;

    /// <summary>Drops the <c>param</c> documentation of removed positional parameters from each record's comment.</summary>
    private static string RemoveParameterDocumentation(string text, List<(string Record, string Parameter)> removed)
    {
        string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
        foreach (var (record, parameter) in removed)
        {
            var declarationPattern = new System.Text.RegularExpressions.Regex($@"\brecord\s+(struct\s+|class\s+)?{record}\b");
            int declaration = lines.FindIndex(declarationPattern.IsMatch);
            if (declaration < 0)
                throw new InvalidOperationException($"Record {record} not found while removing its {parameter} documentation.");
            int first = declaration;
            while (first > 0 && lines[first - 1].TrimStart() is var previous &&
                   (previous.StartsWith("///", StringComparison.Ordinal) || previous.StartsWith('[')))
                first--;
            string tag = $"<param name=\"{parameter}\">";
            int start = lines.FindIndex(first, declaration - first, line => line.Contains(tag, StringComparison.Ordinal));
            if (start < 0)
                continue;
            int end = lines.FindIndex(start, line => line.Contains("</param>", StringComparison.Ordinal));
            lines.RemoveRange(start, end - start + 1);
        }
        return string.Join(newline, lines);
    }

    /// <summary>Marks syntax nodes selected for removal after overlapping edits have been collapsed.</summary>
    private static readonly SyntaxAnnotation Removal = new("reachability-removal");

    /// <summary>Surviving enum members without an initializer that follow a removed member, with their constant value.</summary>
    private static Dictionary<SyntaxNode, string> ImplicitlyValuedSurvivors(SemanticModel model, List<SyntaxNode> removed)
    {
        var survivors = new Dictionary<SyntaxNode, string>();
        foreach (var declaration in removed.OfType<EnumMemberDeclarationSyntax>().Select(m => (EnumDeclarationSyntax)m.Parent!).Distinct())
        {
            bool afterRemoved = false;
            foreach (var member in declaration.Members)
            {
                if (removed.Contains(member))
                {
                    afterRemoved = true;
                    continue;
                }
                if (afterRemoved && member.EqualsValue is null &&
                    model.GetDeclaredSymbol(member) is IFieldSymbol { HasConstantValue: true } field)
                    survivors[member] = Convert.ToString(field.ConstantValue, System.Globalization.CultureInfo.InvariantCulture)!;
            }
        }
        return survivors;
    }

    /// <summary>Promotes a selected variable to its field declaration when every variable is being removed.</summary>
    private static SyntaxNode RemovalNode(SyntaxNode node, SemanticModel model, SymbolIdentity identity, HashSet<string> targets)
    {
        if (node is not VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } || declaration.Parent is not BaseFieldDeclarationSyntax field)
            return node;
        bool allRemoved = declaration.Variables.All(v => identity.Key(model.GetDeclaredSymbol(v)) is { } key && targets.Contains(key));
        return allRemoved ? field : node;
    }

    /// <summary>The reflection full name (nested types joined by '+'), as persisted layouts record it.</summary>
    private static string RuntimeName(INamedTypeSymbol type) => type.ContainingType is { } outer
        ? RuntimeName(outer) + "+" + type.MetadataName
        : type.ContainingNamespace.IsGlobalNamespace ? type.MetadataName : type.ContainingNamespace.ToDisplayString() + "." + type.MetadataName;

    /// <summary>The instance field a removed declaration takes out of persisted object layouts, if any.</summary>
    private static IFieldSymbol? RetiredField(ISymbol symbol) => symbol switch
    {
        IFieldSymbol { IsStatic: false, IsConst: false, ContainingType.TypeKind: TypeKind.Class or TypeKind.Struct } field => field,
        IPropertySymbol { IsStatic: false } property => property.ContainingType.GetMembers().OfType<IFieldSymbol>()
            .FirstOrDefault(f => SymbolEqualityComparer.Default.Equals(f.AssociatedSymbol, property)),
        _ => null,
    };
}
