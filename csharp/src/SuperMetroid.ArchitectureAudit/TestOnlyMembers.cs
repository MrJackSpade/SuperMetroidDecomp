using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// The test-only-member check (#2570): production holds no member, and no accessor, that is wider than its production
/// use only so that checks can reach it. A product member (field, property accessor, method, constructor or event) that
/// is not private, that a check reaches, and that no shipped or developer-tool code outside its own type reaches the same
/// way, is a finding: it is made private (checks observe it through public behaviour, a test-support access extension or
/// <c>PrivateState</c>) or removed. A site where that is impossible, or a false positive of the check, is justified on the
/// member itself with <c>[ArchitectureReview("test-only-member", reason)]</c>; nothing outside the member can justify it.
/// <para>
/// Checks are the verification projects, the audits and the shared test-support sources. Code a member's own type (or a
/// type nested in it) runs is not counted, since a private member serves it. A member no production code reaches at all
/// is reported too; the reachability audit removes such a member only once no check names it.
/// </para>
/// </summary>
internal static class TestOnlyMembers
{
    /// <summary>The finding kind, and the name its review gives.</summary>
    internal const string Finding = "test-only-member";

    /// <summary>A member reached as a whole: a field, method, constructor or event.</summary>
    private const string Use = "use";

    /// <summary>A property or indexer reached through its getter.</summary>
    private const string Read = "read";

    /// <summary>A property or indexer reached through its setter or init accessor.</summary>
    private const string Write = "write";

    /// <summary>Collects every finding, every justified finding and every stale member review across the solution.</summary>
    public static MemberAudit Collect(LoadedSolution solution)
    {
        var trees = FirstCompilations(solution);
        SemanticModels.Bind(trees.Select(tree => (tree.Compilation, tree.Tree)));
        // A file several projects compile declares its members once, in the first of them, as it is walked.
        var declaring = trees.Select(tree => tree.Tree).ToHashSet();
        var uses = new Dictionary<string, MemberUses>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        var reviews = new List<(ISymbol Member, ArchitectureReview Review)>();
        foreach (var (project, compilation) in solution.Projects)
        {
            bool product = ProjectScope.IsProduct(project.AssemblyName);
            foreach (INamedTypeSymbol type in SourceTypes(compilation.Assembly.GlobalNamespace))
            {
                var implementations = InterfaceImplementations(type);
                foreach (ISymbol member in type.GetMembers())
                {
                    if (!member.Locations.Any(location => location.SourceTree is { } tree && declaring.Contains(tree)))
                        continue;
                    reviews.AddRange(Reviews(member).Select(review => (member, review)));
                    if (!product || !IsCandidate(member, implementations))
                        continue;
                    foreach (string way in Ways(member))
                        uses.TryAdd(Key(member, way), new MemberUses(member, way));
                    names.Add(member.Name);
                }
            }
        }

        foreach (var (compilation, tree, file, check) in trees)
            Walk(compilation, tree, file, check, uses, names);
        return Report(solution.RepositoryRoot, uses, reviews);
    }

