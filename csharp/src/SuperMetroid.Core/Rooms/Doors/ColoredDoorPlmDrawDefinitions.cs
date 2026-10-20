using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Calculated yellow, green and red door-cap physical cells.</summary>
internal static class ColoredDoorPlmDrawDefinitions
{
    /// <summary>Yellow left-facing closed cap at $84:A767.</summary>
    private const ushort YellowLeft = 0xa767;
    /// <summary>Native stride for a four-cell cap draw record and its final displacement bytes.</summary>
    internal const int DrawListBytes = 12;

    /// <summary>Color, direction, and animation phase decoded from one native cap pointer.</summary>
    internal readonly record struct Draw(int Color, int Orientation, int Frame)
    {
        /// <summary>Left and right caps draw a vertical four-cell run in native list orientation.</summary>
        internal bool Vertical => Orientation < 2;
        /// <summary>Encodes the native run direction and four-cell count.</summary>
        internal ushort DirectionAndCount => Vertical ? (ushort)0x8004 : (ushort)4;

        /// <summary>
        /// Tiles advance four per color, one per vertical frame, or alternate
        /// tileset rows for horizontal frames. Mirrors follow orientation and
        /// cap half. Closed caps retain shootable/extension collision fields.
        /// Final middle cells clear for every left cap, green/red right caps
        /// and yellow down caps; the other final caps remain solid.
        /// </summary>
        internal ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new ArgumentOutOfRangeException(nameof(cell));
            bool inner = cell is 1 or 2;
            int tile = Color * 4 + (Vertical ? Frame + (inner ? 32 : 0) :
                0x10 + Frame / 2 * 2 + Frame % 2 * 32 + (inner ? 0 : 1));
            int flips = Vertical ? (Orientation == 1 ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == 3 ? 0x800 : 0);
            int collision = Frame == 0 ? (cell == 0 ? 12 : Vertical ? 13 : 5) :
                Frame == 3 && inner && (Orientation == 0 || (Orientation == 1 && Color != 0) || (Orientation == 3 && Color == 0)) ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }
    }

    /// <summary>Decodes only the forty-eight aligned twelve-byte cap pointers into color, orientation, and frame.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        int offset = pointer - YellowLeft;
        bool owned = offset >= 0 && offset < 48 * DrawListBytes && offset % DrawListBytes == 0;
        int index = offset / DrawListBytes;
        draw = owned ? new(index / 16, index / 4 % 4, index % 4) : default;
        return owned;
    }

    /// <summary>Enumerates yellow, green, and red cap frames in their native pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int index = 0; index < 48; index++)
            {
                TryGet((ushort)(YellowLeft + index * DrawListBytes), out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Builds an import/export draw-list DTO while runtime collision cells remain calculated.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = draw.WordAt(cell);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(draw.DirectionAndCount, words, new(0, 0)) });
        return true;
    }

    /// <summary>Names a compiled cap frame by color, direction, and phase for editable artwork.</summary>
    internal static string VisualId(ushort pointer)
    {
        if (!TryDescribe(pointer, out var draw))
            throw new InvalidDataException($"Colored-door draw ${pointer:X4} has no visual ID.");
        string direction = draw.Orientation switch { 0 => "left", 1 => "right", 2 => "up", _ => "down" };
        string color = draw.Color switch { 0 => "yellow", 1 => "green", _ => "red" };
        return $"{color}-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact ordinal visual identity back to its compiled draw record.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        for (int index = 0; index < 48; index++)
        {
            ushort pointer = (ushort)(YellowLeft + index * DrawListBytes);
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        }
        list = default;
        return false;
    }
}
