namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The one run of a station's access arm (<see cref="StationAccessTimingDefinitions"/>, #2480).</summary>
internal enum StationAccessSegment
{
    /// <summary>
    /// The arm's use: drawn retracted, then extended and held, the station activating, a hold, the disengage sound, a
    /// hold, then drawn retracted and held before the access ends.
    /// </summary>
    Use,
}
