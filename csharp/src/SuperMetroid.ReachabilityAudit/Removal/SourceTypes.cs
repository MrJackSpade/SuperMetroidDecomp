using Microsoft.CodeAnalysis;
using SuperMetroid.ReachabilityAudit.Analysis;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// The repository types the relocation's compilations declare, each once, and which of them derive from which: a member
/// that becomes internal is seen by every type deriving from its owner, wherever that type is declared.
/// </summary>
internal sealed class SourceTypes
{
    /// <summary>Gives each type its repository key.</summary>
    private readonly SymbolIdentity identity;

    /// <summary>The types deriving from each repository type, by that type's key.</summary>
    private readonly Dictionary<string, List<INamedTypeSymbol>> derived = new(StringComparer.Ordinal);

    /// <summary>Reads every repository type of <paramref name="compilations"/> once.</summary>
    /// <param name="identity">Gives each type its repository key.</param>
    /// <param name="compilations">The compilations whose types are read.</param>
    public SourceTypes(SymbolIdentity identity, IEnumerable<Compilation> compilations)
    {
        this.identity = identity;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in compilations.Distinct().SelectMany(compilation => RootCollector.AllTypes(compilation.Assembly.GlobalNamespace)))
        {
            if (identity.Key(type) is not { } key || !seen.Add(key))
                continue;
            foreach (var baseType in BaseTypes(type))
            {
                if (identity.Key(baseType) is not { } baseKey)
                    continue;
                if (!derived.TryGetValue(baseKey, out var list))
                    derived[baseKey] = list = [];
                list.Add(type);
            }
        }
    }

    /// <summary>The repository types whose base types include <paramref name="type"/>; the type itself is not among them.</summary>
    public IReadOnlyList<INamedTypeSymbol> DerivedFrom(INamedTypeSymbol type) =>
        identity.Key(type) is { } key && derived.TryGetValue(key, out var list) ? list : [];

    /// <summary>Whether <paramref name="type"/> is <paramref name="owner"/> or derives from it.</summary>
    public bool IsOrDerivesFrom(ITypeSymbol? type, INamedTypeSymbol owner)
    {
        if (type is not INamedTypeSymbol named || identity.Key(owner) is not { } ownerKey)
            return false;
        return identity.Key(named.OriginalDefinition) == ownerKey || BaseTypes(named).Any(baseType => identity.Key(baseType) == ownerKey);
    }

    /// <summary>A type's base types, nearest first, as their definitions.</summary>
    public static IEnumerable<INamedTypeSymbol> BaseTypes(INamedTypeSymbol type)
    {
        for (var baseType = type.BaseType; baseType is not null; baseType = baseType.BaseType)
            yield return baseType.OriginalDefinition;
    }
}
