using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room terrain Samus's block collision probes: the level data and the room objects that react to touched blocks (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public readonly record struct SamusTerrain
{


    /// <summary>Current mutable room collision data used by active spark movement.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>Optional current-room room-object owner receiving block reactions during spark movement.</summary>
    internal ISamusRoomObjects? RoomObjects { get; init; }

    /// <summary>
    /// The session's progression, which Spacetime's malformed palette copy overwrites with raw data (#1318, #1330). Only
    /// the gameplay projectile frame passes it; Spacetime fails loudly without it.
    /// </summary>
    public SuperMetroid.Core.Progression.SharedGameState? Progression { get; init; }
}
