namespace SuperMetroid.Core.Rooms;

/// <summary>Complete $84:B9A2-B9B8 rescue-wall break/event/delete records.</summary>
internal static class EscapeAnimalPlmProgramDefinitions
{
    internal static bool TryReadWord(ushort address, out ushort value)
    {
        int offset = address - EscapeAnimalPlmRomData.ReactionList;
        value = offset switch
        {
            0 => (ushort)RoomPlmInstruction.QueueSoundLibrary2Maximum6,
            3 or 7 or 11 => 4,
            15 => 1,
            5 => (ushort)EscapeAnimalDraw.Frame0,
            9 => (ushort)EscapeAnimalDraw.Frame1,
            13 => (ushort)EscapeAnimalDraw.Frame2,
            17 => (ushort)EscapeAnimalDraw.Blank,
            19 => (ushort)RoomPlmInstruction.SetAnimalsEscapedEvent,
            21 => (ushort)RoomPlmInstruction.Delete,
            _ => 0,
        };
        return value != 0;
    }

    internal static bool TryReadByte(ushort address, out byte value)
    {
        value = RoomPlmShotBlockProgramDefinitions.BreakSoundId;
        return address == EscapeAnimalPlmRomData.ReactionList + 2;
    }
}
