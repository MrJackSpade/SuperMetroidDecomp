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
    internal readonly record struct Draw(CrocomireArenaDraw Frame, CrocomireArenaShape Shape, int WordsPerRun)
    {
        /// <summary>Whether the draw writes wall columns rather than a bridge run.</summary>
        internal bool Wall => Shape != CrocomireArenaShape.Bridge;

        /// <summary>Whether the wall columns are solid.</summary>
        internal bool Solid => Shape == CrocomireArenaShape.SolidWall;

        /// <summary>Wall layouts draw three columns; bridge layouts draw one horizontal run.</summary>
        internal int RunCount => Wall ? 3 : 1;
        /// <summary>Sets the vertical-run bit for wall columns while retaining the native cells per run.</summary>
        internal int DirectionAndCount => WordsPerRun | (Wall ? 0x8000 : 0);
        /// <summary>Offsets the next wall column from the draw origin rather than from the prior column.</summary>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return run + 1 < RunCount ? (sbyte)(run + 1) : (sbyte)0;
        }
        /// <summary>Calculates bridge tiles or the wall's alternating rows and final base row, applying solid collision only to the create-wall state.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)run >= RunCount)
                throw new ArgumentOutOfRangeException(nameof(run));
            if ((uint)block >= WordsPerRun)
                throw new ArgumentOutOfRangeException(nameof(block));
            if (!Wall) return Frame == CrocomireArenaDraw.CrumbleBridgeBlock ? (ushort)0x810b : (ushort)0x0080;
            int tile = block is 0 or >= 6 ? 0x80 :
                0x107 + run + (block == 5 ? 2 : (block - 1) % 2) * 0x20;
            return (ushort)(tile | (Solid ? 0x8000 : 0));
        }
    }

    /// <summary>
    /// Describes a draw handed over by the shared PLM drawing; draws of other families are not the arena's.
    /// </summary>
    internal static bool TryDescribe(PlmDraw draw, out Draw shape)
    {
        if (draw is not PlmFrameDraw<CrocomireArenaDraw> { Frame: var frame })
        {
            shape = default;
            return false;
        }
        shape = Describe(frame);
        return true;
    }

    /// <summary>Assigns native run geometry to each of the five bounded arena draws.</summary>
    private static Draw Describe(CrocomireArenaDraw frame) => frame switch
    {
        CrocomireArenaDraw.ClearBridge => new(frame, CrocomireArenaShape.Bridge, 10),
        CrocomireArenaDraw.CrumbleBridgeBlock or CrocomireArenaDraw.ClearBridgeBlock => new(frame, CrocomireArenaShape.Bridge, 1),
        CrocomireArenaDraw.ClearInvisibleWall => new(frame, CrocomireArenaShape.ClearWall, 8),
        CrocomireArenaDraw.CreateInvisibleWall => new(frame, CrocomireArenaShape.SolidWall, 8),
        _ => throw new InvalidOperationException($"Undefined Crocomire arena draw {frame}."),
    };

    /// <summary>Projects calculated physical cells into temporary artwork records; gameplay evaluates cells directly.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (CrocomireArenaDraw frame in Enum.GetValues<CrocomireArenaDraw>())
            {
                TryGet(PlmDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }

    /// <summary>Reconstructs one supported bridge or wall draw with its native run offsets.</summary>
    internal static bool TryGet(PlmDraw draw, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(draw, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.WordsPerRun];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new(checked((ushort)shape.DirectionAndCount), words, new(shape.NextX(run), 0));
        }
        list = new(draw, runs);
        return true;
    }

    /// <summary>Maps an arena draw to the published bridge or wall artwork key.</summary>
    internal static string VisualId(PlmDraw draw) =>
        PlmDraw.FrameOf<CrocomireArenaDraw>(draw, "Crocomire arena") switch
        {
            CrocomireArenaDraw.ClearBridge => "clear-bridge",
            CrocomireArenaDraw.CrumbleBridgeBlock => "crumble-bridge-block",
            CrocomireArenaDraw.ClearBridgeBlock => "clear-bridge-block",
            CrocomireArenaDraw.ClearInvisibleWall => "clear-invisible-wall",
            CrocomireArenaDraw.CreateInvisibleWall => "create-invisible-wall",
            _ => throw new InvalidOperationException($"Undefined Crocomire arena draw {draw}."),
        };

    /// <summary>Finds the physical bridge or wall layout for one editable visual identity.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out draw);

}
