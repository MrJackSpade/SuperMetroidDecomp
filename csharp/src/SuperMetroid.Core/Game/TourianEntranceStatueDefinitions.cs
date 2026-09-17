namespace SuperMetroid.Core.Game;

/// <summary>Compiled gameplay selectors for the three Tourian entrance statue enemy slots.</summary>
internal static class TourianEntranceStatueDefinitions
{
    /// <summary>$AA:D810..D815, instruction lists selected by even parameter zero, two, or four.</summary>
    private static ReadOnlySpan<ushort> InitialInstructionLists => [0xd7b9, 0xd7a5, 0xd7af];

    /// <summary>Returns the native instruction list selected by the statue's byte-offset parameter.</summary>
    internal static ushort GetInitialInstruction(ushort parameter)
    {
        if (parameter > 4 || (parameter & 1) != 0)
            throw new ArgumentOutOfRangeException(nameof(parameter));
        return InitialInstructionLists[parameter >> 1];
    }
}
