using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical four-block draw lists for grey door caps and the four shared clear-cap
/// frames. Bomb Torizo's special resident door uses the ordinary right-facing art.
/// </summary>
internal static class GreyDoorPlmDrawDefinitions
{
    /// <summary>The cleared-opening frame, which draws no cap.</summary>
    internal const int ClearFrame = -1;

    /// <summary>One of twenty twelve-byte records at $84:A677..A766.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame) : IPlmCapDrawShape
    {
        /// <summary>Left and right caps use a vertical four-cell run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>The direction the cap's four-cell run writes in.</summary>
        public PlmRunDirection Direction => Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;

        /// <summary>
        /// Four cells in native run order. Frame -1 clears collision; frame zero
        /// carries the native shootable/extension types. Opening frames are solid,
        /// except the middle cells of final left/down frames, which are air.
        /// Vertical tiles advance one per frame with a 32-tile inner-row stride;
        /// horizontal tiles alternate tileset rows, advancing two every two frames.
        /// Mirror bits follow orientation and the cell's half of the cap.
        /// </summary>
        public ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            bool inner = cell is 1 or 2;
            int tile;
            if (Frame < 0)
                tile = Vertical ? 0x82 + (inner ? 32 : 0) : 0x83 + (inner ? 0 : 1);
            else
                tile = Vertical ? 0xae + Frame + (inner ? 32 : 0) :
                    0xb2 + Frame / 2 * 2 + Frame % 2 * 32 + (inner ? 0 : 1);
            int flips = Vertical ? (Orientation == ColoredDoorOrientation.Right ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == ColoredDoorOrientation.Down ? 0x800 : 0);
            int collision = Frame < 0 ? 0 : Frame == 0 ? (cell == 0 ? 12 : Vertical ? 13 : 5) :
                Frame == 3 && inner && Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Down ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }
    }

    /// <summary>Decodes a pointer on the twelve-byte grid into clear or animated cap orientation and frame.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= ClearFrame and < 4, out draw);

    /// <summary>Enumerates all twenty native cap records for artwork import and export.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }

    /// <summary>Builds a temporary import or export draw list; runtime calculates physical cells directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompileCap<Draw>(request, TryDescribe, out list);

    /// <summary>Names a supported clear or grey animation frame by direction and frame number.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!TryDescribe(request, out var draw))
            throw new InvalidDataException($"Grey-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return draw.Frame < 0 ? $"clear-{direction}" : $"grey-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves a stable artwork name to its compiled cap draw pointer and shape.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>The four cleared openings, then each facing's four grey frames.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        for (int orientation = 0; orientation < 4; orientation++)
            yield return PlmDraw.Of(new Draw(DoorCapOrientations.FromTableIndex(orientation), ClearFrame));
        for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
                yield return PlmDraw.Of(new Draw(DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
