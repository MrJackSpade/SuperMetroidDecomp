using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Analysis;

namespace SuperMetroid.ReachabilityAudit.Declarations;

/// <summary>Records every type and member declared in repository source.</summary>
internal sealed class DeclarationCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Collected declarations keyed by stable symbol identity across projects.</summary>
    public Dictionary<string, Declaration> Declarations { get; } = [];

    /// <summary>Records repository declarations from a compilation and merges project ownership for shared source.</summary>
    public void Collect(Project project, Compilation compilation)
    {
        foreach (var tree in compilation.SyntaxTrees.Where(identity.IsRepositorySource))
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var node in tree.GetRoot().DescendantNodes())
            {
                if (SymbolIdentity.Normalize(DeclaredSymbol(model, node)) is not { } symbol || identity.Key(symbol) is not { } key)
                    continue;
                if (Declarations.TryGetValue(key, out var existing))
                {
                    existing.Projects.Add(project.Name);
                    continue;
                }
                var location = symbol.Locations.First(l => l.IsInSource && identity.IsRepositorySource(l.SourceTree));
                Declarations[key] = new Declaration(key, Kind(symbol),
                    symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    identity.Relative((location.SourceTree ?? throw new InvalidOperationException(
                        $"{symbol.Name}: a repository source location has a syntax tree.")).FilePath), location.GetLineSpan().StartLinePosition.Line + 1,
                    identity.Key(symbol.ContainingType) ?? "", Shape(symbol), [project.Name]);
                // A reachable member keeps its containing type reachable.
                graph.Edge(key, identity.Key(symbol.ContainingType));
            }
        }
    }

    /// <summary>Finds declared type and member symbols, including positional-record properties.</summary>
    internal static ISymbol? DeclaredSymbol(SemanticModel model, SyntaxNode node) => node switch
    {
        BaseTypeDeclarationSyntax or DelegateDeclarationSyntax or BaseMethodDeclarationSyntax
            or PropertyDeclarationSyntax or IndexerDeclarationSyntax or EventDeclarationSyntax
            or EnumMemberDeclarationSyntax => model.GetDeclaredSymbol(node),
        VariableDeclaratorSyntax { Parent.Parent: BaseFieldDeclarationSyntax } variable => model.GetDeclaredSymbol(variable),
        // A positional record parameter declares the record's property.
        ParameterSyntax { Parent.Parent: RecordDeclarationSyntax record } parameter
            => (model.GetDeclaredSymbol(record) as INamedTypeSymbol)?.GetMembers(parameter.Identifier.ValueText)
                .OfType<IPropertySymbol>().FirstOrDefault(),
        _ => null,
    };

    /// <summary>Classifies a symbol for type, field, enum-member, or other-member reporting.</summary>
    private static DeclarationShape Shape(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol => DeclarationShape.Type,
        IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } => DeclarationShape.EnumMember,
        IFieldSymbol => DeclarationShape.Field,
        _ => DeclarationShape.Member,
    };

    /// <summary>Formats a declaration's language kind for the reachability report.</summary>
    private static string Kind(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
        INamedTypeSymbol { IsRecord: true } => "record",
        INamedTypeSymbol type => type.TypeKind.ToString().ToLowerInvariant(),
        IMethodSymbol { MethodKind: MethodKind.Constructor } => "constructor",
        IMethodSymbol { MethodKind: MethodKind.StaticConstructor } => "static constructor",
        IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator or MethodKind.Conversion } => "operator",
        IMethodSymbol => "method",
        IPropertySymbol { IsIndexer: true } => "indexer",
        IPropertySymbol => "property",
        IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } => "enum member",
        IFieldSymbol { IsConst: true } => "constant",
        IFieldSymbol => "field",
        IEventSymbol => "event",
        _ => symbol.Kind.ToString().ToLowerInvariant(),
    };
}
