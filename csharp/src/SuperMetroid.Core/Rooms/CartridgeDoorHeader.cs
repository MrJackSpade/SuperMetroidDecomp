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
    /// <summary>Encoded size of one retail bank-$83 door header.</summary>
    public const int SizeInBytes = DoorHeaderRomData.RecordByteCount;

    /// <summary>Reads the packed door record named by a load station or room door list.</summary>
    public static CartridgeDoorHeader Load(IImportCartridgeSource cartridge, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(cartridge);
        int address = DoorHeaderRomData.BankAddress | pointer;
        return new CartridgeDoorHeader(
            pointer,
            DestinationRoomPointer: ReadWord(address),
            BitFlags: cartridge.ReadCartridgeByte(address + 2),
            Orientation: cartridge.ReadCartridgeByte(address + 3),
            PlmX: cartridge.ReadCartridgeByte(address + 4),
            PlmY: cartridge.ReadCartridgeByte(address + 5),
            DestinationScreenX: cartridge.ReadCartridgeByte(address + 6),
            DestinationScreenY: cartridge.ReadCartridgeByte(address + 7),
            SamusDistance: ReadWord(address + 8),
            SetupCodePointer: ReadWord(address + 10));

        ushort ReadWord(int wordAddress) => unchecked((ushort)(
            cartridge.ReadCartridgeByte(wordAddress) |
            cartridge.ReadCartridgeByte(SnesAddressMath.AddWithinBank(wordAddress, 1)) << 8));
    }

    /// <summary>
    /// True only for the verified setup routine that writes BGMODE=7, A=D=$0100,
    /// B=C=0, M7X=$0080, and M7Y=$03F0 at $8F:E4E0.
    /// </summary>
    public bool UsesCeresElevatorMode7 =>
        SetupCodePointer == DoorCodes.DoorASM_ToCeresElevatorShaft;
}