    /// <summary>
    /// Each repository source file with the first project that compiles it, as the module inventory binds it, so the two
    /// share one binding; whether the file is a check comes from its folder or that project.
    /// </summary>
    private static List<(Compilation Compilation, SyntaxTree Tree, string File, bool Check)> FirstCompilations(LoadedSolution solution)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var trees = new List<(Compilation, SyntaxTree, string, bool)>();
        foreach (var (project, compilation) in solution.Projects)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                if (!IsRepositoryFile(solution.RepositoryRoot, tree.FilePath) || !seen.Add(Path.GetFullPath(tree.FilePath)))
                    continue;
                string file = Relative(solution.RepositoryRoot, tree.FilePath);
                trees.Add((compilation, tree, file, ProjectScope.IsTestSupport(file) || ProjectScope.IsCheck(project.AssemblyName)));
            }
        }
        return trees;
    }

    /// <summary>Records every place in one file that reaches a candidate member, by the way it reaches it.</summary>
    private static void Walk(Compilation compilation, SyntaxTree tree, string file, bool check,
        Dictionary<string, MemberUses> uses, HashSet<string> names)
    {
        var enclosing = new Dictionary<SyntaxNode, INamedTypeSymbol?>();
        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            switch (node)
            {
                case SimpleNameSyntax name when names.Contains(name.Identifier.ValueText) && !InNameOf(name):
                    RecordAll(Info(compilation, tree, name), name, Access(AccessExpression(name)));
                    break;
                case BaseObjectCreationExpressionSyntax or ConstructorInitializerSyntax or PrimaryConstructorBaseTypeSyntax or AttributeSyntax:
                    RecordAll(Info(compilation, tree, node), node, Use);
                    break;
                case ElementAccessExpressionSyntax or ElementBindingExpressionSyntax:
                    RecordAll(Info(compilation, tree, node), node, Access(AccessExpression((ExpressionSyntax)node)));
                    break;
                case CommonForEachStatementSyntax loop:
                    ForEachStatementInfo each = SemanticModels.At(compilation, tree, loop).GetForEachStatementInfo(loop);
                    Record(each.GetEnumeratorMethod, loop, Use);
                    Record(each.MoveNextMethod, loop, Use);
                    Record(each.CurrentProperty, loop, Read);
                    break;
                case InitializerExpressionSyntax initializer when initializer.IsKind(SyntaxKind.CollectionInitializerExpression):
                    foreach (ExpressionSyntax element in initializer.Expressions)
                        RecordAll(SemanticModels.At(compilation, tree, element).GetCollectionInitializerSymbolInfo(element), element, Use);
                    break;
                default:
                    break;
            }
        }

        void RecordAll(SymbolInfo info, SyntaxNode site, string access)
        {
            Record(info.Symbol, site, access);
            foreach (ISymbol candidate in info.CandidateSymbols)
                Record(candidate, site, access);
        }

        void Record(ISymbol? symbol, SyntaxNode site, string access)
        {
            if (Normalize(symbol) is not { } member || !names.Contains(member.Name))
                return;
            foreach (string way in WaysReached(member, access))
            {
                if (!uses.TryGetValue(Key(member, way), out MemberUses? reached) || IsInside(site, member, enclosing, compilation, tree))
                    continue;
                if (!check)
                {
                    reached.Production++;
                }
                else
                {
                    reached.Checks++;
                    reached.FirstCheck ??= $"{file}:{site.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";
                }
            }
        }
    }

    /// <summary>The bound symbol of <paramref name="node"/>, from the model that bound it.</summary>
    private static SymbolInfo Info(Compilation compilation, SyntaxTree tree, SyntaxNode node) =>
        SemanticModels.At(compilation, tree, node).GetSymbolInfo(node);

    /// <summary>The ways a site that reaches <paramref name="member"/> with <paramref name="access"/> reaches it.</summary>
    private static IEnumerable<string> WaysReached(ISymbol member, string access) => member switch
    {
        IPropertySymbol when access == Use => [Read],
        IPropertySymbol when access == ReadWrite => [Read, Write],
        IPropertySymbol => [access],
        _ => [Use],
    };

    /// <summary>A site that both reads and writes: a compound assignment, an increment or a <c>ref</c> argument.</summary>
    private const string ReadWrite = "read-write";

    /// <summary>The ways a candidate member can be reached, each of which is checked on its own.</summary>
    private static IEnumerable<string> Ways(ISymbol member)
    {
        if (member is not IPropertySymbol property)
        {
            yield return Use;
            yield break;
        }
        if (property.GetMethod is { DeclaredAccessibility: not Accessibility.Private })
            yield return Read;
        if (property.SetMethod is { DeclaredAccessibility: not Accessibility.Private })
            yield return Write;
    }

    /// <summary>
    /// Whether <paramref name="member"/> is one whose width the check judges: a non-private field, property, ordinary
    /// method, constructor or event that nothing outside the repository's own calls demands be wide (an override, an
    /// interface implementation, a virtual or abstract member, an operator, a serializer constructor, an entry point).
    /// </summary>
    private static bool IsCandidate(ISymbol member, HashSet<ISymbol> implementations)
    {
        if (member.IsImplicitlyDeclared || member.IsOverride || member.IsAbstract || member.IsVirtual || member.IsExtern ||
            implementations.Contains(member) || member.ContainingType.TypeKind is TypeKind.Enum or TypeKind.Interface or TypeKind.Extension)
            return false;
        if (member.GetAttributes().Any(attribute => attribute.AttributeClass?.ContainingNamespace?.ToDisplayString() is
                "System.Text.Json.Serialization" or "System.Runtime.InteropServices" or "System.Runtime.CompilerServices"))
            return false;
        return member switch
        {
            IFieldSymbol field => field.DeclaredAccessibility != Accessibility.Private && field.AssociatedSymbol is null,
            IPropertySymbol property => property.ExplicitInterfaceImplementations.IsEmpty && Ways(property).Any(),
            IEventSymbol @event => @event.DeclaredAccessibility != Accessibility.Private && @event.ExplicitInterfaceImplementations.IsEmpty,
            IMethodSymbol method => method.DeclaredAccessibility != Accessibility.Private &&
                method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor && method.Name != "Main" &&
                method.ExplicitInterfaceImplementations.IsEmpty,
            _ => false,
        };
    }

    /// <summary>The members of <paramref name="type"/> that implement one of its interfaces' members.</summary>
    private static HashSet<ISymbol> InterfaceImplementations(INamedTypeSymbol type)
    {
        var implementations = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (INamedTypeSymbol contract in type.AllInterfaces)
        {
            foreach (ISymbol required in contract.GetMembers())
            {
                if (type.FindImplementationForInterfaceMember(required) is { } implementation)
                    implementations.Add(implementation);
            }
        }
        return implementations;
    }

    /// <summary>The declared member a reached symbol stands for: the original definition, unreduced, an accessor's property.</summary>
    private static ISymbol? Normalize(ISymbol? symbol) => symbol switch
    {
        IMethodSymbol { ReducedFrom: { } reduced } => reduced.OriginalDefinition,
        IMethodSymbol { AssociatedSymbol: IPropertySymbol property } => property.OriginalDefinition,
        IFieldSymbol or IPropertySymbol or IMethodSymbol or IEventSymbol => symbol.OriginalDefinition,
        _ => null,
    };

    /// <summary>
    /// The expression whose context says how a name or element access reaches its member: the member access or
    /// conditional access the name ends, out of any parentheses.
    /// </summary>
    private static ExpressionSyntax AccessExpression(ExpressionSyntax node)
    {
        ExpressionSyntax access = node;
        if (access.Parent is MemberAccessExpressionSyntax member && member.Name == access)
            access = member;
        else if (access.Parent is MemberBindingExpressionSyntax binding && binding.Name == access)
            access = binding;
        while (access.Parent is ConditionalAccessExpressionSyntax conditional && conditional.WhenNotNull == access)
            access = conditional;
        while (access.Parent is ParenthesizedExpressionSyntax parenthesized)
            access = parenthesized;
        return access;
    }

    /// <summary>Whether the access expression is written, read, or both, from where it stands.</summary>
    private static string Access(ExpressionSyntax access) => access.Parent switch
    {
        AssignmentExpressionSyntax assignment when assignment.Left == access =>
            assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) ? Write : ReadWrite,
        PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax
            when access.Parent.Kind() is SyntaxKind.PreIncrementExpression or SyntaxKind.PreDecrementExpression or
                SyntaxKind.PostIncrementExpression or SyntaxKind.PostDecrementExpression => ReadWrite,
        ArgumentSyntax { RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword } => Write,
        ArgumentSyntax { RefKindKeyword.RawKind: (int)SyntaxKind.RefKeyword } => ReadWrite,
        ArgumentSyntax { Parent: TupleExpressionSyntax tuple } when IsAssignedTuple(tuple) => Write,
        _ => Read,
    };

    /// <summary>Whether a tuple is the target of a deconstructing assignment, however deeply nested.</summary>
    private static bool IsAssignedTuple(TupleExpressionSyntax tuple) => tuple.Parent switch
    {
        AssignmentExpressionSyntax assignment => assignment.Left == tuple,
        ArgumentSyntax { Parent: TupleExpressionSyntax outer } => IsAssignedTuple(outer),
        _ => false,
    };

    /// <summary>Whether a name stands inside <c>nameof(...)</c>, which names a member without reaching it.</summary>
    private static bool InNameOf(SyntaxNode name)
    {
        for (SyntaxNode? node = name.Parent; node is ExpressionSyntax or ArgumentSyntax or ArgumentListSyntax; node = node.Parent)
        {
            if (node is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } })
                return true;
        }
        return false;
    }

    /// <summary>Whether <paramref name="site"/> is code of <paramref name="member"/>'s own type, or of a type nested in it.</summary>
    private static bool IsInside(SyntaxNode site, ISymbol member, Dictionary<SyntaxNode, INamedTypeSymbol?> enclosing,
        Compilation compilation, SyntaxTree tree)
    {
        string owner = Key(member.ContainingType.OriginalDefinition, Use);
        foreach (BaseTypeDeclarationSyntax declaration in site.Ancestors().OfType<BaseTypeDeclarationSyntax>())
        {
            if (!enclosing.TryGetValue(declaration, out INamedTypeSymbol? type))
                enclosing[declaration] = type = SemanticModels.Of(compilation, tree).GetDeclaredSymbol(declaration);
            if (type is not null && Key(type.OriginalDefinition, Use) == owner)
                return true;
        }
        return false;
    }

    /// <summary>The problems, the count of justified findings and the stale member reviews.</summary>
    private static MemberAudit Report(string root, Dictionary<string, MemberUses> uses, List<(ISymbol Member, ArchitectureReview Review)> reviews)
    {
        var flagged = uses.Values.Where(use => use.Checks != 0 && use.Production == 0)
            .GroupBy(use => use.Member, SymbolEqualityComparer.Default).ToList();
        var reviewed = reviews.ToLookup(review => review.Member, review => review.Review, SymbolEqualityComparer.Default);
        var problems = new List<string>();
        foreach (var (member, review) in reviews.Where(review => string.IsNullOrWhiteSpace(review.Review.Reason)))
            problems.Add($"{Describe(root, member)}: [ArchitectureReview(\"{review.Finding}\")] needs a reason specific to this member.");
        int justified = 0;
        foreach (var finding in flagged.OrderBy(finding => Describe(root, finding.Key!), StringComparer.Ordinal))
        {
            ISymbol member = finding.Key!;
            if (reviewed[member].Any(review => review.Finding == Finding && !string.IsNullOrWhiteSpace(review.Reason)))
            {
                justified++;
                continue;
            }
            foreach (MemberUses use in finding.OrderBy(use => use.Way, StringComparer.Ordinal))
                problems.Add($"{Describe(root, member)} is flagged [{Finding}]: no product code outside {member.ContainingType.Name} " +
                    $"{Verb(use.Way)} it, but {use.Checks} check site(s) do, first {use.FirstCheck}. Make it private (checks observe " +
                    $"public behaviour, a test-support access extension or PrivateState) or remove it, or justify this one member " +
                    $"with [ArchitectureReview(\"{Finding}\", reason)].");
        }
        var flaggedMembers = flagged.Select(finding => finding.Key!).ToHashSet(SymbolEqualityComparer.Default);
        var stale = reviews.Where(review => review.Review.Finding != Finding || !flaggedMembers.Contains(review.Member))
            .Select(review => new StaleReview(Describe(root, review.Member), review.Review)).ToList();
        return new MemberAudit(problems, justified, stale);
    }

    /// <summary>What a site does to a member that is reached the given way.</summary>
    private static string Verb(string way) => way switch
    {
        Read => "reads",
        Write => "writes",
        _ => "uses",
    };

    /// <summary>A member's display name with the file and line of its first declaration.</summary>
    private static string Describe(string root, ISymbol member)
    {
        Location location = member.Locations.First(location => location.IsInSource);
        return $"{Relative(root, location.SourceTree!.FilePath)}:{location.GetLineSpan().StartLinePosition.Line + 1}: " +
            member.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
    }

    /// <summary>The <c>[ArchitectureReview]</c> attributes declared on a member.</summary>
    private static IEnumerable<ArchitectureReview> Reviews(ISymbol member) => member.GetAttributes()
        .Where(attribute => attribute.AttributeClass?.Name == TypeInventory.ReviewAttributeName && attribute.ConstructorArguments.Length == 2)
        .Select(attribute => new ArchitectureReview(
            attribute.ConstructorArguments[0].Value as string ?? "",
            attribute.ConstructorArguments[1].Value as string ?? "",
            attribute.ApplicationSyntaxReference ??
                throw new InvalidOperationException($"{member}: an [ArchitectureReview] has no source location.")));

    /// <summary>
    /// A member's key in one way of reaching it, the same in every compilation that sees it: its documentation id and
    /// declaring assembly.
    /// </summary>
    private static string Key(ISymbol member, string way) =>
        $"{member.GetDocumentationCommentId()}@{member.ContainingAssembly?.Name}#{way}";

    /// <summary>Walks every source type of an assembly, nested types included.</summary>
    private static IEnumerable<INamedTypeSymbol> SourceTypes(INamespaceSymbol root)
    {
        foreach (INamespaceOrTypeSymbol member in root.GetMembers())
        {
            if (member is INamespaceSymbol nested)
            {
                foreach (INamedTypeSymbol type in SourceTypes(nested))
                    yield return type;
            }
            else if (member is INamedTypeSymbol type)
            {
                foreach (INamedTypeSymbol inner in WithNested(type))
                    yield return inner;
            }
        }
    }

    /// <summary>A type and every type nested in it.</summary>
    private static IEnumerable<INamedTypeSymbol> WithNested(INamedTypeSymbol type)
    {
        yield return type;
        foreach (INamedTypeSymbol nested in type.GetTypeMembers())
            foreach (INamedTypeSymbol inner in WithNested(nested))
                yield return inner;
    }

    /// <summary>Whether a path is maintained repository source: not build output, not a temporary probe.</summary>
    private static bool IsRepositoryFile(string root, string path) =>
        path.Length != 0 && Path.GetFullPath(path).StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase) &&
        !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
        !path.Contains($"{Path.DirectorySeparatorChar}test-temp{Path.DirectorySeparatorChar}", StringComparison.Ordinal);

    /// <summary>A slash-separated repository-relative path.</summary>
    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');
}
