using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's bridge and invisible-wall block draws (#2034): the block shape each draws, from
/// <see cref="CrocomireArenaObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class CrocomireArenaDraws
{
    /// <summary>The arena's five frames, pictured by the installed Crocomire arena frames.</summary>
    internal static RoomObjectDrawFamily<CrocomireArenaDraw> Family { get; } =
        new RoomObjectDrawFamily<CrocomireArenaDraw>("Crocomire arena",
            frame => RoomObjectDrawFamily<CrocomireArenaDraw>.Defined(CrocomireArenaObjectDrawDefinitions.Shapes, frame));
}
