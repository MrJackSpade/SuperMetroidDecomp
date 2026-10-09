using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>How a declaration is reported; enum members and fields have their own categories.</summary>
internal enum DeclarationShape
{
    /// <summary>A named type or delegate.</summary>
    Type,
    /// <summary>A method, property, event, or other non-field member.</summary>
    Member,
    /// <summary>A field declared by a type.</summary>
    Field,
    /// <summary>An enum constant, tracked separately so implicit numeric values can be preserved.</summary>
    EnumMember,
}

/// <summary>One repository declaration and every project that compiles it.</summary>
/// <param name="Key">Stable symbol identity combining documentation ID and repository-relative file.</param>
/// <param name="Kind">Human-readable declaration kind used in reports.</param>
/// <param name="Display">C# display form of the declaration.</param>
/// <param name="File">Repository-relative source path.</param>
/// <param name="Line">One-based source line of the declaration.</param>
/// <param name="ContainingTypeKey">Stable key of the containing type, when present.</param>
/// <param name="Shape">Reporting category used to distinguish types, members, fields, and enum constants.</param>
/// <param name="Projects">Names of all solution projects that compile this source declaration.</param>
internal sealed record Declaration(string Key, string Kind, string Display, string File, int Line,
    string ContainingTypeKey, DeclarationShape Shape, HashSet<string> Projects);

/// <summary>Records every type and member declared in repository source.</summary>
/// <param name="identity">Provides source filtering, symbol normalization, and stable keys.</param>
/// <param name="graph">Receives containment edges so reachable members keep their type reachable.</param>
internal sealed class DeclarationCollector(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Declarations keyed by stable symbol identity, merged across projects that share source.</summary>
    public Dictionary<string, Declaration> Declarations { get; } = [];

    /// <summary>Adds declarations from repository source in one compiled project.</summary>
    /// <param name="project">Project whose compilation is being visited.</param>
    /// <param name="compilation">Compilation supplying syntax trees and bound symbols.</param>
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

    /// <summary>Gets the symbol represented by a supported declaration node, including positional record properties.</summary>
    /// <param name="model">Semantic model for the node's syntax tree.</param>
    /// <param name="node">Syntax node to interpret as a declaration.</param>
    /// <returns>The normalized declaration candidate, or null for syntax that is not tracked.</returns>
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

    /// <summary>Maps a Roslyn declaration symbol to its reporting shape.</summary>
    private static DeclarationShape Shape(ISymbol symbol) => symbol switch
    {
        INamedTypeSymbol => DeclarationShape.Type,
        IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } => DeclarationShape.EnumMember,
        IFieldSymbol => DeclarationShape.Field,
        _ => DeclarationShape.Member,
    };

    /// <summary>Returns the concise declaration-kind label written to finding reports.</summary>
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
