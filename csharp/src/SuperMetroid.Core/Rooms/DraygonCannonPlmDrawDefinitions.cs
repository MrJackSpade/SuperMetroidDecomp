namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These full level words and signed row offsets remain gameplay definitions.
/// </summary>
internal static class DraygonCannonPlmDrawDefinitions
{
    /// <summary>Right shield frame A at $84:9FCD.</summary>
    internal const ushort RightShieldA = 0x9fcd;
    /// <summary>Right shield frame B at $84:9FDD.</summary>
    internal const ushort RightShieldB = 0x9fdd;
    /// <summary>Right damaged frame A at $84:A02D.</summary>
    internal const ushort RightDamagedA = 0xa02d;
    /// <summary>Right damaged frame B at $84:A03D.</summary>
    internal const ushort RightDamagedB = 0xa03d;
    /// <summary>Right damaged frame C at $84:A04D.</summary>
    internal const ushort RightDamagedC = 0xa04d;
    /// <summary>Right damaged frame D at $84:A05D.</summary>
    internal const ushort RightDamagedD = 0xa05d;
    /// <summary>Left shield frame A at $84:A0ED.</summary>
    internal const ushort LeftShieldA = 0xa0ed;
    /// <summary>Left shield frame B at $84:A101.</summary>
    internal const ushort LeftShieldB = 0xa101;
    /// <summary>Left damaged frame A at $84:A165.</summary>
    internal const ushort LeftDamagedA = 0xa165;
    /// <summary>Left damaged frame B at $84:A179.</summary>
    internal const ushort LeftDamagedB = 0xa179;
    /// <summary>Left damaged frame C at $84:A18D.</summary>
    internal const ushort LeftDamagedC = 0xa18d;
    /// <summary>Left damaged frame D at $84:A1A1.</summary>
    internal const ushort LeftDamagedD = 0xa1a1;

