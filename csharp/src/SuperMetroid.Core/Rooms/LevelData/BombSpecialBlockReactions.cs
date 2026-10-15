using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Named bomb-special reactions selected through native bank-$94 PLM headers.</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>$94:9DA4..9E43: sixteen normal entries followed by eight area tables.
    /// The bounded normal-BTS compatibility view includes all eighty entries.</summary>
    public const int Count = 80;

    /// <summary>Selects crumble reveal, speed reveal, or the $84:B62F no-op PLM's
    /// deletion list. Normal indices $10..4F alias adjacent area tables;
    /// $1A..1D therefore select Brinstar's four speed-block entries.</summary>
    internal static PlmProgramStart ProgramAt(int index)
    {
        if ((uint)index >= Count)
            throw new IndexOutOfRangeException();
        if (index < 8)
            return PlmProgramStart.Native(RoomPlmInstructionLists.CrumbleRevealBySize(index & 3));
        return index is 14 or 15 or >= 26 and <= 29
            ? PlmProgramStart.Typed(SpeedBoosterBlockProgram.BombReveal)
            : PlmProgramStart.Delete;
    }
}
