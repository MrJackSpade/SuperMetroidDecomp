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
    internal readonly record struct Draw(bool Right, bool Shield, int Frame)
    {
        internal int RunCount => Right ? 2 : 3;
        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }
        internal int WordCount(int run)
        {
            CheckRun(run);
            return Right || run == 2 ? 2 : 1;
        }
        internal sbyte NextX(int run)
        {
            CheckRun(run);
            return !Right && run < 2 ? (sbyte)-1 : (sbyte)0;
        }
        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == (Right ? 0 : 1) ? (sbyte)1 : (sbyte)0;
        }
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

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool right, shield;
        int offset, stride;
        if (pointer is >= RightShieldA and <= RightShieldB)
        { right = true; shield = true; offset = pointer - RightShieldA; stride = 16; }
        else if (pointer is >= RightDamagedA and <= RightDamagedD)
        { right = true; shield = false; offset = pointer - RightDamagedA; stride = 16; }
        else if (pointer is >= LeftShieldA and <= LeftShieldB)
        { right = false; shield = true; offset = pointer - LeftShieldA; stride = 20; }
        else if (pointer is >= LeftDamagedA and <= LeftDamagedD)
        { right = false; shield = false; offset = pointer - LeftDamagedA; stride = 20; }
        else { draw = default; return false; }
        bool owned = offset % stride == 0;
        draw = owned ? new(right, shield, offset / stride) : default;
        return owned;
    }

    private static IEnumerable<ushort> Pointers()
    {
        for (int frame = 0; frame < 2; frame++) yield return (ushort)(RightShieldA + frame * 16);
        for (int frame = 0; frame < 4; frame++) yield return (ushort)(RightDamagedA + frame * 16);
        for (int frame = 0; frame < 2; frame++) yield return (ushort)(LeftShieldA + frame * 20);
        for (int frame = 0; frame < 4; frame++) yield return (ushort)(LeftDamagedA + frame * 20);
    }
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
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet(pointer, out list);
        list = default;
        return false;
    }
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
