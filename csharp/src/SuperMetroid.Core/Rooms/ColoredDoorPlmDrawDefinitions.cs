namespace SuperMetroid.Core.Rooms;

/// <summary>Calculated yellow, green and red door-cap physical cells.</summary>
internal static class ColoredDoorPlmDrawDefinitions
{
    /// <summary>Yellow left-facing closed cap at $84:A767.</summary>
    private const ushort YellowLeft = 0xa767;
    /// <summary>Byte stride between consecutive twelve-byte colored-door draw records in the native table.</summary>
    internal const int DrawListBytes = 12;

    /// <summary>One of forty-eight twelve-byte records at $84:A767..A9A6.</summary>
    /// <param name="Color">Door palette family: yellow, green, or red.</param>
    /// <param name="Orientation">Facing or travel direction encoded by the record's place in the native table.</param>
    /// <param name="Frame">Position of the cap within its four-frame draw sequence.</param>
    internal readonly record struct Draw(int Color, int Orientation, int Frame)
    {
        /// <summary>Whether this orientation uses a vertically stacked pair of cap cells.</summary>
        internal bool Vertical => Orientation < 2;
        /// <summary>Native PLM draw command combining traversal direction with the four-cell count.</summary>
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
            if ((uint)cell >= 4) throw new IndexOutOfRangeException();
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

    /// <summary>Maps an aligned pointer in the colored-door table to its palette, orientation, and frame indices.</summary>
    /// <param name="pointer">Native draw-list pointer to classify.</param>
    /// <param name="draw">Receives the decoded record when the pointer is owned by this table.</param>
    /// <returns><see langword="true"/> when the pointer selects one of the forty-eight records.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        int offset = pointer - YellowLeft;
        bool owned = offset >= 0 && offset < 48 * DrawListBytes && offset % DrawListBytes == 0;
        int index = offset / DrawListBytes;
        draw = owned ? new(index / 16, index / 4 % 4, index % 4) : default;
        return owned;
    }

    /// <summary>Enumerates all forty-eight calculated draw lists in native pointer order.</summary>
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

    // Temporary import/export DTOs; the runtime calculates cells directly.
    /// <summary>Builds the temporary draw-list representation for one pointer owned by the colored-door table.</summary>
    /// <param name="pointer">Native draw-list pointer to resolve.</param>
    /// <param name="list">Receives the four calculated cell words when the pointer is recognized.</param>
    /// <returns><see langword="true"/> when a matching draw list was produced.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = draw.WordAt(cell);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(draw.DirectionAndCount, words, 0, 0) });
        return true;
    }

    /// <summary>Creates the stable asset identifier for a colored-door palette, direction, and animation frame.</summary>
    /// <param name="pointer">Native draw-list pointer whose visual identity is requested.</param>
    /// <returns>An identifier such as <c>yellow-left-frame-0</c>.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a colored-door draw record.</exception>
    internal static string VisualId(ushort pointer)
    {
        if (!TryDescribe(pointer, out var draw))
            throw new InvalidDataException($"Colored-door draw ${pointer:X4} has no visual ID.");
        string direction = draw.Orientation switch { 0 => "left", 1 => "right", 2 => "up", _ => "down" };
        string color = draw.Color switch { 0 => "yellow", 1 => "green", _ => "red" };
        return $"{color}-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact ordinal visual identifier to its calculated draw list.</summary>
    /// <param name="id">Stable identifier produced by <see cref="VisualId"/>.</param>
    /// <param name="list">Receives the matching draw list when the identifier is recognized.</param>
    /// <returns><see langword="true"/> when one of the table's visual identifiers matches.</returns>
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