    /// <summary>
    /// Twelve 2x2 cannon frames. Right lists have two horizontal rows; left lists
    /// split the first row into origin and left cells, preserving native write order.
    /// Shield frames advance two tiles; damaged frames advance one. Lower tiles
    /// are one tileset row (32 tiles) below their upper partners. Right art mirrors X.
    /// </summary>
    /// <param name="Right">Whether the cannon faces right; this controls horizontal mirroring and row layout.</param>
    /// <param name="Shield">Whether this draw uses the intact shield artwork rather than damaged tiles.</param>
    /// <param name="Frame">Zero-based frame within the selected shield or damaged sequence.</param>
    internal readonly record struct Draw(bool Right, bool Shield, int Frame)
    {
        /// <summary>Gets the number of horizontal row runs used by this orientation.</summary>
        internal int RunCount => Right ? 2 : 3;

        /// <summary>Validates a row-run index before calculating its cells or offsets.</summary>
        /// <param name="run">Zero-based run index.</param>
        /// <exception cref="IndexOutOfRangeException">The run is outside this draw's layout.</exception>
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        /// <summary>Returns the number of level words written by one row run.</summary>
        /// <param name="run">Zero-based run index.</param>
        /// <returns>Number of cells emitted in the run.</returns>
        internal int WordCount(int run)
        {
            CheckRun(run);
            return Right || run == 2 ? 2 : 1;
        }
        /// <summary>Returns the horizontal block offset between this run and the next.</summary>
        /// <param name="run">Zero-based run index.</param>
        /// <returns>Signed X offset for the next run.</returns>
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return !Right && run < 2 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Returns the vertical block offset between this run and the next.</summary>
        /// <param name="run">Zero-based run index.</param>
        /// <returns>Signed Y offset for the next run.</returns>
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == (Right ? 0 : 1) ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Calculates the cartridge level word for one cell in a run.</summary>
        /// <param name="run">Zero-based row-run index.</param>
        /// <param name="cell">Zero-based cell index within the run.</param>
        /// <returns>Tile and collision bits for the cell, or the empty level word for an absent damaged tile.</returns>
        /// <exception cref="IndexOutOfRangeException">The run or cell is outside this draw's layout.</exception>
        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            int row = Right ? run : run == 2 ? 1 : 0;
            bool core = Right ? cell == 0 : run == 0 || run == 2 && cell == 1;
            if (!Shield && !core) return 0x00ff;
            int tile = Shield ? 0x113 + Frame * 2 + (core ? 1 : 0) : 0x180 + Frame;
            int collision = !core ? 0 : Shield ? 12 + row : 10;
            return (ushort)(collision << 12 | (Right ? 0x400 : 0) | (tile + row * 32));
        }
    }

    /// <summary>Decodes a supported native draw pointer into orientation, shield state, and frame index.</summary>
    /// <param name="pointer">Bank-$84 pointer from a Draygon cannon draw instruction.</param>
    /// <param name="draw">Receives the decoded layout when the pointer names an owned frame.</param>
    /// <returns><see langword="true"/> when the pointer is an exact start of a supported draw frame.</returns>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool right, shield;
        int offset, stride;
        if (pointer >= RightShieldA && pointer <= RightShieldB)
        { right = true; shield = true; offset = pointer - RightShieldA; stride = 16; }
        else if (pointer >= RightDamagedA && pointer <= RightDamagedD)
        { right = true; shield = false; offset = pointer - RightDamagedA; stride = 16; }
        else if (pointer >= LeftShieldA && pointer <= LeftShieldB)
        { right = false; shield = true; offset = pointer - LeftShieldA; stride = 20; }
        else if (pointer >= LeftDamagedA && pointer <= LeftDamagedD)
        { right = false; shield = false; offset = pointer - LeftDamagedA; stride = 20; }
        else { draw = default; return false; }
        bool owned = offset % stride == 0;
        draw = owned ? new(right, shield, offset / stride) : default;
        return owned;
    }

    /// <summary>Enumerates all right- and left-facing shield and damaged frame pointers in cartridge order.</summary>
    /// <returns>The twelve supported draw pointers.</returns>
    private static IEnumerable<ushort> Pointers()
    {
        for (int frame = 0; frame < 2; frame++) yield return (ushort)(RightShieldA + frame * 16);
        for (int frame = 0; frame < 4; frame++) yield return (ushort)(RightDamagedA + frame * 16);
        for (int frame = 0; frame < 2; frame++) yield return (ushort)(LeftShieldA + frame * 20);
        for (int frame = 0; frame < 4; frame++) yield return (ushort)(LeftDamagedA + frame * 20);
    }
    /// <summary>Gets draw-list views for every supported cannon frame in pointer order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in Pointers())
            {
                TryGet(pointer, out var list);
                yield return list;
            }
        }
    }
    // Temporary artwork DTOs. Runtime drawing calculates cells directly.
    /// <summary>Builds the temporary draw-list representation for a supported native pointer.</summary>
    /// <param name="pointer">Bank-$84 pointer identifying a cannon frame.</param>
    /// <param name="list">Receives the generated row and cell layout on success.</param>
    /// <returns><see langword="true"/> when <paramref name="pointer"/> names a supported frame.</returns>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }
    /// <summary>Finds a draw-list layout by its stable presentation asset identifier.</summary>
    /// <param name="id">Visual ID associated with one of the supported pointer frames.</param>
    /// <param name="list">Receives the matching generated draw-list layout on success.</param>
    /// <returns><see langword="true"/> when the ID matches a supported frame.</returns>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }
    /// <summary>Maps a supported bank-$84 draw pointer to its stable presentation asset identifier.</summary>
    /// <param name="pointer">Pointer identifying a cannon frame.</param>
    /// <returns>The JSON-facing visual ID for the frame.</returns>
    /// <exception cref="InvalidDataException">The pointer does not identify a supported draw frame.</exception>
    internal static string VisualId(ushort pointer) => pointer switch
    {
        RightShieldA => "right-shield-a",
        RightShieldB => "right-shield-b",
        RightDamagedA => "right-damaged-a",
        RightDamagedB => "right-damaged-b",
        RightDamagedC => "right-damaged-c",
        RightDamagedD => "right-damaged-d",
        LeftShieldA => "left-shield-a",
        LeftShieldB => "left-shield-b",
        LeftDamagedA => "left-damaged-a",
        LeftDamagedB => "left-damaged-b",
        LeftDamagedC => "left-damaged-c",
        LeftDamagedD => "left-damaged-d",
        _ => throw new InvalidDataException(
            $"Draygon cannon draw ${pointer:X4} has no visual ID."),
    };

}
