using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room terrain Samus's block collision probes: the address space, the level data, and the PLMs that react to touched blocks (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
public readonly record struct SamusTerrain
{


    /// <summary>Current mutable room collision data used by active spark movement.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>Optional current-room PLM owner receiving block reactions during spark movement.</summary>
    internal ISamusRoomPlms? Plms { get; init; }

    /// <summary>
    /// The session's progression, which SpaceTime's malformed palette copy overwrites as raw bytes (#1318, #1330). Only
    /// the gameplay projectile frame passes it; SpaceTime fails loudly without it.
    /// </summary>
    public SuperMetroid.Core.Progression.Bank80SystemState? Progression { get; init; }
}
