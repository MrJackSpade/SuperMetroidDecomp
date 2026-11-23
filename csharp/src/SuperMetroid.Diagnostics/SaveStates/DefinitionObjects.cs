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
/// a game can reach. A lookup first indexes the catalogs initialized since the last one, so the index never runs a
/// catalog's initializer. A host that captures as the game runs also indexes each catalog on a thread of its own as soon as
/// it is initialized (<see cref="IndexInBackground"/>), so a capture finds them indexed and walks none: initializing and
/// walking every catalog where a capture first needed them stalled the session for seconds, and walking the catalogs
/// initialized by then still stalled the first capture. The walk reads only definition data, fixed once its catalog is
/// initialized, and the field names the compiler gives each type, held apart from the type so reading them runs none of the
/// game's initializers. A restore indexes the catalog its path names, initializing it. An object keeps the path it was
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

    /// <summary>Set when a catalog is initialized, so the background indexer indexes it.</summary>
    private readonly ManualResetEventSlim catalogsInitialized = new(initialState: true);

    /// <summary>The thread indexing catalogs as they are initialized, once a host has started it.</summary>
    private Thread? indexer;

    /// <summary>Why the background indexer stopped, which every later lookup reports.</summary>
    private Exception? indexingFailure;

    /// <summary>
    /// Starts indexing each catalog on a thread of its own as soon as it is initialized, unless that has started: the
    /// catalogs initialized already now, and each catalog initialized later as its initializer finishes. A lookup waits for
    /// a walk in progress. A failed walk stops the indexer and fails every later lookup with the failure.
    /// </summary>
    internal void IndexInBackground()
    {
        lock (gate)
        {
            if (indexer is not null)
                return;
            StateGraphCatalogInitializations.Marked += catalogsInitialized.Set;
            indexer = new Thread(IndexAsInitialized)
            {
                IsBackground = true,
                Name = "Debugger state definition index",
                Priority = ThreadPriority.BelowNormal,
            };
            indexer.Start();
        }
    }

    /// <summary>Indexes the catalogs initialized since the last walk each time one is initialized, until a walk fails.</summary>
    private void IndexAsInitialized()
    {
        try
        {
            while (true)
            {
                catalogsInitialized.Wait();
                catalogsInitialized.Reset();
                lock (gate)
                    _ = Current();
            }
        }
        catch (Exception exception)
        {
            // The indexer's thread is a process boundary: the failure is kept and reported by every later lookup.
            lock (gate)
                indexingFailure = exception;
        }
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
            if (separator < 0 || !Declared.ByPrefix.TryGetValue(path[..(separator + DefinitionCatalog.MemberSeparator.Length)], out DefinitionCatalog? catalog))
                throw new InvalidDataException($"Debugger state names definition '{path}', which no type of this build declares as a static member.");
            BoundObjectGraph current = Current();
            Index(current, catalog);
            return current.Resolve(path);
        }
    }

    /// <summary>The catalogs, by prefix and by type; callers hold the lock.</summary>
    internal Declarations Declared => field ??= Declare();

    /// <summary>The index holding every initialized catalog's objects; callers hold the lock.</summary>
    /// <exception cref="InvalidOperationException">The background indexer failed.</exception>
    private BoundObjectGraph Current()
    {
        if (indexingFailure is not null)
            throw new InvalidOperationException("Indexing the definition catalogs for debugger states failed.", indexingFailure);
        BoundObjectGraph current = index ??= new BoundObjectGraph(types, []);
        IReadOnlyList<Type> initialized = StateGraphCatalogInitializations.Initialized;
        for (; initializedIndexed < initialized.Count; initializedIndexed++)
        {
            // A type marked here that this registry does not hold as a catalog holds nothing a state of it names.
            if (Declared.ByType.TryGetValue(initialized[initializedIndexed], out DefinitionCatalog? catalog))
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

    private Declarations Declare()
    {
        var catalogs = types.Catalogs
            .Select(catalog => new DefinitionCatalog(catalog.Catalog, types.Format(catalog.Type), catalog.Members))
            .ToList();
        return new Declarations(
            catalogs.ToDictionary(catalog => catalog.Prefix, StringComparer.Ordinal),
            catalogs.ToDictionary(catalog => catalog.Type));
    }

    /// <summary>The catalogs of the game's static members.</summary>
    /// <param name="ByPrefix">Each catalog by the prefix its roots' names begin with.</param>
    /// <param name="ByType">Each catalog by its declaring type.</param>
    internal sealed record Declarations(IReadOnlyDictionary<string, DefinitionCatalog> ByPrefix, IReadOnlyDictionary<Type, DefinitionCatalog> ByType);
}
