using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The room's part of Spore Spawn's fight: the scrolling-finished hook his live fight installs. His own state is his body's
/// (#2231).
/// </summary>
[RoomLifetime]
internal sealed class SporeSpawnEncounter
{
    /// <summary>
    /// Whether the room's scrolling-finished hook enforces the arena's minimum camera Y: installed as the live fight
    /// initializes, removed at his death, and never installed for an already defeated load.
    /// </summary>
    internal bool ScrollClampHookInstalled { get; set; }
}
