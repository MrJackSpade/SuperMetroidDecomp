using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The blue-door caps: the closed alias and the four opening frames of each facing, their physical cells from
/// <see cref="BlueDoorPlmDrawDefinitions"/>, and their editable artwork identities.
/// </summary>
internal static class BlueDoorCaps
{
    /// <summary>One of the twenty blue-cap draws: four facings, each with the alias and four opening frames.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, int Frame) : IPlmCapDrawShape
    {
        /// <summary>Whether this cap's four blocks are laid out as a vertical run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>Native run header: four blocks with the vertical direction bit when needed.</summary>
        internal int DirectionAndCount => Vertical ? DoorPlmDrawListDefinitions.VerticalCapRun : DoorPlmDrawListDefinitions.HorizontalCapRun;

        /// <summary>The cap run\'s native direction-and-count word.</summary>
        public int DirectionAndCountWord() => DirectionAndCount;

        /// <summary>The level word of <paramref name="cell"/>, one of the cap's four cells in native run order.</summary>
        public ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            return BlueDoorPlmDrawDefinitions.Cells[this][cell].Raw;
        }
    }

    /// <summary>The closed cap facing <paramref name="orientation"/>, the alias that shows frame zero's artwork.</summary>
    internal static PlmDraw Alias(ColoredDoorOrientation orientation) =>
        PlmDraw.Of(new Draw(orientation, BlueDoorPlmDrawDefinitions.AliasFrame));

    /// <summary>Accepts a blue-cap draw: the alias or one of the four opening frames.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= BlueDoorPlmDrawDefinitions.AliasFrame and < 4, out draw);

    /// <summary>Sixteen original artwork identities; the four physical aliases share frame zero.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable
    {
        get
        {
            for (int orientation = 0; orientation < 4; orientation++)
                for (int frame = 0; frame < 4; frame++)
                {
                    TryGet(PlmDraw.Of(new Draw(DoorCapOrientations.FromTableIndex(orientation), frame)), out var draw);
                    yield return draw;
                }
        }
    }
    /// <summary>Maps a physical closed-cap alias to the matching editable frame-zero artwork.</summary>
    internal static PlmDraw VisualSource(PlmDraw request) =>
        request is PlmFrameDraw<Draw> { Frame: { Frame: BlueDoorPlmDrawDefinitions.AliasFrame } alias } ? PlmDraw.Of(alias with { Frame = 0 }) : request;

    /// <summary>Builds an import/export draw DTO from the cap's cells for a recognized native pointer.</summary>
    /// <remarks>Runtime draws read cells directly rather than retaining these DTOs.</remarks>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.TryCompileCap<Draw>(request, TryDescribe, out list);

    /// <summary>Names an editable cap frame by direction and opening phase.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!TryDescribe(request, out var draw))
            throw new InvalidDataException($"Blue-door draw {request} has no visual ID.");
        string direction = DoorCapOrientations.VisualName(draw.Orientation);
        return $"{direction}-frame-{Math.Max(draw.Frame, 0)}";
    }

    /// <summary>Finds a blue-door draw by its exact editable visual identity.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (var candidate in Editable)
            if (string.Equals(id, VisualId(candidate.Draw), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        list = default;
        return false;
    }
}
