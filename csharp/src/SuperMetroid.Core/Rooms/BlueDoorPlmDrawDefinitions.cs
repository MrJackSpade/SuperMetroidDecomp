namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical four-block level writes for the four opening frames of each blue-door cap.
/// The bank-$84 instruction programs still own sound, frame timing, and deletion.
/// </summary>
internal static class BlueDoorPlmDrawDefinitions
{
    /// <summary>Left-facing blue-cap frame zero at $84:A9B3.</summary>
    internal const ushort LeftFrame0 = 0xa9b3;
    /// <summary>Right-facing blue-cap frame zero at $84:A9EF.</summary>
    private const ushort RightFrame0 = 0xa9ef;
    /// <summary>Up-facing blue-cap frame zero at $84:AA2B.</summary>
    private const ushort UpFrame0 = 0xaa2b;
    /// <summary>Down-facing blue-cap frame zero at $84:AA67.</summary>
    private const ushort DownFrame0 = 0xaa67;
    /// <summary>Byte length of each native four-block draw list including its terminator.</summary>
    internal const int DrawListBytes = 12;

    /// <summary>One of twenty twelve-byte records at $84:A9A7..AA96.</summary>
    /// <param name="Orientation">Facing index: left, right, up, or down, matching the five-record groups in the native table.</param>
    /// <param name="Frame">Opening frame from zero through three; minus one selects the pre-opening collision variant that looks like frame zero.</param>
    internal readonly record struct Draw(int Orientation, int Frame)
    {
        /// <summary>Whether the cap's four cells are arranged as a vertical pair of columns.</summary>
        internal bool Vertical => Orientation < 2;

        /// <summary>Native draw-list direction/count word for the cap's vertical or horizontal layout.</summary>
        internal ushort DirectionAndCount => Vertical ? (ushort)0x8004 : (ushort)4;

        /// <summary>
        /// Native A9A7..AA96 tiles follow orientation mirrors and bounded frame
        /// strides: vertical tiles advance one per frame with a 32-tile inner row;
        /// horizontal tiles alternate tileset rows and advance two per frame pair.
        /// Frame -1 aliases frame zero visually but makes only its first cell solid.
        /// Final vertical frames clear their middle cells; horizontal frames stay solid.
        /// </summary>
        internal ushort WordAt(int cell)
        {
            if ((uint)cell >= 4) throw new IndexOutOfRangeException();
            int frame = Math.Max(Frame, 0);
            bool inner = cell is 1 or 2;
            int tile = Vertical ? 0x0c + frame + (inner ? 32 : 0) :
                0x1c + frame / 2 * 2 + frame % 2 * 32 + (inner ? 0 : 1);
            int flips = Vertical ? (Orientation == 1 ? 0x400 : 0) | (cell >= 2 ? 0x800 : 0) :
                (cell < 2 ? 0x400 : 0) | (Orientation == 3 ? 0x800 : 0);
            int collision = frame == 0 ? (cell == 0 ? (Frame < 0 ? 8 : 12) : Vertical ? 13 : 5) :
                frame == 3 && Vertical && inner ? 0 : 8;
            return (ushort)(collision << 12 | flips | tile);
        }
    }

    /// <summary>Resolves an owned native draw-list pointer into its orientation and opening frame.</summary>
    /// <param name="pointer">Bank-$84 pointer to a twelve-byte blue-cap draw list.</param>
    /// <param name="draw">Receives the decoded orientation and frame, or the default value when the pointer is not owned.</param>
    /// <returns><see langword="true"/> when the pointer lies on a record boundary in the blue-cap table.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        int offset = pointer - (LeftFrame0 - DrawListBytes);
        bool owned = offset >= 0 && offset < 20 * DrawListBytes && offset % DrawListBytes == 0;
        int index = offset / DrawListBytes;
        draw = owned ? new(index / 5, index % 5 - 1) : default;
        return owned;
    }

    /// <summary>Sixteen original artwork identities; the four physical aliases share frame zero.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> Editable
    {
        get
        {
            for (int orientation = 0; orientation < 4; orientation++)
            for (int frame = 0; frame < 4; frame++)
            {
                TryGet((ushort)(LeftFrame0 + orientation * 60 + frame * DrawListBytes), out var draw);
                yield return draw;
            }
        }
    }
    /// <summary>Maps each pre-opening alias to the matching orientation's frame-zero artwork pointer.</summary>
    /// <param name="pointer">Native blue-cap draw-list pointer.</param>
    /// <returns>The frame-zero pointer for a pre-opening alias, or <paramref name="pointer"/> when it is not an alias.</returns>
    internal static ushort VisualSource(ushort pointer) => pointer switch
    {
        LeftFrame0 - DrawListBytes => LeftFrame0,
        RightFrame0 - DrawListBytes => RightFrame0,
        UpFrame0 - DrawListBytes => UpFrame0,
        DownFrame0 - DrawListBytes => DownFrame0,
        _ => pointer,
    };

    // Temporary import/export DTOs; the runtime calculates cells directly.
    /// <summary>Builds the four-cell draw list represented by an owned native pointer.</summary>
    /// <param name="pointer">Bank-$84 pointer to a blue-cap draw list.</param>
    /// <param name="list">Receives the converted draw list, or the default value when the pointer is unrecognized.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> identifies a record in this table.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var words = new ushort[4];
        for (int cell = 0; cell < 4; cell++) words[cell] = draw.WordAt(cell);
        list = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(draw.DirectionAndCount, words, 0, 0) });
        return true;
    }

    /// <summary>Creates the stable facing-and-frame key used to associate blue-cap artwork with its native list.</summary>
    /// <param name="pointer">Pointer to a described blue-cap draw list.</param>
    /// <returns>A key such as <c>left-frame-2</c>; pre-opening aliases use frame zero.</returns>
    /// <exception cref="InvalidDataException">The pointer does not belong to the blue-cap draw table.</exception>
    internal static string VisualId(ushort pointer)
    {
        if (!TryDescribe(pointer, out var draw))
            throw new InvalidDataException($"Blue-door draw ${pointer:X4} has no visual ID.");
        string direction = draw.Orientation switch { 0 => "left", 1 => "right", 2 => "up", _ => "down" };
        return $"{direction}-frame-{Math.Max(draw.Frame, 0)}";
    }

    /// <summary>Finds the editable draw list whose stable artwork key matches the supplied identifier.</summary>
    /// <param name="id">Facing-and-frame key produced by <see cref="VisualId(ushort)"/>.</param>
    /// <param name="list">Receives the matching draw list, or the default value when no key matches.</param>
    /// <returns><see langword="true"/> when an editable blue-cap frame has the requested key.</returns>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (var candidate in Editable)
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        list = default;
        return false;
    }
}
