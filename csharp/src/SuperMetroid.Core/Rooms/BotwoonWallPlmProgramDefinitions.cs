namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Botwoon's two bounded bank-$84 wall programs. The callback machine code at
/// $AB51..$AB66 is deliberately outside these instruction definitions.
/// </summary>
internal static class BotwoonWallPlmProgramDefinitions
{
    /// <summary><c>$84:AB31</c>: nine-row wall crumble program.</summary>
    internal const ushort Crumble = RoomPlmInstructionLists.CrumbleBotwoonWall;
    /// <summary><c>$84:AB67</c>: nine-block wall clear program.</summary>
    internal const ushort Clear = RoomPlmInstructionLists.ClearBotwoonWall;
    /// <summary><c>$84:AB33</c>: number of descending crumble rows.</summary>
    internal const byte CrumbleRows = 9;
    /// <summary><c>$84:AB38</c>: library-two crumble sound effect.</summary>
    internal const byte CrumbleSoundId = 0x0a;
    /// <summary>Native duration of one appearance in the crumble sequence.</summary>
    internal const ushort CrumbleFrameDuration = 4;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address is >= (Crumble + 8) and < (Crumble + 24))
        {
            int offset = address - Crumble - 8;
            if (offset % 4 == 0)
            {
                value = CrumbleFrameDuration;
                return true;
            }
            if (offset % 4 == 2)
            {
                value = checked((ushort)(
                    RoomPlmShotBlockDrawDefinitions.SingleFrame0 +
                    offset / 4 * 6));
                return true;
            }
        }

        // Word positions are byte offsets within each program.
        ushort? word = (address - Crumble) switch
        {
            0 => (ushort)RoomPlmInstruction.SetEightBitTimer,
            3 => (ushort)RoomPlmInstruction.SetBotwoonScrollsBlue,
            5 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            24 => (ushort)RoomPlmInstruction.MoveBotwoonPlmDownOneBlock,
            26 => (ushort)RoomPlmInstruction.DecrementTimerAndGoto,
            28 => Crumble + 5,
            30 => (ushort)RoomPlmInstruction.Delete,
            _ => (address - Clear) switch
            {
                0 => 1,
                2 => BotwoonWallPlmDrawDefinitions.ClearPointer,
                4 => (ushort)RoomPlmInstruction.Delete,
                _ => null,
            },
        };
        value = word ?? 0;
        return word.HasValue;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        byte? operand = (address - Crumble) switch
        {
            2 => CrumbleRows,
            7 => CrumbleSoundId,
            _ => null,
        };
        value = operand ?? 0;
        return operand.HasValue;
    }
}
