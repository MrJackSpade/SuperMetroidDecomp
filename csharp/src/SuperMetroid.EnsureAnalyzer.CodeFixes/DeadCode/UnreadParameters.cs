using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.EnsureAnalyzer.CodeFixes.Cleanup;

namespace SuperMetroid.EnsureAnalyzer.CodeFixes.DeadCode;

/// <summary>
/// The parameters a dead-code fix leaves unread, removed with the code that read them. IDE0060, an unused parameter, is a
/// build error, and the cleanup of e4ce9514eb (2026-11-15) removed the write-only field <c>_room</c> of eight
/// constructors and left each constructor's <c>room</c> parameter unread, so the tree no longer built. A fix reports the
/// code it removes (<see cref="Removing"/>); once it has, <see cref="RemoveAsync"/> takes out each parameter of a method
/// or constructor (a class's primary constructor included) whose every read lies in removed code: the parameter, its
/// <c>param</c> documentation and the argument every call passes it. An argument read by a removed call is removed code
/// too, so a parameter of the caller that only that argument read goes as well, and so on. What cannot go without a
/// choice fails the fix, which leaves the code as it was: an argument whose evaluation could have an effect; a
/// <c>ref</c>, <c>out</c>, <c>in</c>, <c>params</c> or extension receiver parameter; a method that is virtual, abstract,
/// overrides, implements an interface, is partial or extern, or would then clash with another of its type's members; a
/// method used other than by calls, by name or by crefs without parameters; a record's positional parameter; and a
/// parameter documentation still names with <c>paramref</c>.
/// </summary>
internal sealed class UnreadParameters
{
    private readonly Solution _solution;
    private readonly SolutionEdits _edits;
    private readonly Diagnostic _site;

    /// <summary>The spans of the code the fix removes, by tree.</summary>
    private readonly Dictionary<SyntaxTree, List<TextSpan>> _removed = new();

    /// <summary>Parameters removed code reads, in the order found; a parameter found again is checked again.</summary>
    private readonly List<IParameterSymbol> _candidates = [];

    /// <summary>The parameters taken out.</summary>
    private readonly HashSet<IParameterSymbol> _taken = new(SymbolEqualityComparer.Default);

    /// <summary>The items taken out of each parameter or argument list, by their index in it.</summary>
    private readonly Dictionary<SyntaxNode, SortedSet<int>> _listItems = new();

    /// <summary>The methods, properties, indexers and events code is removed from, with their semantic models.</summary>
    private readonly Dictionary<MemberDeclarationSyntax, SemanticModel> _enclosing = new();

    /// <summary>Starts the parameter removals of one fix of <paramref name="solution"/>, whose edits are <paramref name="edits"/>;
    /// <paramref name="site"/> names the fix's finding when a removal fails it.</summary>
    internal UnreadParameters(Solution solution, SolutionEdits edits, Diagnostic site)
    {
        _solution = solution;
        _edits = edits;
        _site = site;
    }

