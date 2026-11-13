namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a door's setup starts in the room it leads into besides changing its screens (#1721).</summary>
public enum DoorRoomEvent
{
    /// <summary>Nothing: the setup only changes screens, or does nothing at all.</summary>
    None,

    /// <summary>
    /// The Ceres elevator shaft is shown as a turning floor, unrotated, about room point (128, 1008). The room load
    /// installs that view before graphics and actors initialize, so the setup only requires it.
    /// </summary>
    ShowsTurningFloor,

    /// <summary>The screen returns from the turning floor to the ordinary room display.</summary>
    EndsTurningFloor,

    /// <summary>The Wrecked Ship entrance's treadmill starts moving rightwards and its west entrance's treadmill object spawns.</summary>
    StartsTreadmillRightwards,

    /// <summary>The Wrecked Ship entrance's treadmill starts moving leftwards and its east entrance's treadmill object spawns.</summary>
    StartsTreadmillLeftwards,

    /// <summary>
    /// The elevatube ride starts from the bottom: Samus is held still and the tube takes its starting height, speed and
    /// acceleration for a ride from the south.
    /// </summary>
    StartsElevatubeRideFromSouth,

    /// <summary>
    /// The elevatube ride starts from the top: Samus is held still and the tube takes its starting height, speed and
    /// acceleration for a ride from the north.
    /// </summary>
    StartsElevatubeRideFromNorth,

    /// <summary>The elevatube ride ends: Samus is released and the tube stops.</summary>
    EndsElevatubeRide,
}
