using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The eight bank-$84 Samus Eater plant draw lists, valued by native address.</summary>
internal enum SamusEaterDraw : ushort
{
    /// <summary>Native floor-idle draw list at $84:9E0D.</summary>
    FloorIdle = 0x9e0d,
    /// <summary>Native floor-chew-1 draw list at $84:9E45.</summary>
    FloorChew1 = 0x9e45,
    /// <summary>Native floor-chew-2 draw list at $84:9E61.</summary>
    FloorChew2 = 0x9e61,
    /// <summary>Native floor-chew-3 draw list at $84:9E7D.</summary>
    FloorChew3 = 0x9e7d,
    /// <summary>Native ceiling-idle draw list at $84:9E99.</summary>
    CeilingIdle = 0x9e99,
    /// <summary>Native ceiling-chew-1 draw list at $84:9ED1.</summary>
    CeilingChew1 = 0x9ed1,
    /// <summary>Native ceiling-chew-2 draw list at $84:9EED.</summary>
    CeilingChew2 = 0x9eed,
    /// <summary>Native ceiling-chew-3 draw list at $84:9F09.</summary>
    CeilingChew3 = 0x9f09,
}

/// <summary>
/// Eight native floor/ceiling plant block layouts at $84:9E0D..9F24. Their
/// level words use consecutive tile pairs, horizontal mouth/foliage mirroring, and
/// a ceiling vertical flip. Three signed runs draw the right mouth, left mouth and
/// foliage. Idle alone restores the special-air parent and horizontal extension.
/// Native unused records after each idle are excluded. No stored draw cache remains;
/// replaceable assets continue to own the visible portion.
/// </summary>
internal static class SamusEaterPlmDrawDefinitions
{
    internal readonly record struct Draw(bool Ceiling, int Phase)
    {
        internal static int Count(int run) => run switch
        {
            0 or 1 => 2,
            2 => 4,
            _ => throw new IndexOutOfRangeException(),
        };
        internal static sbyte NextX(int run) => run switch
        {
            0 or 1 => -2,
            2 => 0,
            _ => throw new IndexOutOfRangeException(),
        };
        internal sbyte NextY(int run) => run switch
        {
            0 or 2 => 0,
            1 => Ceiling ? (sbyte)1 : (sbyte)-1,
            _ => throw new IndexOutOfRangeException(),
        };
        internal ushort WordAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new IndexOutOfRangeException();
            int tile, collision, horizontalFlip;
            if (run == 2)
            {
                // Four-wide foliage: two tiles followed by their mirrored pair.
                tile = 0x180 + Phase * 2 + Math.Min(block, 3 - block);
                collision = 0x2000;
                horizontalFlip = block >= 2 ? 0x400 : 0;
            }
            else
            {
                // Right mouth half precedes the left half in native draw order.
                tile = 0x1a0 + Phase * 2 + (run == 0 ? 1 - block : block);
                horizontalFlip = run == 0 ? 0x400 : 0;
                collision = (run, block) switch
                {
                    (0, 1) or (1, 0) => 0x8000,
                    (0, 0) when Phase == 0 => 0x3000,
                    (1, 1) when Phase == 0 => 0x5000,
                    _ => 0,
                };
            }
            return (ushort)(collision | tile | horizontalFlip | (Ceiling ? 0x800 : 0));
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        var list = (SamusEaterDraw)pointer;
        if (!Enum.IsDefined(list))
        {
            draw = default;
            return false;
        }
        draw = list switch
        {
            SamusEaterDraw.FloorIdle => new(false, 0),
            SamusEaterDraw.FloorChew1 => new(false, 1),
            SamusEaterDraw.FloorChew2 => new(false, 2),
            SamusEaterDraw.FloorChew3 => new(false, 3),
            SamusEaterDraw.CeilingIdle => new(true, 0),
            SamusEaterDraw.CeilingChew1 => new(true, 1),
            SamusEaterDraw.CeilingChew2 => new(true, 2),
            SamusEaterDraw.CeilingChew3 => new(true, 3),
            _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {list}."),
        };
        return true;
    }

    // Materialize temporary DTOs only for the existing artwork import/export interface.
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            for (int ceiling = 0; ceiling < 2; ceiling++)
            for (int phase = 0; phase < 4; phase++)
            {
                ushort pointer = (ushort)((int)(ceiling == 0 ? SamusEaterDraw.FloorIdle : SamusEaterDraw.CeilingIdle) +
                    (phase == 0 ? 0 : (phase + 1) * 28));
                TryGet(pointer, out var list);
                yield return list;
            }
        }
    }

    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<SamusEaterDraw>(pointer, "Samus Eater draw with a visual ID"));

    internal static string VisualId(SamusEaterDraw pointer) => pointer switch
    {
        SamusEaterDraw.FloorIdle => "floor-idle",
        SamusEaterDraw.FloorChew1 => "floor-chew-1",
        SamusEaterDraw.FloorChew2 => "floor-chew-2",
        SamusEaterDraw.FloorChew3 => "floor-chew-3",
        SamusEaterDraw.CeilingIdle => "ceiling-idle",
        SamusEaterDraw.CeilingChew1 => "ceiling-chew-1",
        SamusEaterDraw.CeilingChew2 => "ceiling-chew-2",
        SamusEaterDraw.CeilingChew3 => "ceiling-chew-3",
        _ => throw new InvalidOperationException($"Undefined SamusEaterDraw {pointer}."),
    };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in All)
        {
            if (!string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        if (!TryDescribe(pointer, out Draw draw))
        {
            list = default;
            return false;
        }
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[3];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[Draw.Count(run)];
            for (int block = 0; block < words.Length; block++) words[block] = draw.WordAt(run, block);
            runs[run] = new((ushort)words.Length, words, Draw.NextX(run), draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }
}
