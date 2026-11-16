using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The load station a save station saves at, as the station's placement in its room names it: its activation events
/// carry it, and a save resumes there.
/// </summary>
/// <param name="Station">The load station the save station saves at.</param>
public sealed record SaveStationArgument(LoadStation Station) : RoomObjectRoomArgument;
