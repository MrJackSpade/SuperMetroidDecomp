namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Lossless twelve-byte door definition read from cartridge bank $83.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its final word names setup code that
/// can change PPU mode before gameplay. Treating the station's room pointer as sufficient
/// discarded exactly the Mode-7 switch used by the opening Ceres elevator.
/// </remarks>
public sealed record DoorDefinition(
    string Name,
    RoomHeaderDefinition DestinationRoom,
    DoorOrientation Orientation,
    Position16<RoomBlocks> PlmBlock,
    Position16<RoomScreens> DestinationScreen,
    int SamusDistance,
    DoorSetupCode SetupCode)
{
    /// <summary>The door's Samus-distance word, 0 through 65535.</summary>
    public int SamusDistance { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SamusDistance);

    /// <summary>
    /// True only for the door into the Ceres elevator shaft, whose verified setup routine at $8F:E4E0 shows the room as a
    /// turning floor, unrotated, about room point (128, 1008).
    /// </summary>
    public bool EntersCeresElevatorShaft =>
        SetupCode == DoorSetupCode.DoorASM_ToCeresElevatorShaft;
}
