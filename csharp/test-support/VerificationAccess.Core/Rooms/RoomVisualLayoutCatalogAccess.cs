namespace SuperMetroid.Core.Rooms;

/// <summary>Verification access to <see cref="RoomVisualLayoutCatalog"/> members production does not use.</summary>
internal static class RoomVisualLayoutCatalogAccess
{
    /// <summary>Constructs partial room-geometry layouts for focused visual fixtures.</summary>
    extension(RoomVisualLayoutCatalog)
    {
        /// <summary>Explicitly partial geometry fixtures; never an installed production catalog.</summary>
        internal static RoomVisualLayoutCatalog FromLayoutsForVerification(
            IReadOnlyDictionary<int, RoomVisualLayout> layouts) =>
            new RoomVisualLayoutCatalog(layouts, requiredSources: []);
    }
}
