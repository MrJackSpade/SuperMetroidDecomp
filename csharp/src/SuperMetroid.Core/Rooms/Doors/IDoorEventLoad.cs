namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The room a door leads into while its load composes it, before its graphics and actors initialize (#2485): what a door's
/// room event (<see cref="DoorRoomEvent"/>) installs there. The room's load presents it.
/// </summary>
internal interface IDoorEventLoad
{
    /// <summary>
    /// Shows the Ceres elevator shaft as a turning floor, level about the shaft's rotation centre, and starts the shaft's
    /// turn there, holding level before its first tilt.
    /// </summary>
    void ShowShaftLevelAndStartTurn();
}
