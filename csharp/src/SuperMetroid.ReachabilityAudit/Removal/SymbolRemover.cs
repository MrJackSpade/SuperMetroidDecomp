using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Deletes the declarations of the selected finding categories from repository source. Every
/// part of a partial declaration is removed; a field declaration only when all of its variables
/// are removed. A positional record parameter is removed together with the matching argument of
/// every construction, record base call and <c>with</c> initializer; one only its base call reads becomes a constructor
/// parameter of a record that loses its parameter list, and one read anywhere else goes too, so the build fails at the
/// read (#2599). Removed instance fields, including auto-property backing fields, are listed so persisted debugger-state
/// layouts can retire them.
/// <para>
/// Nothing selected is kept: a removal that leaves the code not compiling is applied, and the build fails where the
/// developer has a choice to make (#2599). Documentation that cites a removed declaration keeps its name as code text, which
/// SME6454 rejects in Core where a sentence must be reworded.
/// </para>
/// </summary>
internal static class SymbolRemover
{
    /// <summary>Selects removable findings in the requested categories and source prefix, excluding test-support-only findings.</summary>
    public static void Remove(LoadedSolution solution, ReachabilityResult result, IReadOnlySet<string> categories,
        string? pathPrefix, string retiredFieldsPath) =>
        Remove(solution, ReachabilityFindings.Classify(result)
            .Where(f => categories.Contains(f.Category) && f.Category != ReachabilityFindings.TestSupportUnusedByTools
                && (pathPrefix is null || f.Declaration.File.StartsWith(pathPrefix, StringComparison.Ordinal)))
            .Select(f => f.Declaration.Key).ToHashSet(), retiredFieldsPath);

