using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The five bank-<c>$84</c> Crocomire arena block-draw lists.</summary>
internal enum CrocomireArenaDraw : ushort
{
    /// <summary><c>$84:9B5B</c>: ten cleared bridge blocks.</summary>
    ClearBridge = 0x9b5b,
    /// <summary><c>$84:9B73</c>: one crumbling bridge block.</summary>
    CrumbleBridgeBlock = 0x9b73,
    /// <summary><c>$84:9B79</c>: one cleared bridge block.</summary>
    ClearBridgeBlock = 0x9b79,
    /// <summary><c>$84:9B7F</c>: three columns of clear wall blocks.</summary>
    ClearInvisibleWall = 0x9b7f,
    /// <summary><c>$84:9BBB</c>: three columns of solid wall blocks.</summary>
    CreateInvisibleWall = 0x9bbb,
}

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one word;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in the solid block bit. Native
/// continuation offsets are relative to the origin, not the preceding column.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    internal readonly record struct Draw(CrocomireArenaDraw Pointer, bool Wall, bool Solid, int WordsPerRun)
    {
        internal int RunCount => Wall ? 3 : 1;
        internal ushort DirectionAndCount => (ushort)(WordsPerRun | (Wall ? 0x8000 : 0));
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return run + 1 < RunCount ? (sbyte)(run + 1) : (sbyte)0;
        }
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount || (uint)block >= WordsPerRun)
                throw new IndexOutOfRangeException();
            if (!Wall) return Pointer == CrocomireArenaDraw.CrumbleBridgeBlock ? (ushort)0x810b : (ushort)0x0080;
            int tile = block is 0 or >= 6 ? 0x80 :
                0x107 + run + (block == 5 ? 2 : (block - 1) % 2) * 0x20;
            return (ushort)(tile | (Solid ? 0x8000 : 0));
        }
    }

    /// <summary>
    /// Describes a draw pointer handed over by the shared PLM draw interpreter; pointers
    /// outside the five arena lists belong to other families.
    /// </summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((CrocomireArenaDraw)pointer))
        {
            draw = default;
            return false;
        }
        draw = Describe((CrocomireArenaDraw)pointer);
        return true;
    }

    private static Draw Describe(CrocomireArenaDraw pointer) => pointer switch
    {
        CrocomireArenaDraw.ClearBridge => new(pointer, false, false, 10),
        CrocomireArenaDraw.CrumbleBridgeBlock or CrocomireArenaDraw.ClearBridgeBlock => new(pointer, false, false, 1),
        CrocomireArenaDraw.ClearInvisibleWall => new(pointer, true, false, 8),
        CrocomireArenaDraw.CreateInvisibleWall => new(pointer, true, true, 8),
        _ => throw new InvalidOperationException($"Undefined Crocomire arena draw {pointer}."),
    };

    // Temporary DTOs serve the existing artwork interface; gameplay evaluates cells directly.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw pointer in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet((ushort)pointer, out var draw);
                yield return draw;
            }
        }
    }

    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribe(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new(shape.DirectionAndCount, words, shape.NextX(run), 0);
        }
        draw = new(pointer, runs);
        return true;
    }

    internal static string VisualId(ushort pointer) =>
        ClosedNativeWords.Decode<CrocomireArenaDraw>(pointer, "Crocomire arena draw") switch
        {
            CrocomireArenaDraw.ClearBridge => "clear-bridge",
            CrocomireArenaDraw.CrumbleBridgeBlock => "crumble-bridge-block",
            CrocomireArenaDraw.ClearBridgeBlock => "clear-bridge-block",
            CrocomireArenaDraw.ClearInvisibleWall => "clear-invisible-wall",
            CrocomireArenaDraw.CreateInvisibleWall => "create-invisible-wall",
            _ => throw new InvalidOperationException($"Undefined Crocomire arena draw ${pointer:X4}."),
        };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
            {
                draw = candidate;
                return true;
            }
        }
        draw = default;
        return false;
    }

}
