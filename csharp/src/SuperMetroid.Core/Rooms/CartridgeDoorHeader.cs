using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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
    private const int DoorBank = 0x830000;

    /// <summary>Encoded size of one retail bank-$83 door header.</summary>
    public const int SizeInBytes = 12;

    /// <summary>Reads the packed door record named by a load station or room door list.</summary>
    public static CartridgeDoorHeader Load(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int address = DoorBank | pointer;
        return new CartridgeDoorHeader(
            pointer,
            DestinationRoomPointer: RomDataReader.ReadWordFixedBank(bus, address),
            BitFlags: bus.ReadByte(address + 2),
            Orientation: bus.ReadByte(address + 3),
            PlmX: bus.ReadByte(address + 4),
            PlmY: bus.ReadByte(address + 5),
            DestinationScreenX: bus.ReadByte(address + 6),
            DestinationScreenY: bus.ReadByte(address + 7),
            SamusDistance: RomDataReader.ReadWordFixedBank(bus, address + 8),
            SetupCodePointer: RomDataReader.ReadWordFixedBank(bus, address + 10));
    }

    /// <summary>
    /// True only for the verified setup routine that writes BGMODE=7, A=D=$0100,
    /// B=C=0, M7X=$0080, and M7Y=$03F0 at $8F:E4E0.
    /// </summary>
    public bool UsesCeresElevatorMode7 =>
        SetupCodePointer == DoorCodes.DoorASM_ToCeresElevatorShaft;
}
