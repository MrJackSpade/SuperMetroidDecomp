using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Lossless twelve-byte door definition read from cartridge bank $83.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its final word names setup code that
/// can change PPU mode before gameplay. Treating the station's room pointer as sufficient
/// discarded exactly the Mode-7 switch used by the opening Ceres elevator.
/// </remarks>
public sealed record CartridgeDoorHeader(
    ushort Pointer,
    ushort DestinationRoomPointer,
    byte BitFlags,
    byte Orientation,
    byte PlmX,
    byte PlmY,
    byte DestinationScreenX,
    byte DestinationScreenY,
    ushort SamusDistance,
    ushort SetupCodePointer)
{

    /// <summary>
    /// True only for the verified setup routine that writes BGMODE=7, A=D=$0100,
    /// B=C=0, M7X=$0080, and M7Y=$03F0 at $8F:E4E0.
    /// </summary>
    public bool UsesCeresElevatorMode7 =>
        SetupCodePointer == DoorCodes.DoorASM_ToCeresElevatorShaft;
}
