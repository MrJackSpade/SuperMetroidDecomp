using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The yellow, green and red door caps: each colour's four frames per facing, their physical cells from
/// <see cref="ColoredDoorPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class ColoredDoorCaps
{
    /// <summary>Color, direction, and animation phase decoded from one native cap pointer.</summary>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame) : IPlmCapDrawShape
    {
        /// <summary>Left and right caps draw a vertical four-cell run in native list orientation.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>The direction the cap's four-block run writes in.</summary>
        public PlmRunDirection Direction => Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;

        /// <summary>The terrain of <paramref name="cell"/>, one of the cap's four cells in run order.</summary>
        public RoomTerrainBlock BlockAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return ColoredDoorPlmDrawDefinitions.Cells[this][cell];
        }
    }

    /// <summary>Decodes only the forty-eight aligned twelve-byte cap pointers into color, orientation, and frame.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        if (request is PlmFrameDraw<Draw> { Frame: { Frame: >= 0 and < 4 } cap })
        {
            draw = cap;
            return true;
        }
        draw = default;
        return false;
    }

    /// <summary>Enumerates yellow, green, and red cap frames in their native pointer order.</summary>
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

    /// <summary>Builds an import/export draw-list DTO while runtime collision cells are read directly.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.TryCompileCap<Draw>(request, TryDescribe, out list);

    /// <summary>Names a compiled cap frame by color, direction, and phase for editable artwork.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!TryDescribe(request, out var draw))
            throw new InvalidDataException($"Colored-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        string color = draw.Color switch
        {
            ColoredDoorColor.Yellow => "yellow",
            ColoredDoorColor.Green => "green",
            ColoredDoorColor.Red => "red",
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {draw.Color}."),
        };
        return $"{color}-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact ordinal visual identity back to its compiled draw record.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        ShotBlockPlmDraws.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Every colour's caps, each facing's four frames in turn.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                    yield return PlmDraw.Of(new Draw(color, DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
