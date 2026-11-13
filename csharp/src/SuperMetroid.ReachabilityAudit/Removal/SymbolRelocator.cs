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

    /// <summary>A selected declaration that stays where it is, with the syntax and semantic context to read what it uses.</summary>
    /// <param name="Symbol">The declared symbol.</param>
    /// <param name="Node">Its declaration.</param>
    /// <param name="Tree">The tree that holds the declaration.</param>
    /// <param name="Model">The tree's semantic model.</param>
    private sealed record StayingDeclaration(ISymbol Symbol, SyntaxNode Node, SyntaxTree Tree, SemanticModel Model);

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

    /// <summary>A cref in moved documentation to a staying member, written once what becomes internal is known.</summary>
    /// <param name="Tree">The tree that holds the cref.</param>
    /// <param name="Cref">The cref.</param>
    /// <param name="Attribute">The cref's attribute, whose element becomes text when the target cannot see the member.</param>
    /// <param name="Bound">The staying member it names.</param>
    /// <param name="Qualified">The cref qualified with the member's owner, when the moved code no longer stands inside it.</param>
    private sealed record MovedCref(SyntaxTree Tree, CrefSyntax Cref, XmlCrefAttributeSyntax Attribute, ISymbol Bound, string? Qualified);

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
        var trees = solution.Projects
            .SelectMany(p => p.Compilation.SyntaxTrees.Where(identity.IsRepositorySource)
                .Select(t => new ProjectTree(Tree:t, p.Compilation, Project:p.Project.AssemblyName)))
            .GroupBy(entry => Path.GetFullPath(entry.Tree.FilePath), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First()).ToArray();

        // 1. Declarations to move.
        var found = new List<Move>();
        var manual = new List<string>();
        var staying = new List<StayingDeclaration>();
        foreach (var (tree, compilation, _) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (node is ParameterSyntax || SymbolIdentity.Normalize(DeclarationCollector.DeclaredSymbol(model, node)) is not { } symbol
                    || identity.Key(symbol) is not { } key || !targets.Contains(key))
                    continue;
                if (node is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: > 1 } })
                {
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} (shared declaration, {Location(identity, node)})");
                    staying.Add(new(symbol, DeclarationNode(node), tree, model));
                }
                else if (Classify(symbol, node) is { } placement)
                {
                    found.Add(new(symbol, DeclarationNode(node), tree, model, placement));
                }
                else
                {
                    manual.Add($"MANUAL {symbol.Kind} {symbol.ToDisplayString()} ({Location(identity, node)})");
                    staying.Add(new(symbol, DeclarationNode(node), tree, model));
                }
            }
        }
        KeepWhatStayingDeclarationsUse(identity, found, staying, manual);
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
        RejectEmptiedBlocks(moves);
        var movedInterfaces = moves.Where(m => m is { Placement: Placement.WholeType, Symbol: INamedTypeSymbol { TypeKind: TypeKind.Interface } })
            .Select(m => (INamedTypeSymbol)m.Symbol).ToHashSet<INamedTypeSymbol>(Comparer);
        var movedInterfaceKeys = movedInterfaces.Select(identity.Key).OfType<string>().ToHashSet();

        // The class each owner's moved members go into, chosen before any reference names it (RelocationHolders).
        var holders = new RelocationHolders(solution, targetDirectory);
        string OwnerKey(INamedTypeSymbol owner) => identity.Key(owner) ??
            throw new InvalidOperationException($"Owner {owner} has no repository identity.");
        static RelocationHolderKind KindOf(Move move) =>
            move.Placement == Placement.ExtensionMember ? RelocationHolderKind.Extensions : RelocationHolderKind.Members;
        bool ImplementsMovedInterface(INamedTypeSymbol owner) => owner.Interfaces.Any(i => movedInterfaces.Contains(i.OriginalDefinition));
        var holderRequests = moves.Where(m => m.Placement != Placement.WholeType)
            .GroupBy(m => (Owner: OwnerKey(m.AdapterOwner), Kind: KindOf(m)))
            .OrderBy(g => g.Key.Owner, StringComparer.Ordinal).ThenBy(g => g.Key.Kind).ToList();
        foreach (var request in holderRequests)
        {
            var owner = request.First().AdapterOwner;
            // An owner implementing a moved interface gets an abstract adapter, and only a static class declares extension blocks.
            if (request.Key.Kind == RelocationHolderKind.Members && ImplementsMovedInterface(owner) &&
                request.FirstOrDefault(m => m.Symbol is INamedTypeSymbol { IsExtension: true }) is { } block)
                throw new InvalidOperationException($"{block.Symbol.ToDisplayString()} cannot move: {owner.ToDisplayString()} implements a " +
                    "moved interface, so its moved members go into an abstract adapter class, which cannot declare an extension block.");
            string directory = Path.Combine(targetDirectory, request
                .Select(m => Path.GetDirectoryName(TargetRelativePath(identity.Relative(m.Tree.FilePath), style.KeepsProjectDirectory)) ?? "")
                .Order(StringComparer.Ordinal).First());
            holders.Resolve(owner, request.Key.Owner, request.Key.Kind,
                request.Key.Kind == RelocationHolderKind.Extensions ? style.ExtensionsName(owner) : style.AdapterName(owner),
                directory, request.Key.Kind == RelocationHolderKind.Members && ImplementsMovedInterface(owner),
                [.. request.Select(m => m.Symbol)]);
            // An owner implementing a moved interface gets a members class of forwarders even when only instance members move.
            if (request.Key.Kind == RelocationHolderKind.Extensions && ImplementsMovedInterface(owner))
                holders.Resolve(owner, request.Key.Owner, RelocationHolderKind.Members, style.AdapterName(owner), directory, true, []);
        }
        string AdapterName(INamedTypeSymbol owner) => holders.Of(OwnerKey(owner), RelocationHolderKind.Members).Name;
        string HolderName(INamedTypeSymbol owner, RelocationHolderKind kind) => holders.Of(OwnerKey(owner), kind).Name;

        // A cref to a member of a nested type or extension block that moves into its owner's adapter. The name that qualifies
        // the container, written with or without its namespace, becomes the adapter's; an unqualified form is qualified with
        // the adapter unless the cref moves into that adapter too.
        IEnumerable<TextEdit> ContainedMemberCref(SemanticModel model, CrefSyntax cref, ISymbol bound, Move container, Move? enclosing)
        {
            string holder = AdapterName(container.AdapterOwner);
            bool inAdapter = enclosing is not null && (enclosing == container ||
                enclosing.Placement == Placement.AdapterMember && SameOwner(enclosing.Owner, container.Owner));
            if (container.Node is ExtensionBlockDeclarationSyntax block)
            {
                bool namesBlock = cref.DescendantNodesAndSelf().OfType<ExtensionMemberCrefSyntax>().Any();
                if (cref is QualifiedCrefSyntax qualified)
                    yield return new TextEdit(qualified.Span, $"{NamespacePrefix(model, qualified.Container)}{holder}." +
                        $"{(namesBlock ? "" : BlockCref(block) + ".")}{qualified.Member}");
                else if (!inAdapter)
                    yield return new TextEdit(cref.Span, $"{holder}.{(namesBlock ? "" : BlockCref(block) + ".")}{cref}");
                yield break;
            }
            string containerKey = identity.Key(container.Symbol) ??
                throw new InvalidOperationException($"{container.Symbol} has no repository identity.");
            var names = cref.DescendantNodesAndSelf().OfType<SimpleNameSyntax>()
                .Where(name => identity.Key(SymbolIdentity.Normalize(model.GetSymbolInfo(name).Symbol)) == containerKey).ToList();
            if (names.Count == 0)
            {
                // The member written alone, which only its own type (or one deriving from it) could resolve.
                if (!inAdapter)
                    yield return new TextEdit(new TextSpan(cref.SpanStart, 0), $"{holder}.{CrefPath(bound, container.Symbol)}.");
                yield break;
            }
            foreach (var name in names)
            {
                if (name.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == name)
                    yield return new TextEdit(qualifiedName.Left.Span, NamespacePrefix(model, qualifiedName.Left) + holder);
                else if (!inAdapter)
                    yield return new TextEdit(new TextSpan(name.SpanStart, 0), holder + ".");
            }
        }

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
        var compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);

        var promotions = new HashSet<ISymbol>(Comparer);
        var setterPromotions = new HashSet<ISymbol>(Comparer);
        void Promote(ISymbol symbol, HashSet<ISymbol> into, bool written)
        {
            if (symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
                into.Add(symbol);
            if (written && symbol is IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal })
                setterPromotions.Add(symbol);
        }
        // Every staying member that becomes internal: the promoted ones and the fields sharing a declaration with one, whose
        // shared modifiers change with it.
        var widened = new HashSet<ISymbol>(Comparer);
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
        // The new name of each underscore-prefixed member that becomes internal (PromotedNames), by repository key. Every
        // reference is written with it. It is empty for the first collection, which finds what becomes internal.
        IReadOnlyDictionary<string, string> renames = new Dictionary<string, string>(StringComparer.Ordinal);
        string? RenamedTo(ISymbol? symbol) => renames.Count > 0 && symbol is IFieldSymbol or IPropertySymbol &&
            identity.Key(symbol) is { } key && renames.TryGetValue(key, out string? name) ? name : null;
        string Spelled(ISymbol symbol, SimpleNameSyntax written) => RenamedTo(symbol) ?? written.ToString();
        string RenamedCref(CrefSyntax cref, ISymbol bound)
        {
            if (RenamedTo(bound) is not { } name)
                return cref.ToString();
            int at = CrefMemberName(cref, bound).SpanStart - cref.SpanStart;
            return cref.ToString().Remove(at, bound.Name.Length).Insert(at, name);
        }
        // Crefs in moved documentation to staying members, written once what becomes internal is known: the target compiles
        // against the reference assembly of the member's project, which holds no private member.
        var movedCrefs = new List<MovedCref>();
        bool HiddenFromTarget(ISymbol symbol) =>
            symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal && !widened.Contains(symbol) ||
            symbol.ContainingType?.OriginalDefinition is { } container && HiddenFromTarget(container);

        void CollectEdits()
        {
            edits.Clear();
            promotions.Clear();
            setterPromotions.Clear();
            widened.Clear();
            neededUsings.Clear();
            movedCrefs.Clear();
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
                            // Moved documentation naming a staying owner member, or a member of one of the owner's staying extension
                            // blocks, needs the owner's name.
                            string? qualifiedText = enclosing is { Placement: not Placement.WholeType } && cref is NameMemberCrefSyntax or ExtensionMemberCrefSyntax &&
                                DeclaringOwner(bound) is { } crefOwner && InOwnerChain(enclosing.AdapterOwner, crefOwner)
                                ? $"{TypeName(crefOwner)}.{StayingBlockOf(bound, cref)}{RenamedCref(cref, bound)}" : null;
                            if (enclosing is not null)
                                movedCrefs.Add(new MovedCref(tree, cref, attribute, bound, qualifiedText));
                            else if (RenamedTo(bound) is { } renamedCref)
                                Edit(tree, CrefMemberName(cref, bound).Span, renamedCref);
                            continue;
                        }
                        // A shipped assembly cannot see the target project: the reference becomes text, a labelled one its label.
                        if (shipped && enclosing is null)
                        {
                            var (textSpan, replacement) = AsText(attribute, cref.ToString());
                            Edit(tree, textSpan, replacement);
                        }
                        else if (cref is QualifiedCrefSyntax qualified && MoveOf(bound) is { Placement: Placement.AdapterMember } crefMove)
                            Edit(tree, qualified.Container.Span, NamespacePrefix(model, qualified.Container) + AdapterName(crefMove.AdapterOwner));
                        else if (cref is NameMemberCrefSyntax && MoveOf(bound) is { Placement: Placement.AdapterMember } nameMove &&
                                 (enclosing is null || !SameOwner(enclosing.Owner, nameMove.Owner)))
                            Edit(tree, cref.Span, $"{AdapterName(nameMove.AdapterOwner)}.{cref}");
                        else if (MoveOf(bound) is { Placement: Placement.ExtensionMember } extensionMove)
                            Edit(tree, cref.Span, ExtensionCref(identity, model, cref, extensionMove,
                                HolderName(extensionMove.AdapterOwner, RelocationHolderKind.Extensions)));
                        else if (MoveOf(bound) is null && Containers(bound).Select(MoveOf).FirstOrDefault(m => m is not null) is
                                 { Placement: Placement.AdapterMember } container)
                            foreach (var (span, text) in ContainedMemberCref(model, cref, bound, container, enclosing))
                                Edit(tree, span, text);
                        continue;
                    }
                    if (node.Ancestors().Any(a => a is CrefSyntax)) continue;
                    if (node is MemberBindingExpressionSyntax binding)
                    {
                        // A conditional access (receiver?.Member) reaches its member as a member access does.
                        var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(binding.Name).Symbol);
                        if (RenamedTo(bound) is { } renamedMember)
                            Edit(tree, binding.Name.Span, renamedMember);
                        if (bound is not null && MoveOf(bound) is { Placement: Placement.ExtensionMember } accessed)
                            RequireExtensionNamespace(tree, model, node, accessed);
                        if (bound is not null && !Moved(bound) && EnclosingMove(tree, node) is { Placement: not Placement.WholeType } reaching &&
                            DeclaringOwner(bound) is { } boundOwner && InOwnerChain(reaching.AdapterOwner, boundOwner))
                            Promote(bound, promotions, IsWritten(binding));
                        continue;
                    }
                    if (node is MemberAccessExpressionSyntax or QualifiedNameSyntax)
                    {
                        var (left, name) = node switch
                        {
                            MemberAccessExpressionSyntax access => new QualifiedNameParts(access.Expression, access.Name),
                            QualifiedNameSyntax qualified => new QualifiedNameParts(qualified.Left, qualified.Right),
                            _ => throw new InvalidOperationException(),
                        };
                        var bound = SymbolIdentity.Normalize(model.GetSymbolInfo(name).Symbol);
                        if (RenamedTo(bound) is { } renamedMember)
                            Edit(tree, name.Span, renamedMember);
                        if (left is ThisExpressionSyntax && bound is not null && EnclosingMove(tree, node) is { Placement: Placement.ExtensionMember })
                        {
                            Promote(bound, promotions, IsWritten(node));
                            continue;
                        }
                        if (bound is not null && MoveOf(bound) is { Placement: Placement.ExtensionMember } accessed)
                            RequireExtensionNamespace(tree, model, node, accessed);
                        // Moved code reaching a staying member of its owner through a qualifier, the owner's name or a receiver of
                        // one of the owner's extension blocks, reaches it from outside the owner.
                        if (bound is not null && !Moved(bound) && EnclosingMove(tree, node) is { Placement: not Placement.WholeType } reaching &&
                            DeclaringOwner(bound) is { } boundOwner && InOwnerChain(reaching.AdapterOwner, boundOwner))
                            Promote(bound, promotions, IsWritten(node));
                        if (bound is null || MoveOf(bound) is not { Placement: Placement.AdapterMember } move)
                            continue;
                        if (model.GetSymbolInfo(left).Symbol is not INamedTypeSymbol) continue;
                        if (EnclosingMove(tree, node) is not { Placement: Placement.AdapterMember } qualifying || !SameOwner(qualifying.Owner, move.Owner))
                            RejectUnimplementedReach(move, node);
                        Edit(tree, left.Span, NamespacePrefix(model, left) + AdapterName(move.AdapterOwner));
                        continue;
                    }
                    if (node is not SimpleNameSyntax simple || simple is IdentifierNameSyntax { IsVar: true } ||
                        simple.Identifier.Text is "nameof" or "dynamic" or "_") continue;
                    if (IsQualifiedName(simple))
                    {
                        // A property pattern names its member on its own.
                        if (simple.Parent is NameColonSyntax && RenamedTo(SymbolIdentity.Normalize(model.GetSymbolInfo(simple).Symbol)) is { } patternName)
                            Edit(tree, simple.Span, patternName);
                        continue;
                    }
                    var symbol = SymbolIdentity.Normalize(model.GetSymbolInfo(simple).Symbol);
                    if (symbol is null) continue;
                    var inside = EnclosingMove(tree, simple);
                    if (MoveOf(symbol) is { Placement: Placement.AdapterMember } target)
                    {
                        // An unqualified use from code that does not move with it into the same adapter.
                        if (inside is null || inside.Placement == Placement.WholeType || !SameOwner(inside.Owner, target.Owner)
                            || inside.Placement == Placement.ExtensionMember)
                        {
                            RejectUnimplementedReach(target, simple);
                            Edit(tree, simple.Span, $"{AdapterName(target.AdapterOwner)}.{simple}");
                        }
                        continue;
                    }
                    if (MoveOf(symbol) is { Placement: Placement.ExtensionMember } extension)
                    {
                        // Another moved instance member of the same owner is reached through the receiver.
                        if (inside is { Placement: Placement.ExtensionMember } && SameOwner(inside.Owner, extension.Owner))
                            Edit(tree, simple.Span, $"self.{simple}");
                        continue;
                    }
                    string? renamed = RenamedTo(symbol);
                    if (inside is null || inside.Placement == Placement.WholeType || inside.Node == simple.Parent)
                    {
                        if (renamed is not null)
                            Edit(tree, simple.Span, Unshadowed(model, simple, symbol, renamed));
                        continue;
                    }
                    if (Moved(symbol)) continue;
                    // A moved member's reference back to a staying member of its owner chain.
                    if (!symbol.IsStatic && symbol is not INamedTypeSymbol && symbol.ContainingType is { } instanceOwner &&
                        inside.Placement == Placement.ExtensionMember && InOwnerChain(inside.AdapterOwner, instanceOwner) &&
                        symbol.Kind is SymbolKind.Field or SymbolKind.Property or SymbolKind.Method or SymbolKind.Event)
                    {
                        Edit(tree, simple.Span, $"self.{Spelled(symbol, simple)}");
                        Promote(symbol, promotions, IsWritten(simple));
                    }
                    else if ((symbol.IsStatic || symbol is INamedTypeSymbol) && symbol.ContainingType is { } staticOwner &&
                        InOwnerChain(inside.AdapterOwner, staticOwner))
                    {
                        Edit(tree, simple.Span, $"{TypeName(staticOwner)}.{Spelled(symbol, simple)}");
                        Promote(symbol, promotions, IsWritten(simple));
                    }
                    else if (renamed is not null)
                        Edit(tree, simple.Span, Unshadowed(model, simple, symbol, renamed));
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

            // A field sharing its declaration with a promoted one shares its modifiers, so it becomes internal with it.
            widened.UnionWith(promotions);
            foreach (var field in promotions.OfType<IFieldSymbol>().ToList())
                foreach (var syntax in DeclarationsOf(identity, field))
                    if (syntax is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: > 1 } shared })
                        foreach (var sibling in shared.Variables)
                            if (SymbolIdentity.Normalize(compilationOf[sibling.SyntaxTree].GetSemanticModel(sibling.SyntaxTree).GetDeclaredSymbol(sibling)) is { } other)
                                widened.Add(other);
            // Moved documentation keeps a cref the target can resolve; one to a member it cannot see becomes text.
            foreach (var (tree, cref, attribute, bound, qualified) in movedCrefs)
            {
                if (HiddenFromTarget(bound))
                {
                    var (textSpan, replacement) = AsText(attribute, qualified ?? cref.ToString());
                    Edit(tree, textSpan, replacement);
                }
                else if (qualified is not null)
                    Edit(tree, cref.Span, qualified);
                else if (RenamedTo(bound) is { } renamedCref)
                    Edit(tree, CrefMemberName(cref, bound).Span, renamedCref);
            }
            foreach (var (tree, names) in neededUsings)
            {
                var unit = tree.GetCompilationUnitRoot();
                string newline = tree.GetText().ToString().Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
                string directives = string.Concat(names.Select(name => $"using {name};{newline}"));
                Edit(tree, new TextSpan(unit.Usings.Count > 0 ? unit.Usings[^1].FullSpan.End : 0, 0), directives);
            }
        }

        // What becomes internal decides the names it takes (PromotedNames) and must keep every rule a promotion can break
        // (PromotionRules); once names change, the edits are collected again with every reference written with its new name.
        CollectEdits();
        var types = new SourceTypes(identity, trees.Select(entry => entry.Compilation));
        renames = PromotedNames.Plan(widened, trees, types, identity);
        PromotionRules.Reject(widened, [.. setterPromotions.OfType<IPropertySymbol>()], symbol => RenamedTo(symbol) ?? symbol.Name, types,
            tree => compilationOf[tree].GetSemanticModel(tree), identity);
        if (renames.Count > 0)
        {
            var promotedBeforeRename = widened.ToHashSet(Comparer);
            CollectEdits();
            if (!widened.SetEquals(promotedBeforeRename))
                throw new InvalidOperationException("Renaming the promoted underscore-prefixed members changed which members moved code reaches.");
            RejectUnwrittenRenames();
        }
        // Every reference to a renamed member is covered by an edit that writes its new name.
        void RejectUnwrittenRenames()
        {
            foreach (var (tree, compilation, _) in trees)
            {
                var model = compilation.GetSemanticModel(tree);
                var written = edits.TryGetValue(tree, out var list) ? list : [];
                foreach (var reference in tree.GetRoot().DescendantNodes(descendIntoTrivia: true).OfType<IdentifierNameSyntax>())
                    if (reference.Identifier.ValueText.StartsWith('_') && RenamedTo(SymbolIdentity.Normalize(model.GetSymbolInfo(reference).Symbol)) is not null &&
                        !written.Any(edit => edit.Span.Contains(reference.Span)))
                        throw new InvalidOperationException($"The reference to {reference} at {reference.GetLocation().GetLineSpan()} was not " +
                            "written with the member's new name.");
            }
        }

        // 3. Accessibility: staying members used by moved code become internal (RelocationModifiers), an underscore-prefixed
        // one under its name without the underscore (PromotedNames).
        foreach (var symbol in setterPromotions)
            foreach (var syntax in DeclarationsOf(identity, symbol))
                if (syntax is PropertyDeclarationSyntax { AccessorList: { } accessors } property &&
                    !InMovedSpan(property.SyntaxTree, property.Span) &&
                    accessors.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration) || a.IsKind(SyntaxKind.InitAccessorDeclaration)) is { } setter)
                    foreach (var (span, text) in RelocationModifiers.PromotedSetter(setter,
                                 symbol.DeclaredAccessibility is not (Accessibility.Public or Accessibility.ProtectedOrInternal)))
                        Edit(property.SyntaxTree, span, text);
        foreach (var symbol in promotions)
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
                if (InMovedSpan(declaration.SyntaxTree, declaration.Span)) continue;
                if (declaration is not MemberDeclarationSyntax member)
                    throw new InvalidOperationException($"{symbol.ToDisplayString()}, which moved code uses, is declared by a " +
                        $"{declaration.Kind()} at {declaration.GetLocation().GetLineSpan()}, which takes no accessibility modifier.");
                foreach (var (span, text) in RelocationModifiers.Promoted(member))
                    Edit(declaration.SyntaxTree, span, text);
            }
        }
        foreach (var symbol in widened)
            if (RenamedTo(symbol) is { } name)
                foreach (var syntax in DeclarationsOf(identity, symbol))
                    Edit(syntax.SyntaxTree, syntax switch
                    {
                        VariableDeclaratorSyntax declarator => declarator.Identifier.Span,
                        PropertyDeclarationSyntax property => property.Identifier.Span,
                        _ => throw new InvalidOperationException($"{symbol.ToDisplayString()} is declared by a {syntax.Kind()} at " +
                            $"{syntax.GetLocation().GetLineSpan()}, whose name the rename cannot find."),
                    }, name);

        // 4. Moved text, grouped into target files.
        // A source project's global usings do not follow its code; each moved declaration takes those it names.
        var globalNamespaces = trees.GroupBy(entry => entry.Compilation).ToDictionary(group => group.Key, group =>
            (IReadOnlySet<string>)group.SelectMany(entry => entry.Tree.GetCompilationUnitRoot().Usings)
                .Where(u => u.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword) && u.Alias is null && u.StaticKeyword.IsKind(SyntaxKind.None) && u.Name is not null)
                .Select(u => u.Name!.ToString()).ToHashSet(StringComparer.Ordinal));
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
            var file = FileFor(holders.Of(OwnerKey(move.AdapterOwner), KindOf(move)).Path, move);
            var adapter = file.Adapter(move.AdapterOwner, kind => HolderName(move.AdapterOwner, kind), movedInterfaces,
                implementation => MoveOf(implementation) is not null);
            bool becomesExtension = move is { Placement: Placement.ExtensionMember, ExtensionReceiver: null };
            if (IsPartialMember(move.Node))
            {
                if (becomesExtension)
                {
                    // An extension block cannot hold a partial member (CS0751): the two parts become one member, rendered once.
                    if (JoinedPartial(move, moves, edits) is not { } joined)
                        continue;
                    text = joined;
                }
                else
                    adapter.Partial = true;
            }
            text = RelocationModifiers.AsHolderMember(text, (CSharpParseOptions)move.Tree.Options, becomesExtension,
                keepsAccessibility: Unimplemented(move.Symbol));
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
            $"{promotions.Count} members made internal; {renames.Count} renamed without their underscore; {manual.Count} left for manual relocation.");
    }

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

    /// <summary>The name in a cref that spells <paramref name="bound"/>'s own name: the last identifier spelling it.</summary>
    /// <exception cref="InvalidOperationException">The cref does not spell the member's name.</exception>
    private static IdentifierNameSyntax CrefMemberName(CrefSyntax cref, ISymbol bound) =>
        cref.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault(name => name.Identifier.ValueText == bound.Name) ??
        throw new InvalidOperationException($"The cref {cref} at {cref.GetLocation().GetLineSpan()} does not spell {bound.Name}.");

    /// <summary>
    /// A renamed member's new name at a reference written without a qualifier: qualified with <c>this</c>, or with its type in
    /// a static context, when a local, a parameter or another symbol of that name would take it there, as the constructor
    /// parameter a field's unprefixed name repeats does.
    /// </summary>
    private static string Unshadowed(SemanticModel model, SimpleNameSyntax reference, ISymbol symbol, string name)
    {
        if (PromotedNames.InMemberLookup(reference) || model.LookupSymbols(reference.SpanStart, name: name).IsEmpty)
            return name;
        return symbol.IsStatic || model.GetEnclosingSymbol(reference.SpanStart) is { IsStatic: true }
            ? $"{TypeName(symbol.ContainingType)}.{name}" : $"this.{name}";
    }

    /// <summary>Whether a source file starts with a UTF-8 byte-order mark, which rewriting it keeps.</summary>
    private static bool HasByteOrderMark(string path) => File.ReadAllBytes(path) is [0xEF, 0xBB, 0xBF, ..];

    /// <summary>
    /// Keeps in place every selected declaration that a declaration staying in place uses, until none is left: a selected
    /// declaration left for manual relocation stays in its assembly, so moving a declaration it uses would leave it
    /// referring to code its assembly no longer holds. The 2026-11-12 cleanup at d44b823793 moved
    /// InstalledImagePlanesConstants out of AssetExtraction while InstalledImagePlanes.CopyTo, which uses it, stayed for
    /// manual relocation, and the tree no longer compiled. A staying declaration inside a type that moves whole moves with
    /// that type, so it holds nothing back while that type moves.
    /// </summary>
    private static void KeepWhatStayingDeclarationsUse(SymbolIdentity identity, List<Move> found, List<StayingDeclaration> staying,
        List<string> manual)
    {
        bool Holds(Move move, ISymbol used) => identity.Key(move.Symbol) is { } key &&
            (identity.Key(used) == key || (move.Symbol is INamedTypeSymbol && Containers(used).Any(type => identity.Key(type) == key)));
        bool MovesWithType(StayingDeclaration declaration) => found.Any(move => move.Symbol is INamedTypeSymbol &&
            move.Tree == declaration.Tree && move.Node.FullSpan.Contains(declaration.Node.Span));
        bool kept = true;
        while (kept)
        {
            kept = false;
            foreach (StayingDeclaration declaration in staying.ToList())
            {
                if (MovesWithType(declaration))
                    continue;
                foreach (SyntaxNode use in declaration.Node.DescendantNodes())
                {
                    if (SymbolIdentity.Normalize(declaration.Model.GetSymbolInfo(use).Symbol) is not { } used)
                        continue;
                    foreach (Move move in found.Where(move => Holds(move, used)).ToList())
                    {
                        found.Remove(move);
                        manual.Add($"MANUAL {move.Symbol.Kind} {move.Symbol.ToDisplayString()} (kept in place: " +
                            $"{declaration.Symbol.ToDisplayString()} stays and uses it, {Location(identity, move.Node)})");
                        staying.Add(new(move.Symbol, move.Node, move.Tree, move.Model));
                        kept = true;
                    }
                }
            }
        }
    }

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

    /// <summary>
    /// The cref of a member that moves into an extension block of <paramref name="holder"/>, in the C# 14 form
    /// <c>Holder.extension(Receiver).Member</c>. The member keeps its parameters as written, a qualified reference keeps
    /// its namespace, and a receiver written as the owner keeps that spelling. The 2026-11-13 cleanup at a7f8ec16e6 moved
    /// <c>SamusProjectileListLayout.Prefix</c> into an extension block and left
    /// <c>&lt;inheritdoc cref="SamusProjectileListLayout.Prefix"/&gt;</c> naming it where it was (CS1574).
    /// </summary>
    private static string ExtensionCref(SymbolIdentity identity, SemanticModel model, CrefSyntax cref, Move move, string holder)
    {
        switch (cref)
        {
            case QualifiedCrefSyntax { Member: ExtensionMemberCrefSyntax member } qualified:
                return $"{NamespacePrefix(model, qualified.Container)}{holder}.{member}";
            case QualifiedCrefSyntax qualified:
                bool writtenAsOwner = move.ExtensionReceiver is null && identity.Key(model.GetSymbolInfo(qualified.Container).Symbol) is { } written &&
                    written == identity.Key(move.AdapterOwner);
                return $"{NamespacePrefix(model, qualified.Container)}{holder}." +
                    $"{ExtensionOf(move, writtenAsOwner ? qualified.Container.ToString() : null)}.{qualified.Member}";
            case ExtensionMemberCrefSyntax:
                return $"{holder}.{cref}";
            default:
                return $"{holder}.{ExtensionOf(move, null)}.{cref}";
        }
    }

    /// <summary>
    /// The <c>extension(Receiver)</c> part of a cref to a moved instance member: its own block's type parameters and receiver
    /// type when it was declared in one, else the owner, or <paramref name="writtenOwner"/> when the reference spelled it.
    /// Generic arguments take the brace form crefs use.
    /// </summary>
    private static string ExtensionOf(Move move, string? writtenOwner)
    {
        return move.Node.Parent is ExtensionBlockDeclarationSyntax block ? BlockCref(block)
            : $"extension({writtenOwner ?? TypeName(move.AdapterOwner)})";
    }

    /// <summary>The <c>extension(Receiver)</c> part of a cref to a member of <paramref name="block"/>, with its type parameters.</summary>
    /// <exception cref="InvalidOperationException">The block does not take exactly one receiver.</exception>
    private static string BlockCref(ExtensionBlockDeclarationSyntax block) =>
        block.ParameterList?.Parameters is [{ Type: { } receiver }]
            ? $"extension{CrefBraces(block.TypeParameterList?.ToString() ?? "")}({CrefBraces(receiver.ToString())})"
            : throw new InvalidOperationException($"The extension block at {block.GetLocation().GetLineSpan()} does not take one receiver.");

    /// <summary>
    /// The <c>extension(Receiver).</c> part a cref written as a bare member name needs once it is qualified with the owner,
    /// when it names a member of one of the owner's extension blocks; empty otherwise.
    /// </summary>
    private static string StayingBlockOf(ISymbol bound, CrefSyntax cref) =>
        cref is NameMemberCrefSyntax && bound.ContainingType is { IsExtension: true } extension
            ? (extension.DeclaringSyntaxReferences is [var reference] && reference.GetSyntax() is ExtensionBlockDeclarationSyntax block
                ? BlockCref(block) + "."
                : throw new InvalidOperationException($"{extension.ToDisplayString()} has no single extension block declaration."))
            : "";

    /// <summary>
    /// The types from <paramref name="container"/> down to the one declaring <paramref name="member"/>, as a cref names them:
    /// <c>Outer.Inner{T}</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="container"/> does not contain the member.</exception>
    private static string CrefPath(ISymbol member, ISymbol container)
    {
        var path = new List<string>();
        for (var type = member.ContainingType; type is not null; type = type.ContainingType)
        {
            path.Insert(0, type.TypeParameters.Length == 0 ? type.Name : $"{type.Name}{{{string.Join(",", type.TypeParameters.Select(p => p.Name))}}}");
            if (Comparer.Equals(type.OriginalDefinition, container))
                return string.Join(".", path);
        }
        throw new InvalidOperationException($"{container.ToDisplayString()} does not contain {member.ToDisplayString()}.");
    }

    /// <summary>Generic brackets in the brace form a cref writes them in.</summary>
    private static string CrefBraces(string text) => text.Replace('<', '{').Replace('>', '}');

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

    /// <summary>
    /// Every repository declaration of a symbol: each part of a partial type, and both parts of a partial method, property or
    /// event, whose accessibility must stay identical (CS8799). A part a source generator writes (a LibraryImport stub, say)
    /// is not repository source: the generator writes it again from the declaration it implements.
    /// </summary>
    internal static IEnumerable<SyntaxNode> DeclarationsOf(SymbolIdentity identity, ISymbol symbol)
    {
        ISymbol? implementation = symbol switch
        {
            IMethodSymbol method => method.PartialImplementationPart,
            IPropertySymbol property => property.PartialImplementationPart,
            IEventSymbol @event => @event.PartialImplementationPart,
            _ => null,
        };
        return symbol.DeclaringSyntaxReferences.Concat(implementation?.DeclaringSyntaxReferences ?? [])
            .Where(reference => identity.IsRepositorySource(reference.SyntaxTree))
            .Select(reference => reference.GetSyntax()).Distinct();
    }

    /// <summary>
    /// Fails when every member of an extension block that stays would move out of it one by one. A block is reached only
    /// through its members, so a block none of whose members stays is itself selected and moves whole; one emptied member by
    /// member means the audit took it for reachable through another declaration. The 2026-11-13 fixture work found blocks of
    /// one class taking the same receiver sharing one key, so a verification-only block left an empty block in shipped code
    /// (<see cref="SymbolIdentity"/> keys each block by where it is declared).
    /// </summary>
    /// <exception cref="InvalidOperationException">A staying block would be left with no members.</exception>
    private static void RejectEmptiedBlocks(IReadOnlyList<Move> moves)
    {
        foreach (var group in moves.Where(m => m.Placement == Placement.ExtensionMember && m.Node.Parent is ExtensionBlockDeclarationSyntax)
                     .GroupBy(m => (ExtensionBlockDeclarationSyntax)m.Node.Parent!))
            if (group.Key.Members.All(member => group.Any(m => m.Node == member)) &&
                !moves.Any(m => m.Tree == group.First().Tree && m.Node.FullSpan.Contains(group.Key.Span)))
                throw new InvalidOperationException($"Every member of the extension block at {group.Key.GetLocation().GetLineSpan()} " +
                    "would move out of it one by one, leaving the block empty, while the block itself is not selected to move.");
    }

    /// <summary>Whether a declaration is one part of a partial method, property, indexer or event.</summary>
    private static bool IsPartialMember(SyntaxNode node) =>
        node is MethodDeclarationSyntax or BasePropertyDeclarationSyntax && ((MemberDeclarationSyntax)node).Modifiers.Any(SyntaxKind.PartialKeyword);

    /// <summary>
    /// Whether a symbol is a partial method declared without an implementation: implicitly private, with every call to it,
    /// arguments included, left out of the program. An accessibility modifier would require an implementation (CS8795).
    /// </summary>
    private static bool Unimplemented(ISymbol symbol) => symbol is IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: null };

    /// <summary>
    /// Fails when code that does not move into the same class reaches a moved partial method without an implementation: it
    /// moves as written, private to its new class, and cannot be made internal without an implementation.
    /// </summary>
    /// <exception cref="InvalidOperationException">The moved method has no implementation.</exception>
    private static void RejectUnimplementedReach(Move target, SyntaxNode site)
    {
        if (Unimplemented(target.Symbol))
            throw new InvalidOperationException($"{target.Symbol.ToDisplayString()}, a partial method without an implementation, " +
                $"moves as written and stays private to its new class, but {site.GetLocation().GetLineSpan()} reaches it from outside that class.");
    }

    /// <summary>
    /// The text of an instance partial method or property that becomes an extension member, which cannot be partial (CS0751):
    /// one member, the implementation's declaration under the implementation's documentation, or the definition's when the
    /// implementation has none. Returns the text for the implementation's move and null for the definition's, which it covers.
    /// </summary>
    /// <exception cref="InvalidOperationException">The implementation is not repository source that moves with the definition.</exception>
    private static string? JoinedPartial(Move move, IReadOnlyList<Move> moves, Dictionary<SyntaxTree, List<TextEdit>> edits)
    {
        ISymbol? implementation = move.Symbol switch
        {
            IMethodSymbol method => method.PartialImplementationPart,
            IPropertySymbol property => property.PartialImplementationPart,
            _ => null,
        };
        Move? MoveDeclaring(ISymbol? part) => part?.DeclaringSyntaxReferences is [var reference]
            ? moves.FirstOrDefault(m => string.Equals(m.Tree.FilePath, reference.SyntaxTree.FilePath, StringComparison.OrdinalIgnoreCase) &&
                m.Node.Span == reference.Span)
            : null;
        if (MoveDeclaring(implementation) is not { } implementationMove || MoveDeclaring(move.Symbol) is not { } definitionMove)
            throw new InvalidOperationException($"{move.Symbol.ToDisplayString()} becomes an extension member, which cannot be partial " +
                $"(CS0751), and its two parts are not both repository source that moves ({Path.GetFileName(move.Tree.FilePath)}, " +
                $"line {move.Node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}).");
        if (move != implementationMove)
            return null;
        var documented = move.Node.GetLeadingTrivia().Any(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)) ? move : definitionMove;
        return Rewritten(documented.Tree, TextSpan.FromBounds(documented.Node.FullSpan.Start, documented.Node.SpanStart), edits) +
            Rewritten(move.Tree, TextSpan.FromBounds(move.Node.SpanStart, move.Node.FullSpan.End), edits);
    }

    /// <summary>The type that declares a member: for a member of an extension block, the static class declaring the block.</summary>
    private static INamedTypeSymbol? DeclaringOwner(ISymbol symbol) =>
        symbol.ContainingType is { IsExtension: true, ContainingType: { } declaring } ? declaring : symbol.ContainingType;

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
        /// <param name="owner">The type whose members move.</param>
        /// <param name="holderName">The name of the owner's class of each kind, as <see cref="RelocationHolders"/> chose it.</param>
        /// <param name="movedInterfaces">The interfaces that move whole.</param>
        /// <param name="moves">Whether a symbol moves.</param>
        public AdapterClass Adapter(INamedTypeSymbol owner, Func<RelocationHolderKind, string> holderName, HashSet<INamedTypeSymbol> movedInterfaces,
            Func<ISymbol, bool> moves)
        {
            if (adapters.TryGetValue(owner, out var adapter))
                return adapter;
            var implemented = owner.Interfaces.Select(i => i.OriginalDefinition).Where(movedInterfaces.Contains).ToList();
            adapters[owner] = adapter = new AdapterClass(owner,
                [.. implemented.Select(i => i.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat))], style, holderName);
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
                    if (classes.TryGetValue(adapter.MembersName, out var membersClass))
                    {
                        // RelocationHolders merges partial members only into a partial class.
                        if (adapter.Partial && !membersClass.Modifiers.Any(SyntaxKind.PartialKeyword))
                            throw new InvalidOperationException($"{adapter.MembersName} is not partial, so the partial members moving into it cannot.");
                        InsertBefore(membersClass.CloseBraceToken, adapter.Members, "");
                    }
                    else
                        appended.Append("\r\n").Append(adapter.RenderMembers());
                }
                if (adapter.Extensions.Count == 0)
                    continue;
                if (!classes.TryGetValue(adapter.ExtensionsName, out var extensionsClass))
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
    private sealed class AdapterClass(INamedTypeSymbol owner, List<string> interfaces, RelocationStyle style,
        Func<RelocationHolderKind, string> holderName)
    {
        /// <summary>The shipped type whose moved members this adapter holds.</summary>
        public INamedTypeSymbol Owner { get; } = owner;
        /// <summary>The class that holds the owner's moved static members.</summary>
        public string MembersName => holderName(RelocationHolderKind.Members);
        /// <summary>The class that holds the owner's moved instance members.</summary>
        public string ExtensionsName => holderName(RelocationHolderKind.Extensions);
        /// <summary>Moved interfaces implemented by this adapter through relocated members and forwarders.</summary>
        public List<string> Interfaces { get; } = interfaces;
        /// <summary>Static members, nested types, and retained-contract forwarders in the adapter.</summary>
        public List<string> Members { get; } = [];
        /// <summary>Whether a moved partial member is among <see cref="Members"/>, which only a partial class declares (CS0751).</summary>
        public bool Partial { get; set; }
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
            string partial = Partial ? "partial " : "";
            if (Interfaces.Count > 0)
                output.Append($"[ToolingFor(typeof({ownerName}))]\r\ninternal abstract {partial}class {MembersName} : {string.Join(", ", Interfaces)}\r\n");
            else
                output.Append($"internal static {partial}class {MembersName}\r\n");
            return output.Append("{\r\n").Append(string.Join("\r\n", Members.Select(m => m.Trim('\r', '\n')))).Append("\r\n}\r\n").ToString();
        }

        /// <summary>Emits the class holding the owner's moved instance members.</summary>
        public string RenderExtensions() =>
            new StringBuilder("/// <summary>").Append(string.Format(style.ExtensionsSummary, TypeName(Owner))).Append("</summary>\r\n")
                .Append($"internal static class {ExtensionsName}\r\n{{\r\n")
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
