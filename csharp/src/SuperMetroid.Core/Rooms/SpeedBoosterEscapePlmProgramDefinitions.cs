namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The complete nine-word $84:B88A instruction list for the Speed Booster
/// escape controller. The three sleeping handoffs remain resident PLM behavior,
/// not editable presentation data. Each six-byte phase installs its named callback
/// and sleeps. Only aligned words B88A..B89A are owned; no stored word table remains.
/// </summary>
internal static class SpeedBoosterEscapePlmProgramDefinitions
{
    /// <summary><c>$84:B88A</c>: first instruction of the controller list.</summary>
    internal const ushort Start = RoomPlmInstructionLists.SpeedBoosterEscape;

    internal const int WordCount = 9;

    internal static bool TryReadMechanicsWord(ushort address, out ushort value)
    {
        int offset = address - Start;
        if (offset >= 0 && (offset & 1) == 0 && offset / 2 < WordCount)
        {
            // Three install/callback/sleep records hand control to the next phase.
            value = (offset % 6) switch
            {
                0 => (ushort)RoomPlmInstruction.InstallPreInstruction,
                4 => (ushort)RoomPlmInstruction.Sleep,
                _ => (offset / 6) switch
                {
                    0 => SpeedBoosterEscapePlmRomData.WaitForSpeedBoosterPreInstruction,
                    1 => SpeedBoosterEscapePlmRomData.WaitForSamusLeftPreInstruction,
                    2 => SpeedBoosterEscapePlmRomData.AdvanceLavaPreInstruction,
                    _ => throw new InvalidOperationException("Invalid bounded escape phase."),
                },
            };
            return true;
        }

        value = 0;
        return false;
    }
}
