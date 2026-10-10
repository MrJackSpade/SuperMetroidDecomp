using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The 22 Mother Brain fake-death room draw lists at $84:94A3..9716, valued by native address.</summary>
internal enum MotherBrainFakeDeathDraw : ushort
{
    /// <summary><c>$84:94A3</c>: fill wall.</summary>
    FillWall = 0x94a3,
    /// <summary><c>$84:94B1</c>: escape door.</summary>
    EscapeDoor = 0x94b1,
    /// <summary><c>$84:9505</c>: background row 2.</summary>
    BackgroundRow2 = 0x9505,
    /// <summary><c>$84:9523</c>: background row 3.</summary>
    BackgroundRow3 = 0x9523,
    /// <summary><c>$84:9541</c>: background row 4.</summary>
    BackgroundRow4 = 0x9541,
    /// <summary><c>$84:955F</c>: background row 5.</summary>
    BackgroundRow5 = 0x955f,
    /// <summary><c>$84:957D</c>: background row 6.</summary>
    BackgroundRow6 = 0x957d,
    /// <summary><c>$84:959B</c>: background row 7.</summary>
    BackgroundRow7 = 0x959b,
    /// <summary><c>$84:95B9</c>: background row 8.</summary>
    BackgroundRow8 = 0x95b9,
    /// <summary><c>$84:95D7</c>: background row 9.</summary>
    BackgroundRow9 = 0x95d7,
    /// <summary><c>$84:95F5</c>: background row a.</summary>
    BackgroundRowA = 0x95f5,
    /// <summary><c>$84:9613</c>: background row b.</summary>
    BackgroundRowB = 0x9613,
    /// <summary><c>$84:9631</c>: background row c.</summary>
    BackgroundRowC = 0x9631,
    /// <summary><c>$84:964F</c>: background row d.</summary>
    BackgroundRowD = 0x964f,
    /// <summary><c>$84:966D</c>: cartridge-unused background row E.</summary>
    BackgroundRowEUnused = 0x966d,
    /// <summary><c>$84:968B</c>: cartridge-unused background row F.</summary>
    BackgroundRowFUnused = 0x968b,
    /// <summary><c>$84:96A9</c>: clear ceiling block.</summary>
    ClearCeilingBlock = 0x96a9,
    /// <summary><c>$84:96B1</c>: clear ceiling tube.</summary>
    ClearCeilingTube = 0x96b1,
    /// <summary><c>$84:96BF</c>: clear bottom middle side tube.</summary>
    ClearBottomMiddleSideTube = 0x96bf,
    /// <summary><c>$84:96CB</c>: clear bottom middle tubes.</summary>
    ClearBottomMiddleTubes = 0x96cb,
    /// <summary><c>$84:96EF</c>: clear bottom left tube.</summary>
    ClearBottomLeftTube = 0x96ef,
    /// <summary><c>$84:9703</c>: clear bottom right tube.</summary>
    ClearBottomRightTube = 0x9703,
}

