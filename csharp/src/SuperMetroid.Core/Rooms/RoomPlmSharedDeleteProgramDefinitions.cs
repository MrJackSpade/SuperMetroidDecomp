namespace SuperMetroid.Core.Rooms;

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

    /// <summary>Resolves the shared delete opcode when queried at its starting word address.</summary>
    /// <param name="address">Bank-$84 offset to query.</param>
    /// <param name="value">Receives the delete opcode on success, or zero when the address is not the opcode start.</param>
    /// <returns><see langword="true"/> only for the first byte of the shared delete instruction.</returns>
    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address == Start)
        {
            value = RoomPlmInstructionCodes.Delete;
            return true;
        }
        value = 0;
        return false;
    }

    /// <summary>Resolves either stored byte of the shared delete opcode.</summary>
    /// <param name="address">Bank-$84 offset to query.</param>
    /// <param name="value">Receives the selected low or high opcode byte, or zero when the address is outside the word.</param>
    /// <returns><see langword="true"/> for the opcode's start or end byte.</returns>
    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        if (address is Start or End)
        {
            value = address == Start
                ? unchecked((byte)RoomPlmInstructionCodes.Delete)
                : unchecked((byte)(RoomPlmInstructionCodes.Delete >> 8));
            return true;
        }
        value = 0;
        return false;
    }
}
