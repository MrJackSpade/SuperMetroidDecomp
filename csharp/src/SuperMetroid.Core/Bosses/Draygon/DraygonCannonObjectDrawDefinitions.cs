using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The block shape of each of the twelve reachable right- and left-facing Draygon wall-cannon frames (#1608): the 2x2
/// square each frame covers around the cannon's core block, and how each cell collides. Unused diagonal orientations
/// are not claimed. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class DraygonCannonObjectDrawDefinitions
{
    /// <summary>A right shield frame: the shootable core with the rim to its right, over the core's extension and the lower rim.</summary>
    private static readonly RoomBlockShape RightShield = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock), new(new(1, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.VerticalExtension), new(new(1, 1), RoomCollisionType.Air),
    ]);

    /// <summary>A right damaged frame: the spiked core over its spiked lower half, each with an empty cell to its right.</summary>
    private static readonly RoomBlockShape RightDamaged = new(
    [
        new(new(0, 0), RoomCollisionType.SpikeBlock), new(new(1, 0), RoomCollisionType.Air),
        new(new(0, 1), RoomCollisionType.SpikeBlock), new(new(1, 1), RoomCollisionType.Air),
    ]);

    /// <summary>A left shield frame: the shootable core with the rim to its left, over the lower rim and the core's extension.</summary>
    private static readonly RoomBlockShape LeftShield = new(
    [
        new(new(0, 0), RoomCollisionType.ShootableBlock), new(new(-1, 0), RoomCollisionType.Air),
        new(new(-1, 1), RoomCollisionType.Air), new(new(0, 1), RoomCollisionType.VerticalExtension),
    ]);

    /// <summary>A left damaged frame: the spiked core over its spiked lower half, each with an empty cell to its left.</summary>
    private static readonly RoomBlockShape LeftDamaged = new(
    [
        new(new(0, 0), RoomCollisionType.SpikeBlock), new(new(-1, 0), RoomCollisionType.Air),
        new(new(-1, 1), RoomCollisionType.Air), new(new(0, 1), RoomCollisionType.SpikeBlock),
    ]);

    /// <summary>The block shape each of the twelve reachable cannon frames draws.</summary>
    internal static readonly Dictionary<DraygonCannonDraw, RoomBlockShape> Shapes = new()
    {
        [DraygonCannonDraw.RightShieldA] = RightShield,
        [DraygonCannonDraw.RightShieldB] = RightShield,
        [DraygonCannonDraw.RightDamagedA] = RightDamaged,
        [DraygonCannonDraw.RightDamagedB] = RightDamaged,
        [DraygonCannonDraw.RightDamagedC] = RightDamaged,
        [DraygonCannonDraw.RightDamagedD] = RightDamaged,
        [DraygonCannonDraw.LeftShieldA] = LeftShield,
        [DraygonCannonDraw.LeftShieldB] = LeftShield,
        [DraygonCannonDraw.LeftDamagedA] = LeftDamaged,
        [DraygonCannonDraw.LeftDamagedB] = LeftDamaged,
        [DraygonCannonDraw.LeftDamagedC] = LeftDamaged,
        [DraygonCannonDraw.LeftDamagedD] = LeftDamaged,
    };
}
