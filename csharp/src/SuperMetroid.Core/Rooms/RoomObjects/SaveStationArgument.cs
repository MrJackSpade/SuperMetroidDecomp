using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The load station a save station saves at, which its activation events carry and a save resumes at. Room data names it
/// by its number in the area's station list; the import decodes that number into the station.
/// </summary>
/// <param name="Station">The load station the save station saves at.</param>
public sealed record SaveStationArgument(LoadStation Station) : RoomObjectRoomArgument;
