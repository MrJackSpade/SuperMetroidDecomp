using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.Declarations;
using static SuperMetroid.ReachabilityAudit.Removal.RelocationSymbols;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The relocation's reference step: every edit that keeps the solution's references and documentation crefs resolving
/// once the moves are made, and the staying members moved code reaches from outside their owners, which become internal.
/// Outside moved spans the edits apply in place; inside, to the moved text. Each instance collects once, with the names
/// the promoted members take (<see cref="RenameMap"/>): the first collection, with none, finds what becomes internal.
/// </summary>
internal sealed class ReferenceRewriter
{
    /// <summary>A cref in moved documentation to a staying member, written once what becomes internal is known.</summary>
    /// <param name="Tree">The tree that holds the cref.</param>
    /// <param name="Cref">The cref.</param>
    /// <param name="Attribute">The cref's attribute, whose element becomes text when the target cannot see the member.</param>
    /// <param name="Bound">The staying member it names.</param>
    /// <param name="Qualified">The cref qualified with the member's owner, when the moved code no longer stands inside it.</param>
    private sealed record MovedCref(SyntaxTree Tree, CrefSyntax Cref, XmlCrefAttributeSyntax Attribute, ISymbol Bound, string? Qualified);

    /// <summary>Every repository tree, once.</summary>
    private readonly IReadOnlyList<ProjectTree> trees;

    /// <summary>The declarations that move.</summary>
    private readonly RelocationMoves moves;

    /// <summary>The class each owner's moved members go into.</summary>
    private readonly RelocationHolders holders;

    /// <summary>Gives each symbol its repository key.</summary>
    private readonly SymbolIdentity identity;

    /// <summary>The assemblies that ship to players, which cannot see the target.</summary>
    private readonly IReadOnlySet<string> shippedAssemblies;

    /// <summary>The names promoted underscore-prefixed members take.</summary>
    private readonly RenameMap renames;

    /// <summary>The compilation of each repository tree.</summary>
    private readonly Dictionary<SyntaxTree, Compilation> compilationOf;

    /// <summary>The edits collected, by tree.</summary>
    private readonly Dictionary<SyntaxTree, List<TextEdit>> edits = new();

    /// <summary>The staying members moved code reaches from outside their owners.</summary>
    private readonly HashSet<ISymbol> promotions = new(Comparer);

    /// <summary>The staying properties whose private or protected setter moved code writes.</summary>
    private readonly HashSet<ISymbol> setterPromotions = new(Comparer);

    /// <summary>
    /// Every staying member that becomes internal: the promoted ones and the fields sharing a declaration with one, whose
    /// shared modifiers change with it.
    /// </summary>
    private readonly HashSet<ISymbol> widened = new(Comparer);

    /// <summary>The moves that reach each member of <see cref="widened"/>, through it or through a field it shares a declaration with.</summary>
    private readonly Dictionary<ISymbol, HashSet<RelocationMove>> reachedBy = new(Comparer);

    /// <summary>
    /// The extension namespaces staying call sites of moved instance members need in scope, by tree; moved code lands in
    /// that namespace already.
    /// </summary>
    private readonly Dictionary<SyntaxTree, SortedSet<string>> neededUsings = new();

    /// <summary>
    /// Crefs in moved documentation to staying members, written once what becomes internal is known: the target compiles
    /// against the reference assembly of the member's project, which holds no private member.
    /// </summary>
    private readonly List<MovedCref> movedCrefs = [];

    /// <summary>A rewriter of the solution's references for these moves, spelling renamed members as <paramref name="renames"/> says.</summary>
    /// <param name="trees">Every repository tree, once.</param>
    /// <param name="moves">The declarations that move.</param>
    /// <param name="holders">The class each owner's moved members go into.</param>
    /// <param name="identity">Gives each symbol its repository key.</param>
    /// <param name="shippedAssemblies">The assemblies that ship to players.</param>
    /// <param name="renames">The names promoted underscore-prefixed members take.</param>
    public ReferenceRewriter(IReadOnlyList<ProjectTree> trees, RelocationMoves moves, RelocationHolders holders, SymbolIdentity identity,
        IReadOnlySet<string> shippedAssemblies, RenameMap renames)
    {
        this.trees = trees;
        this.moves = moves;
        this.holders = holders;
        this.identity = identity;
        this.shippedAssemblies = shippedAssemblies;
        this.renames = renames;
        compilationOf = trees.ToDictionary(entry => entry.Tree, entry => entry.Compilation);
    }

