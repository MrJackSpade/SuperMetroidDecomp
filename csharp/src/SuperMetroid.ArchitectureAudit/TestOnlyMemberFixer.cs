using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.SourceLayout;
using SuperMetroid.ReachabilityAudit.Removal;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// The test-only-member census's fix for its <c>narrow</c> findings (#2570): each member, or accessor, production's own
/// type reaches but only checks reach from outside it becomes private, and its owner's generated
/// <c>&lt;Owner&gt;PrivateAccess</c> class in test-support names it as the owner's own code does, reaching the private
/// member through <c>PrivateState</c>, so checks keep their call syntax. C# binds a check's name to that extension member
/// because the private member is no longer accessible to it.
/// <para>
/// A finding the fix cannot make mechanically is listed for a person: a member production does not need (<c>unused</c>
/// or <c>unneeded</c>, which leaves production instead), a generated declaration (its generator is fixed), and a member
/// whose shape no extension member can stand in for (an indexer, an event, a generic member or owner, a constructor, a
/// positional record property, a required member, a <c>ref struct</c> in its signature, a field sharing its declaration
/// with one that stays). Every edit is computed and validated before any file is written.
/// </para>
/// </summary>
internal static class TestOnlyMemberFixer
{
    /// <summary>Makes every mechanical narrow finding private with its shim, and lists the rest; returns the exit code.</summary>
    public static int Fix(LoadedSolution solution)
    {
        var findings = TestOnlyMembers.Collect(solution);
        var manual = new List<string>();
        var edits = new Dictionary<SyntaxTree, List<TextChange>>();
        var shims = new Dictionary<string, PrivateAccessShims>(StringComparer.Ordinal);
        var narrowed = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        int fixedMembers = 0;
        foreach (var member in findings.GroupBy(finding => finding.Uses.Member, SymbolEqualityComparer.Default)
                     .OrderBy(group => group.Key!.ToDisplayString(), StringComparer.Ordinal))
        {
            ISymbol symbol = member.Key!;
            var ways = member.ToList();
            if (ThroughPrivateState(symbol, ways) is { } rewrite)
            {
                if (rewrite.Refusal is null && rewrite.Edits.All(edit => Fits(edits, edit.Tree, edit.Change)))
                {
                    foreach (var (tree, change) in rewrite.Edits)
                        Add(edits, tree, change);
                    narrowed.Add(symbol);
                    fixedMembers++;
                }
                else
                {
                    manual.Add($"MANUAL {Describe(solution.RepositoryRoot, symbol)} ({string.Join(", ", ways.Select(way => $"{way.Uses.Way} {way.Shape}"))}): " +
                        (rewrite.Refusal ?? "a check site's rewrite overlaps another"));
                }
                continue;
            }
            string? refusal = Refusal(symbol, ways);
            if (refusal is not null)
            {
                manual.Add($"MANUAL {Describe(solution.RepositoryRoot, symbol)} ({string.Join(", ", ways.Select(way => $"{way.Uses.Way} {way.Shape}"))}): {refusal}");
                continue;
            }
            foreach (var (tree, change) in Narrow(symbol))
                Add(edits, tree, change);
            Shims(solution.RepositoryRoot, shims, PrivateAccessShims.OwnerOf(symbol)).Add(symbol, Written(ways));
            narrowed.Add(symbol);
            fixedMembers++;
        }
        foreach (var (tree, change) in CheckSiteRewrites(solution, narrowed))
            Add(edits, tree, change);

        var files = new List<(string Path, string Text)>();
        foreach (var (tree, changes) in edits)
        {
            SourceText before = tree.GetText();
            SourceText after = before.WithChanges(changes.OrderBy(change => change.Span.Start));
            int errorsBefore = tree.GetDiagnostics().Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            int errorsAfter = CSharpSyntaxTree.ParseText(after, (CSharpParseOptions)tree.Options).GetDiagnostics()
                .Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
            if (errorsAfter != errorsBefore)
                throw new InvalidOperationException($"{tree.FilePath}: narrowing leaves {errorsAfter - errorsBefore} new syntax errors.");
            files.Add((tree.FilePath, after.ToString()));
        }
        foreach (PrivateAccessShims owner in shims.Values)
            files.Add((owner.Path, owner.Render()));
        foreach (var (path, text) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: File.Exists(path) && HasByteOrderMark(path)));
        }
        PrivateAccessShims.WriteImports(solution.RepositoryRoot);
        foreach (string line in manual)
            Console.WriteLine(line);
        Console.WriteLine($"Narrowed {fixedMembers} members in {edits.Count} files with {shims.Count} private-access classes; " +
            $"{manual.Count} left for a person.");
        return 0;
    }

    /// <summary>Why a member's findings cannot be fixed mechanically, or null.</summary>
    private static string? Refusal(ISymbol symbol, List<TestOnlyMemberFinding> ways)
    {
        if (ways.FirstOrDefault(way => way.Shape != "narrow") is { } needless)
            return needless.Shape == "unused" ? "production never uses it: it leaves production" :
                "production does not need this way of reaching it: it leaves production";
        if (symbol.DeclaringSyntaxReferences.Any(reference => GeneratedSource.IsGenerated(reference.SyntaxTree, CancellationToken.None) ||
                reference.SyntaxTree.FilePath.Contains(".Generated.", StringComparison.OrdinalIgnoreCase)))
            return "generated declaration: fix its generator";
        INamedTypeSymbol owner = symbol.ContainingType;
        for (INamedTypeSymbol? type = owner; type is not null; type = type.ContainingType)
        {
            if (type.IsGenericType)
                return "generic owner";
            if (type.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                return "owner not visible to test-support";
        }
        if (symbol.DeclaringSyntaxReferences.Length != 1 && symbol is not IMethodSymbol { IsPartialDefinition: true })
            return "declared in several places";
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Constructor }:
                return "constructor: checks construct it through public behaviour or PrivateState.New";
            case IMethodSymbol { IsGenericMethod: true }:
                return "generic method";
            case IMethodSymbol { IsPartialDefinition: true } or IMethodSymbol { PartialImplementationPart: not null }:
                return "partial method";
            case IMethodSymbol method when method.Parameters.Any(parameter => parameter.Type.IsRefLikeType || parameter.Type.TypeKind == TypeKind.Pointer ||
                                                                          parameter.RefKind is RefKind.RefReadOnlyParameter) ||
                                           method.ReturnType.IsRefLikeType || method.ReturnsByRef || method.ReturnsByRefReadonly:
                return "ref struct, pointer or by-reference return in its signature";
            case IPropertySymbol { IsIndexer: true }:
                return "indexer";
            case IPropertySymbol property when property.Type.IsRefLikeType || property.ReturnsByRef || property.ReturnsByRefReadonly:
                return "ref struct or by-reference property";
            case IPropertySymbol { IsRequired: true } or IFieldSymbol { IsRequired: true }:
                return "required member";
            case IPropertySymbol property when property.DeclaringSyntaxReferences[0].GetSyntax() is ParameterSyntax:
                return "positional record property";
            case IPropertySymbol property when !ways.Any(way => way.Uses.Way == "read") && property.GetMethod is not null &&
                                               property.SetMethod is null:
                return "property shape";
            case IPropertySymbol property when Partial(property, ways):
                return "one accessor stays wider: checks reach the other through PrivateState";
            case IFieldSymbol { IsConst: true }:
                return "constant: checks may name it where a constant is required";
            case IFieldSymbol field when field.Type.IsRefLikeType || field.Type.TypeKind == TypeKind.Pointer || field.IsFixedSizeBuffer:
                return "ref struct, pointer or fixed buffer field";
            case IFieldSymbol field when field.DeclaringSyntaxReferences[0].GetSyntax() is VariableDeclaratorSyntax
            {
                Parent: VariableDeclarationSyntax { Variables.Count: > 1 },
            }:
                return "field sharing its declaration";
            case IEventSymbol:
                return "event";
            case IMethodSymbol or IPropertySymbol or IFieldSymbol:
                return null;
            default:
                return $"{symbol.Kind}";
        }
    }

    /// <summary>
    /// Whether only one of a property's accessors becomes private while the other stays wider: a check reaching the
    /// private one would still bind to the property, which its own code keeps accessible, and not to the extension.
    /// </summary>
    private static bool Partial(IPropertySymbol property, List<TestOnlyMemberFinding> ways)
    {
        bool reads = ways.Any(way => way.Uses.Way == "read");
        bool writes = ways.Any(way => way.Uses.Way == "write");
        bool getterStays = property.GetMethod is { DeclaredAccessibility: not Accessibility.Private } && !reads;
        bool setterStays = property.SetMethod is { DeclaredAccessibility: not Accessibility.Private } && !writes;
        return getterStays || setterStays;
    }

    /// <summary>
    /// The edits for a narrow finding no extension member can stand in for, which checks then reach through
    /// <c>PrivateState</c> at each site: a constructor (<c>new Owner(...)</c> becomes <c>PrivateState.New</c>), and a
    /// property one of whose accessors stays wider (a check's read becomes <c>PrivateState.Get</c> and its assignment
    /// <c>PrivateState.Set</c>, since the check would bind to the property its own code keeps accessible). Null for any
    /// other finding; a refusal for a site no such call can stand in for.
    /// </summary>
    private static SiteRewrite? ThroughPrivateState(ISymbol symbol, List<TestOnlyMemberFinding> ways)
    {
        bool constructor = symbol is IMethodSymbol { MethodKind: MethodKind.Constructor };
        bool partialProperty = symbol is IPropertySymbol property && !property.IsIndexer && Partial(property, ways);
        if (!constructor && !partialProperty || ways.Any(way => way.Shape != "narrow"))
            return null;
        if (symbol.DeclaringSyntaxReferences.Any(reference => GeneratedSource.IsGenerated(reference.SyntaxTree, CancellationToken.None) ||
                reference.SyntaxTree.FilePath.Contains(".Generated.", StringComparison.OrdinalIgnoreCase)))
            return new SiteRewrite([], "generated declaration: fix its generator");
        if (PrivateAccessShims.Refusal(symbol) is { } shape && !constructor)
            return new SiteRewrite([], shape);
        for (INamedTypeSymbol? container = symbol.ContainingType; container is not null; container = container.ContainingType)
            if (container.IsGenericType || container.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                return new SiteRewrite([], "generic owner, or one test-support cannot see");
        if (symbol.DeclaringSyntaxReferences.Length != 1 || symbol.DeclaringSyntaxReferences[0].GetSyntax() is not MemberDeclarationSyntax declaration)
            return new SiteRewrite([], "declared by no single member declaration (a primary constructor or a partial member)");
        string owner = PrivateAccessShims.TypeText(symbol.ContainingType);
        string state = "global::SuperMetroid.Core.PrivateState";
        var result = new List<(SyntaxTree Tree, TextChange Change)>();
        var sites = ways.SelectMany(way => way.Uses.CheckSites.Select(site => (way.Uses.Way, Site: site))).ToList();
        if (constructor)
        {
            var method = (IMethodSymbol)symbol;
            if (method.Parameters.Any(parameter => parameter.RefKind != RefKind.None || parameter.Type.IsRefLikeType))
                return new SiteRewrite([], "constructor with a by-reference or ref struct parameter");
            foreach (TextChange change in Private(declaration.Modifiers, declaration))
                result.Add((declaration.SyntaxTree, change));
            string types = "[" + string.Join(", ", method.Parameters.Select(parameter => $"typeof({PrivateAccessShims.TypeOfText(parameter.Type)})")) + "]";
            foreach (var (_, site) in sites)
            {
                if (site is not BaseObjectCreationExpressionSyntax { Initializer: null, ArgumentList: { } arguments } creation ||
                    arguments.Arguments.Any(argument => argument.NameColon is not null || !argument.RefKindKeyword.IsKind(SyntaxKind.None)) ||
                    arguments.Arguments.Count != method.Parameters.Length)
                    return new SiteRewrite([], $"constructor site no PrivateState.New stands in for at {site.GetLocation().GetLineSpan()}");
                string passed = string.Join(", ", arguments.Arguments.Select((argument, index) =>
                    $"({PrivateAccessShims.TypeText(method.Parameters[index].Type)})({argument.Expression})"));
                result.Add((creation.SyntaxTree, new TextChange(creation.Span, $"{state}.New<{owner}>({types}, new object?[] {{ {passed} }})")));
            }
            return new SiteRewrite(result, null);
        }
        var narrowedProperty = (IPropertySymbol)symbol;
        if (declaration is not PropertyDeclarationSyntax { AccessorList: { } accessors })
            return new SiteRewrite([], "property without accessors to narrow");
        var narrowedWays = ways.Select(way => way.Uses.Way).ToHashSet(StringComparer.Ordinal);
        foreach (AccessorDeclarationSyntax accessor in accessors.Accessors)
        {
            bool getter = accessor.IsKind(SyntaxKind.GetAccessorDeclaration);
            if (!narrowedWays.Contains(getter ? "read" : "write"))
                continue;
            var existing = accessor.Modifiers.Where(IsAccessibility).ToList();
            if (existing.Count > 0)
            {
                result.Add((accessor.SyntaxTree, new TextChange(existing[0].Span, "private")));
                foreach (SyntaxToken other in existing.Skip(1))
                    result.Add((accessor.SyntaxTree, new TextChange(TextSpan.FromBounds(other.SpanStart, other.GetNextToken().SpanStart), "")));
            }
            else
            {
                result.Add((accessor.SyntaxTree, new TextChange(new TextSpan(accessor.Keyword.SpanStart, 0), "private ")));
            }
        }
        string type = PrivateAccessShims.TypeText(narrowedProperty.Type);
        foreach (var (way, site) in sites)
        {
            if (site is not SimpleNameSyntax { Parent: MemberAccessExpressionSyntax access } name || access.Name != name)
                return new SiteRewrite([], $"property site without a receiver at {site.GetLocation().GetLineSpan()}");
            string receiver = narrowedProperty.IsStatic ? "" : access.Expression.ToString();
            if (way == "read")
            {
                if (TestOnlyMembers.IsWrite(name))
                    return new SiteRewrite([], $"property site that both reads and writes at {site.GetLocation().GetLineSpan()}");
                string read = narrowedProperty.IsStatic ? $"{state}.StaticGet<{type}>(typeof({owner}), \"{symbol.Name}\")"
                    : symbol.ContainingType.IsValueType ? $"{state}.ValueGet<{owner}, {type}>({receiver}, \"{symbol.Name}\")"
                    : $"{state}.Get<{owner}, {type}>({receiver}, \"{symbol.Name}\")";
                result.Add((site.SyntaxTree, new TextChange(access.Span, read)));
                continue;
            }
            if (access.Parent is not AssignmentExpressionSyntax { Parent: ExpressionStatementSyntax } assignment ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) || assignment.Left != access || symbol.ContainingType.IsValueType)
                return new SiteRewrite([], $"property write no PrivateState.Set stands in for at {site.GetLocation().GetLineSpan()}");
            string write = narrowedProperty.IsStatic ? $"{state}.StaticSet(typeof({owner}), \"{symbol.Name}\", ({type})({assignment.Right}))"
                : $"{state}.Set({receiver}, \"{symbol.Name}\", ({type})({assignment.Right}))";
            result.Add((site.SyntaxTree, new TextChange(assignment.Span, write)));
        }
        return new SiteRewrite(result, null);
    }

    /// <summary>Whether a change overlaps none of a tree's edits so far.</summary>
    private static bool Fits(Dictionary<SyntaxTree, List<TextChange>> edits, SyntaxTree tree, TextChange change) =>
        !edits.TryGetValue(tree, out var list) || !list.Any(other => other.Span.OverlapsWith(change.Span) || other.Span.Start == change.Span.Start);
    /// <summary>Whether any check writes the member: assigns it, increments it or passes it by reference.</summary>
    private static bool Written(List<TestOnlyMemberFinding> ways) =>
        ways.Any(way => way.Uses.Way == "write") || ways.SelectMany(way => way.Uses.CheckSites).Any(site =>
            site is SimpleNameSyntax name && TestOnlyMembers.IsWrite(name));

    /// <summary>The edits that make a member private: its own accessibility, and its accessors' modifiers dropped.</summary>
    private static IEnumerable<(SyntaxTree Tree, TextChange Change)> Narrow(ISymbol symbol)
    {
        SyntaxNode declaration = symbol.DeclaringSyntaxReferences[0].GetSyntax();
        MemberDeclarationSyntax member = declaration switch
        {
            VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } => field,
            MemberDeclarationSyntax declared => declared,
            _ => throw new InvalidOperationException($"{symbol.ToDisplayString()} is declared by a {declaration.Kind()}."),
        };
        SyntaxTree tree = member.SyntaxTree;
        foreach (TextChange change in Private(member.Modifiers, member))
            yield return (tree, change);
        if (member is PropertyDeclarationSyntax { AccessorList: { } accessors })
        {
            foreach (AccessorDeclarationSyntax accessor in accessors.Accessors)
                foreach (SyntaxToken modifier in accessor.Modifiers.Where(IsAccessibility))
                    yield return (tree, new TextChange(TextSpan.FromBounds(modifier.SpanStart, modifier.GetNextToken().SpanStart), ""));
        }
    }

    /// <summary>The edits that replace a declaration's accessibility modifiers with <c>private</c>.</summary>
    private static IEnumerable<TextChange> Private(SyntaxTokenList modifiers, MemberDeclarationSyntax member)
    {
        var accessibility = modifiers.Where(IsAccessibility).ToList();
        if (accessibility.Count == 0)
        {
            int at = modifiers.Count > 0 ? modifiers[0].SpanStart : member.ChildNodesAndTokens()
                .First(child => !child.IsKind(SyntaxKind.AttributeList)).SpanStart;
            yield return new TextChange(new TextSpan(at, 0), "private ");
            yield break;
        }
        yield return new TextChange(accessibility[0].Span, "private");
        foreach (SyntaxToken other in accessibility.Skip(1))
            yield return new TextChange(TextSpan.FromBounds(other.SpanStart, other.GetNextToken().SpanStart), "");
    }

    /// <summary>Whether a modifier states accessibility.</summary>
    private static bool IsAccessibility(SyntaxToken modifier) => modifier.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword or
        SyntaxKind.ProtectedKeyword or SyntaxKind.PrivateKeyword;

    /// <summary>Adds a change to a tree's edits.</summary>
    private static void Add(Dictionary<SyntaxTree, List<TextChange>> edits, SyntaxTree tree, TextChange change)
    {
        if (!edits.TryGetValue(tree, out var list))
            edits[tree] = list = [];
        if (list.Any(other => other.Span.OverlapsWith(change.Span) || other.Span == change.Span && change.Span.IsEmpty))
            throw new InvalidOperationException($"{tree.FilePath}: two narrowing edits overlap at {change.Span}.");
        list.Add(change);
    }

    /// <summary>The generated private-access class of <paramref name="owner"/>, created on first use.</summary>
    private static PrivateAccessShims Shims(string root, Dictionary<string, PrivateAccessShims> shims, INamedTypeSymbol owner)
    {
        string key = PrivateAccessShims.TypeOfText(owner);
        if (!shims.TryGetValue(key, out PrivateAccessShims? shim))
            shims[key] = shim = PrivateAccessShims.For(root, owner);
        return shim;
    }

    /// <summary>
    /// The edits that keep checks naming a narrowed member outside its type compiling: <c>nameof(Owner.Member)</c> becomes
    /// the member's name as text, since <c>nameof</c> binds to the extension member standing in for it, and a documentation
    /// cref to it becomes the name as code text, since a cref cannot name a member its site cannot see.
    /// </summary>
    private static IEnumerable<(SyntaxTree Tree, TextChange Change)> CheckSiteRewrites(LoadedSolution solution, HashSet<ISymbol> narrowed)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, compilation) in solution.Projects)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                if (!seen.Add(Path.GetFullPath(tree.FilePath)) || tree.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                    continue;
                SemanticModel model = SemanticModels.Of(compilation, tree);
                SyntaxNode root = tree.GetRoot();
                foreach (InvocationExpressionSyntax nameOf in root.DescendantNodes().OfType<InvocationExpressionSyntax>()
                             .Where(invocation => invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" } &&
                                                  invocation.ArgumentList.Arguments.Count == 1))
                {
                    if (Narrowed(model.GetSymbolInfo(nameOf.ArgumentList.Arguments[0].Expression), narrowed) is { } member &&
                        !Inside(model, nameOf, member))
                        yield return (tree, new TextChange(nameOf.Span, SymbolDisplay.FormatLiteral(member.Name, quote: true)));
                }
                foreach (XmlEmptyElementSyntax see in root.DescendantNodes(descendIntoTrivia: true).OfType<XmlEmptyElementSyntax>())
                {
                    if (see.Attributes.OfType<XmlCrefAttributeSyntax>().FirstOrDefault() is { } cref &&
                        Narrowed(model.GetSymbolInfo(cref.Cref), narrowed) is { } member && !Inside(model, see, member))
                        yield return (tree, new TextChange(see.Span, $"<c>{member.Name}</c>"));
                }
            }
        }
    }

    /// <summary>The narrowed member a bound name stands for, or null.</summary>
    private static ISymbol? Narrowed(SymbolInfo info, HashSet<ISymbol> narrowed) =>
        info.CandidateSymbols.Prepend(info.Symbol).OfType<ISymbol>()
            .Select(symbol => symbol is IMethodSymbol { ReducedFrom: { } reduced } ? reduced : symbol)
            .Select(symbol => symbol.OriginalDefinition).FirstOrDefault(narrowed.Contains);

    /// <summary>Whether <paramref name="site"/> is code of the member's own type, which still sees it.</summary>
    private static bool Inside(SemanticModel model, SyntaxNode site, ISymbol member) =>
        site.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any(declaration =>
            SymbolEqualityComparer.Default.Equals(model.GetDeclaredSymbol(declaration)?.OriginalDefinition, member.ContainingType.OriginalDefinition));
    /// <summary>Whether a file starts with a UTF-8 byte-order mark.</summary>
    private static bool HasByteOrderMark(string path)
    {
        byte[] start = new byte[3];
        using FileStream stream = File.OpenRead(path);
        return stream.Read(start, 0, 3) == 3 && start is [0xEF, 0xBB, 0xBF];
    }

    /// <summary>A member's display name with the file and line of its declaration.</summary>
    private static string Describe(string root, ISymbol member)
    {
        Location location = member.Locations.First(location => location.IsInSource);
        return $"{Path.GetRelativePath(root, location.SourceTree!.FilePath).Replace('\\', '/')}:{location.GetLineSpan().StartLinePosition.Line + 1}: " +
            member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }

}
