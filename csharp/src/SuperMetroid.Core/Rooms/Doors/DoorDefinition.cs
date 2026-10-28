namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>One door: where it leads, how it is passed, where its cap stands and what it runs as Samus passes.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its setup routine can change how the
/// room is shown before gameplay. Treating the station's room as sufficient discarded
/// exactly the turning-floor view of the opening Ceres elevator.
/// </remarks>
/// <param name="Name">The door's name, the pinned disassembly's label for its record.</param>
/// <param name="DestinationRoom">The room the door leads to.</param>
/// <param name="Orientation">The direction Samus travels through the door and what closes behind her on arrival.</param>
/// <param name="PlmBlock">The block of the destination room where the arrival's closing door cap stands.</param>
/// <param name="DestinationScreen">The destination room's screen the camera arrives on.</param>
/// <param name="SamusSpeed">How far the transition pushes Samus on each step of the door-opening scroll.</param>
/// <param name="SetupCode">The setup routine the door runs while Samus passes through it.</param>
public sealed record DoorDefinition(
    string Name,
    RoomHeaderDefinition DestinationRoom,
    DoorOrientation Orientation,
    Position16<RoomBlocks> PlmBlock,
    Position16<RoomScreens> DestinationScreen,
    DoorSamusSpeed SamusSpeed,
    DoorSetupCode SetupCode);
