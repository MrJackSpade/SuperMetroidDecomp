using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Analysis;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>
/// The test-only-member census (#2570): every product member, and every accessor, wider than its production use only so
/// that checks can reach it. A product member (field, property accessor, method, constructor or event) that is not
/// private, that a check reaches, and that no shipped or developer-tool code outside its own type reaches the same way,
/// is listed: it is made private (checks observe it through public behaviour, a test-support access extension or
/// <c>PrivateState</c>) or removed.
/// <para>
/// Checks are the files only verification projects compile. Code a member's own type (or a type nested in it) runs is not
/// counted, since a private member serves it; what System.Text.Json reads or binds of a payload shipped code serializes is
/// production use. A member production never uses at all is dead code, which the reachability prune moves into
/// test-support with what it reaches, reaching private state through the same private-access members (#2570), so the
/// census leaves it to the prune. The architecture audit's push gate fails on every finding.
/// </para>
/// <para>
/// A private member, or a private accessor, is listed too when a check names it: a check binds to it only when nothing
/// stands in for it, so its generated private-access member is missing, and the fix regenerates that member. The
/// generated classes can therefore be deleted and rebuilt from the checks that name them.
/// </para>
/// </summary>
internal static class TestOnlyMembers
{
    /// <summary>The finding kind each listed site is reported under.</summary>
    internal const string Finding = "test-only-member";

    /// <summary>A member reached as a whole: a field, method, constructor or event.</summary>
    private const string Use = "use";

    /// <summary>A property or indexer reached through its getter.</summary>
    private const string Read = "read";

    /// <summary>A property or indexer reached through its setter or init accessor.</summary>
    private const string Write = "write";

    /// <summary>A site that both reads and writes: a compound assignment, an increment or a <c>ref</c> argument.</summary>
    private const string ReadWrite = "read-write";

    /// <summary>Every test-only way of reaching a member of the solution's product assemblies.</summary>
    public static List<TestOnlyMemberFinding> Collect(LoadedSolution solution)
    {
        var trees = FirstCompilations(solution);
        SemanticModels.Bind(trees.Select(tree => (tree.Compilation, tree.Tree)));
        // A file several projects compile declares its members once, in the first of them, as it is walked.
        var declaring = trees.Select(tree => tree.Tree).ToHashSet();
        var uses = new Dictionary<string, MemberUses>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (project, compilation) in solution.Projects)
        {
            if (!ProjectScope.IsProduct(project.AssemblyName))
                continue;
            foreach (INamedTypeSymbol type in SourceTypes(compilation.Assembly.GlobalNamespace))
            {
                var implementations = InterfaceImplementations(type);
                foreach (ISymbol member in type.GetMembers())
                {
                    if (!member.Locations.Any(location => location.SourceTree is { } tree && declaring.Contains(tree)) ||
                        !IsCandidate(member, implementations))
                        continue;
                    foreach (string way in Ways(member))
                        uses.TryAdd(Key(member, way), new MemberUses(member, way));
                    names.Add(member.Name);
                }
            }
        }

