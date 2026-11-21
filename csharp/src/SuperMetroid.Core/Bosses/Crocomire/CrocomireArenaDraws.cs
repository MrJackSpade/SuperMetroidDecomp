using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws (#2034): the block shape each draws, from
/// <see cref="CrocomireArenaObjectDrawDefinitions"/>, and the installed picture (#1557) each shows. Each arena mutation is
/// its own draw (#2557): the mutation names the shape it draws, so no second identity maps one to the other.
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>The arena's five frames, pictured by the installed Crocomire arena frames.</summary>
    internal static RoomObjectDrawFamily<CrocomireArenaMutation> Family { get; } =
        new RoomObjectDrawFamily<CrocomireArenaMutation>("Crocomire arena",
            frame => RoomObjectDrawFamily<CrocomireArenaMutation>.Defined(CrocomireArenaObjectDrawDefinitions.Shapes, frame));

    /// <summary>
    /// The change each mutation makes after its empty setup: its own draw, drawn on its first update; it is gone on the
    /// next.
    /// </summary>
    internal static IReadOnlyDictionary<CrocomireArenaMutation, TerrainChange> Changes { get; } =
        CrocomireArenaObjectDrawDefinitions.Shapes.Keys.ToDictionary(mutation => mutation, mutation => TerrainChange.DrawnOnce(Family.Of(mutation)));
}