/// <summary>Mother Brain fake-death room physical PLM draws at $84:94A3..9716.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitions
{
    /// <summary><c>$84:9717</c>: first byte of the following glass draw region.</summary>
    internal const ushort EndExclusive = 0x9717;

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    internal static string VisualId(ushort pointer) => VisualId(
        ClosedNativeWords.Decode<MotherBrainFakeDeathDraw>(pointer, "Mother Brain fake-death draw with a visual ID"));

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
    internal readonly record struct RegularDraw(MotherBrainFakeDeathDraw Pointer, int Height, int Width, bool Ceiling, bool Fill, int Side)
    {
        internal int RunCount => Width;
        internal int Count(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return Side != 0 && run == 1 ? 1 : Height;
        }
        internal bool Vertical(int run) => !Fill && !(Side != 0 && run == 1);
        internal sbyte NextX(int run)
        {
            Count(run);
            return run + 1 == RunCount ? (sbyte)0 : (sbyte)(Side == 0 ? 1 : Side);
        }
        internal ushort WordAt(int run, int block)
        {
            int count = Count(run);
            if ((uint)block >= count) throw new IndexOutOfRangeException();
            if (Fill) return Pointer == MotherBrainFakeDeathDraw.BackgroundRowEUnused ? (ushort)0x8319 : (ushort)0x8044;
            if (Side != 0 && run == 1) return 0x00ff;
            return Ceiling ? (block == 0 ? (ushort)0x12fc : (ushort)0x00ff) :
                (block == count - 1 ? (ushort)0x1339 : (ushort)0x00ff);
        }
    }

    internal static bool TryDescribeRegular(ushort pointer, out RegularDraw draw)
    {
        var list = (MotherBrainFakeDeathDraw)pointer;
        draw = default;
        if (!Enum.IsDefined(list)) return false;
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

    private static bool TryGetRegular(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeRegular(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[shape.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.Count(run)];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new((ushort)(words.Length | (shape.Vertical(run) ? 0x8000 : 0)), words, shape.NextX(run), 0);
        }
        draw = new(pointer, runs);
        return true;
    }

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
    private static readonly ushort[][] BackgroundVisuals =
    [
        [0x0241, 0x0242, 0x02fc, 0x02fc, 0x02fc, 0x0243, 0x0244, 0x02fc, 0x0245, 0x0642, 0x0241, 0x0241, 0x0246],
        [0x09ef, 0x01b2, 0x01e5, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e5, 0x01e5, 0x05b2, 0x09ef, 0x09ef, 0x01b2],
        [0x01b1, 0x01d2, 0x01c6, 0x01c7, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a6, 0x09ca, 0x060c, 0x05b1, 0x0a09],
        [0x01d1, 0x01f2, 0x01a4, 0x01e7, 0x01a4, 0x0226, 0x0227, 0x01a5, 0x01a4, 0x020d, 0x0e09, 0x01b1, 0x01ab],
        [0x01b1, 0x0212, 0x01c4, 0x01c9, 0x01c4, 0x0206, 0x0207, 0x01c5, 0x01c4, 0x0628, 0x01ac, 0x01ec, 0x01ec],
        [0x01b1, 0x0a0c, 0x05ca, 0x0dc7, 0x01aa, 0x01a8, 0x01a8, 0x01a8, 0x01a8, 0x0628, 0x01ab, 0x01cd, 0x01cd],
        [0x01d1, 0x01d0, 0x05ea, 0x00ff, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a7, 0x0a0d, 0x0609, 0x01eb, 0x01d0],
        [0x01eb, 0x01eb, 0x05ea, 0x00ff, 0x00ff, 0x0206, 0x0207, 0x00ff, 0x01a6, 0x00ff, 0x0a2c, 0x0609, 0x01ae],
        [0x01ec, 0x01af, 0x05ea, 0x05c7, 0x05c6, 0x0206, 0x0207, 0x01a8, 0x01a6, 0x01a8, 0x01a8, 0x05d2, 0x01ae],
        [0x01ac, 0x01af, 0x01b2, 0x05e7, 0x01e5, 0x0226, 0x0227, 0x01e5, 0x01a6, 0x01e6, 0x01e5, 0x05b2, 0x01cd],
        [0x060c, 0x01ef, 0x01b2, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e6, 0x01e5, 0x01e5, 0x01e5, 0x05b2, 0x01ef],
        [0x0248, 0x0249, 0x024a, 0x024b, 0x0339, 0x024c, 0x024d, 0x0339, 0x024e, 0x0339, 0x0339, 0x024f, 0x0249],
    ];

    /// <summary>
    /// Twelve horizontal thirteen-cell rows at 84:9505..966C, spaced thirty
    /// bytes apart. Top/bottom rows retain collision type one; interior rows
    /// are air. Visual bits remain independent from these physical boundaries.
    /// </summary>
    internal readonly record struct BackgroundDraw(int Row)
    {
        internal ushort WordAt(int column)
        {
            if ((uint)column >= 13) throw new IndexOutOfRangeException();
            return (ushort)(BackgroundVisuals[Row][column] | (Row is 0 or 11 ? 0x1000 : 0));
        }
    }

    internal static bool TryDescribeBackground(ushort pointer, out BackgroundDraw draw)
    {
        int offset = pointer - (int)MotherBrainFakeDeathDraw.BackgroundRow2;
        bool owned = offset >= 0 && pointer <= (int)MotherBrainFakeDeathDraw.BackgroundRowD && offset % 30 == 0;
        draw = owned ? new(offset / 30) : default;
        return owned;
    }

    private static bool TryGetBackground(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBackground(pointer, out var row)) return false;
        var words = new ushort[13];
        for (int column = 0; column < words.Length; column++) words[column] = row.WordAt(column);
        draw = new(pointer, new RoomPlmShotBlockDrawDefinitions.Run[] { new(13, words, 0, 0) });
        return true;
    }

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

    // Called only after validating the two-column/four-row door domain.
    private static ushort EscapeDoorVisualAt(int column, int row) => row switch
    {
        0 => (ushort)(DoorUpperEdgeLeft + column),
        1 => column == 0 ? DoorInteriorLeft : DoorInteriorRight,
        2 => DoorLowerInterior,
        3 => (ushort)(DoorLowerEdgeLeft + column),
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>
    /// 84:94A3/94B1: a three-cell wall centered on the origin, or a two-column
    /// four-cell door. Both use vertical runs. Wall caps mirror tile 30F around
    /// center tile 340 and are solid. The door's first column has a door parent
    /// above three vertical extensions; its second column is air.
    /// </summary>
    internal readonly record struct BoundaryDraw(MotherBrainFakeDeathDraw Pointer)
    {
        internal int Count(int run)
        {
            if ((uint)run >= 2) throw new IndexOutOfRangeException();
            return Pointer == MotherBrainFakeDeathDraw.EscapeDoor ? 4 : 2 - run;
        }
        internal sbyte NextX(int run)
        {
            Count(run);
            return Pointer == MotherBrainFakeDeathDraw.EscapeDoor && run == 0 ? (sbyte)1 : (sbyte)0;
        }
        internal sbyte NextY(int run)
        {
            Count(run);
            return Pointer == MotherBrainFakeDeathDraw.FillWall && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        internal ushort WordAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new IndexOutOfRangeException();
            if (Pointer == MotherBrainFakeDeathDraw.FillWall)
                return (ushort)(0x8000 | (run == 0 && block == 0 ? 0x340 : 0x30f | (run == 1 ? 0x800 : 0)));
            RoomCollisionType collision = run == 1 ? RoomCollisionType.Air :
                block == 0 ? RoomCollisionType.DoorBlock : RoomCollisionType.VerticalExtension;
            return (ushort)(((int)collision << 12) | EscapeDoorVisualAt(run, block));
        }
    }

    internal static bool TryDescribeBoundary(ushort pointer, out BoundaryDraw draw)
    {
        var list = (MotherBrainFakeDeathDraw)pointer;
        bool owned = list is MotherBrainFakeDeathDraw.FillWall or MotherBrainFakeDeathDraw.EscapeDoor;
        draw = owned ? new(list) : default;
        return owned;
    }

    private static bool TryGetBoundary(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        draw = default;
        if (!TryDescribeBoundary(pointer, out var shape)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[2];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[shape.Count(run)];
            for (int block = 0; block < words.Length; block++) words[block] = shape.WordAt(run, block);
            runs[run] = new((ushort)(0x8000 | words.Length), words, shape.NextX(run), shape.NextY(run));
        }
        draw = new(pointer, runs);
        return true;
    }

    private static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> BoundaryDraws()
    {
        TryGetBoundary((ushort)MotherBrainFakeDeathDraw.FillWall, out var wall);
        yield return wall;
        TryGetBoundary((ushort)MotherBrainFakeDeathDraw.EscapeDoor, out var door);
        yield return door;
    }
}
