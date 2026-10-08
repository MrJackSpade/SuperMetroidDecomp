namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one word;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in the solid block bit. Native
/// continuation offsets are relative to the origin, not the preceding column.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    /// <summary><c>$84:9B5B</c>: ten cleared bridge blocks.</summary>
    internal const ushort ClearBridge = 0x9b5b;
    /// <summary><c>$84:9B73</c>: one crumbling bridge block.</summary>
    internal const ushort CrumbleBridgeBlock = 0x9b73;
    /// <summary><c>$84:9B79</c>: one cleared bridge block.</summary>
    internal const ushort ClearBridgeBlock = 0x9b79;
    /// <summary><c>$84:9B7F</c>: three columns of clear wall blocks.</summary>
    internal const ushort ClearInvisibleWall = 0x9b7f;
    /// <summary><c>$84:9BBB</c>: three columns of solid wall blocks.</summary>
    internal const ushort CreateInvisibleWall = 0x9bbb;

    internal readonly record struct Draw(ushort Pointer, bool Wall, bool Solid, int WordsPerRun)
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
            if (!Wall) return Pointer == CrumbleBridgeBlock ? (ushort)0x810b : (ushort)0x0080;
            int tile = block is 0 or >= 6 ? 0x80 :
                0x107 + run + (block == 5 ? 2 : (block - 1) % 2) * 0x20;
            return (ushort)(tile | (Solid ? 0x8000 : 0));
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        draw = pointer switch
        {
            ClearBridge => new(pointer, false, false, 10),
            CrumbleBridgeBlock or ClearBridgeBlock => new(pointer, false, false, 1),
            ClearInvisibleWall => new(pointer, true, false, 8),
            CreateInvisibleWall => new(pointer, true, true, 8),
            _ => default,
        };
        return draw.Pointer != 0;
    }

    // Temporary DTOs serve the existing artwork interface; gameplay evaluates cells directly.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in EnumeratePointers())
            {
                TryGet(pointer, out var draw);
                yield return draw;
            }
        }
    }

    private static IEnumerable<ushort> EnumeratePointers()
    {
        yield return ClearBridge;
        yield return CrumbleBridgeBlock;
        yield return ClearBridgeBlock;
        yield return ClearInvisibleWall;
        yield return CreateInvisibleWall;
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

    internal static string VisualId(ushort pointer) => pointer switch
    {
        ClearBridge => "clear-bridge",
        CrumbleBridgeBlock => "crumble-bridge-block",
        ClearBridgeBlock => "clear-bridge-block",
        ClearInvisibleWall => "clear-invisible-wall",
        CreateInvisibleWall => "create-invisible-wall",
        _ => throw new InvalidDataException(
            $"Crocomire arena draw ${pointer:X4} has no visual ID."),
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
