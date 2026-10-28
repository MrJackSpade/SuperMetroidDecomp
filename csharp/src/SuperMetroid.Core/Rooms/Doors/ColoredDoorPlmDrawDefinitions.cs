using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Calculated yellow, green and red door-cap physical cells.</summary>
internal static class ColoredDoorPlmDrawDefinitions
{

    /// <summary>Color, direction, and animation phase decoded from one native cap pointer.</summary>
    internal readonly record struct Draw(ColoredDoorColor Color, ColoredDoorOrientation Orientation, [property: IntegerQuantity("cap animation frame index")] int Frame) : IPlmCapDrawShape
    {
        /// <summary>Left and right caps draw a vertical four-cell run in native list orientation.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>The direction the cap's four-cell run writes in.</summary>
        public PlmRunDirection Direction => Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;

        /// <summary>
        /// Tiles advance four per color, one per vertical frame, or alternate
        /// tileset rows for horizontal frames. Mirrors follow orientation and
        /// cap half. Closed caps retain shootable/extension collision fields.
        /// Final middle cells clear for every left cap, green/red right caps
        /// and yellow down caps; the other final caps remain solid.
        /// </summary>
        public ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            bool inner = cell is 1 or 2;
            int tile = ColorTileOffset + (Vertical ? Frame + (inner ? 32 : 0) :
                0x10 + Frame / 2 * 2 + Frame % 2 * 32 + (inner ? 0 : 1));
            int flips = Vertical ? (Orientation == ColoredDoorOrientation.Right ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == ColoredDoorOrientation.Down ? 0x800 : 0);
            int collision = Frame == 0 ? (cell == 0 ? 12 : Vertical ? 13 : 5) :
                Frame == 3 && inner && (Orientation == ColoredDoorOrientation.Left || (Orientation == ColoredDoorOrientation.Right && Color != ColoredDoorColor.Yellow) || (Orientation == ColoredDoorOrientation.Down && Color == ColoredDoorColor.Yellow)) ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }

        /// <summary>How far each colour's cap tiles sit from the yellow cap's: four tiles per colour.</summary>
        private int ColorTileOffset => Color switch
        {
            ColoredDoorColor.Yellow => 0,
            ColoredDoorColor.Green => 4,
            ColoredDoorColor.Red => 8,
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {Color}."),
        };
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

    /// <summary>Builds an import/export draw-list DTO while runtime collision cells remain calculated.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompileCap<Draw>(request, TryDescribe, out list);

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
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Draws(), VisualId, TryGet, id, out list);

    /// <summary>Every colour's caps, each facing's four frames in turn.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                    yield return PlmDraw.Of(new Draw(color, DoorCapOrientations.FromTableIndex(orientation), frame));
    }
}
