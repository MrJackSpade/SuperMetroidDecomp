using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room terrain Samus's block collision probes: the address space, the level data, and the PLMs that react to touched blocks (#633).</summary>
public readonly record struct SamusTerrain
{


    /// <summary>Current mutable room collision data used by active spark movement.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>Optional current-room PLM owner receiving block reactions during spark movement.</summary>
    public RoomPlmSystem? Plms { get; init; }

    /// <summary>
    /// The progression mirror SpaceTime's malformed palette copy overwrites (#1318), until #1330 gives progression a
    /// single owner. Only the gameplay projectile frame passes it; SpaceTime fails loudly without it.
    /// </summary>
    public SuperMetroid.Core.Saves.SaveMirrorImage? ProgressionMirror { get; init; }
}