    /// <summary>Deletes the named declarations, given by their symbol keys.</summary>
    public static void Remove(LoadedSolution solution, HashSet<string> targets, string retiredFieldsPath)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var retired = new SortedSet<string>(StringComparer.Ordinal);
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int removedNodes = 0, deletedFiles = 0;
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource).Select(t => new CompiledTree(Tree:t, p.Compilation)))
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
                var model = SemanticModels.Of(compilation, tree);
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
                // Documentation that links a removed declaration keeps its name as code text.
                var orphanedLinks = root.DescendantNodes(descendIntoTrivia: true).OfType<XmlEmptyElementSyntax>()
                    .Where(element => element.Attributes.OfType<XmlCrefAttributeSyntax>().FirstOrDefault() is { } cref
                        && !outermost.Any(n => n.FullSpan.Contains(element.Span))
                        && Removed(SymbolIdentity.Normalize(model.GetSymbolInfo(cref.Cref).Symbol), identity, targets))
                    .ToList();
                if (outermost.Count == 0 && orphanedLinks.Count == 0 && positional?.Conversions.Count is not > 0)
                    continue;
                // Removing an enum member must not renumber the members after it: each surviving
                // implicitly valued member that follows a removed one gets its value written out.
                var renumbered = ImplicitlyValuedSurvivors(model, outermost);
                var marked = root.ReplaceNodes(outermost.Concat(renumbered.Keys).Concat(orphanedLinks), (original, rewritten) =>
                    original is XmlEmptyElementSyntax link
                        ? SyntaxFactory.XmlElement("c", SyntaxFactory.SingletonList<XmlNodeSyntax>(
                            SyntaxFactory.XmlText(link.Attributes.OfType<XmlCrefAttributeSyntax>().First().Cref.ToString())))
                    : renumbered.TryGetValue(original, out var value)
                        ? ((EnumMemberDeclarationSyntax)rewritten).WithIdentifier(((EnumMemberDeclarationSyntax)rewritten).Identifier.WithTrailingTrivia())
                            .WithEqualsValue(SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.Token(SyntaxKind.EqualsToken).WithLeadingTrivia(SyntaxFactory.Space).WithTrailingTrivia(SyntaxFactory.Space),
                                SyntaxFactory.ParseExpression(value)).WithTrailingTrivia(((EnumMemberDeclarationSyntax)rewritten).Identifier.TrailingTrivia))
                        : rewritten.WithAdditionalAnnotations(Removal));
                var edited = marked.RemoveNodes(marked.GetAnnotatedNodes(Removal), SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepDirectives);
                removedNodes += outermost.Count;
                if (edited is CompilationUnitSyntax unit && !unit.DescendantNodes().OfType<MemberDeclarationSyntax>()
                        .Any(m => m is not BaseNamespaceDeclarationSyntax))
                {
                    File.Delete(path);
                    deletedFiles++;
                    continue;
                }
                bool bom = File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];
                string text = (edited ?? throw new InvalidOperationException($"{path}: removing members deleted the whole syntax tree."))
                    .ToFullString();
                if (positional is not null)
                {
                    text = RemoveParameterDocumentation(text, positional.DocumentedParameters);
                    text = PositionalRecordConversion.Apply(text, (CSharpParseOptions)tree.Options, positional.Conversions, path);
                }
                File.WriteAllText(path, text, new UTF8Encoding(bom));
            }
        }
        File.WriteAllLines(retiredFieldsPath, retired, new UTF8Encoding(false));
        Console.WriteLine($"Removed {removedNodes} declarations of {targets.Count} findings; deleted {deletedFiles} emptied files; " +
            $"{retired.Count} retired instance fields written to {retiredFieldsPath}.");
    }

    /// <summary>Whether a symbol is removed itself or inside a removed type.</summary>
    private static bool Removed(ISymbol? symbol, SymbolIdentity identity, HashSet<string> targets)
    {
        for (var current = symbol; current is not null and not INamespaceSymbol; current = current.ContainingSymbol)
            if (identity.Key(SymbolIdentity.Normalize(current)) is { } key && targets.Contains(key))
                return true;
        return false;
    }

    /// <summary>The syntax a set of positional-parameter removals takes out of one file, and the records it converts.</summary>
    private sealed record PositionalEdits(List<SyntaxNode> Nodes, List<RecordParameter> DocumentedParameters, List<RecordConversion> Conversions);

    /// <summary>
    /// For every targeted positional record parameter: the parameter itself and the matching
    /// argument of each primary-constructor call, record base call and <c>with</c> initializer.
    /// </summary>
    /// <remarks>
    /// A positional parameter is also a constructor parameter, and code of its record can read it as one (#2599): the
    /// base call, an initializer, a member. A dead generated property whose parameter is read is retired without the
    /// read: when every positional property of the record is dead and only the base call reads parameters, the record
    /// becomes one without a parameter list whose constructor takes the parameters read and passes them to its base
    /// (<see cref="PositionalRecordConversion"/>). Otherwise the parameter goes anyway, and the build fails at each read
    /// (CS0103): moving the code that reads it into a constructor is a rewrite of the record, a choice the developer makes
    /// where the build stops, and the prune never keeps a site it was asked to remove. The cleanup of 0ea41f339b deleted
    /// <c>KraidColours.BandShade</c>'s parameter <c>Shade</c>, which its base call <c>: BodyShown(Shade)</c> read, and the
    /// tree no longer compiled (CS0103): that shape is the conversion's. An argument passed to a removed parameter whose
    /// evaluation calls code or writes state stays as written, so the call fails to compile there (CS1729, or CS1739 for
    /// a named one) instead of the effect silently going with the argument.
    /// </remarks>
    private static Dictionary<SyntaxTree, PositionalEdits> PositionalParameterEdits(
        CompiledTree[] trees, SymbolIdentity identity, HashSet<string> targets,
        SortedSet<string> retired)
    {
        var edits = new Dictionary<SyntaxTree, PositionalEdits>();
        PositionalEdits For(SyntaxTree tree) => edits.TryGetValue(tree, out var e) ? e : edits[tree] = new([], [], []);

        // Primary constructor documentation id -> its removed parameter ordinals.
        var removedByConstructor = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
        foreach (var (tree, compilation) in trees)
        {
            var model = SemanticModels.Of(compilation, tree);
            foreach (var record in tree.GetRoot().DescendantNodes().OfType<RecordDeclarationSyntax>())
            {
                if (record.ParameterList is not { } list || model.GetDeclaredSymbol(record) is not INamedTypeSymbol type)
                    continue;
                // A record removed whole takes its parameters and their reads with it.
                bool removedWhole = Removed(type, identity, targets);
                var targeted = new List<(ParameterSyntax Syntax, IParameterSymbol Symbol, IPropertySymbol Property, string Key)>();
                foreach (var parameter in list.Parameters)
                {
                    if (model.GetDeclaredSymbol(parameter) is not IParameterSymbol symbol ||
                        type.GetMembers(symbol.Name).OfType<IPropertySymbol>().FirstOrDefault() is not { } property ||
                        identity.Key(property) is not { } key || !targets.Contains(key))
                        continue;
                    targeted.Add((parameter, symbol, property, key));
                }
                if (targeted.Count == 0)
                    continue;
                var reads = targeted.ToDictionary(entry => entry.Symbol.Name, entry => ParameterReads(entry.Symbol, type, compilation), StringComparer.Ordinal);
                var baseCall = record.BaseList?.Types.OfType<PrimaryConstructorBaseTypeSyntax>().SingleOrDefault();
                bool InBaseCall(SyntaxNode read) => baseCall is not null && read.SyntaxTree == tree && baseCall.ArgumentList.Span.Contains(read.Span);
                bool converts = !removedWhole && reads.Values.Any(found => found.Count > 0) && type.DeclaringSyntaxReferences.Length == 1 &&
                    targeted.Count == list.Parameters.Count && reads.Values.All(found => found.All(InBaseCall));
                if (converts)
                {
                    For(tree).Conversions.Add(new RecordConversion(record.Identifier.Text, [.. list.Parameters.Select(parameter => parameter.Identifier.Text)],
                        reads.Where(entry => entry.Value.Count > 0).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal)));
                    Console.WriteLine($"Converted record {type.ToDisplayString()}: its dead properties are retired and the constructor keeps " +
                        $"what its base call reads ({string.Join(", ", reads.Where(entry => entry.Value.Count > 0).Select(entry => entry.Key))})");
                }
                foreach (var (parameter, symbol, property, _) in targeted)
                {
                    if (RetiredField(property) is { } field)
                        retired.Add($"{RuntimeName(field.ContainingType)}\t{field.Name}");
                    if (converts && reads[symbol.Name].Count > 0)
                        continue;
                    if (!converts)
                    {
                        For(tree).Nodes.Add(parameter);
                        For(tree).DocumentedParameters.Add(new RecordParameter(record.Identifier.Text, symbol.Name));
                    }
                    var constructor = (IMethodSymbol)symbol.ContainingSymbol;
                    string id = constructor.GetDocumentationCommentId()
                        ?? throw new InvalidOperationException($"{constructor.ToDisplayString()} has no documentation id.");
                    if (!removedByConstructor.TryGetValue(id, out var ordinals))
                        removedByConstructor[id] = ordinals = [];
                    ordinals.Add(symbol.Ordinal);
                }
            }
        }
        if (removedByConstructor.Count == 0)
            return edits;

        foreach (var (tree, compilation) in trees)
        {
            var model = SemanticModels.Of(compilation, tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is WithExpressionSyntax with)
                {
                    // `record with { P = value }` writes a removed property: drop that assignment, unless its value calls code
                    // or writes state, which stays and fails to compile there.
                    foreach (var assignment in with.Initializer.Expressions.OfType<AssignmentExpressionSyntax>())
                        if (model.GetSymbolInfo(assignment.Left).Symbol is IPropertySymbol property &&
                            identity.Key(property) is { } key && targets.Contains(key) &&
                            !assignment.Right.DescendantNodesAndSelf().Any(HasEffect))
                            For(tree).Nodes.Add(assignment);
                    continue;
                }
                (ArgumentListSyntax? arguments, ISymbol? called) = node switch
                {
                    BaseObjectCreationExpressionSyntax creation => new ArgumentListTarget(creation.ArgumentList, model.GetSymbolInfo(creation).Symbol),
                    PrimaryConstructorBaseTypeSyntax baseType => new ArgumentListTarget(baseType.ArgumentList, model.GetSymbolInfo(baseType).Symbol),
                    ConstructorInitializerSyntax initializer => new ArgumentListTarget(initializer.ArgumentList, model.GetSymbolInfo(initializer).Symbol),
                    _ => new ArgumentListTarget(null, null),
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
                    // An argument with an effect stays, and the call no longer compiles: dropping it would drop the effect.
                    if (!argument.Expression.DescendantNodesAndSelf().Any(HasEffect))
                        For(tree).Nodes.Add(argument);
                }
            }
        }
        return edits;
    }

    /// <summary>Every read of a positional parameter, as a constructor parameter, in its record's declarations.</summary>
    private static List<SyntaxNode> ParameterReads(IParameterSymbol parameter, INamedTypeSymbol record, Compilation compilation) =>
        [.. record.DeclaringSyntaxReferences.SelectMany(reference =>
        {
            var model = SemanticModels.Of(compilation, reference.SyntaxTree);
            return reference.GetSyntax().DescendantNodes().OfType<IdentifierNameSyntax>()
                .Where(name => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name).Symbol, parameter));
        })];

    /// <summary>Whether evaluating this expression node could call code or write state.</summary>
    private static bool HasEffect(SyntaxNode node) => node is InvocationExpressionSyntax or AssignmentExpressionSyntax
        or AwaitExpressionSyntax or BaseObjectCreationExpressionSyntax
        || node is PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax &&
            node.RawKind is (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreIncrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PreDecrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression
                or (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression;

    /// <summary>Drops the <c>param</c> documentation of removed positional parameters from each record's comment.</summary>
    private static string RemoveParameterDocumentation(string text, List<RecordParameter> removed)
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

    /// <summary>Marks selected syntax nodes so Roslyn removes exactly the rewritten declarations.</summary>
    private static readonly SyntaxAnnotation Removal = new("reachability-removal");

    /// <summary>Surviving enum members without an initializer that follow a removed member, with their constant value.</summary>
    private static Dictionary<SyntaxNode, string> ImplicitlyValuedSurvivors(SemanticModel model, List<SyntaxNode> removed)
    {
        var survivors = new Dictionary<SyntaxNode, string>();
        foreach (var declaration in removed.OfType<EnumMemberDeclarationSyntax>().Select(m => m.Parent).OfType<EnumDeclarationSyntax>().Distinct())
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
                    survivors[member] = Convert.ToString(field.ConstantValue, System.Globalization.CultureInfo.InvariantCulture) ??
                        throw new InvalidOperationException($"Enum member {field.Name} has no constant text.");
            }
        }
        return survivors;
    }

    /// <summary>Removes an entire field declaration only when every declared variable is targeted; otherwise selects its individual declarator.</summary>
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
