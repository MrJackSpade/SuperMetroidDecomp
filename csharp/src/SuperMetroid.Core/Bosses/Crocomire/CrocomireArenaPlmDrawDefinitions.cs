using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6. Bridge runs repeat one word;
/// wall columns use consecutive tiles, two alternating rows and a final base row.
/// Clear/create share artwork and differ only in the solid block bit. Native
/// continuation offsets are relative to the origin, not the preceding column.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    /// <summary>Decoded bridge or three-column wall geometry with its independently selected solid collision bit.</summary>
    internal readonly record struct Draw(CrocomireArenaDraw Pointer, bool Wall, bool Solid, int WordsPerRun)
    {
        /// <summary>Wall layouts draw three columns; bridge layouts draw one horizontal run.</summary>
        internal int RunCount => Wall ? 3 : 1;
        /// <summary>Sets the vertical-run bit for wall columns while retaining the native cells per run.</summary>
        internal ushort DirectionAndCount => (ushort)(WordsPerRun | (Wall ? 0x8000 : 0));
        /// <summary>Offsets the next wall column from the draw origin rather than from the prior column.</summary>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return run + 1 < RunCount ? (sbyte)(run + 1) : (sbyte)0;
        }
        /// <summary>Calculates bridge tiles or the wall's alternating rows and final base row, applying solid collision only to the create-wall state.</summary>
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

    /// <summary>Assigns native run geometry to each of the five bounded arena draw pointers.</summary>
    private static Draw Describe(CrocomireArenaDraw pointer) => pointer switch
    {
        CrocomireArenaDraw.ClearBridge => new(pointer, false, false, 10),
        CrocomireArenaDraw.CrumbleBridgeBlock or CrocomireArenaDraw.ClearBridgeBlock => new(pointer, false, false, 1),
        CrocomireArenaDraw.ClearInvisibleWall => new(pointer, true, false, 8),
        CrocomireArenaDraw.CreateInvisibleWall => new(pointer, true, true, 8),
        _ => throw new InvalidOperationException($"Undefined Crocomire arena draw {pointer}."),
    };

    /// <summary>Projects calculated physical cells into temporary artwork records; gameplay evaluates cells directly.</summary>
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

    /// <summary>Reconstructs one supported bridge or wall draw with its native run offsets.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribe(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new(shape.DirectionAndCount, words, new(shape.NextX(run), 0));
        }
        draw = new(pointer, runs);
        return true;
    }

    /// <summary>Maps the bank-$84 arena draw identity to the published bridge or wall artwork key.</summary>
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

    /// <summary>Finds the physical bridge or wall layout for one editable visual identity.</summary>
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
