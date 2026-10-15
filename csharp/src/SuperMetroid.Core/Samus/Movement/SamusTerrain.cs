using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room terrain Samus's block collision probes: the address space, the level data, and the PLMs that react to touched blocks (#633).</summary>
public readonly record struct SamusTerrain
{
    /// <summary>Address space used by Samus pose metadata and the translated movement/collision helpers.</summary>
    public required ISnesAddressSpace Bus { get; init; }

    /// <summary>Current mutable room collision data used by active spark movement.</summary>
    public required RoomLevelData Level { get; init; }

    /// <summary>Optional current-room PLM owner receiving block reactions during spark movement.</summary>
    public RoomPlmSystem? Plms { get; init; }
}
