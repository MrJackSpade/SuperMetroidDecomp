using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Plms;

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
    /// <summary>Floor or ceiling orientation and one of four mouth animation phases for a native draw list.</summary>
    internal readonly record struct Draw(bool Ceiling, [property: IntegerQuantity("animation frame index within the eater's draw cycle")] int Phase)
    {
        /// <summary>The mouth runs each contain two blocks; the foliage run contains four.</summary>
        internal static int Count(int run) => run switch
        {
            0 or 1 => 2,
            2 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>Moves left between mouth blocks while keeping the foliage run in its original column.</summary>
        internal static sbyte NextX(int run) => run switch
        {
            0 or 1 => -2,
            2 => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>Offsets the second mouth run toward the plant body, reversing the step for ceiling plants.</summary>
        internal sbyte NextY(int run) => run switch
        {
            0 or 2 => 0,
            1 => Ceiling ? (sbyte)1 : (sbyte)-1,
            _ => throw new ArgumentOutOfRangeException(nameof(run)),
        };
        /// <summary>Encodes the phase tile, collision type, and orientation flips for a single mouth or foliage block.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new ArgumentOutOfRangeException(nameof(block));
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

    /// <summary>Decodes one of eight bank-$84 plant draw pointers into its orientation and chew phase.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        if (!Enum.IsDefined((SamusEaterDraw)pointer))
        {
            draw = default;
            return false;
        }
        SamusEaterDraw list = (SamusEaterDraw)pointer;
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

    /// <summary>Materializes the eight native plant layouts as temporary DTOs for artwork import and export.</summary>
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

    /// <summary>Maps a validated native draw pointer to the artwork catalog's orientation and phase ID.</summary>
    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<SamusEaterDraw>(pointer, "Samus Eater draw with a visual ID"));

    /// <summary>Names a typed draw list by its floor or ceiling orientation and idle or chew phase.</summary>
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

    /// <summary>Finds the native layout matching an artwork catalog ID by searching the eight supported lists.</summary>
    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Builds the three native signed runs for a supported plant draw pointer from its phase and orientation.</summary>
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
            runs[run] = new((ushort)words.Length, words, new(Draw.NextX(run), draw.NextY(run)));
        }
        list = new(pointer, runs);
        return true;
    }
}
