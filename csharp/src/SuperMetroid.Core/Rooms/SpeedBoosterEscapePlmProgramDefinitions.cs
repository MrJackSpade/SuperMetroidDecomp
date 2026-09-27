namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The complete nine-word $84:B88A instruction list for the Speed Booster
/// escape controller. The three sleeping handoffs remain resident PLM behavior,
/// not editable presentation data.
/// </summary>
internal static class SpeedBoosterEscapePlmProgramDefinitions
{
    /// <summary><c>$84:B88A</c>: first instruction of the controller list.</summary>
    internal const ushort Start = RoomPlmInstructionLists.SpeedBoosterEscape;

    private static readonly ushort[] Words =
    [
        RoomPlmInstructionCodes.InstallPreInstruction,
        SpeedBoosterEscapePlmRomData.WaitForSpeedBoosterPreInstruction,
        RoomPlmInstructionCodes.Sleep,
        RoomPlmInstructionCodes.InstallPreInstruction,
        SpeedBoosterEscapePlmRomData.WaitForSamusLeftPreInstruction,
        RoomPlmInstructionCodes.Sleep,
        RoomPlmInstructionCodes.InstallPreInstruction,
        SpeedBoosterEscapePlmRomData.AdvanceLavaPreInstruction,
        RoomPlmInstructionCodes.Sleep,
    ];

    internal static int WordCount => Words.Length;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - Start;
        if (offset >= 0 && (offset & 1) == 0 && offset / 2 < Words.Length)
        {
            value = Words[offset / 2];
            return true;
        }

        value = 0;
        return false;
    }
}
