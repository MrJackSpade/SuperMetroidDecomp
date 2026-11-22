namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One door: where it leads, how it is passed, where its cap stands and what it runs as Samus passes.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its setup can change how the
/// room is shown before gameplay, as the opening Ceres elevator's door shows its room as a turning floor.
/// </remarks>
/// <param name="Name">The door's name.</param>
/// <param name="DestinationRoom">The room the door leads to.</param>
/// <param name="Orientation">The direction Samus travels through the door and what closes behind her on arrival.</param>
/// <param name="RoomObjectBlock">
/// The block of the destination room where the arrival closes a resident door or places its closing cap, or null for a
/// door whose arrival closes nothing at any block.
/// </param>
/// <param name="DestinationScreen">The destination room's screen the camera arrives on.</param>
/// <param name="SamusSpeed">How far the transition pushes Samus on each step of the door-opening scroll.</param>
/// <param name="Setup">The setup the door runs while Samus passes through it.</param>
public sealed partial record DoorDefinition(
    string Name,
    RoomDefinition DestinationRoom,
    DoorOrientation Orientation,
    Position16<RoomBlocks>? RoomObjectBlock,
    Position16<RoomScreens> DestinationScreen,
    DoorSamusSpeed SamusSpeed,
    DoorSetup Setup)
{
    /// <summary>
    /// Whether passing the door exchanges explored maps (#1500): the live explored map becomes the map of the area Samus
    /// leaves, then loads from the stored map of the area she enters. The doors between areas do, elevators among them,
    /// except the escape's door from the Climb down into Tourian's fourth escape room, which leaves the live map as it is.
    /// </summary>
    public bool ExchangesExploredMaps { get; init; }
}
