namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The shared two-byte bank-$84 delete list used by many no-op and one-shot PLMs.
/// This is program data, distinct from the delete instruction's own address.
/// </summary>
internal static class RoomPlmSharedDeleteProgramDefinitions
{
    /// <summary>$84:AAE3, shared InstList_PLM_Delete's sole opcode.</summary>
    internal const ushort Start = RoomPlmInstructionLists.Delete;
    /// <summary>$84:AAE4, high byte of the same sole opcode.</summary>
    internal const ushort End = Start + 1;

    /// <summary>Recognizes only the shared delete list's opcode word, not the delete handler address.</summary>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == Start)
        {
            value = (ushort)RoomPlmInstruction.Delete;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>Reads either byte of the sole little-endian delete opcode at $84:AAE3-AAE4.</summary>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address is < Start or > End)
        {
            value = 0;
            return false;
        }
        value = unchecked((byte)((ushort)RoomPlmInstruction.Delete >> (8 * (address - Start))));
        return true;
    }
}
