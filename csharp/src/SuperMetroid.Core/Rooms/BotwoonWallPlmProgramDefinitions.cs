namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Botwoon's two bounded bank-$84 wall programs. The callback machine code at
/// $AB51..$AB66 is deliberately outside these instruction definitions.
/// </summary>
internal static class BotwoonWallPlmProgramDefinitions
{
    /// <summary><c>$84:AB31</c>: nine-row wall crumble program.</summary>
    internal const ushort Crumble = RoomPlmInstructionLists.CrumbleBotwoonWall;
    /// <summary><c>$84:AB51</c>: first byte of the following scroll callback.</summary>
    internal const ushort CrumbleEndExclusive = RoomPlmInstructionCodes.SetBotwoonScrollsBlue;
    /// <summary><c>$84:AB67</c>: nine-block wall clear program.</summary>
    internal const ushort Clear = RoomPlmInstructionLists.ClearBotwoonWall;
    /// <summary><c>$84:AB6D</c>: first byte of the following Kraid program.</summary>
    internal const ushort ClearEndExclusive = 0xab6d;
    /// <summary><c>$84:AB33</c>: number of descending crumble rows.</summary>
    internal const byte CrumbleRows = 9;
    /// <summary><c>$84:AB38</c>: library-two crumble sound effect.</summary>
    internal const byte CrumbleSoundId = 0x0a;
    /// <summary>Native duration of one appearance in the crumble sequence.</summary>
    internal const ushort CrumbleFrameDuration = 4;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        if (address >= Crumble + 8 && address < Crumble + 24)
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

        value = address switch
        {
            Crumble => RoomPlmInstructionCodes.SetEightBitTimer,
            Crumble + 3 => RoomPlmInstructionCodes.SetBotwoonScrollsBlue,
            Crumble + 5 => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            Crumble + 24 => RoomPlmInstructionCodes.MoveBotwoonPlmDownOneBlock,
            Crumble + 26 => RoomPlmInstructionCodes.DecrementTimerAndGoto,
            Crumble + 28 => Crumble + 5,
            Crumble + 30 => RoomPlmInstructionCodes.Delete,
            Clear => 1,
            Clear + 2 => BotwoonWallPlmDrawDefinitions.ClearPointer,
            Clear + 4 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return address is Crumble or Crumble + 3 or Crumble + 5 or
            Crumble + 24 or Crumble + 26 or Crumble + 28 or
            Crumble + 30 or Clear or Clear + 2 or Clear + 4;
    }

    internal static bool TryReadMechanicsByte(ushort address, out byte value)
    {
        value = address switch
        {
            Crumble + 2 => CrumbleRows,
            Crumble + 7 => CrumbleSoundId,
            _ => 0,
        };
        return address is Crumble + 2 or Crumble + 7;
    }

    internal static IEnumerable<ushort> NativeWordAddresses()
    {
        yield return Crumble;
        yield return checked((ushort)(Crumble + 3));
        yield return checked((ushort)(Crumble + 5));
        for (int frame = 0; frame < 4; frame++)
        {
            yield return checked((ushort)(Crumble + 8 + frame * 4));
            yield return checked((ushort)(Crumble + 10 + frame * 4));
        }
        yield return checked((ushort)(Crumble + 24));
        yield return checked((ushort)(Crumble + 26));
        yield return checked((ushort)(Crumble + 28));
        yield return checked((ushort)(Crumble + 30));
        yield return Clear;
        yield return checked((ushort)(Clear + 2));
        yield return checked((ushort)(Clear + 4));
    }
}
