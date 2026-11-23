namespace SuperMetroid.Core;

/// <summary>
/// The definition catalogs whose type initializers have begun, in the order they began (#2619). The compiler gives each
/// catalog a static field whose initializer marks it here, so the catalog is marked as its type initializer runs. An object
/// a catalog's static members hold exists only once that initializer has run, so a debugger state indexes only the
/// catalogs marked here and never runs a catalog's initializer itself: initializing and walking every catalog of the game
/// stalled the session for seconds where a capture first needed them.
/// </summary>
/// <remarks>
/// The marks are kept in an array replaced whole on each mark, not in a read-only static member, so this type is no catalog
/// itself and marking runs no initializer of its own catalog. A catalog is marked while its initializer runs: reading its
/// members on another thread waits for the initializer to finish.
/// </remarks>
public static partial class StateGraphCatalogInitializations
{
    /// <summary>The catalogs marked so far, in the order their initializers began.</summary>
    private static Type[] initialized = [];

    /// <summary>The catalogs whose type initializers have begun, in the order they began.</summary>
    public static IReadOnlyList<Type> Initialized => Volatile.Read(ref initialized);

    /// <summary>
    /// Raised on the thread running a catalog's initializer after the catalog is marked. A handler runs inside that type
    /// initializer, so it only signals: it reads no catalog and takes no lock a capture holds.
    /// </summary>
    public static event Action? Marked;

    /// <summary>Marks <paramref name="catalog"/> as initialized; its type initializer calls this as it runs.</summary>
    /// <param name="catalog">The catalog's type.</param>
    /// <returns>True, which the catalog's generated marker field holds.</returns>
    public static bool Mark(Type catalog)
    {
        Ensure.NotNull(catalog);
        Type[] seen, next;
        do
        {
            seen = Volatile.Read(ref initialized);
            next = [.. seen, catalog];
        }
        while (!ReferenceEquals(Interlocked.CompareExchange(ref initialized, next, seen), seen));
        Marked?.Invoke();
        return true;
    }
}
