using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Moves shipped-assembly symbols that only tools reach into a development-only project.
/// <list type="bullet">
/// <item>Top-level types move whole, keeping their namespace, to the same relative path under the target.</item>
/// <item>Static members and nested types of a type that stays move into a per-owner adapter class,
/// <c>&lt;Owner&gt;Tooling</c>, in the owner's namespace. It implements any moved interface the owner
/// implemented and carries <c>[ToolingFor(typeof(Owner))]</c>.</item>
/// <item>Instance methods and properties move into <c>extension(Owner self)</c> blocks.</item>
/// </list>
/// Moved bodies are rewritten so that references back to the owner resolve; owner members they
/// use that were private become internal. Every reference and documentation cref in the solution
/// is retargeted, and crefs in shipped assemblies, which cannot see the target, become plain text.
/// Anything that cannot move mechanically (instance fields, constructors, indexers, operators,
/// generic owners, mutating struct members) is reported and left in place.
/// </summary>
internal static class SymbolRelocator
{
    private enum Placement { WholeType, AdapterMember, ExtensionMember }

    private sealed record Move(ISymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model, Placement Placement)
    {
        public INamedTypeSymbol? Owner => Placement == Placement.WholeType ? null : Symbol.ContainingType;
    }

    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

    public static void Relocate(LoadedSolution solution, ReachabilityResult result, string category,
        string targetDirectory, IReadOnlySet<string> shippedAssemblies)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var targets = ReachabilityFindings.Classify(result).Where(f => f.Category == category)
            .Select(f => f.Declaration.Key).ToHashSet();
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource)
                .Select(t => (Tree: t, p.Compilation, Project: p.Project.AssemblyName)))
            .GroupBy(entry => Path.GetFullPath(entry.Tree.FilePath), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();

        // 1. Declarations to move.
        var found = new List<Move>();
        var manual = new List<string>();
        foreach (var (tree, compilation, _) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is ParameterSyntax || SymbolIdentity.Normalize(DeclarationCollector.DeclaredSymbol(model, node)) is not { } symbol
                    || identity.Key(symbol) is not { } key || !targets.Contains(key))
                    continue;
                if (node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: > 1 } })
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} (shared declaration, {Location(identity, node)})");
                else if (Classify(symbol, node) is { } placement)
                    found.Add(new(symbol, DeclarationNode(node), tree, model, placement));
                else
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} ({Location(identity, node)})");
            }
        }
        var seen = found.Select(m => identity.Key(m.Symbol)).ToHashSet();
        foreach (string key in targets.Where(k => !seen.Contains(k) && !manual.Any(m => m.Contains(k.Split('@')[0][2..], StringComparison.Ordinal))))
            manual.Add($"MANUAL unmatched declaration {key}");
        // A member moves with its type when the type itself moves.
        var movingTypes = found.Where(m => m.Symbol is INamedTypeSymbol).Select(m => (INamedTypeSymbol)m.Symbol)
            .ToHashSet<INamedTypeSymbol>(Comparer);
        // Every part of a partial type moves; other declarations are distinct per symbol.
        var moves = found.Where(m => !Containers(m.Symbol).Any(movingTypes.Contains))
            .GroupBy(m => m.Node).Select(g => g.First()).ToList();
        // Nested types of a staying owner go into its adapter.
        moves = [.. moves.Select(m => m.Symbol is INamedTypeSymbol { ContainingType: not null } ? m with { Placement = Placement.AdapterMember } : m)];
        var moveBySymbol = moves.GroupBy(m => identity.Key(m.Symbol)!).ToDictionary(g => g.Key, g => g.First());
        var movingTypeKeys = movingTypes.Select(identity.Key).OfType<string>().ToHashSet();
        Move? MoveOf(ISymbol? symbol) => identity.Key(symbol) is { } key && moveBySymbol.TryGetValue(key, out var m) ? m : null;
        bool Moved(ISymbol symbol) => MoveOf(symbol) is not null || Containers(symbol).Any(c => identity.Key(c) is { } k && movingTypeKeys.Contains(k));
        bool SameOwner(INamedTypeSymbol? a, INamedTypeSymbol? b) => a is not null && b is not null && identity.Key(a) == identity.Key(b);
        var movedInterfaces = moves.Where(m => m is { Placement: Placement.WholeType, Symbol: INamedTypeSymbol { TypeKind: TypeKind.Interface } })
            .Select(m => (INamedTypeSymbol)m.Symbol).ToHashSet<INamedTypeSymbol>(Comparer);
        var movedInterfaceKeys = movedInterfaces.Select(identity.Key).OfType<string>().ToHashSet();

        // 2. Edits: outside moved spans they apply in place; inside, to the moved text.
        var edits = new Dictionary<SyntaxTree, List<(TextSpan Span, string Text)>>();
        void Edit(SyntaxTree tree, TextSpan span, string text)
        {
            if (!edits.TryGetValue(tree, out var list)) edits[tree] = list = [];
            list.Add((span, text));
        }
        var movedSpans = moves.GroupBy(m => m.Tree).ToDictionary(g => g.Key, g => g.Select(m => m.Node.FullSpan).ToList());
        bool InMovedSpan(SyntaxTree tree, TextSpan span) =>
            movedSpans.TryGetValue(tree, out var spans) && spans.Any(s => s.Contains(span));
        Move? EnclosingMove(SyntaxTree tree, SyntaxNode node) =>
            moves.FirstOrDefault(m => m.Tree == tree && m.Node.FullSpan.Contains(node.Span));

        var promotions = new HashSet<ISymbol>(Comparer);
        var setterPromotions = new HashSet<ISymbol>(Comparer);
        void Promote(ISymbol symbol, HashSet<ISymbol> into, bool written)
        {
            if (symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                into.Add(symbol);
            if (written && symbol is IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal })
                setterPromotions.Add(symbol);
        }
        foreach (var (tree, compilation, assembly) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            bool shipped = shippedAssemblies.Contains(assembly);
            var root = tree.GetRoot();
            foreach (var node in root.DescendantNodes(descendIntoTrivia: true))
            {
                if (node is CrefSyntax cref && cref.Parent is XmlCrefAttributeSyntax attribute)
                {
                    var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(cref).Symbol);
                    var enclosing = EnclosingMove(tree, cref);
                    if (bound is null) continue;
                    if (!Moved(bound))
                    {
                        // Moved documentation naming a staying owner member needs the owner's name.
                        if (enclosing is { Placement: not Placement.WholeType } && cref is NameMemberCrefSyntax &&
                            bound.ContainingType is { } crefOwner && InOwnerChain(enclosing.Owner!, crefOwner))
                            Edit(tree, cref.Span, $"{TypeName(crefOwner)}.{cref}");
                        continue;
                    }
                    // A shipped assembly cannot see the target project: the reference becomes text.
                    if (shipped && enclosing is null && attribute.Parent is XmlEmptyElementSyntax element)
                        Edit(tree, element.Span, $"<c>{cref}</c>");
                    else if (cref is QualifiedCrefSyntax qualified && MoveOf(bound) is { Placement: Placement.AdapterMember } crefMove)
                        Edit(tree, qualified.Container.Span, AdapterName(crefMove.Owner!));
                    else if (cref is NameMemberCrefSyntax && MoveOf(bound) is { Placement: Placement.AdapterMember } nameMove &&
                             (enclosing is null || !SameOwner(enclosing.Owner, nameMove.Owner)))
                        Edit(tree, cref.Span, $"{AdapterName(nameMove.Owner!)}.{cref}");
                    continue;
                }
                if (node.Ancestors().Any(a => a is CrefSyntax)) continue;
                if (node is MemberAccessExpressionSyntax or QualifiedNameSyntax)
                {
                    var (left, name) = node switch
                    {
                        MemberAccessExpressionSyntax access => ((SyntaxNode)access.Expression, access.Name),
                        QualifiedNameSyntax qualified => (qualified.Left, (SimpleNameSyntax)qualified.Right),
                        _ => throw new InvalidOperationException(),
                    };
                    var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(name).Symbol);
                    if (left is ThisExpressionSyntax && bound is not null && EnclosingMove(tree, node) is { Placement: Placement.ExtensionMember })
                    {
                        Promote(bound, promotions, IsWritten(node));
                        continue;
                    }
                    if (bound is null || MoveOf(bound) is not { Placement: Placement.AdapterMember } move)
                        continue;
                    if (model.GetSymbolInfo(left).Symbol is not INamedTypeSymbol) continue;
                    Edit(tree, left.Span, NamespacePrefix(model, left) + AdapterName(move.Owner!));
                    continue;
                }
                if (node is not SimpleNameSyntax simple || IsQualifiedName(simple) ||
                    simple is IdentifierNameSyntax { IsVar: true } || simple.Identifier.Text is "nameof" or "dynamic" or "_") continue;
                var symbol = SymbolIdentity.Normalize(model.GetSymbolInfo(simple).Symbol);
                if (symbol is null) continue;
                var inside = EnclosingMove(tree, simple);
                if (MoveOf(symbol) is { Placement: Placement.AdapterMember } target)
                {
                    // An unqualified use from code that does not move with it into the same adapter.
                    if (inside is null || inside.Placement == Placement.WholeType || !SameOwner(inside.Owner, target.Owner)
                        || inside.Placement == Placement.ExtensionMember)
                        Edit(tree, simple.Span, $"{AdapterName(target.Owner!)}.{simple}");
                    continue;
                }
                if (MoveOf(symbol) is { Placement: Placement.ExtensionMember } extension)
                {
                    // Another moved instance member of the same owner is reached through the receiver.
                    if (inside is { Placement: Placement.ExtensionMember } && SameOwner(inside.Owner, extension.Owner))
                        Edit(tree, simple.Span, $"self.{simple}");
                    continue;
                }
                if (inside is null || inside.Placement == Placement.WholeType || inside.Node == simple.Parent) continue;
                if (Moved(symbol)) continue;
                // A moved member's reference back to a staying member of its owner chain.
                if (!symbol.IsStatic && symbol is not INamedTypeSymbol && symbol.ContainingType is { } instanceOwner &&
                    inside.Placement == Placement.ExtensionMember && InOwnerChain(inside.Owner!, instanceOwner) &&
                    symbol.Kind is SymbolKind.Field or SymbolKind.Property or SymbolKind.Method or SymbolKind.Event)
                {
                    Edit(tree, simple.Span, $"self.{simple}");
                    Promote(symbol, promotions, IsWritten(simple));
                }
                else if ((symbol.IsStatic || symbol is INamedTypeSymbol) && symbol.ContainingType is { } staticOwner &&
                    InOwnerChain(inside.Owner!, staticOwner))
                {
                    Edit(tree, simple.Span, $"{TypeName(staticOwner)}.{simple}");
                    Promote(symbol, promotions, IsWritten(simple));
                }
            }
            foreach (var self in root.DescendantNodes().OfType<ThisExpressionSyntax>())
                if (EnclosingMove(tree, self) is { Placement: Placement.ExtensionMember })
                    Edit(tree, self.Span, "self");
            // Staying types no longer implement moved interfaces; their adapters do.
            foreach (var baseList in root.DescendantNodes().OfType<BaseListSyntax>())
            {
                if (InMovedSpan(tree, baseList.Span)) continue;
                var kept = baseList.Types.Where(t => model.GetTypeInfo(t.Type).Type is not INamedTypeSymbol bt ||
                    identity.Key(bt.OriginalDefinition) is not { } baseKey || !movedInterfaceKeys.Contains(baseKey)).ToList();
                if (kept.Count == baseList.Types.Count) continue;
                var previous = baseList.GetFirstToken().GetPreviousToken();
                Edit(tree, kept.Count == 0 ? TextSpan.FromBounds(previous.Span.End, baseList.Span.End) : baseList.Span,
                    kept.Count == 0 ? "" : ": " + string.Join(", ", kept.Select(t => t.ToString())));
            }
        }

        // 3. Accessibility: staying members used by moved code become internal.
        foreach (var symbol in setterPromotions)
            foreach (var reference in symbol.DeclaringSyntaxReferences)
                if (reference.GetSyntax() is PropertyDeclarationSyntax { AccessorList: { } accessors } property &&
                    !InMovedSpan(property.SyntaxTree, property.Span) &&
                    accessors.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) is { } setter &&
                    setter.Modifiers.Where(m => m.Kind() is SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword).ToList() is [var firstModifier, ..] setterModifiers)
                    Edit(property.SyntaxTree, TextSpan.FromBounds(firstModifier.SpanStart, setterModifiers[^1].Span.End), "internal");
        foreach (var symbol in promotions)
            foreach (var reference in symbol.DeclaringSyntaxReferences)
            {
                var syntax = reference.GetSyntax();
                var declaration = syntax is VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax sharedField } ? sharedField : DeclarationNode(syntax);
                if (declaration is not MemberDeclarationSyntax member || InMovedSpan(declaration.SyntaxTree, declaration.Span)) continue;
                var accessibility = member.Modifiers.Where(m => m.Kind() is SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword).ToList();
                if (symbol.DeclaredAccessibility is Accessibility.Internal or Accessibility.Public or Accessibility.ProtectedOrInternal) continue;
                if (accessibility.Count > 0)
                    Edit(declaration.SyntaxTree, TextSpan.FromBounds(accessibility[0].SpanStart, accessibility[^1].Span.End), "internal");
                else
                {
                    var first = member.Modifiers.Count > 0 ? member.Modifiers[0] : member.GetFirstToken(includeSkipped: false);
                    if (member.AttributeLists.Count > 0) first = member.AttributeLists[^1].GetLastToken().GetNextToken();
                    Edit(declaration.SyntaxTree, new TextSpan(first.SpanStart, 0), "internal ");
                }
            }

        // 4. Moved text, grouped into target files.
        var files = new SortedDictionary<string, TargetFile>(StringComparer.Ordinal);
        TargetFile FileFor(string path, SyntaxTree source)
        {
            if (!files.TryGetValue(path, out var file))
                files[path] = file = new TargetFile(Usings(source), Namespace(source));
            return file;
        }
        foreach (var move in moves)
        {
            string text = Rewritten(move.Tree, move.Node.FullSpan, edits);
            string relative = TargetRelativePath(identity.Relative(move.Tree.FilePath));
            if (move.Placement == Placement.WholeType)
            {
                FileFor(Path.Combine(targetDirectory, relative), move.Tree).Types.Add(text);
                continue;
            }
            string adapterFile = Path.Combine(targetDirectory, Path.GetDirectoryName(relative)!, AdapterName(move.Owner!) + ".cs");
            var file = FileFor(adapterFile, move.Tree);
            var adapter = file.Adapter(move.Owner!, movedInterfaces, implementation => MoveOf(implementation) is not null);
            text = AsAdapterMember(text);
            (move.Placement == Placement.ExtensionMember ? adapter.Extensions : adapter.Members).Add(text);
        }

        // 5. Apply in-place edits, removing moved declarations.
        foreach (var group in moves.GroupBy(m => m.Tree))
            foreach (var move in group)
                Edit(move.Tree, move.Node.FullSpan, "");
        int deleted = 0;
        foreach (var (tree, list) in edits)
        {
            string path = Path.GetFullPath(tree.FilePath);
            var spans = movedSpans.TryGetValue(tree, out var moved) ? moved : [];
            var applied = list.Where(e => e.Text == "" && spans.Contains(e.Span) || !spans.Any(s => s.Contains(e.Span)))
                .Distinct().OrderByDescending(e => e.Span.Start).ThenByDescending(e => e.Span.End).ToList();
            var text = new StringBuilder(tree.GetText().ToString());
            int floor = int.MaxValue;
            foreach (var (span, replacement) in applied)
            {
                if (span.End > floor)
                    throw new InvalidOperationException($"Overlapping relocation edits in {path} at {span}.");
                text.Remove(span.Start, span.Length).Insert(span.Start, replacement);
                floor = span.Start;
            }
            string output = text.ToString();
            if (CSharpSyntaxTree.ParseText(output).GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>().Any()
                || CSharpSyntaxTree.ParseText(output).GetRoot().DescendantNodes().OfType<DelegateDeclarationSyntax>().Any())
                File.WriteAllText(path, output, new UTF8Encoding(File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..]));
            else
            {
                File.Delete(path);
                deleted++;
            }
        }
        foreach (var (path, file) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            // A later pass adds to the adapters and files an earlier pass created.
            File.WriteAllText(path, File.Exists(path) ? file.MergeInto(File.ReadAllText(path)) : file.Render(), new UTF8Encoding(true));
        }
        foreach (string line in manual) Console.WriteLine(line);
        Console.WriteLine($"Relocated {moves.Count} declarations into {files.Count} files; deleted {deleted} emptied files; " +
            $"{promotions.Count} members made internal; {manual.Count} left for manual relocation.");
    }

    private static Placement? Classify(ISymbol symbol, SyntaxNode node)
    {
        if (symbol is INamedTypeSymbol type)
            return type.ContainingType is { IsGenericType: true } ? null : Placement.WholeType;
        if (symbol.ContainingType is not { } owner || owner.IsGenericType) return null;
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: true }:
            case IPropertySymbol { IsStatic: true, IsIndexer: false }:
            case IFieldSymbol { IsStatic: true }:
                return Placement.AdapterMember;
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false, IsAbstract: false, IsVirtual: false, IsOverride: false } method
                when method.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node):
            case IPropertySymbol { IsStatic: false, IsIndexer: false, IsAbstract: false, IsVirtual: false, IsOverride: false, SetMethod: null } property
                when property.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node)
                    && !HasBackingField(property):
                return Placement.ExtensionMember;
            default:
                return null;
        }
    }

    private static bool HasBackingField(IPropertySymbol property) => property.ContainingType.GetMembers()
        .OfType<IFieldSymbol>().Any(f => Comparer.Equals(f.AssociatedSymbol, property));

    private static bool MutatesStruct(INamedTypeSymbol owner, SyntaxNode node) =>
        owner.TypeKind == TypeKind.Struct && node.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any();

    private static SyntaxNode DeclarationNode(SyntaxNode node) => node switch
    {
        VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } when
            ((VariableDeclarationSyntax)node.Parent!).Variables.Count == 1 => field,
        _ => node,
    };

    private static IEnumerable<INamedTypeSymbol> Containers(ISymbol symbol)
    {
        for (var type = symbol.ContainingType; type is not null; type = type.ContainingType)
            yield return type.OriginalDefinition;
    }

    /// <summary>Whether the name is assigned, incremented or passed by reference.</summary>
    private static bool IsWritten(SyntaxNode name)
    {
        var expression = name.Parent is MemberAccessExpressionSyntax access && access.Name == name ? access : name;
        return expression.Parent switch
        {
            AssignmentExpressionSyntax assignment => assignment.Left == expression,
            PrefixUnaryExpressionSyntax or PostfixUnaryExpressionSyntax => true,
            ArgumentSyntax argument => !argument.RefKindKeyword.IsKind(SyntaxKind.None),
            _ => false,
        };
    }

    /// <summary>The namespace part written before a qualified type name, with its trailing dot.</summary>
    private static string NamespacePrefix(SemanticModel model, SyntaxNode left)
    {
        for (var node = left; node is QualifiedNameSyntax or MemberAccessExpressionSyntax;)
        {
            var qualifier = node is QualifiedNameSyntax q ? (SyntaxNode)q.Left : ((MemberAccessExpressionSyntax)node).Expression;
            if (model.GetSymbolInfo(qualifier).Symbol is INamespaceSymbol)
                return qualifier + ".";
            node = qualifier;
        }
        return "";
    }

    private static bool InOwnerChain(INamedTypeSymbol owner, INamedTypeSymbol candidate)
    {
        for (var type = owner; type is not null; type = type.ContainingType)
            for (var current = type; current is not null; current = current.BaseType)
                if (Comparer.Equals(current.OriginalDefinition, candidate.OriginalDefinition)) return true;
        return false;
    }

    private static bool IsQualifiedName(SimpleNameSyntax name) => name.Parent switch
    {
        MemberAccessExpressionSyntax access => access.Name == name,
        QualifiedNameSyntax qualified => qualified.Right == name,
        MemberBindingExpressionSyntax => true,
        AliasQualifiedNameSyntax => true,
        NameColonSyntax or NameEqualsSyntax => true,
        _ => name.Parent is CrefSyntax || name.Ancestors().Any(a => a is UsingDirectiveSyntax),
    };

    /// <summary>A staying type's name as written from its own namespace: containing types joined by '.'.</summary>
    private static string TypeName(INamedTypeSymbol type) =>
        type.ContainingType is { } outer ? TypeName(outer) + "." + type.Name : type.Name;

    internal static string AdapterName(INamedTypeSymbol owner) => FlatName(owner) + "Tooling";

    private static string FlatName(INamedTypeSymbol type) =>
        type.ContainingType is { } outer ? FlatName(outer) + type.Name : type.Name;

    private static string Location(SymbolIdentity identity, SyntaxNode node) =>
        $"{identity.Relative(node.SyntaxTree.FilePath)}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";

    /// <summary>csharp/src/SuperMetroid.Core/Game/X.cs maps to Core/Game/X.cs under the target project.</summary>
    private static string TargetRelativePath(string repositoryRelative)
    {
        string[] parts = repositoryRelative.Split('/');
        int project = Array.FindIndex(parts, p => p.StartsWith("SuperMetroid.", StringComparison.Ordinal));
        if (project < 0) throw new InvalidOperationException($"{repositoryRelative} is not under a project directory.");
        return Path.Combine([parts[project]["SuperMetroid.".Length..], .. parts[(project + 1)..]]);
    }

    private static string Rewritten(SyntaxTree tree, TextSpan span, Dictionary<SyntaxTree, List<(TextSpan Span, string Text)>> edits)
    {
        var text = new StringBuilder(tree.GetText().ToString(span));
        if (edits.TryGetValue(tree, out var list))
            foreach (var (editSpan, replacement) in list.Where(e => span.Contains(e.Span) && e.Span != span).Distinct()
                .OrderByDescending(e => e.Span.Start))
                text.Remove(editSpan.Start - span.Start, editSpan.Length).Insert(editSpan.Start - span.Start, replacement);
        return text.ToString();
    }

    /// <summary>Private and protected members become internal once outside their owner.</summary>
    private static string AsAdapterMember(string text)
    {
        var member = SyntaxFactory.ParseMemberDeclaration(text.Trim('\r', '\n'));
        if (member is null) return text;
        // An explicit interface implementation takes no accessibility modifier.
        if (member is BasePropertyDeclarationSyntax { ExplicitInterfaceSpecifier: not null } or
            MethodDeclarationSyntax { ExplicitInterfaceSpecifier: not null })
            return text;
        var accessibility = member.Modifiers.Where(m => m.Kind() is SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword).ToList();
        if (accessibility.Count == 0 && member.Modifiers.Any(m => m.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword))
            return text;
        string body = text;
        if (accessibility.Count > 0)
        {
            int offset = text.IndexOf(text.Trim('\r', '\n'), StringComparison.Ordinal);
            int start = offset + accessibility[0].SpanStart - member.FullSpan.Start;
            int end = offset + accessibility[^1].Span.End - member.FullSpan.Start;
            return body[..start] + "internal" + body[end..];
        }
        int firstToken = text.IndexOf(text.Trim('\r', '\n'), StringComparison.Ordinal) +
            (member.Modifiers.Count > 0 ? member.Modifiers[0].SpanStart : member.GetFirstToken().SpanStart) - member.FullSpan.Start;
        if (member.AttributeLists.Count > 0)
            firstToken = text.IndexOf(text.Trim('\r', '\n'), StringComparison.Ordinal) + member.AttributeLists[^1].GetLastToken().GetNextToken().SpanStart - member.FullSpan.Start;
        return body[..firstToken] + "internal " + body[firstToken..];
    }

    private static IReadOnlyList<string> Usings(SyntaxTree tree) =>
        [.. tree.GetCompilationUnitRoot().Usings.Select(u => u.ToString())];

    private static string Namespace(SyntaxTree tree) =>
        tree.GetCompilationUnitRoot().Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";

    private sealed class TargetFile(IReadOnlyList<string> usings, string ns)
    {
        public List<string> Types { get; } = [];
        private readonly Dictionary<INamedTypeSymbol, AdapterClass> adapters = new(Comparer);

        public AdapterClass Adapter(INamedTypeSymbol owner, HashSet<INamedTypeSymbol> movedInterfaces, Func<ISymbol, bool> moves)
        {
            if (adapters.TryGetValue(owner, out var adapter))
                return adapter;
            var implemented = owner.Interfaces.Select(i => i.OriginalDefinition).Where(movedInterfaces.Contains).ToList();
            adapters[owner] = adapter = new AdapterClass(owner,
                [.. implemented.Select(i => i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))]);
            // Contract members gameplay still uses stay on the owner; the adapter forwards to them.
            foreach (var contract in implemented.SelectMany(i => i.GetMembers())
                         .Where(m => m is IPropertySymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary }))
                if (owner.FindImplementationForInterfaceMember(contract) is { } implementation &&
                    !moves(SymbolIdentity.Normalize(implementation)!))
                    adapter.Members.Add(Forwarder(owner, implementation));
            return adapter;
        }

        private static string Forwarder(INamedTypeSymbol owner, ISymbol implementation)
        {
            var format = SymbolDisplayFormat.MinimallyQualifiedFormat;
            string target = $"{TypeName(owner)}.{implementation.Name}";
            return implementation switch
            {
                IPropertySymbol property => $"    public static {property.Type.ToDisplayString(format)} {property.Name} => {target};",
                IMethodSymbol method => $"    public static {method.ReturnType.ToDisplayString(format)} {method.Name}(" +
                    string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString(format)} {p.Name}")) +
                    $") => {target}(" + string.Join(", ", method.Parameters.Select(p => p.Name)) + ");",
                _ => throw new InvalidOperationException($"Cannot forward {implementation.ToDisplayString()}."),
            };
        }

        /// <summary>Adds this file's usings, members and types to an existing target file.</summary>
        public string MergeInto(string existing)
        {
            var root = CSharpSyntaxTree.ParseText(existing).GetCompilationUnitRoot();
            var inserts = new List<(int Position, string Text)>();
            var appended = new StringBuilder();
            var presentUsings = root.Usings.Select(u => u.ToString()).ToHashSet(StringComparer.Ordinal);
            string newUsings = string.Concat(usings.Where(u => !presentUsings.Contains(u)).Select(u => u + "\r\n"));
            if (newUsings.Length > 0)
                inserts.Add((root.Usings.Count > 0 ? root.Usings[^1].FullSpan.End : 0, newUsings));
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToDictionary(c => c.Identifier.Text, c => c);
            foreach (var adapter in adapters.Values)
            {
                bool appendedAdapter = false;
                foreach (var (name, members, extensions) in new[]
                         { (AdapterName(adapter.Owner), adapter.Members, false), (AdapterName(adapter.Owner) + "Extensions", adapter.Extensions, true) })
                {
                    if (members.Count == 0 || appendedAdapter) continue;
                    if (!classes.TryGetValue(name, out var existingClass))
                    {
                        appended.Append("\r\n").Append(adapter.Render());
                        appendedAdapter = true;
                        continue;
                    }
                    var block = existingClass.Members.FirstOrDefault(m => m.IsKind(SyntaxKind.ExtensionBlockDeclaration));
                    int close = extensions && block is not null ? block.GetLastToken().SpanStart : existingClass.CloseBraceToken.SpanStart;
                    // Members keep their owner-relative indentation; extension members sit one level deeper.
                    string indent = extensions ? "    " : "";
                    inserts.Add((existing.LastIndexOf('\n', close - 1) + 1, string.Concat(members.Select(member =>
                        string.Join("\r\n", member.Trim('\r', '\n').Replace("\r\n", "\n").Split('\n')
                            .Select(line => line.Length == 0 ? line : indent + line)) + "\r\n"))));
                }
            }
            foreach (string type in Types)
                appended.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            var output = new StringBuilder(existing);
            foreach (var (position, text) in inserts.OrderByDescending(i => i.Position))
                output.Insert(position, text);
            output.Append(appended);
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        public string Render()
        {
            var output = new StringBuilder();
            var allUsings = usings.ToList();
            if (adapters.Values.Any(a => a.Interfaces.Count > 0) && !allUsings.Contains("using SuperMetroid.Tooling;"))
                allUsings.Add("using SuperMetroid.Tooling;");
            foreach (string u in allUsings) output.Append(u).Append("\r\n");
            if (allUsings.Count > 0) output.Append("\r\n");
            if (ns.Length > 0) output.Append("namespace ").Append(ns).Append(";\r\n");
            foreach (string type in Types)
                output.Append("\r\n").Append(type.Trim('\r', '\n')).Append("\r\n");
            foreach (var adapter in adapters.Values.OrderBy(a => AdapterName(a.Owner), StringComparer.Ordinal))
                output.Append("\r\n").Append(adapter.Render());
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }
    }

    private sealed class AdapterClass(INamedTypeSymbol owner, List<string> interfaces)
    {
        public INamedTypeSymbol Owner { get; } = owner;
        public List<string> Interfaces { get; } = interfaces;
        public List<string> Members { get; } = [];
        public List<string> Extensions { get; } = [];

        public string Render()
        {
            var output = new StringBuilder();
            string ownerName = TypeName(Owner);
            if (Members.Count > 0)
            {
                output.Append($"/// <summary>Development-tool members of <see cref=\"{ownerName}\"/>; never linked by player hosts.</summary>\r\n");
                if (Interfaces.Count > 0)
                    output.Append($"[ToolingFor(typeof({ownerName}))]\r\ninternal abstract class {AdapterName(Owner)} : {string.Join(", ", Interfaces)}\r\n");
                else
                    output.Append($"internal static class {AdapterName(Owner)}\r\n");
                output.Append("{\r\n");
                output.Append(string.Join("\r\n", Members.Select(m => m.Trim('\r', '\n')))).Append("\r\n}\r\n");
            }
            if (Extensions.Count > 0)
            {
                if (Members.Count > 0) output.Append("\r\n");
                output.Append($"/// <summary>Development-tool instance members of <see cref=\"{ownerName}\"/>.</summary>\r\n");
                output.Append($"internal static class {AdapterName(Owner)}Extensions\r\n{{\r\n    extension({ownerName} self)\r\n    {{\r\n");
                foreach (string member in Extensions)
                    output.Append(Indent(member.Trim('\r', '\n'))).Append("\r\n");
                output.Append("    }\r\n}\r\n");
            }
            return output.ToString();
        }

        private static string Indent(string text) =>
            string.Join("\r\n", text.Replace("\r\n", "\n").Split('\n').Select(l => l.Length == 0 ? l : "    " + l));
    }
}
