using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.BotwoonWallObjectDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>How the cells of Botwoon's wall clear collide.</summary>
internal static class BotwoonWallObjectDraws
{
    /// <summary>
    /// How one of the nine cells of the single vertical run collides: air. What it shows is the clear's installed picture
    /// (#1557).
    /// </summary>
    internal static RoomCollisionType CollisionAt(int index)
    {
        if ((uint)index >= BlockCount) throw new ArgumentOutOfRangeException(nameof(index));
        return RoomCollisionType.Air;
    }
}
