namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a station does as its access arm's use reaches a moment (<see cref="StationAccessTimingDefinitions"/>, #2480).</summary>
internal enum StationAccessCue
{
    /// <summary>The fully extended arm's hold is over: the station activates.</summary>
    Activates,

    /// <summary>The hold after the station's activation is over: the arm disengages, sounding, and starts to retract.</summary>
    Disengages,
}
