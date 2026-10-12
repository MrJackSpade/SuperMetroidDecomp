using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>How a declaration is reported; enum members and fields have their own categories.</summary>
internal enum DeclarationShape
{
    /// <summary>Identifies type.</summary>
    Type,
    /// <summary>Identifies member.</summary>
    Member,
    /// <summary>Identifies field.</summary>
    Field,
    /// <summary>Identifies enum member.</summary>
    EnumMember,
}

/// <summary>One repository declaration and every project that compiles it.</summary>
internal sealed record Declaration(string Key, string Kind, string Display, string File, int Line,
    string ContainingTypeKey, DeclarationShape Shape, HashSet<string> Projects);

/// <summary>Records every type and member declared in repository source.</summary>
internal sealed class DeclarationCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Gets declarations.</summary>
    public Dictionary<string, Declaration> Declarations { get; } = [];

    /// <summary>Collect.</summary>
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
                var location = symbol.Locations.First(l => l.IsInSource && identity.IsRepositorySource(l.SourceTree!));
                Declarations[key] = new Declaration(key, Kind(symbol),
                    symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                    identity.Relative(location.SourceTree!.FilePath), location.GetLineSpan().StartLinePosition.Line + 1,
                    identity.Key(symbol.ContainingType) ?? "", Shape(symbol), [project.Name]);
                // A reachable member keeps its containing type reachable.
                graph.Edge(key, identity.Key(symbol.ContainingType));
            }
        }
    }

    /// <summary>Declared symbol.</summary>
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

    /// <summary>Shape.</summary>
    private static DeclarationShape Shape(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol => DeclarationShape.Type,
        IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } => DeclarationShape.EnumMember,
        IFieldSymbol => DeclarationShape.Field,
        _ => DeclarationShape.Member,
    };

    /// <summary>Kind.</summary>
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
