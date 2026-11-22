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

    /// <summary>
    /// The declared types reached that an object of another runtime type can be of (a class others derive from, an interface,
    /// an array), each with its test and the catalogs reaching it.
    /// </summary>
    private readonly List<(Func<int, object, bool> Probe, int Index, bool Enumerates, ulong[] Reaching)> declared = [];

    /// <summary>
    /// The catalogs reaching each declared type an object is of only when it has exactly that runtime type (a value type, a
    /// sealed class), by the type: such an object is matched by its runtime type, never tested.
    /// </summary>
    private readonly Dictionary<Type, ulong[]> exactly = [];

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
        // Each declared type is numbered once, with what it holds as numbers, so each catalog's walk compares numbers.
        var numbers = new Dictionary<string, int>(StringComparer.Ordinal);
        var keys = new List<string>();
        int Number(string key)
        {
            if (numbers.TryGetValue(key, out int number))
                return number;
            numbers.Add(key, keys.Count);
            keys.Add(key);
            return keys.Count - 1;
        }
        var held = new List<int[]?>();
        var anything = new List<bool>();
        int[] Held(int number)
        {
            while (held.Count <= number)
            {
                held.Add(null);
                anything.Add(false);
            }
            if (held[number] is { } known)
                return known;
            if (!types.DeclaredHolds.TryGetValue(keys[number], out (bool Anything, IReadOnlyList<string> Held) holds))
                throw new InvalidOperationException($"The definition catalogs reach the declared type {keys[number]}, but no registration says what it holds.");
            int[] next = [.. holds.Held.Select(Number)];
            while (held.Count < keys.Count)
            {
                held.Add(null);
                anything.Add(false);
            }
            held[number] = next;
            anything[number] = holds.Anything;
            return next;
        }
        var reaching = new List<ulong[]?>();
        var visited = new List<int>();
        var pending = new Stack<int>();
        for (int catalog = 0; catalog < catalogs.Count; catalog++)
        {
            pending.Clear();
            foreach (Core.StateGraphCatalogMember member in catalogs[catalog].Members)
                pending.Push(Number(member.Declared));
            while (pending.Count > 0)
            {
                int number = pending.Pop();
                while (visited.Count < keys.Count)
                {
                    visited.Add(0);
                    reaching.Add(null);
                }
                if (visited[number] == catalog + 1)
                    continue;
                visited[number] = catalog + 1;
                int[] next = Held(number);
                (reaching[number] ??= new ulong[words])[catalog / WordBits] |= 1UL << (catalog % WordBits);
                if (anything[number])
                    universal[catalog / WordBits] |= 1UL << (catalog % WordBits);
                foreach (int further in next)
                    pending.Push(further);
            }
        }
        for (int number = 0; number < reaching.Count; number++)
        {
            if (reaching[number] is not { } set ||
                !types.DeclaredProbes.TryGetValue(keys[number], out (Func<int, object, bool> Probe, int Index, Type? Exact, bool Enumerates) test))
                continue;
            if (test.Exact is null)
            {
                declared.Add((test.Probe, test.Index, test.Enumerates, set));
                continue;
            }
            // A runtime type two declared types are exactly (a type and its nullable form) is reached by both's catalogs.
            if (!exactly.TryGetValue(test.Exact, out ulong[]? merged))
                exactly.Add(test.Exact, merged = new ulong[words]);
            for (int word = 0; word < words; word++)
                merged[word] |= set[word];
        }
    }

    /// <summary>
    /// The catalogs that may hold an object of <paramref name="value"/>'s type where a game can reach it, in catalog order:
    /// those reaching a declared type the object is of, and those reaching a location that can hold anything. A bare
    /// <see cref="object"/> holds nothing a state could name it by, only its identity as a lock, so no catalog is searched
    /// for one: a state holds it as a new object.
    /// </summary>
    internal IReadOnlyList<DefinitionCatalog> Candidates(object value)
    {
        Type type = value.GetType();
        if (type == typeof(object))
            return [];
        if (found.TryGetValue(type, out DefinitionCatalog[]? known))
            return known;
        ulong[] set = [.. universal];
        if (exactly.TryGetValue(type, out ulong[]? exact))
        {
            for (int word = 0; word < set.Length; word++)
                set[word] |= exact[word];
        }
        bool enumerates = value is System.Collections.IEnumerable;
        foreach ((Func<int, object, bool> probe, int index, bool onlyEnumerating, ulong[] reaching) in declared)
        {
            // A declared type whose catalogs are all candidates already adds none, and one only an object that enumerates can
            // be of holds no other, so whether the object is of either is not asked.
            if ((onlyEnumerating && !enumerates) || Covers(set, reaching) || !probe(index, value))
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

    /// <summary>Whether every catalog in <paramref name="catalogs"/> is in <paramref name="set"/>.</summary>
    private static bool Covers(ulong[] set, ulong[] catalogs)
    {
        for (int word = 0; word < set.Length; word++)
        {
            if ((catalogs[word] & ~set[word]) != 0)
                return false;
        }
        return true;
    }
}
