using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Moves shipped-assembly symbols into a development-only project or test-support, per <see cref="RelocationStyle"/>.
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
    /// <summary>Whether a declaration moves whole, into an owner adapter, or as an instance extension.</summary>
    private enum Placement
    {
        /// <summary>Moves a selected top-level type to the tool project while retaining its namespace.</summary>
        WholeType,
        /// <summary>Moves a static or nested member into the staying owner's tooling adapter.</summary>
        AdapterMember,
        /// <summary>Moves an instance method or property into an extension block for its owner.</summary>
        ExtensionMember,
    }

    /// <summary>Captures a selected symbol with the syntax and semantic context needed to rewrite it.</summary>
    private sealed record Move(ISymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model, Placement Placement)
    {
        /// <summary>
        /// The staying type that receives an adapter for a moved member. A member of an extension block belongs to
        /// the static class declaring the block, not to the block's synthesized extension type.
        /// </summary>
        public INamedTypeSymbol? Owner => Placement == Placement.WholeType ? null
            : Symbol.ContainingType is { IsExtension: true, ContainingType: { } declaring } ? declaring : Symbol.ContainingType;

        /// <summary>The receiver parameter of the extension block that declared this member, or null for a type member.</summary>
        public string? ExtensionReceiver => Node.Parent is ExtensionBlockDeclarationSyntax { ParameterList.Parameters: [var receiver] }
            ? receiver.ToString() : null;

        /// <summary>The adapter owner of a member move; whole-type moves have none.</summary>
        public INamedTypeSymbol AdapterOwner => Owner ??
            throw new InvalidOperationException($"{Symbol.Name} moves as a whole type and has no adapter owner.");
    }

    /// <summary>Compares Roslyn symbols across declaration and reference passes.</summary>
    private static readonly SymbolEqualityComparer Comparer = SymbolEqualityComparer.Default;

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
        string AdapterName(INamedTypeSymbol owner) => style.AdapterName(owner);
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource)
                .Select(t => new ProjectTree(Tree:t, p.Compilation, Project:p.Project.AssemblyName)))
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
        var moveBySymbol = moves.GroupBy(m => identity.Key(m.Symbol) ??
                throw new InvalidOperationException($"Selected symbol {m.Symbol} has no repository identity."))
            .ToDictionary(g => g.Key, g => g.First());
        var movingTypeKeys = movingTypes.Select(identity.Key).OfType<string>().ToHashSet();
        Move? MoveOf(ISymbol? symbol) => identity.Key(symbol) is { } key && moveBySymbol.TryGetValue(key, out var m) ? m : null;
        bool Moved(ISymbol symbol) => MoveOf(symbol) is not null || Containers(symbol).Any(c => identity.Key(c) is { } k && movingTypeKeys.Contains(k));
        bool SameOwner(INamedTypeSymbol? a, INamedTypeSymbol? b) => a is not null && b is not null && identity.Key(a) == identity.Key(b);
        var movedInterfaces = moves.Where(m => m is { Placement: Placement.WholeType, Symbol: INamedTypeSymbol { TypeKind: TypeKind.Interface } })
            .Select(m => (INamedTypeSymbol)m.Symbol).ToHashSet<INamedTypeSymbol>(Comparer);
        var movedInterfaceKeys = movedInterfaces.Select(identity.Key).OfType<string>().ToHashSet();

        // 2. Edits: outside moved spans they apply in place; inside, to the moved text.
        var edits = new Dictionary<SyntaxTree, List<TextEdit>>();
        void Edit(SyntaxTree tree, TextSpan span, string text)
        {
            if (!edits.TryGetValue(tree, out var list)) edits[tree] = list = [];
            list.Add(new TextEdit(span, text));
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
        // A staying call site of a moved instance member resolves it as an extension, which needs the
        // adapter's namespace in scope; moved code lands in that namespace already.
        var neededUsings = new Dictionary<SyntaxTree, SortedSet<string>>();
        void RequireExtensionNamespace(SyntaxTree tree, SemanticModel model, SyntaxNode site, Move extension)
        {
            if (EnclosingMove(tree, site) is not null || extension.AdapterOwner.ContainingNamespace is not { IsGlobalNamespace: false } ns)
                return;
            string name = ns.ToDisplayString();
            for (var scope = model.GetEnclosingSymbol(site.SpanStart)?.ContainingNamespace; scope is { IsGlobalNamespace: false }; scope = scope.ContainingNamespace)
                if (scope.ToDisplayString() == name)
                    return;
            if (tree.GetCompilationUnitRoot().Usings.Any(u => u.Alias is null && u.StaticKeyword.IsKind(SyntaxKind.None) && u.Name?.ToString() == name))
                return;
            if (!neededUsings.TryGetValue(tree, out var names)) neededUsings[tree] = names = new(StringComparer.Ordinal);
            names.Add(name);
        }
        foreach (var (tree, compilation, assembly) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            bool shipped = shippedAssemblies.Contains(assembly);
            var root = tree.GetRoot();
            foreach (var node in root.DescendantNodes(descendIntoTrivia: true))
            {
                // Moved code constructing a staying type needs the constructor visible from the target.
                if (node is BaseObjectCreationExpressionSyntax creation && EnclosingMove(tree, creation) is { Placement: not Placement.WholeType } &&
                    SymbolIdentity.Normalize(model.GetSymbolInfo(creation).Symbol) is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor &&
                    !Moved(constructor))
                {
                    Promote(constructor, promotions, false);
                    continue;
                }
                if (node is CrefSyntax cref && cref.Parent is XmlCrefAttributeSyntax attribute)
                {
                    var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(cref).Symbol);
                    var enclosing = EnclosingMove(tree, cref);
                    if (bound is null) continue;
                    if (!Moved(bound))
                    {
                        // Moved documentation naming a staying owner member needs the owner's name.
                        if (enclosing is { Placement: not Placement.WholeType } && cref is NameMemberCrefSyntax &&
                            bound.ContainingType is { } crefOwner && InOwnerChain(enclosing.AdapterOwner, crefOwner))
                            Edit(tree, cref.Span, $"{TypeName(crefOwner)}.{cref}");
                        continue;
                    }
                    // A shipped assembly cannot see the target project: the reference becomes text.
                    if (shipped && enclosing is null && attribute.Parent is XmlEmptyElementSyntax element)
                        Edit(tree, element.Span, $"<c>{cref}</c>");
                    else if (cref is QualifiedCrefSyntax qualified && MoveOf(bound) is { Placement: Placement.AdapterMember } crefMove)
                        Edit(tree, qualified.Container.Span, AdapterName(crefMove.AdapterOwner));
                    else if (cref is NameMemberCrefSyntax && MoveOf(bound) is { Placement: Placement.AdapterMember } nameMove &&
                             (enclosing is null || !SameOwner(enclosing.Owner, nameMove.Owner)))
                        Edit(tree, cref.Span, $"{AdapterName(nameMove.AdapterOwner)}.{cref}");
                    continue;
                }
                if (node.Ancestors().Any(a => a is CrefSyntax)) continue;
                if (node is MemberAccessExpressionSyntax or QualifiedNameSyntax)
                {
                    var (left, name) = node switch
                    {
                        MemberAccessExpressionSyntax access => new QualifiedNameParts(access.Expression, access.Name),
                        QualifiedNameSyntax qualified => new QualifiedNameParts(qualified.Left, qualified.Right),
                        _ => throw new InvalidOperationException(),
                    };
                    var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(name).Symbol);
                    if (left is ThisExpressionSyntax && bound is not null && EnclosingMove(tree, node) is { Placement: Placement.ExtensionMember })
                    {
                        Promote(bound, promotions, IsWritten(node));
                        continue;
                    }
                    if (bound is not null && MoveOf(bound) is { Placement: Placement.ExtensionMember } accessed)
                        RequireExtensionNamespace(tree, model, node, accessed);
                    if (bound is null || MoveOf(bound) is not { Placement: Placement.AdapterMember } move)
                        continue;
                    if (model.GetSymbolInfo(left).Symbol is not INamedTypeSymbol) continue;
                    Edit(tree, left.Span, NamespacePrefix(model, left) + AdapterName(move.AdapterOwner));
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
                        Edit(tree, simple.Span, $"{AdapterName(target.AdapterOwner)}.{simple}");
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
                    inside.Placement == Placement.ExtensionMember && InOwnerChain(inside.AdapterOwner, instanceOwner) &&
                    symbol.Kind is SymbolKind.Field or SymbolKind.Property or SymbolKind.Method or SymbolKind.Event)
                {
                    Edit(tree, simple.Span, $"self.{simple}");
                    Promote(symbol, promotions, IsWritten(simple));
                }
                else if ((symbol.IsStatic || symbol is INamedTypeSymbol) && symbol.ContainingType is { } staticOwner &&
                    InOwnerChain(inside.AdapterOwner, staticOwner))
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

        foreach (var (tree, names) in neededUsings)
        {
            var unit = tree.GetCompilationUnitRoot();
            string newline = tree.GetText().ToString().Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
            string directives = string.Concat(names.Select(name => $"using {name};{newline}"));
            Edit(tree, new TextSpan(unit.Usings.Count > 0 ? unit.Usings[^1].FullSpan.End : 0, 0), directives);
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
        // A source project's global usings do not follow its code; each moved declaration takes those it names.
        var globalNamespaces = trees.GroupBy(entry => entry.Compilation).ToDictionary(group => group.Key, group =>
            (IReadOnlySet<string>)group.SelectMany(entry => entry.Tree.GetCompilationUnitRoot().Usings)
                .Where(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) && u.Alias is null && u.StaticKeyword.IsKind(SyntaxKind.None) && u.Name is not null)
                .Select(u => u.Name!.ToString()).ToHashSet(StringComparer.Ordinal));
        var compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);
        var files = new SortedDictionary<string, TargetFile>(StringComparer.Ordinal);
        TargetFile FileFor(string path, Move move)
        {
            if (!files.TryGetValue(path, out var file))
                files[path] = file = new TargetFile([.. Usings(move.Tree)], Namespace(move.Tree), style);
            foreach (string name in GlobalNamespacesNamed(move, globalNamespaces[compilationOf[move.Tree]]))
                file.RequireUsing($"using {name};");
            return file;
        }
        foreach (var move in moves)
        {
            string text = Rewritten(move.Tree, move.Node.FullSpan, edits);
            string relative = TargetRelativePath(identity.Relative(move.Tree.FilePath), style.KeepsProjectDirectory);
            if (move.Placement == Placement.WholeType)
            {
                FileFor(Path.Combine(targetDirectory, relative), move).Types.Add(text);
                continue;
            }
            // Static members and instance extensions live in separately named classes, each in its own file.
            string className = move.Placement == Placement.ExtensionMember ? style.ExtensionsName(move.AdapterOwner) : AdapterName(move.AdapterOwner);
            string adapterFile = Path.Combine(targetDirectory, Path.GetDirectoryName(relative) ?? "", className + ".cs");
            var file = FileFor(adapterFile, move);
            var adapter = file.Adapter(move.AdapterOwner, movedInterfaces, implementation => MoveOf(implementation) is not null);
            text = AsAdapterMember(text);
            if (move.Placement == Placement.ExtensionMember)
                adapter.Extensions.Add(new RelocatedExtension(move.ExtensionReceiver ?? adapter.SelfReceiver,
                    move.ExtensionReceiver is null ? text : Outdent(text)));
            else
                adapter.Members.Add(text);
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
                File.WriteAllText(path, output, new UTF8Encoding(HasByteOrderMark(path)));
            else
            {
                File.Delete(path);
                deleted++;
            }
        }
        foreach (var (path, file) in files)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                throw new InvalidOperationException($"{path} has no containing directory."));
            // A later pass adds to the adapters and files an earlier pass created.
            // A merge keeps the existing file's byte-order mark; new files get one.
            bool existing = File.Exists(path);
            File.WriteAllText(path, existing ? file.MergeInto(File.ReadAllText(path)) : file.Render(),
                new UTF8Encoding(!existing || HasByteOrderMark(path)));
        }
        foreach (string line in manual) Console.WriteLine(line);
        Console.WriteLine($"Relocated {moves.Count} declarations into {files.Count} files; deleted {deleted} emptied files; " +
            $"{promotions.Count} members made internal; {manual.Count} left for manual relocation.");
    }

    /// <summary>Whether a source file starts with a UTF-8 byte-order mark, which rewriting it keeps.</summary>
    private static bool HasByteOrderMark(string path) => File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];

    /// <summary>Places supported declarations and leaves unsafe shapes for manual relocation.</summary>
    private static Placement? Classify(ISymbol symbol, SyntaxNode node)
    {
        if (symbol is INamedTypeSymbol type)
            return type.ContainingType is { IsGenericType: true } ? null : Placement.WholeType;
        if (symbol.ContainingType is not { } owner || owner.IsGenericType) return null;
        // A member of an extension block keeps its receiver: it moves into a block with that same receiver.
        if (owner.IsExtension)
            return symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } or IPropertySymbol { IsIndexer: false }
                ? Placement.ExtensionMember : null;
        // An enum value cannot leave its enum.
        if (owner.TypeKind == TypeKind.Enum) return null;
        switch (symbol)
        {
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: true }:
            case IPropertySymbol { IsStatic: true, IsIndexer: false }:
            case IFieldSymbol { IsStatic: true }:
                return Placement.AdapterMember;
            case IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false, IsAbstract: false, IsVirtual: false, IsOverride: false } method
                when method.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node) && !ReadsPrimaryConstructorParameter(owner, node):
            case IPropertySymbol { IsStatic: false, IsIndexer: false, IsAbstract: false, IsVirtual: false, IsOverride: false, SetMethod: null } property
                when property.ExplicitInterfaceImplementations.IsEmpty && !MutatesStruct(owner, node)
                    && !HasBackingField(property) && !ReadsPrimaryConstructorParameter(owner, node):
                return Placement.ExtensionMember;
            default:
                return null;
        }
    }

    /// <summary>Rejects auto-properties whose compiler-generated storage cannot move as an extension.</summary>
    private static bool HasBackingField(IPropertySymbol property) => property.ContainingType.GetMembers()
        .OfType<IFieldSymbol>().Any(f => Comparer.Equals(f.AssociatedSymbol, property));

    /// <summary>
    /// Rejects members that read a primary-constructor parameter the owner captures: an extension outside the type cannot
    /// reach it. A same-named local is rejected too, which only leaves the member for manual relocation.
    /// </summary>
    private static bool ReadsPrimaryConstructorParameter(INamedTypeSymbol owner, SyntaxNode node)
    {
        HashSet<string> parameters = owner.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>().SelectMany(type => type.ParameterList?.Parameters ?? [])
            .Select(parameter => parameter.Identifier.ValueText).ToHashSet(StringComparer.Ordinal);
        return parameters.Count != 0 && node.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Any(name => parameters.Contains(name.Identifier.ValueText));
    }

    /// <summary>Rejects struct members that assign into state because an extension receiver would copy it.</summary>
    private static bool MutatesStruct(INamedTypeSymbol owner, SyntaxNode node) =>
        owner.TypeKind == TypeKind.Struct && node.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any();

    /// <summary>Includes a single-variable field's whole declaration when relocating its symbol.</summary>
    private static SyntaxNode DeclarationNode(SyntaxNode node) => node switch
    {
        VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax field } when
            ((VariableDeclarationSyntax)node.Parent).Variables.Count == 1 => field,
        _ => node,
    };

    /// <summary>Walks all containing types so members of moved types are not moved twice.</summary>
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

    /// <summary>Checks whether a candidate owner appears in the original owner's type or base chain.</summary>
    private static bool InOwnerChain(INamedTypeSymbol owner, INamedTypeSymbol candidate)
    {
        for (var type = owner; type is not null; type = type.ContainingType)
            for (var current = type; current is not null; current = current.BaseType)
                if (Comparer.Equals(current.OriginalDefinition, candidate.OriginalDefinition)) return true;
        return false;
    }

    /// <summary>Recognizes names already qualified by a member, alias, using, or documentation context.</summary>
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

    /// <summary>Formats a repository-relative source location for manual relocation reports.</summary>
    private static string Location(SymbolIdentity identity, SyntaxNode node) =>
        $"{identity.Relative(node.SyntaxTree.FilePath)}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}";

    /// <summary>
    /// csharp/src/SuperMetroid.Core/Game/X.cs maps to Core/Game/X.cs under the target, or to Game/X.cs
    /// when the target already stands for the source project.
    /// </summary>
    private static string TargetRelativePath(string repositoryRelative, bool keepsProjectDirectory)
    {
        string[] parts = repositoryRelative.Split('/');
        int project = Array.FindIndex(parts, p => p.StartsWith("SuperMetroid.", StringComparison.Ordinal));
        if (project < 0) throw new InvalidOperationException($"{repositoryRelative} is not under a project directory.");
        return keepsProjectDirectory
            ? Path.Combine([parts[project]["SuperMetroid.".Length..], .. parts[(project + 1)..]])
            : Path.Combine(parts[(project + 1)..]);
    }

    /// <summary>Applies reference edits inside a moved declaration from the end to preserve original spans.</summary>
    private static string Rewritten(SyntaxTree tree, TextSpan span, Dictionary<SyntaxTree, List<TextEdit>> edits)
    {
        var text = new StringBuilder(tree.GetText().ToString(span));
        if (edits.TryGetValue(tree, out var list))
            foreach (var (editSpan, replacement) in list.Where(e => span.Contains(e.Span) && e.Span != span).Distinct()
                .OrderByDescending(e => e.Span.Start))
                text.Remove(editSpan.Start - span.Start, editSpan.Length).Insert(editSpan.Start - span.Start, replacement);
        return text.ToString();
    }

    /// <summary>Removes the one indentation level an extension block added, so the member re-indents like a type member.</summary>
    private static string Outdent(string text) =>
        string.Join("\n", text.Replace("\r\n", "\n").Split('\n').Select(l => l.StartsWith("    ", StringComparison.Ordinal) ? l[4..] : l));

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

    /// <summary>Copies source using directives into a relocated target file.</summary>
    private static IReadOnlyList<string> Usings(SyntaxTree tree) =>
        [.. tree.GetCompilationUnitRoot().Usings.Select(u => u.ToString())];

    /// <summary>
    /// The source project's global-using namespaces whose types, attributes or extension methods the moved declaration
    /// names without qualification, other than those its own namespace already encloses.
    /// </summary>
    private static IEnumerable<string> GlobalNamespacesNamed(Move move, IReadOnlySet<string> globalNamespaces)
    {
        if (globalNamespaces.Count == 0)
            yield break;
        string own = Namespace(move.Tree);
        var named = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var name in move.Node.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.Parent is QualifiedNameSyntax qualified && qualified.Right == name ||
                name.Parent is MemberAccessExpressionSyntax access && access.Name == name && access.Expression is not (ThisExpressionSyntax or BaseExpressionSyntax) &&
                move.Model.GetSymbolInfo(name).Symbol is not IMethodSymbol { IsExtensionMethod: true } and not IMethodSymbol { ReducedFrom: not null })
                continue;
            INamedTypeSymbol? type = move.Model.GetSymbolInfo(name).Symbol switch
            {
                INamedTypeSymbol typeSymbol => typeSymbol,
                IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType,
                IMethodSymbol { ReducedFrom: { } extension } => extension.ContainingType,
                IMethodSymbol { IsExtensionMethod: true } extension => extension.ContainingType,
                _ => null,
            };
            while (type?.ContainingType is { } outer)
                type = outer;
            if (type?.ContainingNamespace is not { IsGlobalNamespace: false } containing)
                continue;
            string ns = containing.ToDisplayString();
            if (globalNamespaces.Contains(ns) && own != ns && !own.StartsWith(ns + ".", StringComparison.Ordinal))
                named.Add(ns);
        }
        foreach (string ns in named)
            yield return ns;
    }

    /// <summary>Retains a moved declaration's source namespace.</summary>
    private static string Namespace(SyntaxTree tree) =>
        tree.GetCompilationUnitRoot().Members.OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";

    /// <summary>Accumulates whole types and per-owner adapters for one destination source file.</summary>
    private sealed class TargetFile(List<string> usings, string ns, RelocationStyle style)
    {
        /// <summary>Adds a using directive the moved code needs, once.</summary>
        public void RequireUsing(string directive)
        {
            if (!usings.Contains(directive))
                usings.Add(directive);
        }

        /// <summary>Whole declarations to append in this destination namespace.</summary>
        public List<string> Types { get; } = [];
        /// <summary>Adapters indexed by their original shipped owner.</summary>
        private readonly Dictionary<INamedTypeSymbol, AdapterClass> adapters = new(Comparer);

        /// <summary>Gets or creates the owner adapter, adding forwarders for retained interface contracts.</summary>
        public AdapterClass Adapter(INamedTypeSymbol owner, HashSet<INamedTypeSymbol> movedInterfaces, Func<ISymbol, bool> moves)
        {
            if (adapters.TryGetValue(owner, out var adapter))
                return adapter;
            var implemented = owner.Interfaces.Select(i => i.OriginalDefinition).Where(movedInterfaces.Contains).ToList();
            adapters[owner] = adapter = new AdapterClass(owner,
                [.. implemented.Select(i => i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))], style);
            // Contract members gameplay still uses stay on the owner; the adapter forwards to them.
            foreach (var contract in implemented.SelectMany(i => i.GetMembers())
                         .Where(m => m is IPropertySymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary }))
                if (owner.FindImplementationForInterfaceMember(contract) is { } implementation &&
                    SymbolIdentity.Normalize(implementation) is { } normalized && !moves(normalized))
                    adapter.Members.Add(Forwarder(owner, implementation));
            return adapter;
        }

        /// <summary>Emits a static adapter method or property that forwards a retained interface contract.</summary>
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
            var inserts = new List<TextInsertion>();
            var appended = new StringBuilder();
            var presentUsings = root.Usings.Select(u => u.ToString()).ToHashSet(StringComparer.Ordinal);
            string newUsings = string.Concat(usings.Where(u => !presentUsings.Contains(u)).Select(u => u + "\r\n"));
            if (newUsings.Length > 0)
                inserts.Add(new TextInsertion(root.Usings.Count > 0 ? root.Usings[^1].FullSpan.End : 0, newUsings));
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToDictionary(c => c.Identifier.Text, c => c);
            // Text goes in before a closing brace, at the start of its line, each line indented by the given prefix.
            void InsertBefore(SyntaxToken closeBrace, IEnumerable<string> members, string indent) =>
                inserts.Add(new TextInsertion(existing.LastIndexOf('\n', closeBrace.SpanStart - 1) + 1, string.Concat(members.Select(member =>
                    string.Join("\r\n", member.Trim('\r', '\n').Replace("\r\n", "\n").Split('\n')
                        .Select(line => line.Length == 0 ? line : indent + line)) + "\r\n"))));
            foreach (var adapter in adapters.Values)
            {
                if (adapter.Members.Count > 0)
                {
                    if (classes.TryGetValue(style.AdapterName(adapter.Owner), out var membersClass))
                        InsertBefore(membersClass.CloseBraceToken, adapter.Members, "");
                    else
                        appended.Append("\r\n").Append(adapter.RenderMembers());
                }
                if (adapter.Extensions.Count == 0)
                    continue;
                if (!classes.TryGetValue(style.ExtensionsName(adapter.Owner), out var extensionsClass))
                {
                    appended.Append("\r\n").Append(adapter.RenderExtensions());
                    continue;
                }
                // Instance members join the class's block for their receiver, or open one.
                foreach (var receiver in adapter.Extensions.Select(e => e.Receiver).Distinct(StringComparer.Ordinal))
                {
                    var block = extensionsClass.Members.OfType<ExtensionBlockDeclarationSyntax>().FirstOrDefault(b =>
                        b.ParameterList?.Parameters is [var parameter] && parameter.ToString() == receiver);
                    if (block is not null)
                        InsertBefore(block.CloseBraceToken, adapter.ExtensionsFor(receiver), "    ");
                    else
                        InsertBefore(extensionsClass.CloseBraceToken, [adapter.ExtensionBlock(receiver)], "");
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

        /// <summary>Writes using directives, moved types, and generated adapters into a new target file.</summary>
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
            foreach (var adapter in adapters.Values.OrderBy(a => style.AdapterName(a.Owner), StringComparer.Ordinal))
            {
                if (adapter.Members.Count > 0)
                    output.Append("\r\n").Append(adapter.RenderMembers());
                if (adapter.Extensions.Count > 0)
                    output.Append("\r\n").Append(adapter.RenderExtensions());
            }
            return output.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
        }
    }

    /// <summary>Renders static and instance-extension members for one original owner.</summary>
    private sealed class AdapterClass(INamedTypeSymbol owner, List<string> interfaces, RelocationStyle style)
    {
        /// <summary>The shipped type whose moved members this adapter holds.</summary>
        public INamedTypeSymbol Owner { get; } = owner;
        /// <summary>Moved interfaces implemented by this adapter through relocated members and forwarders.</summary>
        public List<string> Interfaces { get; } = interfaces;
        /// <summary>Static members, nested types, and retained-contract forwarders in the adapter.</summary>
        public List<string> Members { get; } = [];
        /// <summary>Instance methods and properties rendered into extension blocks, each with its receiver parameter.</summary>
        public List<RelocatedExtension> Extensions { get; } = [];

        /// <summary>The receiver of a moved instance member of the owner itself.</summary>
        public string SelfReceiver => TypeName(Owner) + " self";

        /// <summary>The moved members whose block takes <paramref name="receiver"/>.</summary>
        public List<string> ExtensionsFor(string receiver) =>
            [.. Extensions.Where(e => e.Receiver == receiver).Select(e => e.Text)];

        /// <summary>Emits the class holding the owner's moved static members.</summary>
        public string RenderMembers()
        {
            string ownerName = TypeName(Owner);
            var output = new StringBuilder("/// <summary>").Append(string.Format(style.MembersSummary, ownerName)).Append("</summary>\r\n");
            if (Interfaces.Count > 0)
                output.Append($"[ToolingFor(typeof({ownerName}))]\r\ninternal abstract class {style.AdapterName(Owner)} : {string.Join(", ", Interfaces)}\r\n");
            else
                output.Append($"internal static class {style.AdapterName(Owner)}\r\n");
            return output.Append("{\r\n").Append(string.Join("\r\n", Members.Select(m => m.Trim('\r', '\n')))).Append("\r\n}\r\n").ToString();
        }

        /// <summary>Emits the class holding the owner's moved instance members.</summary>
        public string RenderExtensions() =>
            new StringBuilder("/// <summary>").Append(string.Format(style.ExtensionsSummary, TypeName(Owner))).Append("</summary>\r\n")
                .Append($"internal static class {style.ExtensionsName(Owner)}\r\n{{\r\n")
                .Append(string.Join("\r\n", Extensions.Select(e => e.Receiver).Distinct(StringComparer.Ordinal).Select(ExtensionBlock)))
                .Append("}\r\n").ToString();

        /// <summary>The documented block for one receiver holding its moved members, indented one level.</summary>
        public string ExtensionBlock(string receiver)
        {
            string ownerName = TypeName(Owner);
            var output = new StringBuilder("    /// <summary>").Append(string.Format(style.ExtensionBlockSummary, ownerName)).Append("</summary>\r\n")
                .Append($"    extension({receiver})\r\n    {{\r\n");
            foreach (string member in ExtensionsFor(receiver))
                output.Append(Indent(member.Trim('\r', '\n'))).Append("\r\n");
            return output.Append("    }\r\n").ToString();
        }

        /// <summary>Adds one indentation level to moved instance members inside an extension block.</summary>
        private static string Indent(string text) =>
            string.Join("\r\n", text.Replace("\r\n", "\n").Split('\n').Select(l => l.Length == 0 ? l : "    " + l));
    }
}
