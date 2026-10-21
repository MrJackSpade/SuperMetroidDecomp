namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Lossless twelve-byte door definition read from cartridge bank $83.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its final word names setup code that
/// can change PPU mode before gameplay. Treating the station's room pointer as sufficient
/// discarded exactly the Mode-7 switch used by the opening Ceres elevator.
/// </remarks>
public sealed record CartridgeDoorHeader(
    string Name,
    RoomHeaderDefinition DestinationRoom,
    CartridgeDoorOrientation Orientation,
    Position16<RoomBlocks> PlmBlock,
    Position16<RoomScreens> DestinationScreen,
    int SamusDistance,
    DoorSetupCode SetupCode)
{
    /// <summary>The door's Samus-distance word, 0 through 65535.</summary>
    public int SamusDistance { get; } = Ensure.BetweenInclusive(0, ushort.MaxValue, SamusDistance);

    /// <summary>
    /// True only for the verified setup routine that writes BGMODE=7, A=D=$0100,
    /// B=C=0, M7X=$0080, and M7Y=$03F0 at $8F:E4E0.
    /// </summary>
    public bool UsesCeresElevatorMode7 =>
        SetupCode == DoorSetupCode.DoorASM_ToCeresElevatorShaft;
}
