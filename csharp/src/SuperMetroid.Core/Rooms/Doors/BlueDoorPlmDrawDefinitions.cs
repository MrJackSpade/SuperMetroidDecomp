using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Physical four-block level writes for the four opening frames of each blue-door cap.
/// The bank-$84 instruction programs still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorPlmDrawDefinitions
{
    /// <summary>The physical closed-cap alias frame, which shows frame zero's artwork with only its first cell solid.</summary>
    internal const int AliasFrame = -1;

    /// <summary>One of the twenty blue-cap draws: four facings, each with the alias and four opening frames.</summary>
    internal readonly record struct Draw(ColoredDoorOrientation Orientation, int Frame) : IPlmCapDrawShape
    {
        /// <summary>Whether this cap's four blocks are laid out as a vertical run.</summary>
        internal bool Vertical => Orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right;
        /// <summary>Native run header: four blocks with the vertical direction bit when needed.</summary>
        internal int DirectionAndCount => Vertical ? 0x8004 : 4;

        /// <summary>The cap run\'s native direction-and-count word.</summary>
        public int DirectionAndCountWord() => DirectionAndCount;

        /// <summary>
        /// Native A9A7..AA96 tiles follow orientation mirrors and bounded frame
        /// strides: vertical tiles advance one per frame with a 32-tile inner row;
        /// horizontal tiles alternate tileset rows and advance two per frame pair.
        /// Frame -1 aliases frame zero visually but makes only its first cell solid.
        /// Final vertical frames clear their middle cells; horizontal frames stay solid.
        /// </summary>
        public ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            int frame = Math.Max(Frame, 0);
            bool inner = cell is 1 or 2;
            int tile = Vertical ? 0x0c + frame + (inner ? 32 : 0) :
                0x1c + frame / 2 * 2 + frame % 2 * 32 + (inner ? 0 : 1);
            int flips = Vertical ? (Orientation == ColoredDoorOrientation.Right ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == ColoredDoorOrientation.Down ? 0x800 : 0);
            int collision = frame == 0 ? (cell == 0 ? (Frame < 0 ? 8 : 12) : Vertical ? 13 : 5) :
                frame == 3 && Vertical && inner ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }
    }

    /// <summary>The closed cap facing <paramref name="orientation"/>, the alias that shows frame zero's artwork.</summary>
    internal static PlmDraw Alias(ColoredDoorOrientation orientation) => PlmDraw.Of(new Draw(orientation, AliasFrame));

    /// <summary>Accepts a blue-cap draw: the alias or one of the four opening frames.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw) =>
        PlmDraw.TryFrame(request, static (Draw cap) => cap.Frame is >= AliasFrame and < 4, out draw);

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
        request is PlmFrameDraw<Draw> { Frame: { Frame: AliasFrame } alias } ? PlmDraw.Of(alias with { Frame = 0 }) : request;

    /// <summary>Builds an import/export draw DTO from calculated cells for a recognized native pointer.</summary>
    /// <remarks>Runtime draws calculate cells directly rather than retaining these DTOs.</remarks>
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
