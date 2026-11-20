using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Botwoon's wall clear and its block shape (#1608): the nine open blocks down the wall's column from its top block. The
/// crumble's frames are shot-block breakup draws. What the clear shows is its installed picture (#1557).
/// </summary>
internal static class BotwoonWallObjectDrawDefinitions
{
    /// <summary>The nine cleared wall blocks of an already-defeated room; the crumble frames are shot-block breakup draws.</summary>
    internal static RoomObjectFrameDraw<BotwoonWallDraw> Clear { get; } = BotwoonWallObjectDraws.Family.Of(BotwoonWallDraw.Clear);

    /// <summary>The cleared wall: nine open blocks down the column from the wall's top block.</summary>
    private static readonly RoomBlockShape ClearedWall = RoomBlockShapes.Line(down: true, 9, RoomCollisionType.Air);

    /// <summary>The block shape the wall's own frame draws.</summary>
    internal static readonly Dictionary<BotwoonWallDraw, RoomBlockShape> Shapes = new()
    {
        [BotwoonWallDraw.Clear] = ClearedWall,
    };
}