    /// <summary>Collects every edit and every promotion; one call per rewriter.</summary>
    /// <exception cref="InvalidOperationException">The rewriter has collected already.</exception>
    public ReferenceEdits Collect()
    {
        if (edits.Count > 0 || promotions.Count > 0)
            throw new InvalidOperationException("A reference rewriter collects once.");
        foreach (var (tree, compilation, assembly) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            bool shipped = shippedAssemblies.Contains(assembly);
            var root = tree.GetRoot();
            foreach (var node in root.DescendantNodes(descendIntoTrivia: true))
            {
                // Moved code constructing a staying type needs the constructor visible from the target.
                if (node is BaseObjectCreationExpressionSyntax creation && EnclosingMove(tree, creation) is { Placement: not RelocationPlacement.WholeType } constructing &&
                    SymbolIdentity.Normalize(model.GetSymbolInfo(creation).Symbol) is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor &&
                    !Moved(constructor))
                {
                    Promote(constructor, promotions, false, constructing);
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
                        string? qualifiedText = enclosing is { Placement: not RelocationPlacement.WholeType } && cref is NameMemberCrefSyntax or ExtensionMemberCrefSyntax &&
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
                    else if (cref is QualifiedCrefSyntax qualified && MoveOf(bound) is { Placement: RelocationPlacement.AdapterMember } crefMove)
                        Edit(tree, qualified.Container.Span, NamespacePrefix(model, qualified.Container) + AdapterName(crefMove.AdapterOwner));
                    else if (cref is NameMemberCrefSyntax && MoveOf(bound) is { Placement: RelocationPlacement.AdapterMember } nameMove &&
                             (enclosing is null || !SameOwner(enclosing.Owner, nameMove.Owner)))
                        Edit(tree, cref.Span, $"{AdapterName(nameMove.AdapterOwner)}.{cref}");
                    else if (MoveOf(bound) is { Placement: RelocationPlacement.ExtensionMember } extensionMove)
                        Edit(tree, cref.Span, ExtensionCref(identity, model, cref, extensionMove,
                            HolderName(extensionMove.AdapterOwner, RelocationHolderKind.Extensions)));
                    else if (MoveOf(bound) is null && Containers(bound).Select(MoveOf).FirstOrDefault(m => m is not null) is
                             { Placement: RelocationPlacement.AdapterMember } container)
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
                    if (bound is not null && MoveOf(bound) is { Placement: RelocationPlacement.ExtensionMember } accessed)
                        RequireExtensionNamespace(tree, model, node, accessed);
                    if (bound is not null && !Moved(bound) && EnclosingMove(tree, node) is { Placement: not RelocationPlacement.WholeType } reaching &&
                        DeclaringOwner(bound) is { } boundOwner && InOwnerChain(reaching.AdapterOwner, boundOwner))
                        Promote(bound, promotions, IsWritten(binding), reaching);
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
                    if (left is ThisExpressionSyntax && bound is not null && EnclosingMove(tree, node) is { Placement: RelocationPlacement.ExtensionMember } extending)
                    {
                        Promote(bound, promotions, IsWritten(node), extending);
                        continue;
                    }
                    if (bound is not null && MoveOf(bound) is { Placement: RelocationPlacement.ExtensionMember } accessed)
                        RequireExtensionNamespace(tree, model, node, accessed);
                    // Moved code reaching a staying member of its owner through a qualifier, the owner's name or a receiver of
                    // one of the owner's extension blocks, reaches it from outside the owner.
                    if (bound is not null && !Moved(bound) && EnclosingMove(tree, node) is { Placement: not RelocationPlacement.WholeType } reaching &&
                        DeclaringOwner(bound) is { } boundOwner && InOwnerChain(reaching.AdapterOwner, boundOwner))
                        Promote(bound, promotions, IsWritten(node), reaching);
                    if (bound is null || MoveOf(bound) is not { Placement: RelocationPlacement.AdapterMember } move)
                        continue;
                    if (model.GetSymbolInfo(left).Symbol is not INamedTypeSymbol) continue;
                    if (EnclosingMove(tree, node) is not { Placement: RelocationPlacement.AdapterMember } qualifying || !SameOwner(qualifying.Owner, move.Owner))
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
                if (MoveOf(symbol) is { Placement: RelocationPlacement.AdapterMember } target)
                {
                    // An unqualified use from code that does not move with it into the same adapter.
                    if (inside is null || inside.Placement == RelocationPlacement.WholeType || !SameOwner(inside.Owner, target.Owner)
                        || inside.Placement == RelocationPlacement.ExtensionMember)
                    {
                        RejectUnimplementedReach(target, simple);
                        Edit(tree, simple.Span, $"{AdapterName(target.AdapterOwner)}.{simple}");
                    }
                    continue;
                }
                if (MoveOf(symbol) is { Placement: RelocationPlacement.ExtensionMember } extension)
                {
                    // Another moved instance member of the same owner is reached through the receiver.
                    if (inside is { Placement: RelocationPlacement.ExtensionMember } && SameOwner(inside.Owner, extension.Owner))
                        Edit(tree, simple.Span, $"self.{simple}");
                    continue;
                }
                string? renamed = RenamedTo(symbol);
                if (inside is null || inside.Placement == RelocationPlacement.WholeType || inside.Node == simple.Parent)
                {
                    if (renamed is not null)
                        Edit(tree, simple.Span, Unshadowed(model, simple, symbol, renamed));
                    continue;
                }
                if (Moved(symbol)) continue;
                // A moved member's reference back to a staying member of its owner chain.
                if (!symbol.IsStatic && symbol is not INamedTypeSymbol && symbol.ContainingType is { } instanceOwner &&
                    inside.Placement == RelocationPlacement.ExtensionMember && InOwnerChain(inside.AdapterOwner, instanceOwner) &&
                    symbol.Kind is SymbolKind.Field or SymbolKind.Property or SymbolKind.Method or SymbolKind.Event)
                {
                    Edit(tree, simple.Span, $"self.{Spelled(symbol, simple)}");
                    Promote(symbol, promotions, IsWritten(simple), inside);
                }
                else if ((symbol.IsStatic || symbol is INamedTypeSymbol) && symbol.ContainingType is { } staticOwner &&
                    InOwnerChain(inside.AdapterOwner, staticOwner))
                {
                    Edit(tree, simple.Span, $"{TypeName(staticOwner)}.{Spelled(symbol, simple)}");
                    Promote(symbol, promotions, IsWritten(simple), inside);
                }
                else if (renamed is not null)
                    Edit(tree, simple.Span, Unshadowed(model, simple, symbol, renamed));
            }
            foreach (var self in root.DescendantNodes().OfType<ThisExpressionSyntax>())
                if (EnclosingMove(tree, self) is { Placement: RelocationPlacement.ExtensionMember })
                    Edit(tree, self.Span, "self");
            // Staying types no longer implement moved interfaces; their adapters do.
            foreach (var baseList in root.DescendantNodes().OfType<BaseListSyntax>())
            {
                if (InMovedSpan(tree, baseList.Span)) continue;
                var kept = baseList.Types.Where(t => model.GetTypeInfo(t.Type).Type is not INamedTypeSymbol bt ||
                    identity.Key(bt.OriginalDefinition) is not { } baseKey || !moves.MovedInterfaceKeys.Contains(baseKey)).ToList();
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
                            {
                                widened.Add(other);
                                Reached(other, reachedBy[field]);
                            }
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
        return new ReferenceEdits(edits, promotions, setterPromotions, widened, reachedBy);
    }

    /// <summary>Fails unless every reference to a renamed member is covered by an edit that writes its new name.</summary>
    /// <exception cref="InvalidOperationException">A reference keeps the member's old name.</exception>
    public void RejectUnwrittenRenames()
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

    /// <summary>Adds an edit to <paramref name="tree"/>.</summary>
    private void Edit(SyntaxTree tree, TextSpan span, string text)
    {
        if (!edits.TryGetValue(tree, out var list)) edits[tree] = list = [];
        list.Add(new TextEdit(span, text));
    }

    /// <summary>The move of a symbol, or null when it stays.</summary>
    private RelocationMove? MoveOf(ISymbol? symbol) => moves.MoveOf(symbol);

    /// <summary>Whether a symbol moves, itself or with the type containing it.</summary>
    private bool Moved(ISymbol symbol) => moves.Moved(symbol);

    /// <summary>Whether two owners are the same type.</summary>
    private bool SameOwner(INamedTypeSymbol? a, INamedTypeSymbol? b) => moves.SameOwner(a, b);

    /// <summary>Whether a span lies inside a moved declaration.</summary>
    private bool InMovedSpan(SyntaxTree tree, TextSpan span) => moves.InMovedSpan(tree, span);

    /// <summary>The move whose declaration holds a node, or null.</summary>
    private RelocationMove? EnclosingMove(SyntaxTree tree, SyntaxNode node) => moves.EnclosingMove(tree, node);

    /// <summary>The name of the class holding an owner's moved static members.</summary>
    private string AdapterName(INamedTypeSymbol owner) => holders.Of(moves.OwnerKey(owner), RelocationHolderKind.Members).Name;

    /// <summary>The name of the class holding an owner's moved members of a kind.</summary>
    private string HolderName(INamedTypeSymbol owner, RelocationHolderKind kind) => holders.Of(moves.OwnerKey(owner), kind).Name;

    /// <summary>The new name of a renamed member, or null.</summary>
    private string? RenamedTo(ISymbol? symbol) => renames.NewName(symbol);

    /// <summary>A reference's name as it is written once renamed members take their new names.</summary>
    private string Spelled(ISymbol symbol, SimpleNameSyntax written) => RenamedTo(symbol) ?? written.ToString();

    /// <summary>A cref's text with a renamed member's new name in place of its old one.</summary>
    private string RenamedCref(CrefSyntax cref, ISymbol bound)
    {
        if (RenamedTo(bound) is not { } name)
            return cref.ToString();
        int at = CrefMemberName(cref, bound).SpanStart - cref.SpanStart;
        return cref.ToString().Remove(at, bound.Name.Length).Insert(at, name);
    }

    /// <summary>Whether the target cannot see a staying symbol: it, or a type containing it, stays private.</summary>
    private bool HiddenFromTarget(ISymbol symbol) =>
        symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal && !widened.Contains(symbol) ||
        symbol.ContainingType?.OriginalDefinition is { } container && HiddenFromTarget(container);

    /// <summary>Records that moved code reaches a staying member, and writes its setter when <paramref name="written"/>.</summary>
    private void Promote(ISymbol symbol, HashSet<ISymbol> into, bool written, RelocationMove by)
    {
        if (symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
        {
            into.Add(symbol);
            Reached(symbol, [by]);
        }
        if (written && symbol is IPropertySymbol { SetMethod.DeclaredAccessibility: Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal })
        {
            setterPromotions.Add(symbol);
            Reached(symbol, [by]);
        }
    }

    /// <summary>Records that <paramref name="moves"/> reach a member that becomes internal for them.</summary>
    private void Reached(ISymbol symbol, IEnumerable<RelocationMove> moves)
    {
        if (!reachedBy.TryGetValue(symbol, out var reaching))
            reachedBy[symbol] = reaching = [];
        reaching.UnionWith(moves);
    }

    /// <summary>
    /// Records the namespace a staying call site of a moved instance member needs, which resolves it as an extension; moved
    /// code lands in that namespace already.
    /// </summary>
    private void RequireExtensionNamespace(SyntaxTree tree, SemanticModel model, SyntaxNode site, RelocationMove extension)
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

    /// <summary>
    /// The edits for a cref to a member of a nested type or extension block that moves into its owner's adapter. The name
    /// that qualifies the container, written with or without its namespace, becomes the adapter's; an unqualified form is
    /// qualified with the adapter unless the cref moves into that adapter too.
    /// </summary>
    private IEnumerable<TextEdit> ContainedMemberCref(SemanticModel model, CrefSyntax cref, ISymbol bound, RelocationMove container, RelocationMove? enclosing)
    {
        string holder = AdapterName(container.AdapterOwner);
        bool inAdapter = enclosing is not null && (enclosing == container ||
            enclosing.Placement == RelocationPlacement.AdapterMember && SameOwner(enclosing.Owner, container.Owner));
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
    private static string ExtensionCref(SymbolIdentity identity, SemanticModel model, CrefSyntax cref, RelocationMove move, string holder)
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
    private static string ExtensionOf(RelocationMove move, string? writtenOwner)
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

    /// <summary>
    /// Fails when code that does not move into the same class reaches a moved partial method without an implementation: it
    /// moves as written, private to its new class, and cannot be made internal without an implementation.
    /// </summary>
    /// <exception cref="InvalidOperationException">The moved method has no implementation.</exception>
    private static void RejectUnimplementedReach(RelocationMove target, SyntaxNode site)
    {
        if (Unimplemented(target.Symbol))
            throw new InvalidOperationException($"{target.Symbol.ToDisplayString()}, a partial method without an implementation, " +
                $"moves as written and stays private to its new class, but {site.GetLocation().GetLineSpan()} reaches it from outside that class.");
    }
}
