using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey door caps and the four shared cleared openings: their physical cells from
/// <see cref="GreyDoorPlmDrawDefinitions"/> and their editable artwork identities.
/// </summary>
internal static class GreyDoorCaps
{
    /// <summary>One of twenty twelve-byte records at $84:A677..A766.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame) : IPlmCapDrawShape
    {
        /// <summary>Left and right caps use a vertical four-cell run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>Encodes four blocks and the native vertical-run flag for the draw interpreter.</summary>
        internal int DirectionAndCount => Vertical ? DoorPlmDrawListDefinitions.VerticalCapRun : DoorPlmDrawListDefinitions.HorizontalCapRun;

        /// <summary>The cap run\'s native direction-and-count word.</summary>
        public int DirectionAndCountWord() => DirectionAndCount;

        /// <summary>The level word of <paramref name="cell"/>, one of the cap's four cells in native run order.</summary>
        public ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return GreyDoorPlmDrawDefinitions.Cells[this][cell].Raw;
        }
    }

    /// <summary>Decodes a pointer on the twelve-byte grid into clear or animated cap orientation and frame.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= GreyDoorPlmDrawDefinitions.ClearFrame and < 4, out draw);

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

    /// <summary>Builds a temporary import or export draw list; runtime reads physical cells directly.</summary>
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
            yield return PlmDraw.Of(new Draw(DoorCapOrientations.FromTableIndex(orientation), GreyDoorPlmDrawDefinitions.ClearFrame));
        for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
                yield return PlmDraw.Of(new Draw(DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
