using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four horizontal crumble frames and six-row clear draw selected by the
/// Tourian access-floor PLMs. These words are physical level mutations;
/// replaceable block appearance is a separate concern. Uniform row words and
/// origin-relative continuation geometry are calculated without stored layouts
/// (<see cref="TourianAccessDraws"/>).
/// </summary>
internal static class TourianAccessPlmDrawDefinitions
{
    /// <summary>
    /// The rows and air block of each Tourian access draw. Clear selects six blank rows; empty selects one. The three
    /// crumble records show successive blocks $53..55, unmirrored. Every row is four air cells.
    /// </summary>
    internal static readonly IReadOnlyDictionary<TourianAccessDraw, TourianAccessDrawFill> Fills =
        new Dictionary<TourianAccessDraw, TourianAccessDrawFill>
        {
            [TourianAccessDraw.Clear] = new(6, RoomTerrainBlocks.BlankAir),
            [TourianAccessDraw.EmptyRow] = new(1, RoomTerrainBlocks.BlankAir),
            [TourianAccessDraw.CrumbleFirst] = new(1, RoomTerrainBlock.Of(0x53, BlockMirroring.None, RoomCollisionType.Air)),
            [TourianAccessDraw.CrumbleSecond] = new(1, RoomTerrainBlock.Of(0x54, BlockMirroring.None, RoomCollisionType.Air)),
            [TourianAccessDraw.CrumbleThird] = new(1, RoomTerrainBlock.Of(0x55, BlockMirroring.None, RoomCollisionType.Air)),
        };

    /// <summary>The artwork identity naming each empty, crumble or six-row clear frame in visual files.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessDraw, string> VisualIds = new Dictionary<TourianAccessDraw, string>
    {
        [TourianAccessDraw.EmptyRow] = "crumble-empty-row",
        [TourianAccessDraw.CrumbleFirst] = "crumble-frame-0",
        [TourianAccessDraw.CrumbleSecond] = "crumble-frame-1",
        [TourianAccessDraw.CrumbleThird] = "crumble-frame-2",
        [TourianAccessDraw.Clear] = "clear-six-rows",
    };
}
