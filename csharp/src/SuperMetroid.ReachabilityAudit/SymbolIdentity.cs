using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Stable keys for repository source symbols. A key combines the documentation-comment ID with
/// the declaring file, so a shared-source file compiled into several assemblies is one symbol.
/// </summary>
internal sealed class SymbolIdentity(string repositoryRoot)
{
    /// <summary>Suffix of the key that stands for a source type's compiler-generated constructor.</summary>
    public const string ImplicitConstructorSuffix = "#implicit-ctor";

    private static readonly string ObjSegment = $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}";
    private static readonly string BinSegment = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";

    /// <summary>Hand-written repository source; generated files under obj/ and bin/ are excluded.</summary>
    public bool IsRepositorySource(SyntaxTree tree)
    {
        string path = Path.GetFullPath(tree.FilePath);
        return path.StartsWith(repositoryRoot, StringComparison.OrdinalIgnoreCase)
            && !path.Contains(ObjSegment, StringComparison.OrdinalIgnoreCase)
            && !path.Contains(BinSegment, StringComparison.OrdinalIgnoreCase);
    }

    public string Relative(string path) => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    /// <summary>
    /// Maps a referenced symbol to the declaration that owns it: generic and reduced-extension
    /// forms to their definition, accessors and backing fields to their property or event.
    /// Locals, parameters, lambdas and local functions have no identity of their own.
    /// </summary>
    public static ISymbol? Normalize(ISymbol? symbol)
    {
        switch (symbol)
        {
            case IMethodSymbol method:
                method = (method.ReducedFrom ?? method).OriginalDefinition;
                method = method.PartialDefinitionPart ?? method;
                if (method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet or MethodKind.EventAdd
                        or MethodKind.EventRemove or MethodKind.EventRaise
                    && method.AssociatedSymbol is { } associated)
                    return Normalize(associated);
                return method.MethodKind is MethodKind.LocalFunction or MethodKind.AnonymousFunction ? null : method;
            case IPropertySymbol property:
                property = property.OriginalDefinition;
                return property.PartialDefinitionPart ?? property;
            case INamedTypeSymbol type:
                return type.OriginalDefinition;
            case IFieldSymbol field:
                field = field.OriginalDefinition;
                return field.AssociatedSymbol is IPropertySymbol or IEventSymbol ? Normalize(field.AssociatedSymbol) : field;
            case IEventSymbol @event:
                return @event.OriginalDefinition;
            default:
                return null;
        }
    }

    /// <summary>The symbol's key, or null when it is not declared in repository source.</summary>
    public string? Key(ISymbol? symbol)
    {
        symbol = Normalize(symbol);
        if (symbol is null)
            return null;
        // A compiler-generated constructor carries its type's location; it stands for "instantiated".
        if (symbol is IMethodSymbol { MethodKind: MethodKind.Constructor, IsImplicitlyDeclared: true } constructor)
            return Key(constructor.ContainingType) is { } typeKey ? typeKey + ImplicitConstructorSuffix : null;
        string? file = symbol.Locations.Where(l => l.IsInSource && IsRepositorySource(l.SourceTree!))
            .Select(l => Relative(l.SourceTree!.FilePath)).Order(StringComparer.Ordinal).FirstOrDefault();
        return file is not null && symbol.GetDocumentationCommentId() is { } id ? $"{id}@{file}" : null;
    }
}
