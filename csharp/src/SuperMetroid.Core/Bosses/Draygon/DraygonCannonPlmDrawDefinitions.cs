using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These full level words and signed row offsets remain gameplay definitions.
/// </summary>
internal static class DraygonCannonPlmDrawDefinitions
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

    /// <summary>
    /// The cells of each of the twelve 2x2 cannon frames, one row per run in native write order. Right lists have two
    /// horizontal rows; left lists split the first row into origin and left cells. Shield frames advance two tiles;
    /// damaged frames advance one and leave the cells beside the core empty. Lower tiles are one tileset row (32 tiles)
    /// below their upper partners. Right art mirrors X.
    /// </summary>
    internal static readonly Dictionary<DraygonCannonDraws.Draw, RoomLevelWord[][]> Cells = new()
    {
        [new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 0)] = RightShield(0),
        [new(HorizontalDirection.Right, DraygonCannonCondition.Shielded, 1)] = RightShield(1),
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 0)] = RightDamaged(0),
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 1)] = RightDamaged(1),
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 2)] = RightDamaged(2),
        [new(HorizontalDirection.Right, DraygonCannonCondition.Damaged, 3)] = RightDamaged(3),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 0)] = LeftShield(0),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Shielded, 1)] = LeftShield(1),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 0)] = LeftDamaged(0),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 1)] = LeftDamaged(1),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 2)] = LeftDamaged(2),
        [new(HorizontalDirection.Left, DraygonCannonCondition.Damaged, 3)] = LeftDamaged(3),
    };

    /// <summary>Each reachable cannon draw's editable artwork key: its facing, condition and pose.</summary>
    internal static readonly Dictionary<DraygonCannonDraw, string> VisualIds = new()
    {
        [DraygonCannonDraw.RightShieldA] = "right-shield-a",
        [DraygonCannonDraw.RightShieldB] = "right-shield-b",
        [DraygonCannonDraw.RightDamagedA] = "right-damaged-a",
        [DraygonCannonDraw.RightDamagedB] = "right-damaged-b",
        [DraygonCannonDraw.RightDamagedC] = "right-damaged-c",
        [DraygonCannonDraw.RightDamagedD] = "right-damaged-d",
        [DraygonCannonDraw.LeftShieldA] = "left-shield-a",
        [DraygonCannonDraw.LeftShieldB] = "left-shield-b",
        [DraygonCannonDraw.LeftDamagedA] = "left-damaged-a",
        [DraygonCannonDraw.LeftDamagedB] = "left-damaged-b",
        [DraygonCannonDraw.LeftDamagedC] = "left-damaged-c",
        [DraygonCannonDraw.LeftDamagedD] = "left-damaged-d",
    };

    /// <summary>The empty cell a damaged frame leaves beside its core.</summary>
    private static RoomLevelWord Empty => RoomLevelWord.Create(0xff, BlockMirroring.None, RoomCollisionType.Air);

    /// <summary>A right shield frame: the shootable core then its rim, over the core's extension and the lower rim.</summary>
    private static RoomLevelWord[][] RightShield(int frame) =>
    [
        [
            RoomLevelWord.Create(0x114 + frame * 2, BlockMirroring.Horizontal, RoomCollisionType.ShootableBlock),
            RoomLevelWord.Create(0x113 + frame * 2, BlockMirroring.Horizontal, RoomCollisionType.Air),
        ],
        [
            RoomLevelWord.Create(0x134 + frame * 2, BlockMirroring.Horizontal, RoomCollisionType.VerticalExtension),
            RoomLevelWord.Create(0x133 + frame * 2, BlockMirroring.Horizontal, RoomCollisionType.Air),
        ],
    ];

    /// <summary>A right damaged frame: the spiked core over its lower half, each beside an empty cell.</summary>
    private static RoomLevelWord[][] RightDamaged(int frame) =>
    [
        [RoomLevelWord.Create(0x180 + frame, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock), Empty],
        [RoomLevelWord.Create(0x1a0 + frame, BlockMirroring.Horizontal, RoomCollisionType.SpikeBlock), Empty],
    ];

    /// <summary>A left shield frame: the shootable core, the rim to its left, then the lower rim and core extension.</summary>
    private static RoomLevelWord[][] LeftShield(int frame) =>
    [
        [RoomLevelWord.Create(0x114 + frame * 2, BlockMirroring.None, RoomCollisionType.ShootableBlock)],
        [RoomLevelWord.Create(0x113 + frame * 2, BlockMirroring.None, RoomCollisionType.Air)],
        [
            RoomLevelWord.Create(0x133 + frame * 2, BlockMirroring.None, RoomCollisionType.Air),
            RoomLevelWord.Create(0x134 + frame * 2, BlockMirroring.None, RoomCollisionType.VerticalExtension),
        ],
    ];

    /// <summary>A left damaged frame: the spiked core, an empty cell to its left, then an empty cell and the lower half.</summary>
    private static RoomLevelWord[][] LeftDamaged(int frame) =>
    [
        [RoomLevelWord.Create(0x180 + frame, BlockMirroring.None, RoomCollisionType.SpikeBlock)],
        [Empty],
        [Empty, RoomLevelWord.Create(0x1a0 + frame, BlockMirroring.None, RoomCollisionType.SpikeBlock)],
    ];
}
