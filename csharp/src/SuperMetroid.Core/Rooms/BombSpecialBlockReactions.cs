namespace SuperMetroid.Core.Rooms;

/// <summary>Normal bomb-special reactions from the cartridge's bank-$94 table.</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>
    /// $94:9DA4..9E43, BlockBombedReact_Special_Plm: eighty normal BTS entries.
    /// Entries not selecting crumble or speed reveals use the $84:B62F no-op PLM.
    /// </summary>
    public static ReadOnlySpan<ushort> InstructionLists => instructionLists;

    private static readonly ushort[] instructionLists = Build();

    private static ushort[] Build()
    {
        var entries = new ushort[80];
        Array.Fill(entries, RoomPlmInstructionLists.Delete);
        for (int index = 0; index < 8; index++)
            entries[index] = RoomPlmInstructionLists.CrumbleRevealBySize[index & 3];
        entries[0x0e] = entries[0x0f] = RoomPlmInstructionLists.BombReactionSpeedBlock;
        for (int index = 0x1a; index <= 0x1d; index++)
            entries[index] = RoomPlmInstructionLists.BombReactionSpeedBlock;
        return entries;
    }
}
