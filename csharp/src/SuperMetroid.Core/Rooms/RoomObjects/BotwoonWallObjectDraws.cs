using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.BotwoonWallObjectDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The blocks of Botwoon's wall.</summary>
internal static class BotwoonWallObjectDraws
{
    /// <summary>Nine identical air blocks; indexed only within the single vertical run. Editable appearance is applied
    /// separately.</summary>
    internal static RoomTerrainBlock BlockAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return RoomTerrainBlocks.BlankAir;
    }
}
