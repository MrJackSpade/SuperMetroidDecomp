namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The packed room argument of a grey door (<c>$84:BE3F</c> and <c>Setup_GreyDoor</c>): its high byte selects the unlock
/// condition, and the setup keeps only the door bit and the never-persist flag.
/// </summary>
internal static class GreyDoorRoomArgumentDefinitions
{
    /// <summary>Bits 10..14 of the argument hold the condition's even byte offset, doubled.</summary>
    private const byte ConditionBits = 0x7c;
    /// <summary>The bits the setup keeps: bit 15 (never persist) and the door bit index in bits 0..9.</summary>
    private const ushort DoorBitArgumentBits = 0x83ff;

    /// <summary>The byte offset of the unlock condition in the <c>$84:BE4B</c> table.</summary>
    internal static int ConditionOffset(int roomArgument) =>
        (SnesLittleEndian.HighByte(checked((ushort)roomArgument)) & ConditionBits) >> 1;

    /// <summary>The argument the setup leaves: the door bit and the never-persist flag.</summary>
    internal static int DoorBitArgument(int roomArgument) => roomArgument & DoorBitArgumentBits;
}
