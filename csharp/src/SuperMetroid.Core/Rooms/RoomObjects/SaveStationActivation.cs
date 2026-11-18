using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The save station an action came from (#2383): the load station it saves at, its area and its block.</summary>
/// <param name="Station">The load station the save station saves at, which identifies the resident station a save returns to.</param>
/// <param name="Area">The room's area.</param>
/// <param name="Block">The station's room block, rather than a side-access trigger's.</param>
public readonly record struct SaveStationActivation(LoadStation Station, AreaId Area, Position16<RoomBlocks> Block);
