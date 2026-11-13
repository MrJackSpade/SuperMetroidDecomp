using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Analysis;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The names promoted fields and properties take. An underscore names a type's own private state (SME6354, #1328): other
/// types reach it through "a member without the underscore". The 2026-11-13 cleanup at 5eddba348d made
/// <c>RoomObjectSlotTable._places</c> internal for moved test code, and the build failed on SME6354. So a promoted
/// underscore-prefixed field or property is renamed to its name without the leading underscores, which SME6354 accepts
/// and which every rule that reads names as words (they split a name at its underscores) reads as before; the relocator
/// rewrites every reference, documentation crefs and <c>nameof</c> among them. The rename is refused, failing the run,
/// when it would not be a pure rename:
/// <list type="bullet">
/// <item>the name is not an identifier, or is a keyword;</item>
/// <item>the owner, a type containing it or one of their base types already has a member of that name, or the owner
/// or one of its type parameters is so named;</item>
/// <item>inside the owner, a type it contains or a type deriving from it, the name already stands for something other
/// than a local, parameter, range variable, label or local function, which the renamed member would take its place as;</item>
/// <item>code reaches a member of that name through a value of the owner's type or a type deriving from it (an extension
/// member, say), which would reach the renamed member instead;</item>
/// <item>a string literal spells the old name, as reflection or a serializer would read it, the member is serialized or
/// marked as reached by reflection, or code that the build does not compile (an inactive <c>#if</c> branch) names it.</item>
/// </list>
/// A type deriving from the owner that declares a member of the new name is refused by <see cref="PromotionRules"/>.
/// </summary>
internal static class PromotedNames
{
    /// <summary>The namespaces whose attributes say a member is serialized under its name.</summary>
    private static readonly HashSet<string> SerializationNamespaces = ["System.Text.Json.Serialization", "System.Runtime.Serialization"];

    /// <summary>
    /// The new name of each promoted underscore-prefixed field and property that declares an underscore name and is private
    /// or private protected (the accessibilities SME6354 accepts), by repository key.
    /// </summary>
    /// <param name="promoted">The staying members that become internal.</param>
    /// <param name="trees">Every repository tree, with its compilation.</param>
    /// <param name="types">The repository types and their derived types.</param>
    /// <param name="identity">Gives each member its repository key.</param>
    /// <exception cref="InvalidOperationException">A rename would not be a pure rename.</exception>
    public static IReadOnlyDictionary<string, string> Plan(IReadOnlyCollection<ISymbol> promoted, IReadOnlyList<ProjectTree> trees,
        SourceTypes types, SymbolIdentity identity)
    {
        var renamed = promoted.Where(symbol => symbol is IFieldSymbol or IPropertySymbol && symbol.Name.StartsWith('_') &&
                symbol.DeclaredAccessibility is Accessibility.Private or Accessibility.ProtectedAndInternal)
            .OrderBy(symbol => symbol.ToDisplayString(), StringComparer.Ordinal).ToList();
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        if (renamed.Count == 0)
            return names;
        foreach (var symbol in renamed)
        {
            string name = Unprefixed(symbol);
            RejectDeclaredName(symbol, name);
            RejectNamedUse(symbol);
            if (renamed.FirstOrDefault(other => !SymbolEqualityComparer.Default.Equals(other, symbol) &&
                    SymbolEqualityComparer.Default.Equals(other.ContainingType, symbol.ContainingType) && Unprefixed(other) == name) is { } twin)
                throw new InvalidOperationException($"{symbol.ToDisplayString()} and {twin.ToDisplayString()} would both become '{name}'.");
            names[identity.Key(symbol) ?? throw new InvalidOperationException($"{symbol.ToDisplayString()} has no repository identity.")] = name;
        }
        foreach (var (tree, compilation, _) in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            foreach (var symbol in renamed)
                RejectChangedReferences(symbol, Unprefixed(symbol), model, root, types);
        }
        return names;
    }

