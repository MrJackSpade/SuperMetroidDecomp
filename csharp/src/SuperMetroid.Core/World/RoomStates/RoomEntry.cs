using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// How a room load enters its room, beyond the door, room definition, camera and viewport mode (#2560): a closed choice of
/// a plain load, an entry at a load station with the gunship scenario its enemies load with, and an entry through a door,
/// whose closing room object spawns after the room's room objects and whose destination assets a door transition may have
/// built before consuming its door.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal abstract partial record RoomEntry
{
    /// <summary>Only the three entries below are room entries.</summary>
    private RoomEntry()
    {
    }

    /// <summary>A room entered neither at a load station nor through a door's transition.</summary>
    internal static RoomEntry Plain { get; } = new PlainEntry();

    /// <summary>The Landing Site gunship scenario the room's enemies load with.</summary>
    internal virtual GunshipLoadScenario GunshipScenario => GunshipLoadScenario.Ordinary;

    /// <summary>Whether the entry door's closing room object spawns after the room's room objects, as a door transition does.</summary>
    internal virtual bool RunsDoorClosingObject => false;

    /// <summary>The destination assets a door transition built before consuming its door; none to load them here.</summary>
    internal virtual LoadedRoomAssets? PreloadedAssets => null;

    /// <summary>The load station the room is entered at; none for a room entered otherwise.</summary>
    internal virtual LoadStation? LoadStation => null;

    /// <summary>A room entered neither at a load station nor through a door's transition.</summary>
    private sealed partial record PlainEntry : RoomEntry;

    /// <summary>A room entered at <paramref name="Station"/>.</summary>
    /// <param name="Station">The load station the room is entered at.</param>
    /// <param name="Gunship">The Landing Site gunship scenario the room's enemies load with.</param>
    internal sealed partial record AtStation(LoadStation Station, GunshipLoadScenario Gunship = GunshipLoadScenario.Ordinary) : RoomEntry
    {
        /// <inheritdoc/>
        internal override GunshipLoadScenario GunshipScenario => Gunship;

        /// <inheritdoc/>
        internal override LoadStation? LoadStation => Station;
    }

    /// <summary>A room entered through a door's transition, whose closing room object spawns after the room's.</summary>
    /// <param name="Preloaded">The destination assets the transition built before consuming its door; none to load them here.</param>
    internal sealed partial record ThroughDoor(LoadedRoomAssets? Preloaded) : RoomEntry
    {
        /// <inheritdoc/>
        internal override bool RunsDoorClosingObject => true;

        /// <inheritdoc/>
        internal override LoadedRoomAssets? PreloadedAssets => Preloaded;
    }
}
