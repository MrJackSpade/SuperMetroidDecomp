using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain fake-death room physical PLM draws at $84:94A3..9716.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitions
{
    /// <summary><c>$84:9717</c>: first byte of the following glass draw region.</summary>
    internal const ushort EndExclusive = 0x9717;

    /// <summary>Enumerates wall, door, mural rows and tube-clear draws in native family order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    /// <summary>Validates a native draw pointer before assigning its editable artwork ID.</summary>
    internal static string VisualId(ushort pointer) => VisualId(
        ClosedNativeWords.Decode<MotherBrainFakeDeathDraw>(pointer, "Mother Brain fake-death draw with a visual ID"));

    /// <summary>Maps the closed bank-$84 draw domain to stable visual document names.</summary>
    internal static string VisualId(MotherBrainFakeDeathDraw pointer) => pointer switch
    {
        MotherBrainFakeDeathDraw.FillWall => "fill-wall",
        MotherBrainFakeDeathDraw.EscapeDoor => "escape-door",
        MotherBrainFakeDeathDraw.BackgroundRow2 => "background-row-2",
        MotherBrainFakeDeathDraw.BackgroundRow3 => "background-row-3",
        MotherBrainFakeDeathDraw.BackgroundRow4 => "background-row-4",
        MotherBrainFakeDeathDraw.BackgroundRow5 => "background-row-5",
        MotherBrainFakeDeathDraw.BackgroundRow6 => "background-row-6",
        MotherBrainFakeDeathDraw.BackgroundRow7 => "background-row-7",
        MotherBrainFakeDeathDraw.BackgroundRow8 => "background-row-8",
        MotherBrainFakeDeathDraw.BackgroundRow9 => "background-row-9",
        MotherBrainFakeDeathDraw.BackgroundRowA => "background-row-a",
        MotherBrainFakeDeathDraw.BackgroundRowB => "background-row-b",
        MotherBrainFakeDeathDraw.BackgroundRowC => "background-row-c",
        MotherBrainFakeDeathDraw.BackgroundRowD => "background-row-d",
        MotherBrainFakeDeathDraw.BackgroundRowEUnused => "background-row-e-unused",
        MotherBrainFakeDeathDraw.BackgroundRowFUnused => "background-row-f-unused",
        MotherBrainFakeDeathDraw.ClearCeilingBlock => "clear-ceiling-block",
        MotherBrainFakeDeathDraw.ClearCeilingTube => "clear-ceiling-tube",
        MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube => "clear-bottom-middle-side-tube",
        MotherBrainFakeDeathDraw.ClearBottomMiddleTubes => "clear-bottom-middle-tubes",
        MotherBrainFakeDeathDraw.ClearBottomLeftTube => "clear-bottom-left-tube",
        MotherBrainFakeDeathDraw.ClearBottomRightTube => "clear-bottom-right-tube",
        _ => throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {pointer}."),
    };

    /// <summary>Finds a compiled physical draw by its case-sensitive editable visual ID.</summary>
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

    /// <summary>
    /// Native 84:966D..9716: two thirteen-cell solid fill rows and six tube
    /// clears. Tube interiors become blank FF; ceiling/floor endpoints restore
    /// 12FC/1339. The middle pair duplicates a seven-cell column; side tubes
    /// clear one extra cell toward the center using signed origin-relative X.
    /// </summary>
    internal readonly record struct RegularDraw(MotherBrainFakeDeathDraw Pointer, int RunLength, int RunCount, bool Ceiling, bool Fill, int Side)
    {
        /// <summary>Uses one cell for a side-tube's second run and the full authored length elsewhere.</summary>
        internal int Count(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return Side != 0 && run == 1 ? 1 : RunLength;
        }
        /// <summary>Tube clears run vertically except for a side-tube's extra centerward cell.</summary>
        internal bool Vertical(int run) => !Fill && !(Side != 0 && run == 1);
        /// <summary>Moves to the next paired tube column, using signed direction for the side clears.</summary>
        internal sbyte NextX(int run)
        {
            Count(run);
            return run + 1 == RunCount ? (sbyte)0 : (sbyte)(Side == 0 ? 1 : Side);
        }
        /// <summary>Preserves solid fill rows or clears tube interiors while restoring ceiling and floor endpoints.</summary>
        internal ushort WordAt(int run, int block)
        {
            int count = Count(run);
            if ((uint)block >= count) throw new ArgumentOutOfRangeException(nameof(block));
            if (Fill) return Pointer == MotherBrainFakeDeathDraw.BackgroundRowEUnused ? (ushort)0x8319 : (ushort)0x8044;
            if (Side != 0 && run == 1) return 0x00ff;
            return Ceiling ? (block == 0 ? (ushort)0x12fc : (ushort)0x00ff) :
                (block == count - 1 ? (ushort)0x1339 : (ushort)0x00ff);
        }
    }

    /// <summary>Recognizes only the two unused fill rows and six tube-clear lists in the shared draw-pointer space.</summary>
    internal static bool TryDescribeRegular(ushort pointer, out RegularDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: words outside this family belong to other owners.
        if (!Enum.IsDefined((MotherBrainFakeDeathDraw)pointer)) return false;
        var list = (MotherBrainFakeDeathDraw)pointer;
        switch (list)
        {
            case MotherBrainFakeDeathDraw.BackgroundRowEUnused or MotherBrainFakeDeathDraw.BackgroundRowFUnused:
                draw = new(list, 13, 1, false, true, 0); return true;
            case MotherBrainFakeDeathDraw.ClearCeilingBlock: draw = new(list, 2, 1, true, false, 0); return true;
            case MotherBrainFakeDeathDraw.ClearCeilingTube: draw = new(list, 5, 1, true, false, 0); return true;
            case MotherBrainFakeDeathDraw.ClearBottomMiddleSideTube: draw = new(list, 4, 1, false, false, 0); return true;
            case MotherBrainFakeDeathDraw.ClearBottomMiddleTubes: draw = new(list, 7, 2, false, false, 0); return true;
            case MotherBrainFakeDeathDraw.ClearBottomLeftTube: draw = new(list, 5, 2, false, false, 1); return true;
            case MotherBrainFakeDeathDraw.ClearBottomRightTube: draw = new(list, 5, 2, false, false, -1); return true;
            // The boundary and background families own these lists.
            case MotherBrainFakeDeathDraw.FillWall or MotherBrainFakeDeathDraw.EscapeDoor or
                MotherBrainFakeDeathDraw.BackgroundRow2 or MotherBrainFakeDeathDraw.BackgroundRow3 or
                MotherBrainFakeDeathDraw.BackgroundRow4 or MotherBrainFakeDeathDraw.BackgroundRow5 or
                MotherBrainFakeDeathDraw.BackgroundRow6 or MotherBrainFakeDeathDraw.BackgroundRow7 or
                MotherBrainFakeDeathDraw.BackgroundRow8 or MotherBrainFakeDeathDraw.BackgroundRow9 or
                MotherBrainFakeDeathDraw.BackgroundRowA or MotherBrainFakeDeathDraw.BackgroundRowB or
                MotherBrainFakeDeathDraw.BackgroundRowC or MotherBrainFakeDeathDraw.BackgroundRowD:
                return false;
            default:
                throw new InvalidOperationException($"Undefined MotherBrainFakeDeathDraw {list}.");
        }
    }

    /// <summary>Expands a regular shape into ordered PLM runs with native vertical flags and continuation offsets.</summary>
    internal static bool TryGetRegular(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeRegular(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.Count(run)];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new((ushort)(words.Length | (shape.Vertical(run) ? 0x8000 : 0)), words, new(shape.NextX(run), 0));
        }
        draw = new(pointer, runs);
        return true;
    }

    /// <summary>Walks contiguous bank-$84 regular records by their encoded run sizes through $9716.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> RegularDraws()
    {
        // Each next record follows its counts, cells and two-byte continuation pairs.
        for (int pointer = (int)MotherBrainFakeDeathDraw.BackgroundRowEUnused; pointer < EndExclusive;)
        {
            TryGetRegular((ushort)pointer, out var draw);
            yield return draw;
            foreach (var run in draw.Runs.ToArray()) pointer += 4 + run.LevelWords.Length * 2;
        }
    }

    /// <summary>
    /// Retained picture composition, not numeric samples: 84:9505..966C places
    /// this 13x12 Tourian pipe/panel mural at room (2,2), as shown by A9:8C87..8D10.
    /// Tileset E (8F:E720) expands these cells into individually placed bends,
    /// junctions, panels and gaps. Repeated pipe segments do not determine their
    /// routing; a switch or fitted rule would merely re-encode that chosen picture.
    /// Retain under #1165's nonsense exception. Geometry and collision are calculated
    /// separately. Provenance and reconstruction are in motherBrainBackgroundArtworkReview.
    /// </summary>
    private static readonly RoomLevelWord[][] BackgroundVisuals =
    [
        [Tile(577), Tile(578), Tile(764), Tile(764), Tile(764), Tile(579), Tile(580), Tile(764), Tile(581), Tile(578, FlipX), Tile(577), Tile(577), Tile(582)],
        [Tile(495, FlipY), Tile(434), Tile(485), Tile(485), Tile(486), Tile(485), Tile(485), Tile(485), Tile(485), Tile(434, FlipX), Tile(495, FlipY), Tile(495, FlipY), Tile(434)],
        [Tile(433), Tile(466), Tile(454), Tile(455), Tile(255), Tile(518), Tile(519), Tile(255), Tile(422), Tile(458, FlipY), Tile(524, FlipX), Tile(433, FlipX), Tile(521, FlipY)],
        [Tile(465), Tile(498), Tile(420), Tile(487), Tile(420), Tile(550), Tile(551), Tile(421), Tile(420), Tile(525), Tile(521, FlipXY), Tile(433), Tile(427)],
        [Tile(433), Tile(530), Tile(452), Tile(457), Tile(452), Tile(518), Tile(519), Tile(453), Tile(452), Tile(552, FlipX), Tile(428), Tile(492), Tile(492)],
        [Tile(433), Tile(524, FlipY), Tile(458, FlipX), Tile(455, FlipXY), Tile(426), Tile(424), Tile(424), Tile(424), Tile(424), Tile(552, FlipX), Tile(427), Tile(461), Tile(461)],
        [Tile(465), Tile(464), Tile(490, FlipX), Tile(255), Tile(255), Tile(518), Tile(519), Tile(255), Tile(423), Tile(525, FlipY), Tile(521, FlipX), Tile(491), Tile(464)],
        [Tile(491), Tile(491), Tile(490, FlipX), Tile(255), Tile(255), Tile(518), Tile(519), Tile(255), Tile(422), Tile(255), Tile(556, FlipY), Tile(521, FlipX), Tile(430)],
        [Tile(492), Tile(431), Tile(490, FlipX), Tile(455, FlipX), Tile(454, FlipX), Tile(518), Tile(519), Tile(424), Tile(422), Tile(424), Tile(424), Tile(466, FlipX), Tile(430)],
        [Tile(428), Tile(431), Tile(434), Tile(487, FlipX), Tile(485), Tile(550), Tile(551), Tile(485), Tile(422), Tile(486), Tile(485), Tile(434, FlipX), Tile(461)],
        [Tile(524, FlipX), Tile(495), Tile(434), Tile(485), Tile(486), Tile(485), Tile(485), Tile(486), Tile(485), Tile(485), Tile(485), Tile(434, FlipX), Tile(495)],
        [Tile(584), Tile(585), Tile(586), Tile(587), Tile(825), Tile(588), Tile(589), Tile(825), Tile(590), Tile(825), Tile(825), Tile(591), Tile(585)],
    ];

    /// <summary>A mirrored mural cell.</summary>
    private const LevelBlockFlipFlags FlipX = LevelBlockFlipFlags.Horizontal;
    /// <summary>A vertically flipped mural cell.</summary>
    private const LevelBlockFlipFlags FlipY = LevelBlockFlipFlags.Vertical;
    /// <summary>A mural cell flipped both ways.</summary>
    private const LevelBlockFlipFlags FlipXY = LevelBlockFlipFlags.Horizontal | LevelBlockFlipFlags.Vertical;

    /// <summary>One air mural cell: tileset tile <paramref name="index"/> with <paramref name="flips"/>.</summary>
    private static RoomLevelWord Tile(ushort index, LevelBlockFlipFlags flips = LevelBlockFlipFlags.None) =>
        RoomLevelWord.Create(index, flips, RoomCollisionType.Air);

    /// <summary>
    /// Twelve horizontal thirteen-cell rows at 84:9505..966C, spaced thirty
    /// bytes apart. Top/bottom rows retain collision type one; interior rows
    /// are air. Visual bits remain independent from these physical boundaries.
    /// </summary>
    internal readonly record struct BackgroundDraw(int Row)
    {
        /// <summary>Combines a selected mural tile with solid collision only on its upper and lower border rows.</summary>
        internal ushort WordAt(int column)
        {
            if ((uint)column >= 13) throw new ArgumentOutOfRangeException(nameof(column));
            return (ushort)(BackgroundVisuals[Row][column].Raw | (Row is 0 or 11 ? 0x1000 : 0));
        }
    }

    /// <summary>Recognizes the twelve 30-byte-spaced mural row pointers from $9505 through $964F.</summary>
    internal static bool TryDescribeBackground(ushort pointer, out BackgroundDraw draw)
    {
        int offset = pointer - (int)MotherBrainFakeDeathDraw.BackgroundRow2;
        bool owned = offset >= 0 && pointer <= (int)MotherBrainFakeDeathDraw.BackgroundRowD && offset % 30 == 0;
        draw = owned ? new(offset / 30) : default;
        return owned;
    }

    /// <summary>Builds one thirteen-cell horizontal mural run from its selected row and collision boundary.</summary>
    internal static bool TryGetBackground(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBackground(pointer, out var row)) return false;
        var words = new ushort[13];
        for (int column = 0; column < words.Length; column++) words[column] = row.WordAt(column);
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(13, words, new(0, 0)) });
        return true;
    }

    /// <summary>Enumerates all twelve mural rows at their native 30-byte pointer stride.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BackgroundDraws()
    {
        for (int pointer = (int)MotherBrainFakeDeathDraw.BackgroundRow2;
             pointer <= (int)MotherBrainFakeDeathDraw.BackgroundRowD; pointer += 30)
        {
            TryGetBackground((ushort)pointer, out var draw);
            yield return draw;
        }
    }

    /// <summary>84:94B3/94BF: left tile of the two-wide broken upper edge (222/223).</summary>
    private const ushort DoorUpperEdgeLeft = 0x222;
    /// <summary>84:94B9/94C5: left tile of the two-wide broken lower edge (220/221).</summary>
    private const ushort DoorLowerEdgeLeft = 0x220;
    /// <summary>84:94B5: upper interior panel with its left vertical seam.</summary>
    private const ushort DoorInteriorLeft = 0x1af;
    /// <summary>84:94C1: upper interior panel without that left seam.</summary>
    private const ushort DoorInteriorRight = 0x1eb;
    /// <summary>84:94B7/94C3: repeated lower-interior strip across both columns.</summary>
    private const ushort DoorLowerInterior = 0x1d0;

    /// <summary>Selects the authored tile for one validated column and row of the two-wide escape door.</summary>
    private static ushort EscapeDoorVisualAt(int column, int row) => row switch
    {
        0 => (ushort)(DoorUpperEdgeLeft + column),
        1 => column == 0 ? DoorInteriorLeft : DoorInteriorRight,
        2 => DoorLowerInterior,
        3 => (ushort)(DoorLowerEdgeLeft + column),
        _ => throw new ArgumentOutOfRangeException(nameof(row)),
    };

    /// <summary>
    /// 84:94A3/94B1: a three-cell wall centered on the origin, or a two-column
    /// four-cell door. Both use vertical runs. Wall caps mirror tile 30F around
    /// center tile 340 and are solid. The door's first column has a door parent
    /// above three vertical extensions; its second column is air.
    /// </summary>
    internal readonly record struct BoundaryDraw(MotherBrainFakeDeathDraw Pointer)
    {
        /// <summary>Chooses four cells per escape-door column or the wall's two-cell and one-cell runs.</summary>
        internal int Count(int run)
        {
            if ((uint)run >= 2) throw new ArgumentOutOfRangeException(nameof(run));
            return Pointer == MotherBrainFakeDeathDraw.EscapeDoor ? 4 : 2 - run;
        }
        /// <summary>Steps from the escape door's left column to its right air column.</summary>
        internal sbyte NextX(int run)
        {
            Count(run);
            return Pointer == MotherBrainFakeDeathDraw.EscapeDoor && run == 0 ? (sbyte)1 : (sbyte)0;
        }
        /// <summary>Raises the second wall cap above its first run's centered tile.</summary>
        internal sbyte NextY(int run)
        {
            Count(run);
            return Pointer == MotherBrainFakeDeathDraw.FillWall && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Builds solid wall caps or the escape door's parent, extensions and air-backed artwork.</summary>
        internal ushort WordAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new ArgumentOutOfRangeException(nameof(block));
            if (Pointer == MotherBrainFakeDeathDraw.FillWall)
                return (ushort)(0x8000 | (run == 0 && block == 0 ? 0x340 : 0x30f | (run == 1 ? 0x800 : 0)));
            RoomCollisionType collision = run == 1 ? RoomCollisionType.Air :
                block == 0 ? RoomCollisionType.DoorBlock : RoomCollisionType.VerticalExtension;
            return (ushort)(((int)collision << 12) | EscapeDoorVisualAt(run, block));
        }
    }

    /// <summary>Recognizes only the wall-fill and escape-door pointers among the shared fake-death draws.</summary>
    internal static bool TryDescribeBoundary(ushort pointer, out BoundaryDraw draw)
    {
        draw = default;
        // Shared draw-pointer space: words outside this family belong to other owners.
        if (!Enum.IsDefined((MotherBrainFakeDeathDraw)pointer)) return false;
        var list = (MotherBrainFakeDeathDraw)pointer;
        bool owned = list is MotherBrainFakeDeathDraw.FillWall or MotherBrainFakeDeathDraw.EscapeDoor;
        if (owned) draw = new(list);
        return owned;
    }

    /// <summary>Expands a validated wall or escape-door shape into two vertical PLM runs.</summary>
    internal static bool TryGetBoundary(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBoundary(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[2];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.Count(run)];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new((ushort)(0x8000 | words.Length), words, new(shape.NextX(run), shape.NextY(run)));
        }
        draw = new(pointer, runs);
        return true;
    }

    /// <summary>Yields the wall-fill then escape-door lists at their native pointers.</summary>
    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BoundaryDraws()
    {
        TryGetBoundary((ushort)MotherBrainFakeDeathDraw.FillWall, out var wall);
        yield return wall;
        TryGetBoundary((ushort)MotherBrainFakeDeathDraw.EscapeDoor, out var door);
        yield return door;
    }
}
