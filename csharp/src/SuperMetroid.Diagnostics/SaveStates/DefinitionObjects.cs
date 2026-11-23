using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects the game's static members hold, and every object reachable from them, each addressed by its path from the
/// member it was first reached from (#1623): the catalogs' identities, their behaviour objects and their definition data.
/// A captured state names such an object by its path instead of holding a copy, and a restore resolves the path to the
/// object this build's member holds, so a restored state holds the catalog's own members.
/// </summary>
/// <remarks>
/// A root is a static read-only member of a type the game's assembly declares, holding an object other than a string,
/// which the compiler registers (#2602). Each declaring type is a <see cref="DefinitionCatalog"/>. One index holds the
/// objects of every catalog whose type initializer has run (<see cref="StateGraphCatalogInitializations"/>, #2619): an
/// object a catalog's members hold exists only once its initializer has run, so a catalog not yet initialized holds nothing
/// a game can reach. A lookup first indexes the catalogs initialized since the last one (<see cref="IndexInitialized"/>),
/// so the index never runs a catalog's initializer and walks each catalog once; initializing and walking every catalog
/// where a capture first needed them stalled the session for seconds. A catalog's registration is found through its type
/// when it is first indexed. A restore indexes the catalog its path names, initializing it. An object keeps the path it was
/// first indexed under, its shortest from the roots indexed with it.
/// </remarks>
/// <param name="types">The registry holding the catalogs.</param>
internal sealed class DefinitionObjects(StateGraphTypes types)
{
    /// <summary>The process's index, which every capture and restore consults unless handed another.</summary>
    internal static DefinitionObjects Process => process.Value;

    private static readonly Lazy<DefinitionObjects> process = new(static () => new DefinitionObjects(StateGraphTypes.Shared));

    /// <summary>Guards the index, whose paths and resolutions every capture and restore share.</summary>
    internal readonly Lock gate = new();

    /// <summary>The indexed catalogs' objects; none until a lookup first indexes the initialized catalogs.</summary>
    private BoundObjectGraph? index;

    /// <summary>The catalogs indexed so far.</summary>
    private readonly HashSet<DefinitionCatalog> indexed = [];

    /// <summary>How many of the initialized catalogs (<see cref="StateGraphCatalogInitializations.Initialized"/>) were indexed.</summary>
    private int initializedIndexed;

    /// <summary>Each registered catalog by its declaring type.</summary>
    private readonly Dictionary<Type, DefinitionCatalog> byType = [];

    /// <summary>Each registered catalog by the prefix its roots' names begin with.</summary>
    private readonly Dictionary<string, DefinitionCatalog> byPrefix = new(StringComparer.Ordinal);

    /// <summary>How many of the registry's catalogs are in <see cref="byType"/> and <see cref="byPrefix"/>.</summary>
    private int catalogsKnown;

    /// <summary>
    /// Indexes the catalogs initialized since the last lookup, as every lookup first does; a store calls it as the session
    /// loads, so its first capture walks only the catalogs initialized since.
    /// </summary>
    internal void IndexInitialized()
    {
        lock (gate)
            _ = Current();
    }

    /// <summary>
    /// Whether <paramref name="value"/> is held by a static member, and its path when it is, after indexing the catalogs
    /// initialized since the last lookup. A bare <see cref="object"/> holds nothing a state could name it by, only its
    /// identity as a lock, so it is never named: a state holds it as a new object.
    /// </summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        Ensure.NotNull(value);
        lock (gate)
        {
            BoundObjectGraph current = Current();
            if (value.GetType() == typeof(object))
            {
                path = null;
                return false;
            }
            return current.TryGetPath(value, out path);
        }
    }

    /// <summary>The object <paramref name="path"/> names among the static members' objects.</summary>
    /// <exception cref="InvalidDataException">The path names no object this build's static members hold.</exception>
    internal object Resolve(string path)
    {
        Ensure.NotNull(path);
        lock (gate)
        {
            int separator = path.IndexOf(DefinitionCatalog.MemberSeparator, StringComparison.Ordinal);
            if (separator < 0 || CatalogNamed(path[..(separator + DefinitionCatalog.MemberSeparator.Length)]) is not { } catalog)
                throw new InvalidDataException($"Debugger state names definition '{path}', which no type of this build declares as a static member.");
            BoundObjectGraph current = Current();
            Index(current, catalog);
            return current.Resolve(path);
        }
    }

    /// <summary>Every catalog of the game's static members, in their prefixes' ordinal order, registering every type first.</summary>
    internal IReadOnlyList<DefinitionCatalog> AllCatalogs()
    {
        lock (gate)
        {
            types.RegisterEverything();
            Learn();
            return [.. byPrefix.OrderBy(entry => entry.Key, StringComparer.Ordinal).Select(entry => entry.Value)];
        }
    }

    /// <summary>The index holding every initialized catalog's objects; callers hold the lock.</summary>
    private BoundObjectGraph Current()
    {
        BoundObjectGraph current = index ??= new BoundObjectGraph(types, []);
        IReadOnlyList<Type> initialized = StateGraphCatalogInitializations.Initialized;
        for (; initializedIndexed < initialized.Count; initializedIndexed++)
        {
            // A type marked here that this registry does not hold as a catalog holds nothing a state of it names.
            if (CatalogOf(initialized[initializedIndexed]) is { } catalog)
                Index(current, catalog);
        }
        return current;
    }

    /// <summary>Indexes <paramref name="catalog"/>'s objects unless they were; callers hold the lock.</summary>
    private void Index(BoundObjectGraph current, DefinitionCatalog catalog)
    {
        if (indexed.Add(catalog))
            current.Add(catalog.Roots());
    }

    /// <summary>The catalog <paramref name="type"/> declares, registering its declarations first; callers hold the lock.</summary>
    private DefinitionCatalog? CatalogOf(Type type)
    {
        if (byType.TryGetValue(type, out DefinitionCatalog? catalog))
            return catalog;
        types.RegisterDeclaring(type);
        Learn();
        return byType.GetValueOrDefault(type);
    }

    /// <summary>
    /// The catalog whose roots' names begin with <paramref name="prefix"/>, registering the types its name names, then every
    /// type, until it is found; callers hold the lock.
    /// </summary>
    private DefinitionCatalog? CatalogNamed(string prefix)
    {
        if (byPrefix.TryGetValue(prefix, out DefinitionCatalog? catalog))
            return catalog;
        types.RegisterNamed(prefix[..^DefinitionCatalog.MemberSeparator.Length]);
        Learn();
        if (byPrefix.TryGetValue(prefix, out catalog))
            return catalog;
        types.RegisterEverything();
        Learn();
        return byPrefix.GetValueOrDefault(prefix);
    }

    /// <summary>Takes in the catalogs registered since the last time; callers hold the lock.</summary>
    private void Learn()
    {
        IReadOnlyList<(Type Catalog, StateGraphName Type, IReadOnlyList<StateGraphCatalogMember> Members)> registered = types.CatalogsRegistered();
        for (; catalogsKnown < registered.Count; catalogsKnown++)
        {
            (Type declaring, StateGraphName name, IReadOnlyList<StateGraphCatalogMember> members) = registered[catalogsKnown];
            var catalog = new DefinitionCatalog(declaring, types.Format(name), members);
            byType.Add(declaring, catalog);
            byPrefix.Add(catalog.Prefix, catalog);
        }
    }
}
