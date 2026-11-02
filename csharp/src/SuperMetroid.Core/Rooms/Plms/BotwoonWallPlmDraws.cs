using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.Plms.BotwoonWallPlmDrawDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The blocks of Botwoon's wall.</summary>
internal static class BotwoonWallPlmDraws
{
    /// <summary>Nine identical air blocks; indexed only within the single vertical run. Editable appearance is applied
    /// separately.</summary>
    internal static RoomTerrainBlock BlockAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return RoomTerrainBlocks.BlankAir;
    }
}
