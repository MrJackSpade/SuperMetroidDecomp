using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Lossless twelve-byte door definition read from cartridge bank $83.</summary>
/// <remarks>
/// A load-station entry identifies both a room and the door through which that room is
/// entered. The latter is behaviorally significant: its final word names setup code that
/// can change PPU mode before gameplay. Treating the station's room pointer as sufficient
/// discarded exactly the Mode-7 switch used by the opening Ceres elevator.
/// </remarks>
/// <param name="Pointer">Bank-$83 address identifying this twelve-byte door record.</param>
/// <param name="DestinationRoomPointer">Bank-$8F room-header address loaded after the door transition.</param>
/// <param name="Orientation">Native door-facing and transition-direction flags.</param>
/// <param name="PlmX">Horizontal room-block coordinate of the door's PLM.</param>
/// <param name="PlmY">Vertical room-block coordinate of the door's PLM.</param>
/// <param name="DestinationScreenX">Destination horizontal screen coordinate used when entering the room.</param>
/// <param name="DestinationScreenY">Destination vertical screen coordinate used when entering the room.</param>
/// <param name="SamusDistance">Signed native travel distance used to advance Samus through the door transition.</param>
/// <param name="SetupCodePointer">Bank-$8F setup-routine address, or the native sentinel value when no setup routine is specified.</param>
public sealed record CartridgeDoorHeader(
    ushort Pointer,
    ushort DestinationRoomPointer,
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
