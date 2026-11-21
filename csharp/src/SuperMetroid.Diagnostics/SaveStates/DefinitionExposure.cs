namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// Which definition catalogs may hold an object where a game can reach it (#1716), from what the compiler read of the
/// catalogs' declarations (#2602): each declared type a catalog's members reach, what a location declared with it holds,
/// and a test of whether an object is of it. A catalog may hold an object when a declared type its members reach, closed
/// over what each holds, is one the object is of; a catalog reaching a location that can hold anything may hold every
/// object. Finding the catalogs that may hold an object runs no initializer.
/// </summary>
internal sealed class DefinitionExposure
{
    /// <summary>The bits one word of a catalog set holds.</summary>
    private const int WordBits = 64;

    /// <summary>The catalogs, in their prefixes' ordinal order.</summary>
    private readonly IReadOnlyList<DefinitionCatalog> catalogs;

    /// <summary>The declared types reached, each with its test and the catalogs reaching it.</summary>
    private readonly List<(Func<object, bool> Probe, ulong[] Reaching)> declared = [];

    /// <summary>The catalogs that may hold an object of every type.</summary>
    private readonly ulong[] universal;

    /// <summary>The catalogs found for each runtime type asked about.</summary>
    private readonly Dictionary<Type, DefinitionCatalog[]> found = [];

    /// <summary>Closes each catalog over what its members' declared types hold.</summary>
    /// <param name="types">The registry, holding what each declared type holds and the test of each.</param>
    /// <param name="catalogs">The catalogs, in their prefixes' ordinal order.</param>
    /// <exception cref="InvalidOperationException">A declared type a catalog reaches has no registration of what it holds.</exception>
    internal DefinitionExposure(StateGraphTypes types, IReadOnlyList<DefinitionCatalog> catalogs)
    {
        this.catalogs = catalogs;
        int words = (catalogs.Count + WordBits - 1) / WordBits;
        universal = new ulong[words];
        var reaching = new Dictionary<string, ulong[]>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        for (int catalog = 0; catalog < catalogs.Count; catalog++)
        {
            pending.Clear();
            visited.Clear();
            foreach (Core.StateGraphCatalogMember member in catalogs[catalog].Members)
                pending.Push(member.Declared);
            while (pending.Count > 0)
            {
                string key = pending.Pop();
                if (!visited.Add(key))
                    continue;
                if (!types.DeclaredHolds.TryGetValue(key, out (bool Anything, IReadOnlyList<string> Held) holds))
                    throw new InvalidOperationException($"The definition catalogs reach the declared type {key}, but no registration says what it holds.");
                if (!reaching.TryGetValue(key, out ulong[]? set))
                    reaching.Add(key, set = new ulong[words]);
                set[catalog / WordBits] |= 1UL << (catalog % WordBits);
                if (holds.Anything)
                    universal[catalog / WordBits] |= 1UL << (catalog % WordBits);
                foreach (string next in holds.Held)
                    pending.Push(next);
            }
        }
        foreach ((string key, ulong[] set) in reaching)
        {
            if (types.DeclaredProbes.TryGetValue(key, out Func<object, bool>? probe))
                declared.Add((probe, set));
        }
    }

    /// <summary>
    /// The catalogs that may hold an object of <paramref name="value"/>'s type where a game can reach it, in catalog order:
    /// those reaching a declared type the object is of, and those reaching a location that can hold anything.
    /// </summary>
    internal IReadOnlyList<DefinitionCatalog> Candidates(object value)
    {
        Type type = value.GetType();
        if (found.TryGetValue(type, out DefinitionCatalog[]? known))
            return known;
        ulong[] set = [.. universal];
        foreach ((Func<object, bool> probe, ulong[] reaching) in declared)
        {
            if (!probe(value))
                continue;
            for (int word = 0; word < set.Length; word++)
                set[word] |= reaching[word];
        }
        var candidates = new List<DefinitionCatalog>();
        for (int catalog = 0; catalog < catalogs.Count; catalog++)
            if ((set[catalog / WordBits] & (1UL << (catalog % WordBits))) != 0)
                candidates.Add(catalogs[catalog]);
        DefinitionCatalog[] result = [.. candidates];
        found.Add(type, result);
        return result;
    }
}
