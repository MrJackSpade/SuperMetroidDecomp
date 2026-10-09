namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical four-block draw lists for grey door caps and the four shared clear-cap
/// frames. Bomb Torizo's special resident door uses the ordinary right-facing art.
/// </summary>
internal static class GreyDoorPlmDrawDefinitions
{
    /// <summary>Shared left-facing door-clear frame at $84:A677.</summary>
    private const ushort ClearLeft = 0xa677;
    /// <summary>Byte stride of one twelve-byte grey-door draw record in the native table.</summary>
    internal const int DrawListBytes = 12;

    /// <summary>One of twenty twelve-byte records at $84:A677..A766.</summary>
    /// <param name="Orientation">Door-facing orientation index shared by its clear and four opening frames.</param>
    /// <param name="Frame">Clear-cap selector when negative, otherwise the opening animation frame index.</param>
    internal readonly record struct Draw(int Orientation, int Frame)
    {
        /// <summary>Whether the orientation lays out the cap's four cells as two vertical rows.</summary>
        internal bool Vertical => Orientation < 2;
        /// <summary>Native PLM draw command selecting the cell traversal direction and four-cell count.</summary>
        internal ushort DirectionAndCount => Vertical ? (ushort)0x8004 : (ushort)4;

        /// <summary>
        /// Four cells in native run order. Frame -1 clears collision; frame zero
        /// carries the native shootable/extension types. Opening frames are solid,
        /// except the middle cells of final left/down frames, which are air.
        /// Vertical tiles advance one per frame with a 32-tile inner-row stride;
        /// horizontal tiles alternate tileset rows, advancing two every two frames.
        /// Mirror bits follow orientation and the cell's half of the cap.
        /// </summary>
        internal ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new IndexOutOfRangeException();
            bool inner = cell is 1 or 2;
            int tile;
            if (Frame < 0)
                tile = Vertical ? 0x82 + (inner ? 32 : 0) : 0x83 + (inner ? 0 : 1);
            else
                tile = Vertical ? 0xae + Frame + (inner ? 32 : 0) :
                    0xb2 + Frame / 2 * 2 + Frame % 2 * 32 + (inner ? 0 : 1);
            int flips = Vertical ? (Orientation == 1 ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == 3 ? 0x800 : 0);
            int collision = Frame < 0 ? 0 : Frame == 0 ? (cell == 0 ? 12 : Vertical ? 13 : 5) :
                Frame == 3 && inner && Orientation is 0 or 3 ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }
    }

    /// <summary>Decodes an aligned native table pointer into its orientation and clear/opening frame.</summary>
    /// <param name="pointer">Native draw-list pointer to classify.</param>
    /// <param name="draw">Receives the decoded orientation/frame pair for an owned pointer.</param>
    /// <returns><see langword="true"/> when the pointer selects one of the twenty table entries.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        int offset = pointer - ClearLeft;
        bool owned = offset >= 0 && offset < 20 * DrawListBytes && offset % DrawListBytes == 0;
        int index = offset / DrawListBytes;
        draw = owned ? new(index < 4 ? index : (index - 4) / 4, index < 4 ? -1 : (index - 4) % 4) : default;
        return owned;
    }

    /// <summary>Enumerates the twenty calculated clear and opening draw lists in pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int index = 0; index < 20; index++)
            {
                TryGet((ushort)(ClearLeft + index * DrawListBytes), out var draw);
                yield return draw;
            }
        }
    }

    // Temporary import/export DTOs; the runtime calculates cells directly.
    /// <summary>Builds the four-cell draw-list representation for a pointer owned by the grey-door table.</summary>
    /// <param name="pointer">Native draw-list pointer to resolve.</param>
    /// <param name="list">Receives the generated draw list when the pointer is recognized.</param>
    /// <returns><see langword="true"/> when the pointer identifies a table entry.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = draw.WordAt(cell);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(draw.DirectionAndCount, words, 0, 0) });
        return true;
    }

    /// <summary>Creates the stable visual identifier for a clear cap or one of the grey-door opening frames.</summary>
    /// <param name="pointer">Native draw-list pointer whose asset identity is requested.</param>
    /// <returns>A direction-specific identifier such as <c>clear-left</c> or <c>grey-right-frame-2</c>.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a grey-door draw record.</exception>
    internal static string VisualId(ushort pointer)
    {
        if (!TryDescribe(pointer, out var draw))
            throw new InvalidDataException($"Grey-door draw ${pointer:X4} has no visual ID.");
        string direction = draw.Orientation switch { 0 => "left", 1 => "right", 2 => "up", _ => "down" };
        return draw.Frame < 0 ? $"clear-{direction}" : $"grey-{direction}-frame-{draw.Frame}";
    }

    /// <summary>Resolves an exact ordinal clear/opening visual identifier to its calculated draw list.</summary>
    /// <param name="id">Identifier produced by <see cref="VisualId"/>.</param>
    /// <param name="list">Receives the matching draw list when one exists.</param>
    /// <returns><see langword="true"/> when the identifier belongs to this table.</returns>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        for (int index = 0; index < 20; index++)
        {
            ushort pointer = (ushort)(ClearLeft + index * DrawListBytes);
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        }
        list = default;
        return false;
    }
}
