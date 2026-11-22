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
/// every construction, record base call and <c>with</c> initializer; one its base call or its members' initializers read
/// becomes a constructor parameter of a record that loses its parameter list, and when the record cannot, it is never
/// deleted: its dead property stays (#2599, #2611). A class's or struct's primary-constructor parameter that only removed
/// members read goes too, with every argument passed to it; while anything else reads it, the member keeping it for the
/// debugger state's rebuild stays (#2614, SME6492). A removed interface leaves the base lists naming it. Removed instance
/// fields, including auto-property backing fields, are listed so persisted debugger-state layouts can retire them.
/// <para>
/// A removal that leaves the code not compiling is applied, and the build fails where the developer has a choice to make
/// (#2599). Documentation that cites a removed declaration keeps its name as code text, which SME6454 rejects in Core where
/// a sentence must be reworded. A <c>nameof</c> of a removed declaration becomes the text it produced, which is what it
/// meant at run time (#2611: the prune removed <c>RidleyTailAction.Stab</c> and three more steps whose names
/// <c>RidleyPogoTailAction</c> took through <c>nameof</c>, and none of them compiled).
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
                // A removed interface leaves the base lists naming it (#2614: the cleanup of 67a5170535 deleted IEnemyFrameResults,
                // which RoomEnemySystem still listed, CS0246); a base list it alone filled goes with it.
                var emptiedBaseLists = new HashSet<BaseListSyntax>();
                foreach (var baseType in root.DescendantNodes().OfType<SimpleBaseTypeSyntax>())
                {
                    if (model.GetTypeInfo(baseType.Type).Type is not INamedTypeSymbol { TypeKind: TypeKind.Interface } face ||
                        !Removed(SymbolIdentity.Normalize(face), identity, targets) || baseType.Parent is not BaseListSyntax list)
                        continue;
                    if (list.Types.All(other => other == baseType || model.GetTypeInfo(other.Type).Type is INamedTypeSymbol { TypeKind: TypeKind.Interface } otherFace &&
                            Removed(SymbolIdentity.Normalize(otherFace), identity, targets)))
                        emptiedBaseLists.Add(list);
                    else
                        removals.Add(baseType);
                }
                var emptiedOwners = emptiedBaseLists.Select(list => list.Parent).OfType<TypeDeclarationSyntax>()
                    .Where(owner => !removals.Any(removal => removal.Span.Contains(owner.Span))).ToHashSet();
                var outermost = removals.Distinct().Where(n => !removals.Any(other => other != n && other.Span.Contains(n.Span) && other.Contains(n))).ToList();
                // Documentation that links a removed declaration keeps its name as code text.
                var orphanedLinks = root.DescendantNodes(descendIntoTrivia: true).OfType<XmlEmptyElementSyntax>()
                    .Where(element => element.Attributes.OfType<XmlCrefAttributeSyntax>().FirstOrDefault() is { } cref
                        && !outermost.Any(n => n.FullSpan.Contains(element.Span))
                        && Removed(SymbolIdentity.Normalize(model.GetSymbolInfo(cref.Cref).Symbol), identity, targets))
                    .ToList();
                // A nameof of a removed declaration becomes the text it produced.
                var orphanedNames = root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" }
                        && invocation.ArgumentList.Arguments.Count == 1
                        && !outermost.Any(n => n.Span.Contains(invocation.Span))
                        && model.GetConstantValue(invocation) is { HasValue: true, Value: string }
                        && NamedSymbols(model, invocation.ArgumentList.Arguments[0].Expression).Any(symbol => Removed(symbol, identity, targets)))
                    .ToDictionary(invocation => (SyntaxNode)invocation, invocation => (string)model.GetConstantValue(invocation).Value!);
                if (outermost.Count == 0 && orphanedLinks.Count == 0 && orphanedNames.Count == 0 && emptiedOwners.Count == 0 &&
                    positional?.Conversions.Count is not > 0)
                    continue;
                // A removed method parameter's param documentation goes, and a paramref to it becomes its name as code text.
                var parameterDocs = RemovedParameterDocumentation(root, positional?.MethodParameters ?? []);
                // Removing an enum member must not renumber the members after it: each surviving
                // implicitly valued member that follows a removed one gets its value written out.
                var renumbered = ImplicitlyValuedSurvivors(model, outermost);
                var marked = root.ReplaceNodes(outermost.Concat(renumbered.Keys).Concat(orphanedLinks).Concat(orphanedNames.Keys).Concat(parameterDocs.Keys)
                        .Concat(emptiedOwners),
                    (original, rewritten) =>
                    original is TypeDeclarationSyntax owner && emptiedOwners.Contains(owner)
                        ? WithoutBaseList((TypeDeclarationSyntax)rewritten)
                    : parameterDocs.TryGetValue(original, out string? parameterName)
                        ? original is XmlElementSyntax
                            ? SyntaxFactory.XmlText(RemovedDocumentationLine)
                            : SyntaxFactory.XmlElement("c", SyntaxFactory.SingletonList<XmlNodeSyntax>(SyntaxFactory.XmlText(parameterName)))
                    : orphanedNames.TryGetValue(original, out string? name)
                        ? SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(name))
                            .WithTriviaFrom(rewritten)
                    : original is XmlEmptyElementSyntax link
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
                if (text.Contains(RemovedDocumentationLine, StringComparison.Ordinal))
                {
                    string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
                    text = string.Join(newline, text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n')
                        .Where(line => !line.Contains(RemovedDocumentationLine, StringComparison.Ordinal)));
                }
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

    /// <summary>Marks the documentation line of a removed parameter's <c>param</c> element, whose whole line then goes; it
    /// is text documentation may hold, and no source holds it.</summary>
    private const string RemovedDocumentationLine = "⟦removed-parameter-documentation⟧";

    /// <summary>
    /// The <c>param</c> elements and <c>paramref</c>s naming the removed method parameters in the documentation of the
    /// methods declaring them, each with the parameter's name.
    /// </summary>
    private static Dictionary<SyntaxNode, string> RemovedParameterDocumentation(SyntaxNode root, List<MethodParameter> removed)
    {
        var found = new Dictionary<SyntaxNode, string>();
        if (removed.Count == 0)
            return found;
        foreach (var node in root.DescendantNodes(descendIntoTrivia: true).OfType<XmlNodeSyntax>())
        {
            var attributes = node switch
            {
                XmlEmptyElementSyntax { Name.LocalName.ValueText: "paramref" } reference => reference.Attributes,
                XmlElementSyntax { StartTag.Name.LocalName.ValueText: "param" } element => element.StartTag.Attributes,
                _ => default,
            };
            if (attributes.OfType<XmlNameAttributeSyntax>().FirstOrDefault()?.Identifier.Identifier.ValueText is not { } name ||
                node.Ancestors(ascendOutOfTrivia: true).OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() is not { } declaration ||
                !removed.Contains(new MethodParameter(declaration.SpanStart, name)))
                continue;
            found[node] = name;
        }
        return found;
    }

    /// <summary>The declarations a <c>nameof</c> operand names: the one it binds to, or every candidate of a method group.</summary>
    private static IEnumerable<ISymbol> NamedSymbols(SemanticModel model, ExpressionSyntax operand)
    {
        SymbolInfo info = model.GetSymbolInfo(operand);
        return (info.Symbol is { } symbol ? [symbol] : info.CandidateSymbols.AsEnumerable())
            .Select(SymbolIdentity.Normalize).OfType<ISymbol>();
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
    /// <param name="Nodes">The syntax taken out.</param>
    /// <param name="DocumentedParameters">The removed positional parameters whose record documentation goes.</param>
    /// <param name="Conversions">The records converted.</param>
    /// <param name="MethodParameters">The removed parameters of methods and constructors, whose documentation goes.</param>
    private sealed record PositionalEdits(List<SyntaxNode> Nodes, List<RecordParameter> DocumentedParameters, List<RecordConversion> Conversions,
        List<MethodParameter> MethodParameters);

    /// <summary>A removed parameter of a method or constructor, named by its declaration's start and its own name.</summary>
    /// <param name="Declaration">Where the method or constructor's declaration starts in its file.</param>
    /// <param name="Parameter">The parameter's name.</param>
    private readonly record struct MethodParameter(int Declaration, string Parameter);

    /// <summary>
    /// For every targeted positional record parameter: the parameter itself and the matching
    /// argument of each primary-constructor call, record base call and <c>with</c> initializer.
    /// </summary>
    /// <remarks>
    /// A positional parameter is also a constructor parameter, and code of its record can read it as one (#2599): the
    /// base call or an initializer (in a record's members the name is the property's). When every positional property of
    /// the record is dead and only the base call and initializers read parameters, the record becomes one without a
    /// parameter list whose constructor takes the parameters read, passes them to its base and gives its members their
    /// initial values (<see cref="PositionalRecordConversion"/>); initializers move into the constructor only where running
    /// them after the base constructor changes nothing. Otherwise a parameter the record reads is never deleted (#2611):
    /// its dead property stays, which is the one removal that leaves the record as it behaves. The cleanup of 0ea41f339b
    /// deleted <c>KraidColours.BandShade</c>'s parameter <c>Shade</c>, which its base call <c>: BodyShown(Shade)</c> read,
    /// and the cleanup of 418ca79935 <c>TitleFloorSceneDefinition</c>'s <c>startingZoom</c>, which the initializer of its
    /// live <c>Zoom</c> read; neither tree compiled (CS0103, CS1729). An argument passed to a removed parameter whose
    /// evaluation calls code or writes state stays as written, so the call fails to compile there (CS1729, or CS1739 for
    /// a named one) instead of the effect silently going with the argument.
    /// </remarks>
    private static Dictionary<SyntaxTree, PositionalEdits> PositionalParameterEdits(
        CompiledTree[] trees, SymbolIdentity identity, HashSet<string> targets,
        SortedSet<string> retired)
    {
        var edits = new Dictionary<SyntaxTree, PositionalEdits>();
        PositionalEdits For(SyntaxTree tree) => edits.TryGetValue(tree, out var e) ? e : edits[tree] = new([], [], [], []);

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
                var initializers = PositionalRecordConversion.InstanceInitializers(record).ToList();
                bool InInitializer(SyntaxNode read) => read.SyntaxTree == tree && initializers.Any(initializer => initializer.Span.Contains(read.Span));
                var readNodes = reads.Values.SelectMany(found => found).ToList();
                bool movesInitializers = readNodes.Any(read => !InBaseCall(read));
                // Initializers run before the base constructor and a constructor's body after it: they move only where
                // nothing can tell, a record with no base record or initializers that call no code and write no state.
                bool converts = !removedWhole && readNodes.Count > 0 && type.DeclaringSyntaxReferences.Length == 1 &&
                    targeted.Count == list.Parameters.Count && readNodes.All(read => InBaseCall(read) || InInitializer(read)) &&
                    (!movesInitializers || type.BaseType?.SpecialType is SpecialType.System_Object or SpecialType.System_ValueType ||
                        !initializers.Any(initializer => initializer.Value.DescendantNodesAndSelf().Any(HasEffect)));
                if (converts)
                {
                    For(tree).Conversions.Add(new RecordConversion(record.Identifier.Text, [.. list.Parameters.Select(parameter => parameter.Identifier.Text)],
                        reads.Where(entry => entry.Value.Count > 0).Select(entry => entry.Key).ToHashSet(StringComparer.Ordinal), movesInitializers));
                    Console.WriteLine($"Converted record {type.ToDisplayString()}: its dead properties are retired and the constructor keeps " +
                        $"what its base call and initializers read ({string.Join(", ", reads.Where(entry => entry.Value.Count > 0).Select(entry => entry.Key))})");
                }
                foreach (var (parameter, symbol, property, _) in targeted)
                {
                    // A parameter its record still reads is never deleted (#2611): when the record cannot become one whose
                    // constructor takes it, its dead property stays, which removes nothing the record needs.
                    if (!converts && reads[symbol.Name].Count > 0)
                    {
                        Console.WriteLine($"Kept {type.ToDisplayString()}.{symbol.Name}: its property is dead, but the record reads its " +
                            "parameter, and the record cannot become one whose constructor takes it");
                        continue;
                    }
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
        PrimaryParameterEdits(trees, identity, targets, For, removedByConstructor);
        if (removedByConstructor.Count == 0)
            return edits;

        // Removing an argument can leave a parameter of the caller that only the argument read unread, and IDE0060, an
        // unused parameter, is a build error (#2611: the arguments of KiHunterAcidSpitCue.Launch's and
        // SpacePirateProjectileCue's dead Direction left the facing and direction parameters of the animations building
        // them unread). Such a parameter goes too, with its documentation and the argument every call passes it, and so on
        // up the callers, as the code fixes' unread parameters do.
        var pending = removedByConstructor.Keys.ToHashSet(StringComparer.Ordinal);
        bool first = true;
        while (pending.Count > 0)
        {
            var removedArguments = new List<(ArgumentSyntax Argument, SemanticModel Model)>();
            RemoveArguments(trees, identity, targets, removedByConstructor, pending, first, For, removedArguments);
            first = false;
            pending = RemoveUnreadParameters(removedArguments, edits, For, removedByConstructor);
        }
        return edits;
    }

    /// <summary>
    /// The primary-constructor parameters of classes and structs the removed members read (#2614). A debugger state
    /// rebuilds such a type through its primary constructor from the members keeping each parameter, so a parameter kept by
    /// no member fails SME6492, and an unread one CS9113:
    /// <list type="bullet">
    /// <item>a parameter that only removed members read is dead with them: it goes, with its documentation and the argument
    /// every construction passes it (and so on up the callers, as a removed argument's unread parameters do);</item>
    /// <item>a parameter something else reads (its base call, a member's initializer) lives on, and when no surviving member
    /// keeps it, the removed members that keep it (<see cref="Keeps"/>) stay, which removes nothing the
    /// type needs. A parameter a member's code captures keeps itself.</item>
    /// <item>an attribute's parameters are read from the metadata of its applications (a source generator reads
    /// <c>[GeneratedText("file")]</c>'s argument), so they stay with the members keeping them, whatever source reads
    /// them.</item>
    /// </list>
    /// The cleanup of 67a5170535 removed the members keeping Draygon's steps' <c>phase</c>, which their base calls read, and
    /// fifteen more, and none of them compiled (SME6492).
    /// </summary>
    private static void PrimaryParameterEdits(CompiledTree[] trees, SymbolIdentity identity, HashSet<string> targets,
        Func<SyntaxTree, PositionalEdits> For, Dictionary<string, HashSet<int>> removedByConstructor)
    {
        var kept = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (tree, compilation) in trees)
        {
            var model = SemanticModels.Of(compilation, tree);
            foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (declaration is not (ClassDeclarationSyntax or StructDeclarationSyntax) || declaration.ParameterList is not { } list ||
                    model.GetDeclaredSymbol(declaration) is not INamedTypeSymbol type || Removed(type, identity, targets))
                    continue;
                // The members the prune removes, with where each is declared.
                var removedMembers = type.GetMembers().Where(member => member is IFieldSymbol or IPropertySymbol or IMethodSymbol or IEventSymbol &&
                        identity.Key(SymbolIdentity.Normalize(member)) is { } key && targets.Contains(key))
                    .SelectMany(member => member.DeclaringSyntaxReferences.Select(reference => (Member: member, Syntax: reference.GetSyntax())))
                    .ToList();
                if (removedMembers.Count == 0)
                    continue;
                bool InRemoved(SyntaxNode read) => removedMembers.Any(removed => removed.Syntax.SyntaxTree == read.SyntaxTree &&
                    removed.Syntax.Span.Contains(read.Span));
                foreach (var parameter in list.Parameters)
                {
                    if (model.GetDeclaredSymbol(parameter) is not IParameterSymbol symbol)
                        continue;
                    var reads = ParameterReads(symbol, type, compilation);
                    if (!reads.Any(InRemoved))
                        continue;
                    var liveReads = reads.Where(read => !InRemoved(read)).ToList();
                    bool attribute = IsAttribute(type);
                    if (liveReads.Count == 0 && !attribute)
                    {
                        For(tree).Nodes.Add(parameter);
                        For(tree).DocumentedParameters.Add(new RecordParameter(declaration.Identifier.Text, symbol.Name));
                        var constructor = (IMethodSymbol)symbol.ContainingSymbol;
                        string id = constructor.GetDocumentationCommentId()
                            ?? throw new InvalidOperationException($"{constructor.ToDisplayString()} has no documentation id.");
                        if (!removedByConstructor.TryGetValue(id, out var ordinals))
                            removedByConstructor[id] = ordinals = [];
                        ordinals.Add(symbol.Ordinal);
                        Console.WriteLine($"Removed {type.ToDisplayString()}'s parameter {symbol.Name}: only its removed members read it");
                        continue;
                    }
                    // The parameter lives on: a member keeps it unless only removed members do.
                    if (!attribute && reads.Any(read => !InRemoved(read) && IsCapture(read)) ||
                        type.GetMembers().Any(member => !removedMembers.Any(removed => SymbolEqualityComparer.Default.Equals(removed.Member, member)) &&
                            Keeps(member, symbol, compilation)))
                        continue;
                    foreach (var (member, _) in removedMembers.Where(removed => Keeps(removed.Member, symbol, compilation)))
                    {
                        if (identity.Key(SymbolIdentity.Normalize(member)) is not { } key || !kept.Add(key))
                            continue;
                        Console.WriteLine(attribute
                            ? $"Kept {type.ToDisplayString()}.{member.Name}: it keeps the parameter {symbol.Name} of an attribute, whose " +
                                "arguments are read from its applications' metadata"
                            : $"Kept {type.ToDisplayString()}.{member.Name}: it keeps the parameter {symbol.Name}, which " +
                                "the type still reads, so a debugger state can rebuild it through its primary constructor (SME6492)");
                    }
                }
            }
        }
        targets.ExceptWith(kept);
    }

    /// <summary>Whether <paramref name="type"/> is an attribute.</summary>
    private static bool IsAttribute(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
            if (current.ToDisplayString() == "System.Attribute")
                return true;
        return false;
    }

    /// <summary>Whether a parameter read is a capture: a read in a member's code, not in an initializer or the base call.</summary>
    private static bool IsCapture(SyntaxNode read) =>
        !read.Ancestors().Any(ancestor => ancestor is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax or PropertyDeclarationSyntax }
            or PrimaryConstructorBaseTypeSyntax);

    /// <summary>
    /// Whether <paramref name="member"/> keeps <paramref name="parameter"/> as the state graph's generator maps it
    /// (StateGraphRun.MapArgument): an instance field or auto-property whose initializer refers to the parameter (the
    /// parameter itself, a call on it such as <c>Ensure.NotNull(change)</c>, or anything made from it) and whose type
    /// converts to the parameter's.
    /// </summary>
    private static bool Keeps(ISymbol member, IParameterSymbol parameter, Compilation compilation)
    {
        if (member is not (IFieldSymbol { IsStatic: false, IsConst: false } or IPropertySymbol { IsStatic: false }))
            return false;
        ITypeSymbol type = member is IFieldSymbol field ? field.Type : ((IPropertySymbol)member).Type;
        Conversion conversion = compilation.ClassifyConversion(type, parameter.Type);
        if (!(conversion.IsIdentity || conversion.IsImplicit && (conversion.IsReference || conversion.IsBoxing || conversion.IsNullable)))
            return false;
        return member.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() switch
        {
            VariableDeclaratorSyntax { Initializer.Value: { } value } => RefersTo(value, parameter, compilation),
            PropertyDeclarationSyntax { Initializer.Value: { } value } => RefersTo(value, parameter, compilation),
            _ => false,
        });
    }

    /// <summary>Whether an expression refers to <paramref name="parameter"/>.</summary>
    private static bool RefersTo(ExpressionSyntax value, IParameterSymbol parameter, Compilation compilation)
    {
        var model = SemanticModels.Of(compilation, value.SyntaxTree);
        return value.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>()
            .Any(name => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name).Symbol, parameter));
    }

    /// <summary>
    /// <paramref name="declaration"/> without its base list, whose interfaces are all removed: the line keeps the trivia the
    /// list ended with, and the name before it loses the space before the colon.
    /// </summary>
    private static TypeDeclarationSyntax WithoutBaseList(TypeDeclarationSyntax declaration)
    {
        if (declaration.BaseList is not { } list)
            return declaration;
        SyntaxToken before = list.GetFirstToken().GetPreviousToken();
        var trailing = list.GetTrailingTrivia();
        var replaced = declaration.ReplaceToken(before, before.WithTrailingTrivia(trailing));
        return replaced.WithBaseList(null);
    }

    /// <summary>
    /// Adds to the edits every argument passed to a removed parameter of the methods <paramref name="pending"/> names, and
    /// on the first round every <c>with</c> assignment of a removed property; collects the arguments it removes.
    /// </summary>
    private static void RemoveArguments(CompiledTree[] trees, SymbolIdentity identity, HashSet<string> targets,
        Dictionary<string, HashSet<int>> removedByMethod, HashSet<string> pending, bool withAssignments,
        Func<SyntaxTree, PositionalEdits> For, List<(ArgumentSyntax Argument, SemanticModel Model)> removedArguments)
    {
        foreach (var (tree, compilation) in trees)
        {
            var model = SemanticModels.Of(compilation, tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is WithExpressionSyntax with)
                {
                    // `record with { P = value }` writes a removed property: drop that assignment, unless its value calls code
                    // or writes state, which stays and fails to compile there.
                    if (withAssignments)
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
                    InvocationExpressionSyntax invocation => new ArgumentListTarget(invocation.ArgumentList, model.GetSymbolInfo(invocation).Symbol),
                    _ => new ArgumentListTarget(null, null),
                };
                if (arguments is null || called is not IMethodSymbol method)
                    continue;
                // A call of an extension method in its reduced form passes no argument to the receiver parameter.
                IMethodSymbol declared = method.ReducedFrom ?? method;
                int offset = method.ReducedFrom is null ? 0 : 1;
                if (declared.OriginalDefinition.GetDocumentationCommentId() is not { } id || !pending.Contains(id) ||
                    !removedByMethod.TryGetValue(id, out var removed))
                    continue;
                for (int index = 0; index < arguments.Arguments.Count; index++)
                {
                    var argument = arguments.Arguments[index];
                    int ordinal = (argument.NameColon is { } name
                        ? method.Parameters.Single(p => p.Name == name.Name.Identifier.Text).Ordinal
                        : index) + offset;
                    if (!removed.Contains(ordinal))
                        continue;
                    // An argument with an effect stays, and the call no longer compiles: dropping it would drop the effect.
                    if (!argument.Expression.DescendantNodesAndSelf().Any(HasEffect))
                    {
                        For(tree).Nodes.Add(argument);
                        removedArguments.Add((argument, model));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Takes out every parameter of a method or constructor that the removed arguments read and nothing else in its
    /// declaration reads: the parameter and its documentation, its ordinal recorded so the next round removes the argument
    /// every call passes it. Returns the methods whose parameters it took out. A parameter whose method something else
    /// decides the signature of (virtual, abstract, an override, an interface's, partial or extern), a <c>ref</c>,
    /// <c>out</c>, <c>in</c> or <c>params</c> one, and a primary constructor's are left, and IDE0060 reports them where the
    /// developer has a choice to make.
    /// </summary>
    private static HashSet<string> RemoveUnreadParameters(List<(ArgumentSyntax Argument, SemanticModel Model)> removedArguments,
        Dictionary<SyntaxTree, PositionalEdits> edits, Func<SyntaxTree, PositionalEdits> For, Dictionary<string, HashSet<int>> removedByMethod)
    {
        var taken = new HashSet<string>(StringComparer.Ordinal);
        var seen = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        var candidates = new List<(IParameterSymbol Parameter, SemanticModel Model)>();
        foreach (var (argument, argumentModel) in removedArguments)
            foreach (var name in argument.DescendantNodes().OfType<IdentifierNameSyntax>())
                if (argumentModel.GetSymbolInfo(name).Symbol is IParameterSymbol read && seen.Add(read))
                    candidates.Add((read, argumentModel));
        foreach (var (parameter, model) in candidates)
        {
            if (parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.Constructor } method ||
                method.IsVirtual || method.IsAbstract || method.IsOverride || method.IsExtern || method.PartialDefinitionPart is not null ||
                method.PartialImplementationPart is not null || !method.ExplicitInterfaceImplementations.IsEmpty || ImplementsInterface(method) ||
                parameter.RefKind != RefKind.None || parameter.IsParams || parameter.IsThis ||
                parameter.DeclaringSyntaxReferences is not [{ } reference] ||
                reference.GetSyntax() is not ParameterSyntax { Parent.Parent: BaseMethodDeclarationSyntax declaration } syntax ||
                method.OriginalDefinition.GetDocumentationCommentId() is not { } id)
                continue;
            if (removedByMethod.TryGetValue(id, out var already) && already.Contains(parameter.Ordinal) ||
                model.SyntaxTree != declaration.SyntaxTree)
                continue;
            var removedSpans = edits.TryGetValue(declaration.SyntaxTree, out var treeEdits)
                ? treeEdits.Nodes.Select(node => node.Span).ToList() : [];
            bool read = declaration.DescendantNodes().OfType<IdentifierNameSyntax>()
                .Where(name => SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(name).Symbol, parameter))
                .Any(name => !removedSpans.Any(span => span.Contains(name.Span)));
            if (read)
                continue;
            For(declaration.SyntaxTree).Nodes.Add(syntax);
            For(declaration.SyntaxTree).MethodParameters.Add(new MethodParameter(declaration.SpanStart, parameter.Name));
            if (!removedByMethod.TryGetValue(id, out var ordinals))
                removedByMethod[id] = ordinals = [];
            ordinals.Add(parameter.Ordinal);
            taken.Add(id);
            Console.WriteLine($"Removed parameter {parameter.Name} of {method.ToDisplayString()}: only removed arguments read it");
        }
        return taken;
    }

    /// <summary>Whether a method implements a member of an interface its type implements.</summary>
    private static bool ImplementsInterface(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(face => face.GetMembers()).Any(member =>
            SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

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

    /// <summary>Drops the <c>param</c> documentation of removed primary-constructor parameters from each type's comment.</summary>
    private static string RemoveParameterDocumentation(string text, List<RecordParameter> removed)
    {
        string newline = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').ToList();
        foreach (var (record, parameter) in removed)
        {
            var declarationPattern = new System.Text.RegularExpressions.Regex($@"\b(record(\s+(struct|class))?|class|struct)\s+{record}\b");
            int declaration = lines.FindIndex(declarationPattern.IsMatch);
            if (declaration < 0)
                throw new InvalidOperationException($"Type {record} not found while removing its {parameter} documentation.");
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
