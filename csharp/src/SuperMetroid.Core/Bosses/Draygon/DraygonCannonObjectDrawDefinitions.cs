using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These collision kinds and signed row offsets are gameplay definitions; what each frame shows is its installed
/// picture (#1557).
/// </summary>
internal static class DraygonCannonObjectDrawDefinitions
{
    /// <summary>The facing, shield state and frame of each of the twelve reachable cannon draws.</summary>
    internal static readonly Dictionary<DraygonCannonDraw, DraygonCannonDraws.Draw> Draws = new()
    {
        [DraygonCannonDraw.RightShieldA] = new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 0),
        [DraygonCannonDraw.RightShieldB] = new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 1),
        [DraygonCannonDraw.RightDamagedA] = new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 0),
        [DraygonCannonDraw.RightDamagedB] = new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 1),
        [DraygonCannonDraw.RightDamagedC] = new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 2),
        [DraygonCannonDraw.RightDamagedD] = new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 3),
        [DraygonCannonDraw.LeftShieldA] = new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 0),
        [DraygonCannonDraw.LeftShieldB] = new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 1),
        [DraygonCannonDraw.LeftDamagedA] = new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 0),
        [DraygonCannonDraw.LeftDamagedB] = new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 1),
        [DraygonCannonDraw.LeftDamagedC] = new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 2),
        [DraygonCannonDraw.LeftDamagedD] = new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 3),
    };

    /// <summary>A right shield frame's cells: the shootable core then its rim, over the core's extension and the lower rim.</summary>
    private static readonly RoomCollisionType[][] RightShield =
    [
        [RoomCollisionType.ShootableBlock, RoomCollisionType.Air],
        [RoomCollisionType.VerticalExtension, RoomCollisionType.Air],
    ];

    /// <summary>A right damaged frame's cells: the spiked core over its lower half, each beside an empty cell.</summary>
    private static readonly RoomCollisionType[][] RightDamaged =
    [
        [RoomCollisionType.SpikeBlock, RoomCollisionType.Air],
        [RoomCollisionType.SpikeBlock, RoomCollisionType.Air],
    ];

    /// <summary>A left shield frame's cells: the shootable core, the rim to its left, then the lower rim and the core's extension.</summary>
    private static readonly RoomCollisionType[][] LeftShield =
    [
        [RoomCollisionType.ShootableBlock],
        [RoomCollisionType.Air],
        [RoomCollisionType.Air, RoomCollisionType.VerticalExtension],
    ];

    /// <summary>A left damaged frame's cells: the spiked core, an empty cell to its left, then an empty cell and the lower half.</summary>
    private static readonly RoomCollisionType[][] LeftDamaged =
    [
        [RoomCollisionType.SpikeBlock],
        [RoomCollisionType.Air],
        [RoomCollisionType.Air, RoomCollisionType.SpikeBlock],
    ];

    /// <summary>
    /// How the cells of each of the twelve 2x2 cannon frames collide, one row per run in the original write order. Right
    /// lists have two horizontal rows; left lists split the first row into origin and left cells.
    /// </summary>
    internal static readonly Dictionary<DraygonCannonDraws.Draw, RoomCollisionType[][]> Collisions = new()
    {
        [new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 0)] = RightShield,
        [new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 1)] = RightShield,
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 0)] = RightDamaged,
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 1)] = RightDamaged,
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 2)] = RightDamaged,
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 3)] = RightDamaged,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 0)] = LeftShield,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 1)] = LeftShield,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 0)] = LeftDamaged,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 1)] = LeftDamaged,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 2)] = LeftDamaged,
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 3)] = LeftDamaged,
    };
}
