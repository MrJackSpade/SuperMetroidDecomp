using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Named bomb-special reactions selected through their PLM headers.</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>Sixteen normal entries followed by eight area tables.
    /// The bounded normal-behaviour view includes all eighty entries.</summary>
    public const int Count = 80;

    /// <summary>Selects crumble reveal, speed reveal, or the no-op PLM's
    /// deletion list. Normal indices 16 through 79 alias adjacent area tables;
    /// indices 26 through 29 therefore select Brinstar's four speed-block entries.</summary>
    internal static PlmProgramStart ProgramAt(int index)
    {
        if ((uint)index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 8)
            return PlmProgramStart.Typed(BlockReactionPlmPrograms.CrumbleReveal(index & 3));
        return index is 14 or 15 or >= 26 and <= 29
            ? PlmProgramStart.Typed(SpeedBoosterBlockProgram.BombReveal)
            : PlmProgramStart.Delete;
    }
}
