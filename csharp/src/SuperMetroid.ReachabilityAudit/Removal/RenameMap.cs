using Microsoft.CodeAnalysis;
using SuperMetroid.ReachabilityAudit.Declarations;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>The new name of each underscore-prefixed member that becomes internal (<see cref="PromotedNames"/>), by repository key.</summary>
/// <param name="names">Each renamed member's new name, by its repository key.</param>
/// <param name="identity">Gives each symbol its repository key.</param>
internal sealed class RenameMap(IReadOnlyDictionary<string, string> names, SymbolIdentity identity)
{
    /// <summary>How many members are renamed.</summary>
    public int Count => names.Count;

    /// <summary>The new name of a renamed field or property, or null for any other symbol.</summary>
    public string? NewName(ISymbol? symbol) => names.Count > 0 && symbol is IFieldSymbol or IPropertySymbol &&
        identity.Key(symbol) is { } key && names.TryGetValue(key, out string? name) ? name : null;
}