        // What System.Text.Json reads or binds of a payload shipped code serializes is production use: a document's
        // properties are written and read by the serializer, not by code naming them.
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var serialization = new JsonSerializationScan(identity);
        foreach (var (compilation, tree, file, check) in trees)
            Walk(compilation, tree, file, check, uses, names, check ? null : serialization);
        var serialized = serialization.SerializerAccessedMembers();
        foreach (MemberUses use in uses.Values)
            if (identity.Key(use.Member) is { } key && serialized.Contains(key))
                use.Production++;
        var used = uses.Values.GroupBy(use => use.Member, SymbolEqualityComparer.Default)
            .Where(member => member.Any(use => use.Production + use.InsideReads + use.InsideWrites != 0))
            .Select(member => member.Key!).ToHashSet(SymbolEqualityComparer.Default);
        // A member production never uses at all is dead code the reachability prune moves into test-support, which the
        // asynchronous cleanup runs after each push; only a member production uses, but more widely than it needs, is a finding.
        return [.. uses.Values.Where(use => use.Checks != 0 && use.Production == 0 && used.Contains(use.Member))
            .Select(use => new TestOnlyMemberFinding(use, Shape(use, used: true)))];
    }

    /// <summary>One line per finding, in file order, for the census.</summary>
    public static List<string> Census(LoadedSolution solution) =>
        [.. Collect(solution).Select(finding => $"{Describe(solution.RepositoryRoot, finding.Uses.Member)} is flagged [{Finding}] " +
                $"({finding.Shape}): no product code outside {finding.Uses.Member.ContainingType.Name} {Verb(finding.Uses.Way)} it, " +
                $"but {finding.Uses.Checks} check site(s) do, first {finding.Uses.FirstCheck}.")
            .Order(StringComparer.Ordinal)];

    /// <summary>
    /// What production does with a test-only way of reaching a member: its own type reaches it that way (<c>narrow</c>:
    /// the way becomes private), production uses the member but never this way (<c>unneeded</c>: the way leaves production,
    /// a write-only field included), or production never uses the member (<c>unused</c>: it leaves production).
    /// </summary>
    private static string Shape(MemberUses use, bool used)
    {
        bool inside = use.Way switch
        {
            Write => use.InsideWrites != 0,
            _ => use.InsideReads != 0,
        };
        return inside ? "narrow" : used ? "unneeded" : "unused";
    }

    /// <summary>
    /// Each repository source file with the first project that compiles it, as the module inventory binds it, so the two
    /// share one binding; whether the file is a check comes from its folder or that project.
    /// </summary>
    private static List<(Compilation Compilation, SyntaxTree Tree, string File, bool Check)> FirstCompilations(LoadedSolution solution)
    {
        // A file is a check only when every project compiling it is one: test-support a tool also compiles reaches the
        // members it names directly, without the access a verification project compiles.
        var shipped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (project, compilation) in solution.Projects.Where(project => !ProjectScope.IsCheck(project.Project.AssemblyName)))
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
                shipped.Add(Path.GetFullPath(tree.FilePath));
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var trees = new List<(Compilation, SyntaxTree, string, bool)>();
        foreach (var (project, compilation) in solution.Projects)
        {
            foreach (SyntaxTree tree in compilation.SyntaxTrees)
            {
                string path = Path.GetFullPath(tree.FilePath);
                if (!IsRepositoryFile(solution.RepositoryRoot, tree.FilePath) || !seen.Add(path))
                    continue;
                trees.Add((compilation, tree, Relative(solution.RepositoryRoot, tree.FilePath), !shipped.Contains(path)));
            }
        }
        return trees;
    }

    /// <summary>Records every place in one file that reaches a candidate member, by the way it reaches it.</summary>
    private static void Walk(Compilation compilation, SyntaxTree tree, string file, bool check,
        Dictionary<string, MemberUses> uses, HashSet<string> names, JsonSerializationScan? serialization)
    {
        var enclosing = new Dictionary<SyntaxNode, INamedTypeSymbol?>();
        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            if (serialization is not null && node is InvocationExpressionSyntax invocation &&
                Info(compilation, tree, invocation).Symbol is IMethodSymbol invoked)
                serialization.ObserveInvocation(SemanticModels.At(compilation, tree, invocation), invocation, invoked);
            switch (node)
            {
                case SimpleNameSyntax name when names.Contains(name.Identifier.ValueText) && !InNameOf(name):
                    SymbolInfo info = Info(compilation, tree, name);
                    RecordAll(info, name, Access(AccessExpression(name)));
                    if (check)
                        RecordUnbound(name, info);
                    break;
                case BaseObjectCreationExpressionSyntax or ConstructorInitializerSyntax or PrimaryConstructorBaseTypeSyntax or AttributeSyntax:
                    RecordAll(Info(compilation, tree, node), node, Use);
                    break;
                case ElementAccessExpressionSyntax or ElementBindingExpressionSyntax:
                    RecordAll(Info(compilation, tree, node), node, Access(AccessExpression((ExpressionSyntax)node)));
                    break;
                case CommonForEachStatementSyntax loop:
                    ForEachStatementInfo each = SemanticModels.At(compilation, tree, loop).GetForEachStatementInfo(loop);
                    Record(each.GetEnumeratorMethod, loop, Use, reaches: true);
                    Record(each.MoveNextMethod, loop, Use, reaches: true);
                    Record(each.CurrentProperty, loop, Read, reaches: true);
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
            Record(info.Symbol, site, access, reaches: true);
            foreach (ISymbol candidate in info.CandidateSymbols)
                Record(candidate, site, access, reaches: info.CandidateReason == CandidateReason.Inaccessible);
        }

        // A check's member name that binds to nothing, for want of the private-access member that stood in for it, names the
        // receiver's private members of that name: C# reports the member inaccessible but may keep no candidate (a static
        // access resolved through extension lookup), binds the name to an unrelated extension member of the same name that
        // some other type's generated access declares, or reports the accessible overloads that take other arguments.
        void RecordUnbound(SimpleNameSyntax name, SymbolInfo info)
        {
            ExpressionSyntax? receiver = name.Parent switch
            {
                MemberAccessExpressionSyntax member when member.Name == name => member.Expression,
                MemberBindingExpressionSyntax binding when binding.Name == name =>
                    binding.Ancestors().OfType<ConditionalAccessExpressionSyntax>().FirstOrDefault()?.Expression,
                _ => null,
            };
            if (receiver is null)
                return;
            SemanticModel model = SemanticModels.At(compilation, tree, receiver);
            ITypeSymbol? type = model.GetSymbolInfo(receiver).Symbol as INamedTypeSymbol ?? model.GetTypeInfo(receiver).Type;
            if (type is not INamedTypeSymbol { TypeKind: not TypeKind.Error } named || info.Symbol is not null && !Unrelated(info.Symbol, named))
                return;
            string way = Access(AccessExpression(name));
            for (INamedTypeSymbol? owner = named; owner is not null; owner = owner.BaseType)
            {
                foreach (ISymbol member in owner.GetMembers(name.Identifier.ValueText).Where(member =>
                             member.DeclaredAccessibility == Accessibility.Private || member is IPropertySymbol
                             {
                                 GetMethod.DeclaredAccessibility: Accessibility.Private,
                             } or IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Private }))
                    Record(member, name, way, reaches: true);
            }
        }

        // A site reaches a symbol it binds to, or the inaccessible candidate a check binds to for want of the private-access
        // member standing in for it; a candidate of an overload or ambiguity failure reaches a private way of nothing.
        void Record(ISymbol? symbol, SyntaxNode site, string access, bool reaches)
        {
            if (Normalize(symbol) is not { } member || !names.Contains(member.Name))
                return;
            foreach (string way in WaysReached(member, access))
            {
                if (!uses.TryGetValue(Key(member, way), out MemberUses? reached) || !reaches && IsPrivate(member, way))
                    continue;
                if (check)
                {
                    reached.Checks++;
                    reached.FirstCheck ??= $"{file}:{site.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";
                    reached.CheckSites.Add(site);
                }
                else if (!IsInside(site, member, enclosing, compilation, tree))
                {
                    reached.Production++;
                }
                else
                {
                    if (way == Write || access is Write or ReadWrite && way == Use)
                        reached.InsideWrites++;
                    if (way == Read || access is not Write && way == Use)
                        reached.InsideReads++;
                }
            }
        }
    }

    /// <summary>
    /// Whether <paramref name="bound"/> is an extension member for a receiver <paramref name="receiver"/> is not: what C#
    /// reports in place of the receiver's inaccessible member when another type's extension of the same name exists.
    /// </summary>
    private static bool Unrelated(ISymbol bound, INamedTypeSymbol receiver)
    {
        if (bound.ContainingType is not { TypeKind: TypeKind.Extension, ExtensionParameter.Type: INamedTypeSymbol extended })
            return false;
        for (INamedTypeSymbol? type = receiver; type is not null; type = type.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, extended.OriginalDefinition))
                return false;
        }
        return !receiver.AllInterfaces.Any(contract => SymbolEqualityComparer.Default.Equals(contract.OriginalDefinition, extended.OriginalDefinition));
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

    /// <summary>The ways a candidate member can be reached, private ones included, each of which is checked on its own.</summary>
    private static IEnumerable<string> Ways(ISymbol member)
    {
        if (member is not IPropertySymbol property)
        {
            yield return Use;
            yield break;
        }
        if (property.GetMethod is not null)
            yield return Read;
        if (property.SetMethod is not null)
            yield return Write;
    }

    /// <summary>Whether a way of reaching a member is private: the member itself, or the accessor the way names.</summary>
    private static bool IsPrivate(ISymbol member, string way) => way switch
    {
        Read => ((IPropertySymbol)member).GetMethod!.DeclaredAccessibility == Accessibility.Private,
        Write => ((IPropertySymbol)member).SetMethod!.DeclaredAccessibility == Accessibility.Private,
        _ => member.DeclaredAccessibility == Accessibility.Private,
    };

    /// <summary>
    /// Whether <paramref name="member"/> is one whose width the census judges: a field, property, ordinary method,
    /// constructor or event, private ones included (a check naming one lacks its private-access member), that nothing
    /// outside the repository's own calls demands be wide (an override, an interface implementation, a virtual or
    /// abstract member, an operator, a serializer constructor, an entry point).
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
            IFieldSymbol field => field.AssociatedSymbol is null,
            // A record's components are its value: its synthesized equality, hashing and deconstruction read them.
            IPropertySymbol property => property.ExplicitInterfaceImplementations.IsEmpty &&
                property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not ParameterSyntax,
            IEventSymbol @event => @event.ExplicitInterfaceImplementations.IsEmpty,
            IMethodSymbol method => method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor && method.Name != "Main" &&
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

    /// <summary>Whether a name stands where it is written: assigned, incremented or passed by reference.</summary>
    internal static bool IsWrite(SimpleNameSyntax name) => Access(AccessExpression(name)) is Write or ReadWrite;

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
