using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The four horizontal crumble frames and six-row clear draw selected by the Tourian access-floor room objects: rows of four air
/// cells, stacked down from the origin (<see cref="TourianAccessDraws"/>). What each frame shows is its installed picture
/// (#1557).
/// </summary>
internal static class TourianAccessObjectDrawDefinitions
{
    /// <summary>The rows each Tourian access draw writes: clear writes six; the empty row and the three crumbles one.</summary>
    internal static readonly IReadOnlyDictionary<TourianAccessDraw, int> Rows =
        new Dictionary<TourianAccessDraw, int>
        {
            [TourianAccessDraw.Clear] = 6,
            [TourianAccessDraw.EmptyRow] = 1,
            [TourianAccessDraw.CrumbleFirst] = 1,
            [TourianAccessDraw.CrumbleSecond] = 1,
            [TourianAccessDraw.CrumbleThird] = 1,
        };

    /// <summary>The cells each row writes.</summary>
    internal const int RowWidth = 4;

    /// <summary>How every cell of every Tourian access draw collides.</summary>
    internal const RoomCollisionType Collision = RoomCollisionType.Air;
}