    /// <summary>Records that the fix removes <paramref name="node"/>: a parameter it reads, other than one it declares,
    /// may be left unread.</summary>
    internal void Removing(SyntaxNode node, SemanticModel model)
    {
        if (!_removed.TryGetValue(node.SyntaxTree, out List<TextSpan>? spans))
            _removed[node.SyntaxTree] = spans = [];
        spans.Add(node.Span);
        if (node.Ancestors().FirstOrDefault(ancestor => ancestor is MemberDeclarationSyntax) is MemberDeclarationSyntax
            { RawKind: (int)SyntaxKind.MethodDeclaration or (int)SyntaxKind.PropertyDeclaration or (int)SyntaxKind.IndexerDeclaration or (int)SyntaxKind.EventDeclaration } member)
            _enclosing[member] = model;
        foreach (IdentifierNameSyntax name in node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(name).Symbol is IParameterSymbol parameter && !Declares(node, parameter))
                _candidates.Add(parameter);
        }
    }

    /// <summary>Takes out every parameter left unread, with what goes with it, into the fix's edits.</summary>
    /// <exception cref="CleanupFailure">A parameter left unread cannot be taken out without a choice.</exception>
    internal async Task RemoveAsync(CancellationToken cancellationToken)
    {
        for (int index = 0; index < _candidates.Count; index++)
        {
            IParameterSymbol parameter = _candidates[index];
            if (_taken.Contains(parameter) || parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor or MethodKind.Ordinary } method)
                continue;
            if (await IsReadAsync(parameter, cancellationToken).ConfigureAwait(false))
                continue;
            await TakeAsync(parameter, method, cancellationToken).ConfigureAwait(false);
        }
        foreach (KeyValuePair<MemberDeclarationSyntax, SemanticModel> member in _enclosing)
            RejectLostInstanceAccess(member.Key, member.Value);
        foreach (KeyValuePair<SyntaxNode, SortedSet<int>> list in _listItems)
        {
            SourceEdits edits = await _edits.ForAsync(list.Key.SyntaxTree, cancellationToken).ConfigureAwait(false);
            foreach (TextSpan span in ItemRemovals(Items(list.Key), list.Value))
                edits.Replace(span, string.Empty);
        }
    }

    /// <summary>Whether code the fix keeps reads <paramref name="parameter"/>; documentation naming it is not a read.</summary>
    private async Task<bool> IsReadAsync(IParameterSymbol parameter, CancellationToken cancellationToken)
    {
        foreach (ReferenceLocation reference in await ReferencesAsync(parameter, cancellationToken).ConfigureAwait(false))
        {
            if (!InRemoved(reference.Location) && !InDocumentation(reference.Location))
                return true;
        }
        return false;
    }

    /// <summary>Takes out <paramref name="parameter"/> of <paramref name="method"/>, its documentation and every argument passed to it.</summary>
    private async Task TakeAsync(IParameterSymbol parameter, IMethodSymbol method, CancellationToken cancellationToken)
    {
        string name = $"'{parameter.Name}' of {method.ToDisplayString()}";
        if (parameter.RefKind != RefKind.None || parameter.IsParams || parameter.IsThis)
            throw new CleanupFailure(_site, $"{name}, left unread, is a {parameter.RefKind} or params or receiver parameter, whose arguments cannot simply go");
        if (method.IsVirtual || method.IsAbstract || method.IsOverride || method.IsExtern || method.IsPartialDefinition ||
            method.PartialImplementationPart is not null || method.PartialDefinitionPart is not null || !method.ExplicitInterfaceImplementations.IsEmpty ||
            ImplementsInterface(method))
            throw new CleanupFailure(_site, $"{name}, left unread, belongs to a method whose signature something else decides");
        if (parameter.DeclaringSyntaxReferences.Length != 1 ||
            await parameter.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is not ParameterSyntax { Parent: ParameterListSyntax list } declaration)
            throw new CleanupFailure(_site, $"{name}, left unread, is not declared once in a parameter list");
        if (list.Parent is RecordDeclarationSyntax)
            throw new CleanupFailure(_site, $"{name}, left unread, is a record's positional parameter, which is also its property");
        RejectClash(method, parameter, name);
        foreach (ReferenceLocation reference in await ReferencesAsync(parameter, cancellationToken).ConfigureAwait(false))
        {
            if (!InRemoved(reference.Location) && !InParameterDocumentation(reference.Location))
                throw new CleanupFailure(reference.Location, _site.Id, $"{name}, left unread, is still named in documentation here");
        }
        _taken.Add(parameter);
        Take(list, list.Parameters.IndexOf(declaration));
        if (list.Parent is { } owner)
            await RemoveDocumentationAsync(owner, parameter.Name, cancellationToken).ConfigureAwait(false);
        foreach (ReferencedSymbol referenced in await SymbolFinder.FindReferencesAsync(method, _solution, cancellationToken).ConfigureAwait(false))
        {
            foreach (ReferenceLocation call in referenced.Locations)
                await RemoveArgumentAsync(call, method, parameter, name, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Takes out the argument the call at <paramref name="reference"/> passes to <paramref name="parameter"/>.</summary>
    private async Task RemoveArgumentAsync(ReferenceLocation reference, IMethodSymbol method, IParameterSymbol parameter, string name,
        CancellationToken cancellationToken)
    {
        // A target-typed new is an implicit reference to the constructor it calls, and is edited like any other call.
        if (reference.Location.SourceTree is not { } tree)
            throw new CleanupFailure(reference.Location, _site.Id, $"{name}, left unread, belongs to a method called from outside source");
        if (InRemoved(reference.Location))
            return;
        SyntaxNode root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
        SyntaxNode node = root.FindNode(reference.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
        SemanticModel model = await reference.Document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"{tree.FilePath} has no semantic model.");
        if (node.IsPartOfStructuredTrivia())
        {
            if (node.AncestorsAndSelf().OfType<CrefSyntax>().LastOrDefault() is { } cref &&
                cref.DescendantNodesAndSelf().OfType<CrefParameterListSyntax>().Any())
                throw new CleanupFailure(reference.Location, _site.Id, $"{name}, left unread, is named in a cref with its parameters");
            return;
        }
        if (IsNameOf(node, model, cancellationToken))
            return;
        if (Call(node, method, model, cancellationToken) is not { } arguments)
            throw new CleanupFailure(reference.Location, _site.Id, $"{name}, left unread, belongs to a method used here other than by a call");
        for (int index = 0; index < arguments.Arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments.Arguments[index];
            bool passed = argument.NameColon is { } named ? named.Name.Identifier.ValueText == parameter.Name : index == parameter.Ordinal;
            if (!passed)
                continue;
            if (!SideEffects.IsFree(model.GetOperation(argument.Expression, cancellationToken)))
                throw new CleanupFailure(argument.GetLocation(), _site.Id, $"{name}, left unread, is passed '{argument.Expression}', whose evaluation could have an effect");
            Removing(argument, model);
            Take(arguments, index);
        }
    }

    /// <summary>
    /// Fails when an instance member read its instance only in removed code: it would then read no instance data, and
    /// CA1822, a build error, asks for it to be static, a change to every call of it the fix does not make. A static,
    /// virtual, abstract, overriding or interface-implementing member, which CA1822 leaves alone, is not checked.
    /// </summary>
    private void RejectLostInstanceAccess(MemberDeclarationSyntax member, SemanticModel model)
    {
        if (model.GetDeclaredSymbol(member) is not { IsStatic: false, IsVirtual: false, IsAbstract: false, IsOverride: false } symbol ||
            symbol.ContainingType.AllInterfaces.SelectMany(contract => contract.GetMembers()).Any(contract =>
                SymbolEqualityComparer.Default.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(contract), symbol)))
            return;
        var reads = member.DescendantNodes().OfType<ExpressionSyntax>()
            .Where(expression => ReadsInstance(model.GetOperation(expression), symbol.ContainingType)).ToList();
        if (reads.Count > 0 && reads.All(read => InRemoved(read.GetLocation())))
            throw new CleanupFailure(member.GetLocation(), _site.Id, $"{symbol.ToDisplayString()} reads its instance only in the code the fix removes, " +
                "and would then be asked to be static (CA1822)");
    }

    /// <summary>Whether an operation reads the instance of <paramref name="type"/>: <c>this</c> or <c>base</c>, a member
    /// reached through it, written or not, or a captured primary-constructor parameter.</summary>
    private static bool ReadsInstance(IOperation? operation, INamedTypeSymbol type) => operation switch
    {
        IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance } => true,
        IMemberReferenceOperation { Instance: IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance } } => true,
        IInvocationOperation { Instance: IInstanceReferenceOperation { ReferenceKind: InstanceReferenceKind.ContainingTypeInstance } } => true,
        IParameterReferenceOperation { Parameter.ContainingSymbol: IMethodSymbol { MethodKind: MethodKind.Constructor } constructor } =>
            SymbolEqualityComparer.Default.Equals(constructor.ContainingType, type),
        _ => false,
    };

    /// <summary>The argument list of the call of <paramref name="method"/> whose target <paramref name="node"/> names, or null.</summary>
    private static BaseArgumentListSyntax? Call(SyntaxNode node, IMethodSymbol method, SemanticModel model, CancellationToken cancellationToken)
    {
        foreach (SyntaxNode ancestor in node.AncestorsAndSelf())
        {
            BaseArgumentListSyntax? arguments = ancestor switch
            {
                InvocationExpressionSyntax invocation => invocation.ArgumentList,
                BaseObjectCreationExpressionSyntax creation => creation.ArgumentList,
                ConstructorInitializerSyntax initializer => initializer.ArgumentList,
                PrimaryConstructorBaseTypeSyntax baseType => baseType.ArgumentList,
                _ => null,
            };
            if (ancestor is not (InvocationExpressionSyntax or BaseObjectCreationExpressionSyntax or ConstructorInitializerSyntax or PrimaryConstructorBaseTypeSyntax))
                continue;
            if (arguments is null || arguments.Span.Contains(node.Span) ||
                !SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(ancestor, cancellationToken).Symbol?.OriginalDefinition, method.OriginalDefinition))
                return null;
            return arguments;
        }
        return null;
    }

    /// <summary>Whether <paramref name="node"/> names the method inside <c>nameof</c>, which a parameter does not change.</summary>
    private static bool IsNameOf(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken) =>
        node.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation =>
            invocation.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" } &&
            model.GetOperation(invocation, cancellationToken) is INameOfOperation);

    /// <summary>Fails when the method, less the parameters taken out, would take the parameters of another of its type's
    /// members of its name, which then clash or make calls ambiguous.</summary>
    private void RejectClash(IMethodSymbol method, IParameterSymbol parameter, string name)
    {
        var left = method.Parameters.Where(other => !SymbolEqualityComparer.Default.Equals(other, parameter) && !_taken.Contains(other)).ToList();
        IEnumerable<ISymbol> siblings = method.MethodKind == MethodKind.Constructor
            ? method.ContainingType.InstanceConstructors
            : method.ContainingType.GetMembers(method.Name);
        foreach (IMethodSymbol sibling in siblings.OfType<IMethodSymbol>())
        {
            if (SymbolEqualityComparer.Default.Equals(sibling, method) || sibling.Parameters.Length != left.Count)
                continue;
            if (sibling.Parameters.Zip(left, (first, second) => first.RefKind == second.RefKind &&
                    SymbolEqualityComparer.Default.Equals(first.Type, second.Type)).All(same => same))
                throw new CleanupFailure(_site, $"{name}, left unread, cannot go: {method.Name} would then take the parameters of {sibling.ToDisplayString()}");
        }
    }

    /// <summary>Whether the method implements a member of an interface its type implements.</summary>
    private static bool ImplementsInterface(IMethodSymbol method) =>
        method.ContainingType.AllInterfaces.SelectMany(contract => contract.GetMembers()).Any(member =>
            SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(member), method));

    /// <summary>Takes out the <c>param</c> element documenting <paramref name="parameter"/> in the documentation of
    /// <paramref name="owner"/>, with its lines, which must hold nothing else.</summary>
    private async Task RemoveDocumentationAsync(SyntaxNode owner, string parameter, CancellationToken cancellationToken)
    {
        SyntaxNode documented = owner;
        foreach (DocumentationCommentTriviaSyntax comment in documented.GetLeadingTrivia()
                     .Select(trivia => trivia.GetStructure()).OfType<DocumentationCommentTriviaSyntax>())
        {
            foreach (XmlElementSyntax element in comment.Content.OfType<XmlElementSyntax>())
            {
                if (element.StartTag.Name.ToString() != "param" ||
                    !element.StartTag.Attributes.OfType<XmlNameAttributeSyntax>().Any(attribute => attribute.Identifier.Identifier.ValueText == parameter))
                    continue;
                SourceEdits edits = await _edits.ForAsync(documented.SyntaxTree, cancellationToken).ConfigureAwait(false);
                SourceText text = edits.Text;
                TextLine first = text.Lines.GetLineFromPosition(element.SpanStart);
                TextLine last = text.Lines.GetLineFromPosition(element.Span.End);
                string before = text.ToString(TextSpan.FromBounds(first.Start, element.SpanStart)).Trim();
                string after = text.ToString(TextSpan.FromBounds(element.Span.End, last.End)).Trim();
                if (before != "///" || after.Length > 0)
                    throw new CleanupFailure(element.GetLocation(), _site.Id, $"the documentation of '{parameter}', left unread, shares its lines with other text");
                edits.Replace(TextSpan.FromBounds(first.Start, last.EndIncludingLineBreak), string.Empty);
            }
        }
    }

    /// <summary>Records that the item at <paramref name="index"/> of a parameter or argument list goes.</summary>
    private void Take(SyntaxNode list, int index)
    {
        if (!_listItems.TryGetValue(list, out SortedSet<int>? items))
            _listItems[list] = items = [];
        items.Add(index);
    }

    /// <summary>The items of a parameter or argument list.</summary>
    private static IReadOnlyList<SyntaxNode> Items(SyntaxNode list) => list switch
    {
        BaseParameterListSyntax parameters => parameters.Parameters,
        BaseArgumentListSyntax arguments => arguments.Arguments,
        _ => throw new InvalidOperationException($"{list.Kind()} is not a parameter or argument list."),
    };

    /// <summary>
    /// The spans that take the given items out of a separated list, with their separators: a run of taken items followed by
    /// a kept one goes up to that item; a run that ends the list goes from the end of the kept item before it; every item
    /// going leaves the list empty.
    /// </summary>
    private static IEnumerable<TextSpan> ItemRemovals(IReadOnlyList<SyntaxNode> items, SortedSet<int> taken)
    {
        if (taken.Count == items.Count)
        {
            yield return TextSpan.FromBounds(items[0].SpanStart, items[items.Count - 1].Span.End);
            yield break;
        }
        int index = 0;
        while (index < items.Count)
        {
            if (!taken.Contains(index))
            {
                index++;
                continue;
            }
            int end = index;
            while (end + 1 < items.Count && taken.Contains(end + 1))
                end++;
            yield return end + 1 < items.Count
                ? TextSpan.FromBounds(items[index].SpanStart, items[end + 1].SpanStart)
                : TextSpan.FromBounds(items[index - 1].Span.End, items[end].Span.End);
            index = end + 1;
        }
    }

    /// <summary>Every reference to <paramref name="symbol"/> in the solution.</summary>
    private async Task<List<ReferenceLocation>> ReferencesAsync(ISymbol symbol, CancellationToken cancellationToken) =>
        [.. (await SymbolFinder.FindReferencesAsync(symbol, _solution, cancellationToken).ConfigureAwait(false)).SelectMany(found => found.Locations)];

    /// <summary>Whether a location lies in code the fix removes.</summary>
    private bool InRemoved(Location location) =>
        location.SourceTree is { } tree && _removed.TryGetValue(tree, out List<TextSpan>? spans) && spans.Any(span => span.Contains(location.SourceSpan));

    /// <summary>Whether a location lies in documentation.</summary>
    private static bool InDocumentation(Location location) =>
        location.SourceTree?.GetRoot().FindToken(location.SourceSpan.Start, findInsideTrivia: true).Parent?.IsPartOfStructuredTrivia() == true;

    /// <summary>Whether a location is the name of a <c>param</c> element, which goes with the parameter it documents.</summary>
    private static bool InParameterDocumentation(Location location) =>
        location.SourceTree?.GetRoot().FindToken(location.SourceSpan.Start, findInsideTrivia: true).Parent?
            .AncestorsAndSelf().OfType<XmlNameAttributeSyntax>().FirstOrDefault() is { Parent: XmlElementStartTagSyntax { Name: var tag } } &&
        tag.ToString() == "param";

    /// <summary>Whether <paramref name="node"/> declares <paramref name="parameter"/>.</summary>
    private static bool Declares(SyntaxNode node, IParameterSymbol parameter) =>
        parameter.DeclaringSyntaxReferences.Any(reference => reference.SyntaxTree == node.SyntaxTree && node.Span.Contains(reference.Span));
}
