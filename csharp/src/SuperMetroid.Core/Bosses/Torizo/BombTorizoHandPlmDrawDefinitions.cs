using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Physical block draws selected by Bomb Torizo's resident hand PLM. These
/// terrain blocks and run offsets are cartridge mechanics, not editable art.
/// </summary>
internal static class BombTorizoHandPlmDrawDefinitions
{
    /// <summary>$84:989D: a cleared cell, air with blank tile FF.</summary>
    private static readonly RoomTerrainBlock ClearedCell = RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air);

    /// <summary>
    /// Each draw's horizontal strips in native run order. Intact ($84:9877) is four strips of solid blocks whose tiles are
    /// consecutive within each spatial role: origin pair 65..66, left singleton 64, upper pair 45..46, lower triple
    /// 47..49. Cleared ($84:989D) splits the origin row into two halves, then clears the lower, top and upper rows of a
    /// four-by-four area.
    /// </summary>
    internal static readonly Dictionary<BombTorizoHandDraw, BombTorizoHandStrip[]> Strips = new()
    {
        [BombTorizoHandDraw.Intact] =
        [
            new(new(0, 0), [RoomTerrainBlock.Of(101, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(102, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(-1, 0), [RoomTerrainBlock.Of(100, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(0, -1), [RoomTerrainBlock.Of(69, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(70, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
            new(new(-1, 1), [RoomTerrainBlock.Of(71, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(72, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomTerrainBlock.Of(73, BlockMirroring.None, RoomCollisionType.SolidBlock)]),
        ],
        [BombTorizoHandDraw.Cleared] =
        [
            new(new(0, 0), [ClearedCell, ClearedCell]),
            new(new(-2, 0), [ClearedCell, ClearedCell]),
            new(new(-2, 1), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
            new(new(-2, -2), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
            new(new(-2, -1), [ClearedCell, ClearedCell, ClearedCell, ClearedCell]),
        ],
    };

    /// <summary>The intact and cleared hand draws' stable artwork identities.</summary>
    internal static readonly Dictionary<BombTorizoHandDraw, string> VisualIds = new()
    {
        [BombTorizoHandDraw.Intact] = "intact",
        [BombTorizoHandDraw.Cleared] = "cleared",
    };
}
