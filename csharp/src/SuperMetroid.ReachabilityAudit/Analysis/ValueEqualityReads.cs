using Microsoft.CodeAnalysis;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// The values a value's synthesized equality, hashing and formatting read. A record's equality, hash and text read
/// every instance value it declares and have no source declaration to walk, so comparing, hashing or formatting a
/// value reaches the values of every record that can stand behind the compared type:
/// <list type="bullet">
/// <item>a record: its own values and, since equality is virtual, those of every record deriving from it in any project
/// (a closed hierarchy used as a dictionary key tells its cases apart only by what each derived record adds);</item>
/// <item>an interface: every record implementing it, which is what a key or a comparison typed by it compares;</item>
/// <item>a tuple, an anonymous type, <see cref="Nullable{T}"/> and <see cref="KeyValuePair{TKey, TValue}"/>: the
/// values they hold, which their own equality compares element by element;</item>
/// <item>and, structurally, each of those values' types in turn, however deep.</item>
/// </list>
/// A value typed <see cref="object"/> names no type, so it reaches nothing here.
/// </summary>
internal sealed class ValueEqualityReads
{
    /// <summary>Per base record's documentation ID, the records that derive directly from it.</summary>
    private readonly Dictionary<string, List<INamedTypeSymbol>> _derivedRecords = new(StringComparer.Ordinal);

    /// <summary>Per interface's documentation ID, the records implementing it, directly or through what they derive from.</summary>
    private readonly Dictionary<string, List<INamedTypeSymbol>> _implementingRecords = new(StringComparer.Ordinal);

    /// <summary>
    /// Indexes the records every compilation declares. A derived record or an implementation may be declared in another
    /// project than the record or interface it derives from or implements, so every project's records are indexed, by
    /// documentation ID, which names a type alike in every compilation.
    /// </summary>
    public ValueEqualityReads(IEnumerable<Compilation> compilations)
    {
        foreach (Compilation compilation in compilations)
            foreach (INamedTypeSymbol record in TypesIn(compilation.Assembly.GlobalNamespace).Where(type => type.IsRecord))
            {
                if (record.BaseType is { IsRecord: true } baseRecord)
                    Index(_derivedRecords, baseRecord, record);
                foreach (INamedTypeSymbol implemented in record.AllInterfaces)
                    Index(_implementingRecords, implemented, record);
            }
    }

    /// <summary>Adds to <paramref name="targets"/> every value that comparing, hashing or formatting a <paramref name="type"/> reads.</summary>
    public void Add(ITypeSymbol? type, List<ISymbol?> targets) =>
        Add(type, targets, new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default));

    /// <summary>Adds the values <paramref name="type"/>'s equality reads, each type once, so a recursive record ends.</summary>
    private void Add(ITypeSymbol? type, List<ISymbol?> targets, HashSet<ITypeSymbol> visited)
    {
        if (type is not INamedTypeSymbol named || !visited.Add(named))
            return;
        if (named.IsRecord)
        {
            foreach (ISymbol member in named.GetMembers())
                if (member is IPropertySymbol { IsStatic: false } or IFieldSymbol { IsStatic: false, IsImplicitlyDeclared: false })
                {
                    targets.Add(member);
                    // Value equality is structural: a value compares what it holds.
                    Add(member is IPropertySymbol property ? property.Type : ((IFieldSymbol)member).Type, targets, visited);
                }
            foreach (INamedTypeSymbol derived in Recorded(_derivedRecords, named))
                Add(derived, targets, visited);
        }
        else if (named.TypeKind == TypeKind.Interface)
        {
            foreach (INamedTypeSymbol implementing in Recorded(_implementingRecords, named))
                Add(implementing, targets, visited);
        }
        else if (named.IsTupleType)
        {
            foreach (IFieldSymbol element in named.TupleElements)
                Add(element.Type, targets, visited);
        }
        else if (named.IsAnonymousType)
        {
            foreach (IPropertySymbol property in named.GetMembers().OfType<IPropertySymbol>())
                Add(property.Type, targets, visited);
        }
        else if (named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T || IsKeyValuePair(named))
        {
            foreach (ITypeSymbol held in named.TypeArguments)
                Add(held, targets, visited);
        }
    }

    /// <summary>Whether <paramref name="type"/> is <see cref="KeyValuePair{TKey, TValue}"/>, which compares the key and value it holds.</summary>
    private static bool IsKeyValuePair(INamedTypeSymbol type) =>
        type.OriginalDefinition.GetDocumentationCommentId() == "T:System.Collections.Generic.KeyValuePair`2";

    /// <summary>Records <paramref name="record"/> under <paramref name="type"/>'s documentation ID.</summary>
    private static void Index(Dictionary<string, List<INamedTypeSymbol>> index, INamedTypeSymbol type, INamedTypeSymbol record)
    {
        if (type.OriginalDefinition.GetDocumentationCommentId() is not { } id)
            return;
        if (!index.TryGetValue(id, out List<INamedTypeSymbol>? records))
            index[id] = records = [];
        records.Add(record);
    }

    /// <summary>The records <paramref name="index"/> holds under <paramref name="type"/>'s documentation ID.</summary>
    private static List<INamedTypeSymbol> Recorded(Dictionary<string, List<INamedTypeSymbol>> index, INamedTypeSymbol type) =>
        type.OriginalDefinition.GetDocumentationCommentId() is { } id && index.TryGetValue(id, out List<INamedTypeSymbol>? records)
            ? records
            : [];

    /// <summary>Every type <paramref name="container"/> declares, nested types and nested namespaces' types included.</summary>
    private static IEnumerable<INamedTypeSymbol> TypesIn(INamespaceOrTypeSymbol container)
    {
        foreach (ISymbol member in container.GetMembers())
        {
            if (member is INamespaceSymbol inner)
            {
                foreach (INamedTypeSymbol type in TypesIn(inner))
                    yield return type;
            }
            else if (member is INamedTypeSymbol declared)
            {
                yield return declared;
                foreach (INamedTypeSymbol type in TypesIn(declared))
                    yield return type;
            }
        }
    }
}