    /// <summary>The member's name without its leading underscores.</summary>
    /// <exception cref="InvalidOperationException">What is left is not an identifier, or is a keyword.</exception>
    private static string Unprefixed(ISymbol symbol)
    {
        string name = symbol.Name.TrimStart('_');
        return SyntaxFacts.IsValidIdentifier(name) && SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None ? name
            : throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, which must lose its underscore to " +
                $"become internal (SME6354), but '{name}' is not a name it can take.");
    }

    /// <summary>Fails when the owner, a type containing it or one of their base types already declares the new name.</summary>
    /// <exception cref="InvalidOperationException">The name is taken.</exception>
    private static void RejectDeclaredName(ISymbol symbol, string name)
    {
        var owner = symbol.ContainingType;
        if (owner.Name == name || owner.TypeParameters.Any(parameter => parameter.Name == name))
            throw Collision(symbol, name, owner.ToDisplayString());
        for (var type = owner; type is not null; type = type.ContainingType)
            foreach (var holder in SourceTypes.BaseTypes(type).Prepend(type))
                if (holder.GetMembers(name).FirstOrDefault() is { } taken)
                    throw Collision(symbol, name, taken.ToDisplayString());
    }

    /// <summary>Fails when the member is serialized under its name or marked as reached by reflection.</summary>
    /// <exception cref="InvalidOperationException">Something outside the source reads the member by its name.</exception>
    private static void RejectNamedUse(ISymbol symbol)
    {
        if (symbol.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass is { } type &&
                (type.Name == RootCollector.ReflectionMarker || SerializationNamespaces.Contains(type.ContainingNamespace.ToDisplayString()))) is { } named)
            throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, which must lose its underscore to become " +
                $"internal (SME6354), but [{named.AttributeClass?.Name}] says something reads it by its name.");
    }

    /// <summary>Fails when, in one tree, the rename would change what code refers to or miss what names the member.</summary>
    /// <exception cref="InvalidOperationException">The rename would not be a pure rename.</exception>
    private static void RejectChangedReferences(ISymbol symbol, string name, SemanticModel model, SyntaxNode root, SourceTypes types)
    {
        var owner = symbol.ContainingType;
        var inactive = root.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia) && ContainsWord(trivia.ToString(), symbol.Name)).ToList();
        if (inactive.Count > 0)
            throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, which must lose its underscore to become " +
                $"internal (SME6354), but code the build does not compile names it ({inactive[0].GetLocation().GetLineSpan()}).");
        if (root.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault(literal =>
                literal.IsKind(SyntaxKind.StringLiteralExpression) && literal.Token.ValueText == symbol.Name) is { } spelled)
            throw new InvalidOperationException($"Moved code uses {symbol.ToDisplayString()}, which must lose its underscore to become " +
                $"internal (SME6354), but {spelled} spells its name ({spelled.GetLocation().GetLineSpan()}), as code reaching it by name would.");
        foreach (var identifier in root.DescendantNodes(descendIntoTrivia: true).OfType<IdentifierNameSyntax>().Where(identifier => identifier.Identifier.ValueText == name))
        {
            if (identifier.Parent is MemberAccessExpressionSyntax access && access.Name == identifier ||
                identifier.Parent is MemberBindingExpressionSyntax)
            {
                if (Receiver(identifier, model) is { } receiver && types.IsOrDerivesFrom(receiver, owner))
                    throw Collision(symbol, name, $"{identifier.Parent} ({identifier.GetLocation().GetLineSpan()}), which reaches " +
                        $"{model.GetSymbolInfo(identifier).Symbol?.ToDisplayString() ?? "a member"} through a value of {receiver.ToDisplayString()}");
                continue;
            }
            if (identifier.Parent is QualifiedNameSyntax qualified && qualified.Right == identifier || InMemberLookup(identifier) ||
                !InsideOwnerOrDerived(identifier, model, owner, types))
                continue;
            var bound = model.GetSymbolInfo(identifier) is { Symbol: { } single } ? single : model.GetSymbolInfo(identifier).CandidateSymbols.FirstOrDefault();
            if (bound is null or ILocalSymbol or IParameterSymbol or IRangeVariableSymbol or ILabelSymbol or IMethodSymbol { MethodKind: MethodKind.LocalFunction })
                continue;
            throw Collision(symbol, name, $"{bound.ToDisplayString()}, which '{name}' stands for at {identifier.GetLocation().GetLineSpan()}");
        }
    }

    /// <summary>The type of the value a member access or a conditional member binding reaches its member through.</summary>
    private static ITypeSymbol? Receiver(IdentifierNameSyntax name, SemanticModel model)
    {
        var expression = name.Parent switch
        {
            MemberAccessExpressionSyntax access => access.Expression,
            MemberBindingExpressionSyntax binding => binding.Ancestors().OfType<ConditionalAccessExpressionSyntax>().First().Expression,
            _ => throw new InvalidOperationException($"{name.Parent} is not a member access."),
        };
        return model.GetSymbolInfo(expression).Symbol as INamedTypeSymbol ?? model.GetTypeInfo(expression).Type;
    }

    /// <summary>
    /// Whether a name is looked up among the members of another type rather than in scope: an object or <c>with</c>
    /// initializer's member, a property pattern's member, or a named argument.
    /// </summary>
    internal static bool InMemberLookup(SimpleNameSyntax name) => name.Parent switch
    {
        AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment => assignment.Left == name,
        NameColonSyntax or NameEqualsSyntax => true,
        _ => name.Ancestors().Any(ancestor => ancestor is ExpressionColonSyntax),
    };

    /// <summary>Whether a name is inside the owner, a type the owner contains or a type deriving from the owner.</summary>
    private static bool InsideOwnerOrDerived(SyntaxNode node, SemanticModel model, INamedTypeSymbol owner, SourceTypes types) =>
        node.Ancestors().OfType<BaseTypeDeclarationSyntax>().Any(declaration =>
            types.IsOrDerivesFrom(model.GetDeclaredSymbol(declaration), owner));

    /// <summary>Whether <paramref name="text"/> holds <paramref name="word"/> not as part of a longer identifier.</summary>
    private static bool ContainsWord(string text, string word)
    {
        for (int index = text.IndexOf(word, StringComparison.Ordinal); index >= 0; index = text.IndexOf(word, index + 1, StringComparison.Ordinal))
        {
            bool before = index > 0 && (char.IsLetterOrDigit(text[index - 1]) || text[index - 1] == '_');
            int end = index + word.Length;
            bool after = end < text.Length && (char.IsLetterOrDigit(text[end]) || text[end] == '_');
            if (!before && !after)
                return true;
        }
        return false;
    }

    /// <summary>The failure for a name the renamed member cannot take.</summary>
    private static InvalidOperationException Collision(ISymbol symbol, string name, string taken) =>
        new($"Moved code uses {symbol.ToDisplayString()}, which must lose its underscore to become internal (SME6354), but " +
            $"'{name}' already names {taken}. Rename one of them, and the next run moves the code.");
}
