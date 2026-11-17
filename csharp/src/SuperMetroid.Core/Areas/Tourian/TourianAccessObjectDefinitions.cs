using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>The changes the room objects that clear or crumble the Tourian access floor make.</summary>
internal static class TourianAccessObjectDefinitions
{
    /// <summary>The block the access floor's clearing or crumbling room object stands at, where the crumble starts down.</summary>
    internal static readonly Position16<RoomBlocks> FloorBlock = new(6, 12);

    /// <summary>The rows the crumble walks down.</summary>
    internal const int CrumbleRows = 6;

    /// <summary>How long each crumble image shows.</summary>
    internal const int CrumbleFrameDuration = 4;

    /// <summary>One row down: the crumble's step to its next row.</summary>
    private static readonly Offset16<RoomBlocks> NextRow = new(0, 1);

    /// <summary>One row's crumble, three breakup images and then the empty row, the first drawn after <paramref name="moveBefore"/>.</summary>
    private static TerrainChangeFrame[] CrumbleRow(Offset16<RoomBlocks> moveBefore) =>
    [
        new(TourianAccessDraws.Crumble(BlockBreakupStage.Rock), CrumbleFrameDuration, moveBefore),
        new(TourianAccessDraws.Crumble(BlockBreakupStage.Pieces), CrumbleFrameDuration),
        new(TourianAccessDraws.Crumble(BlockBreakupStage.Dust), CrumbleFrameDuration),
        new(TourianAccessDraws.Family.Of(TourianAccessDraw.EmptyRow), CrumbleFrameDuration),
    ];

    /// <summary>
    /// Each floor state's change, which its room object makes: the crumble walks down six rows, crumbling each through
    /// four images; the clear erases the access at once.
    /// </summary>
    internal static readonly IReadOnlyDictionary<TourianAccessFloor, TerrainChange> Floors =
        new Dictionary<TourianAccessFloor, TerrainChange>
        {
            [TourianAccessFloor.Clear] = TerrainChange.DrawnOnce(TourianAccessDraws.Family.Of(TourianAccessDraw.Clear)),
            [TourianAccessFloor.Crumble] = new TerrainChange(1,
            [
                .. CrumbleRow(Offset16<RoomBlocks>.Zero),
                .. Enumerable.Repeat(NextRow, CrumbleRows - 1).SelectMany(CrumbleRow),
            ]),
        };
}
