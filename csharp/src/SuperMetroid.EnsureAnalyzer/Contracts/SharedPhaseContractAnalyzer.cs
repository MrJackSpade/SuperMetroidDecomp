using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Shared phases iterate typed contracts, never untyped unions, type-keyed lookups or named species (SME6473, #2571). In
/// <c>SuperMetroid.Core</c> the rule reports three shapes:
/// <list type="bullet">
/// <item>a value typed <c>System.Enum</c>: a field, property, parameter, result, local, cast, pattern or type argument
/// (<c>Enum?</c>, <c>Enum[]</c>, <c>IReadOnlyList&lt;Enum&gt;</c>, <c>Cast&lt;Enum&gt;()</c>), an untyped union of
/// several enums its readers recover by type; a generic constraint (<c>where T : struct, Enum</c>), a call of one of
/// <c>Enum</c>'s static members and a <c>typeof</c> or <c>nameof</c> are not values and are left alone;</item>
/// <item>a dictionary keyed by <c>System.Type</c> (<c>Dictionary</c>, <c>IDictionary</c>, <c>IReadOnlyDictionary</c>,
/// <c>ConcurrentDictionary</c>, <c>ImmutableDictionary</c>, <c>FrozenDictionary</c>): a service lookup by runtime type;</item>
/// <item>a species owner (a class implementing <c>IEnemySpecies</c> or <c>IEnemyEntityFactory</c>) named outside the
/// species' own namespaces, the namespaces other than the enemy domain's root that declare an owner, and their
/// sub-namespaces: a shared phase or room system that reaches a species by name. The one type marked
/// <c>[SpeciesComposition]</c>, the composition that builds and registers the owners, may name them; it may hand none out
/// through a member that is not private, and no second type may carry the mark.</item>
/// </list>
/// </summary>
/// <remarks>
/// What replaces each: one typed value per family (a closed hierarchy whose members carry their behaviour), each consumer
/// handed its own typed collaborator at composition, and a shared contract the phase iterates, which the composition
/// registers each owner under.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SharedPhaseContractAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An untyped enum, a type-keyed lookup or a species named outside its own namespaces.</summary>
    public const string SharedPhaseContractId = "SME6473";

    /// <summary>The enemy domain's root namespace, which shares the species' contracts and owns no species.</summary>
    private const string EnemyDomainNamespace = CoreLayerAnalyzer.CoreAssembly + ".Enemies";

    /// <summary>The contracts a species owner implements.</summary>
    private static readonly ImmutableArray<string> OwnerContracts =
        [EnemyDomainNamespace + ".IEnemySpecies", EnemyDomainNamespace + ".IEnemyEntityFactory"];

    /// <summary>The attribute that marks the composition of a room's species.</summary>
    private const string CompositionAttribute = CoreLayerAnalyzer.CoreAssembly + ".SpeciesCompositionAttribute";

    /// <summary>The dictionary types whose key a type-keyed lookup is.</summary>
    private static readonly ImmutableHashSet<string> DictionaryTypes = ImmutableHashSet.Create(StringComparer.Ordinal,
        "System.Collections.Generic.Dictionary`2", "System.Collections.Generic.IDictionary`2",
        "System.Collections.Generic.IReadOnlyDictionary`2", "System.Collections.Concurrent.ConcurrentDictionary`2",
        "System.Collections.Immutable.ImmutableDictionary`2", "System.Collections.Immutable.IImmutableDictionary`2",
        "System.Collections.Frozen.FrozenDictionary`2");

    private static readonly DiagnosticDescriptor Rule = new(
        SharedPhaseContractId,
        "Shared phase without a typed contract",
        "{0}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Behaviour is typed per entity and shared phases call a shared contract (#2571): a System.Enum value is an " +
            "untyped union its readers recover by type, a dictionary keyed by Type is a service lookup by runtime type, and a " +
            "species owner named outside its own namespaces is a shared phase reaching a species by name. Make the value one " +
            "typed family, hand each consumer its typed collaborator at composition, and register the owner under a contract " +
            "the phase iterates.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            INamedTypeSymbol? enumType = start.Compilation.GetSpecialType(SpecialType.System_Enum);
            INamedTypeSymbol? typeType = start.Compilation.GetTypeByMetadataName("System.Type");
            ImmutableArray<INamedTypeSymbol> contracts = [.. OwnerContracts
                .Select(start.Compilation.GetTypeByMetadataName).OfType<INamedTypeSymbol>()];
            ImmutableHashSet<string> speciesNamespaces = SpeciesNamespaces(start.Compilation.Assembly.GlobalNamespace, contracts);
            start.RegisterSyntaxNodeAction(node => AnalyzeName(node, enumType, typeType, contracts, speciesNamespaces),
                SyntaxKind.IdentifierName, SyntaxKind.GenericName);
            start.RegisterSymbolAction(symbol => AnalyzeComposition(symbol, contracts), SymbolKind.NamedType);
            start.RegisterCompilationEndAction(ReportSecondComposition);
        });
    }

    /// <summary>Reports a name that binds to an untyped enum value, a type-keyed dictionary or a species owner named outside its namespaces.</summary>
    private static void AnalyzeName(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumType, INamedTypeSymbol? typeType,
        ImmutableArray<INamedTypeSymbol> contracts, ImmutableHashSet<string> speciesNamespaces)
    {
        var name = (SimpleNameSyntax)context.Node;
        // A qualified name is judged once, at its rightmost part.
        SyntaxNode outer = name.Parent is QualifiedNameSyntax qualified && qualified.Right == name ? qualified : name;
        if (outer.Parent is QualifiedNameSyntax left && left.Left == outer)
            return;
        if (context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol is not { } symbol)
            return;
        if (symbol is INamedTypeSymbol type)
        {
            if (SymbolEqualityComparer.Default.Equals(type, enumType))
            {
                if (IsValuePosition(outer))
                    context.ReportDiagnostic(Diagnostic.Create(Rule, outer.GetLocation(),
                        "A value typed System.Enum is an untyped union of enums; make it one typed family whose members carry their behaviour"));
                return;
            }
            if (typeType is not null && IsTypeKeyedDictionary(type, typeType))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, outer.GetLocation(),
                    $"'{type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' looks a value up by its runtime type; hand each consumer its own typed collaborator at composition"));
                return;
            }
        }
        INamedTypeSymbol? owner = symbol switch
        {
            INamedTypeSymbol named => named,
            IMethodSymbol { MethodKind: MethodKind.Constructor } constructor => constructor.ContainingType,
            IFieldSymbol or IPropertySymbol or IMethodSymbol or IEventSymbol => symbol.ContainingType,
            _ => null,
        };
        if (owner is null || !IsSpeciesOwner(owner, contracts) || InNamespaceOf(context.ContainingSymbol, speciesNamespaces) ||
            InComposition(context.ContainingSymbol))
        {
            return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, outer.GetLocation(),
            $"'{owner.Name}' is a species named outside its own namespaces; register it under a shared contract the phase iterates, in the species composition"));
    }

    /// <summary>
    /// Reports a member of the species composition, other than a private one, that hands a species owner out: its type, or
    /// a type argument of its type, is an owner.
    /// </summary>
    private static void AnalyzeComposition(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol> contracts)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (!IsComposition(type))
            return;
        foreach (ISymbol member in type.GetMembers())
        {
            if (member.DeclaredAccessibility == Accessibility.Private || member.IsImplicitlyDeclared)
                continue;
            ITypeSymbol? handed = member switch
            {
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                IMethodSymbol { MethodKind: MethodKind.Ordinary } method => method.ReturnType,
                _ => null,
            };
            if (handed is not null && HandsOwnerOut(handed, contracts))
                context.ReportDiagnostic(Diagnostic.Create(Rule, member.Locations[0],
                    $"The species composition hands species owner '{member.Name}' out; hand out the shared contract it is registered under"));
        }
    }

    /// <summary>Reports every type marked as a species composition when more than one is.</summary>
    private static void ReportSecondComposition(CompilationAnalysisContext context)
    {
        List<INamedTypeSymbol> compositions = [];
        Collect(context.Compilation.Assembly.GlobalNamespace, compositions);
        if (compositions.Count < 2)
            return;
        foreach (INamedTypeSymbol composition in compositions)
            context.ReportDiagnostic(Diagnostic.Create(Rule, composition.Locations[0],
                $"'{composition.Name}' is one of {compositions.Count} species compositions; a room's species have one composition"));

        static void Collect(INamespaceSymbol ns, List<INamedTypeSymbol> found)
        {
            foreach (INamespaceOrTypeSymbol member in ns.GetMembers())
            {
                if (member is INamespaceSymbol child)
                    Collect(child, found);
                else if (member is INamedTypeSymbol type && IsComposition(type))
                    found.Add(type);
            }
        }
    }

    /// <summary>Whether <paramref name="type"/> is, or holds as a type argument, a species owner.</summary>
    private static bool HandsOwnerOut(ITypeSymbol type, ImmutableArray<INamedTypeSymbol> contracts) => type switch
    {
        INamedTypeSymbol named => IsSpeciesOwner(named, contracts) || named.TypeArguments.Any(argument => HandsOwnerOut(argument, contracts)),
        IArrayTypeSymbol array => HandsOwnerOut(array.ElementType, contracts),
        _ => false,
    };

    /// <summary>
    /// Whether the name stands for a value of the type: anything but the receiver of a static member access, a generic
    /// constraint, a <c>typeof</c> or a <c>nameof</c>.
    /// </summary>
    private static bool IsValuePosition(SyntaxNode name)
    {
        if (name.Parent is MemberAccessExpressionSyntax access &&
            (access.Expression == name || access.Parent is MemberAccessExpressionSyntax { Expression: var receiver } && receiver == access))
        {
            return false;
        }
        foreach (SyntaxNode ancestor in name.Ancestors())
        {
            if (ancestor is TypeParameterConstraintClauseSyntax or TypeOfExpressionSyntax
                or InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } })
            {
                return false;
            }
            if (ancestor is StatementSyntax or MemberDeclarationSyntax)
                return true;
        }
        return true;
    }

    /// <summary>Whether <paramref name="type"/> is one of the dictionaries keyed by <paramref name="typeType"/>.</summary>
    private static bool IsTypeKeyedDictionary(INamedTypeSymbol type, INamedTypeSymbol typeType) =>
        type.IsGenericType && type.TypeArguments.Length == 2 &&
        SymbolEqualityComparer.Default.Equals(type.TypeArguments[0], typeType) &&
        DictionaryTypes.Contains(MetadataName(type.OriginalDefinition));

    /// <summary>The namespace-qualified metadata name of <paramref name="type"/>.</summary>
    private static string MetadataName(INamedTypeSymbol type) =>
        type.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() + "." + type.MetadataName : type.MetadataName;

    /// <summary>Whether <paramref name="type"/> is a class implementing one of the species owners' contracts.</summary>
    private static bool IsSpeciesOwner(INamedTypeSymbol type, ImmutableArray<INamedTypeSymbol> contracts) =>
        type.TypeKind == TypeKind.Class &&
        type.AllInterfaces.Any(implemented => contracts.Contains(implemented.OriginalDefinition, SymbolEqualityComparer.Default));

    /// <summary>
    /// The namespaces that declare a species owner, other than the enemy domain's root: a species' own namespaces, under
    /// which its parts may name it and each other.
    /// </summary>
    private static ImmutableHashSet<string> SpeciesNamespaces(INamespaceSymbol global, ImmutableArray<INamedTypeSymbol> contracts)
    {
        var namespaces = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
        Visit(global);
        return namespaces.ToImmutable();

        void Visit(INamespaceSymbol ns)
        {
            foreach (INamespaceOrTypeSymbol member in ns.GetMembers())
            {
                if (member is INamespaceSymbol child)
                    Visit(child);
                else if (member is INamedTypeSymbol type && IsSpeciesOwner(type, contracts))
                {
                    string name = ns.ToDisplayString();
                    if (name != EnemyDomainNamespace && name.StartsWith(CoreLayerAnalyzer.CoreAssembly + ".", StringComparison.Ordinal))
                        namespaces.Add(name);
                }
            }
        }
    }

    /// <summary>Whether <paramref name="symbol"/> is declared in one of <paramref name="speciesNamespaces"/> or under one.</summary>
    private static bool InNamespaceOf(ISymbol? symbol, ImmutableHashSet<string> speciesNamespaces)
    {
        if (symbol?.ContainingNamespace is not { } ns)
            return false;
        string name = ns.ToDisplayString();
        return speciesNamespaces.Any(species => name == species || name.StartsWith(species + ".", StringComparison.Ordinal));
    }

    /// <summary>Whether <paramref name="symbol"/> is declared in the species composition or a type nested in it.</summary>
    private static bool InComposition(ISymbol? symbol)
    {
        for (INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol?.ContainingType; type is not null; type = type.ContainingType)
        {
            if (IsComposition(type))
                return true;
        }
        return false;
    }

    /// <summary>Whether <paramref name="type"/> carries the species-composition mark.</summary>
    private static bool IsComposition(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == CompositionAttribute);
}
