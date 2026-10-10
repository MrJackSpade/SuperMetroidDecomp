using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The twelve bank-$84 draw lists selected by the reachable Draygon cannon PLM programs.</summary>
internal enum DraygonCannonDraw : ushort
{
    /// <summary>Right shield frame A at $84:9FCD.</summary>
    RightShieldA = 0x9fcd,
    /// <summary>Right shield frame B at $84:9FDD.</summary>
    RightShieldB = 0x9fdd,
    /// <summary>Right damaged frame A at $84:A02D.</summary>
    RightDamagedA = 0xa02d,
    /// <summary>Right damaged frame B at $84:A03D.</summary>
    RightDamagedB = 0xa03d,
    /// <summary>Right damaged frame C at $84:A04D.</summary>
    RightDamagedC = 0xa04d,
    /// <summary>Right damaged frame D at $84:A05D.</summary>
    RightDamagedD = 0xa05d,
    /// <summary>Left shield frame A at $84:A0ED.</summary>
    LeftShieldA = 0xa0ed,
    /// <summary>Left shield frame B at $84:A101.</summary>
    LeftShieldB = 0xa101,
    /// <summary>Left damaged frame A at $84:A165.</summary>
    LeftDamagedA = 0xa165,
    /// <summary>Left damaged frame B at $84:A179.</summary>
    LeftDamagedB = 0xa179,
    /// <summary>Left damaged frame C at $84:A18D.</summary>
    LeftDamagedC = 0xa18d,
    /// <summary>Left damaged frame D at $84:A1A1.</summary>
    LeftDamagedD = 0xa1a1,
}

/// <summary>
/// Physical draw layouts selected by the reachable right- and left-facing
/// Draygon cannon PLM programs. Unused diagonal orientations are not claimed.
/// These full level words and signed row offsets remain gameplay definitions.
/// </summary>
internal static class DraygonCannonPlmDrawDefinitions
{
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
        if (!Enum.IsDefined((DraygonCannonDraw)pointer)) { draw = default; return false; }
        draw = (DraygonCannonDraw)pointer switch
        {
            DraygonCannonDraw.RightShieldA => new(true, true, 0),
            DraygonCannonDraw.RightShieldB => new(true, true, 1),
            DraygonCannonDraw.RightDamagedA => new(true, false, 0),
            DraygonCannonDraw.RightDamagedB => new(true, false, 1),
            DraygonCannonDraw.RightDamagedC => new(true, false, 2),
            DraygonCannonDraw.RightDamagedD => new(true, false, 3),
            DraygonCannonDraw.LeftShieldA => new(false, true, 0),
            DraygonCannonDraw.LeftShieldB => new(false, true, 1),
            DraygonCannonDraw.LeftDamagedA => new(false, false, 0),
            DraygonCannonDraw.LeftDamagedB => new(false, false, 1),
            DraygonCannonDraw.LeftDamagedC => new(false, false, 2),
            DraygonCannonDraw.LeftDamagedD => new(false, false, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {pointer:X4}."),
        };
        return true;
    }

    private static IEnumerable<ushort> Pointers()
    {
        foreach (DraygonCannonDraw draw in Enum.GetValues<DraygonCannonDraw>()) yield return (ushort)draw;
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
    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<DraygonCannonDraw>(pointer, "Draygon cannon draw with a visual ID") switch
        {
            DraygonCannonDraw.RightShieldA => "right-shield-a",
            DraygonCannonDraw.RightShieldB => "right-shield-b",
            DraygonCannonDraw.RightDamagedA => "right-damaged-a",
            DraygonCannonDraw.RightDamagedB => "right-damaged-b",
            DraygonCannonDraw.RightDamagedC => "right-damaged-c",
            DraygonCannonDraw.RightDamagedD => "right-damaged-d",
            DraygonCannonDraw.LeftShieldA => "left-shield-a",
            DraygonCannonDraw.LeftShieldB => "left-shield-b",
            DraygonCannonDraw.LeftDamagedA => "left-damaged-a",
            DraygonCannonDraw.LeftDamagedB => "left-damaged-b",
            DraygonCannonDraw.LeftDamagedC => "left-damaged-c",
            DraygonCannonDraw.LeftDamagedD => "left-damaged-d",
            _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonDraw)} {pointer:X4}."),
        };

}
