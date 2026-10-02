namespace SuperMetroid.Core.Rooms;

/// <summary>Named bomb-special reactions selected through native bank-$94 PLM headers.</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>$94:9DA4..9E43: sixteen normal entries followed by eight area tables.
    /// The bounded normal-BTS compatibility view includes all eighty entries.</summary>
    public const int Count = 80;

    /// <summary>Selects crumble reveal, speed reveal, or the $84:B62F no-op PLM's
    /// deletion list. Normal indices $10..4F alias adjacent area tables;
    /// $1A..1D therefore select Brinstar's four speed-block entries.</summary>
    public static ushort InstructionListAt(int index)
    {
        if ((uint)index >= Count)
            throw new IndexOutOfRangeException();
        if (index < 8)
            return RoomPlmInstructionLists.CrumbleRevealBySize(index & 3);
        return index is 0x0e or 0x0f or >= 0x1a and <= 0x1d
            ? RoomPlmInstructionLists.BombReactionSpeedBlock
            : RoomPlmInstructionLists.Delete;
    }
}
