using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>How a room load enters its room, beyond the door, room header, camera and viewport mode.</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct RoomEntry
{
    /// <summary>The Landing Site gunship scenario the room's enemies load with.</summary>
    public GunshipLoadScenario GunshipScenario { get; init; }

    /// <summary>Whether the entry door's closing PLM spawns after the room's PLMs, as a door transition does.</summary>
    public bool RunDoorClosingPlm { get; init; }

    /// <summary>The destination assets a door transition built before consuming its door, or null to load them here.</summary>
    public LoadedRoomAssets? PreloadedAssets { get; init; }

    /// <summary>The load-station record whose load establishes the room; null for a room entered otherwise.</summary>
    public LoadStationEntry? LoadStation { get; init; }
}
