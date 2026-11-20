using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.DefinitionRun;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The collision runs the door caps' and eye doors' draws share, each stated once (#2557): a cap's blocks from its first
/// block down its column or along its row, and an eye's two blocks down its column.
/// </summary>
internal static class DoorCapCollisionDefinitions
{
    /// <summary>A cap whose four blocks are all solid.</summary>
    internal static IReadOnlyList<RoomCollisionType> SolidCap { get; } = Repeated(RoomCollisionType.SolidBlock, 4);

    /// <summary>A closed cap in a column: its shootable first block, and the three below continuing it.</summary>
    internal static IReadOnlyList<RoomCollisionType> ShootableColumnCap { get; } =
        [RoomCollisionType.ShootableBlock, .. Repeated(RoomCollisionType.VerticalExtension, 3)];

    /// <summary>A closed cap in a row: its shootable first block, and the three beside continuing it.</summary>
    internal static IReadOnlyList<RoomCollisionType> ShootableRowCap { get; } =
        [RoomCollisionType.ShootableBlock, .. Repeated(RoomCollisionType.HorizontalExtension, 3)];

    /// <summary>A closed cap in a column that shots do not open: its solid first block, and the three below continuing it.</summary>
    internal static IReadOnlyList<RoomCollisionType> SolidColumnCap { get; } =
        [RoomCollisionType.SolidBlock, .. Repeated(RoomCollisionType.VerticalExtension, 3)];

    /// <summary>A closed cap in a row that shots do not open: its solid first block, and the three beside continuing it.</summary>
    internal static IReadOnlyList<RoomCollisionType> SolidRowCap { get; } =
        [RoomCollisionType.SolidBlock, .. Repeated(RoomCollisionType.HorizontalExtension, 3)];

    /// <summary>A nearly open cap: its end blocks solid and its two middle blocks clear.</summary>
    internal static IReadOnlyList<RoomCollisionType> MiddleClearCap { get; } =
        [RoomCollisionType.SolidBlock, .. Repeated(RoomCollisionType.Air, 2), RoomCollisionType.SolidBlock];

    /// <summary>A cleared cap: its four blocks all clear.</summary>
    internal static IReadOnlyList<RoomCollisionType> ClearedCap { get; } = Repeated(RoomCollisionType.Air, 4);

    /// <summary>An eye door's one-block middle or bottom part, whose block is spiked in every look.</summary>
    internal static IReadOnlyList<RoomCollisionType> SpikedPart { get; } = [RoomCollisionType.SpikeBlock];

    /// <summary>An eye whose two blocks are solid: closed, half open, flinching or open but solid.</summary>
    internal static IReadOnlyList<RoomCollisionType> SolidEye { get; } = Repeated(RoomCollisionType.SolidBlock, 2);

    /// <summary>The open eye: its shootable first block, and the one below continuing it.</summary>
    internal static IReadOnlyList<RoomCollisionType> ShootableEye { get; } =
        [RoomCollisionType.ShootableBlock, RoomCollisionType.VerticalExtension];
}
