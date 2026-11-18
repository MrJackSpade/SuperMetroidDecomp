namespace SuperMetroid.Core.World;

/// <summary>
/// The installed pictures of the room's room-object blocks, one catalog per family, each found by its type (#2034). They
/// are built from the installation's whole set of room-object pictures (#2290), which a rebinding replaces whole, so every
/// family's draw finds its catalog.
/// </summary>
/// <remarks>
/// A family finds its catalog by the catalog's type because the families live in Rooms, Bosses, Enemies and Areas, all of
/// which World composes: none of them can name <see cref="RoomObjectArtwork"/>, World's whole set, without a module cycle.
/// Each family's catalog is therefore a type of its own, even where two families' catalogs hold the same kind of
/// pictures, and that type is the family's key here (#2487).
/// </remarks>
internal sealed class RoomObjectVisuals
{
    /// <summary>The bound catalogs, each under its own type, which names the one family it pictures.</summary>
    private Dictionary<Type, object> _catalogs;

    /// <summary>Binds the installation's whole set of room-object pictures.</summary>
    /// <param name="artwork">Every family's installed pictures.</param>
    internal RoomObjectVisuals(RoomObjectArtwork artwork) => _catalogs = CatalogsOf(artwork);

    /// <summary>The bound catalog of type <typeparamref name="TCatalog"/>.</summary>
    /// <typeparam name="TCatalog">The catalog's type.</typeparam>
    /// <exception cref="ArgumentException"><typeparamref name="TCatalog"/> is no room-object family's catalog.</exception>
    internal TCatalog Catalog<TCatalog>() where TCatalog : class =>
        _catalogs.TryGetValue(typeof(TCatalog), out object? catalog)
            ? (TCatalog)catalog
            : throw new ArgumentException($"{typeof(TCatalog).Name} is no room-object family's installed pictures.", nameof(TCatalog));

    /// <summary>Replaces the whole set with <paramref name="artwork"/>, as a host binding another installation's artwork does.</summary>
    /// <param name="artwork">Every family's installed pictures.</param>
    internal void Rebind(RoomObjectArtwork artwork) => _catalogs = CatalogsOf(artwork);

    /// <summary><paramref name="artwork"/>'s catalogs, each under its own type.</summary>
    /// <exception cref="ArgumentException">Two families' catalogs share a type.</exception>
    private static Dictionary<Type, object> CatalogsOf(RoomObjectArtwork artwork) =>
        Ensure.NotNull(artwork).Catalogs.ToDictionary(catalog => catalog.GetType());
}
