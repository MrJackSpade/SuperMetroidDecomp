namespace SuperMetroid.Core.Rooms;

/// <summary>Complete $84:B9A2-B9B8 rescue-wall break/event/delete records.</summary>
internal static class EscapeAnimalPlmProgramDefinitions
{
    /// <summary>Looks up a compiled mechanics or draw word in the animal-rescue wall reaction stream.</summary>
    /// <param name="address">Bank-$84 address to check against the reaction program's word operands.</param>
    /// <param name="value">Receives the compiled word when owned, or zero when the address is not a recognized nonzero word.</param>
    /// <returns><see langword="true"/> when the address selects a nonzero compiled word in the reaction stream.</returns>
    internal static bool TryReadWord(ushort address, out ushort value)
    {
        int offset = address - EscapeAnimalPlmRomData.ReactionList;
        value = offset switch
        {
            0 => RoomPlmInstructionCodes.QueueSoundLibrary2Maximum6,
            3 or 7 or 11 => 4,
            15 => 1,
            5 => EscapeAnimalPlmDrawDefinitions.Frame0,
            9 => EscapeAnimalPlmDrawDefinitions.Frame1,
            13 => EscapeAnimalPlmDrawDefinitions.Frame2,
            17 => EscapeAnimalPlmDrawDefinitions.Blank,
            19 => EscapeAnimalPlmRomData.SetEscapedEventInstruction,
            21 => RoomPlmInstructionCodes.Delete,
            _ => 0,
        };
        return value != 0;
    }

    /// <summary>Identifies the single sound-ID byte embedded in the rescue-wall reaction stream.</summary>
    /// <param name="address">Bank-$84 address to compare with the reaction list's sound byte.</param>
    /// <param name="value">Receives the wall-break sound ID, including when <paramref name="address"/> is not the owned byte.</param>
    /// <returns><see langword="true"/> only for the sound byte immediately following the reaction list's opening word.</returns>
    internal static bool TryReadByte(ushort address, out byte value)
    {
        value = RoomPlmShotBlockProgramDefinitions.BreakSoundId;
        return address == EscapeAnimalPlmRomData.ReactionList + 2;
    }
}
