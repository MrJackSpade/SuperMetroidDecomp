namespace SuperMetroid.Core.Rooms;

/// <summary>Mother Brain fake-death room physical PLM draws at $84:94A3..9716.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitions
{
    /// <summary><c>$84:94A3</c>: fill wall.</summary>
    internal const ushort FillWall = 0x94a3;
    /// <summary><c>$84:94B1</c>: escape door.</summary>
    internal const ushort EscapeDoor = 0x94b1;
    /// <summary><c>$84:9505</c>: background row 2.</summary>
    internal const ushort BackgroundRow2 = 0x9505;
    /// <summary><c>$84:9523</c>: background row 3.</summary>
    internal const ushort BackgroundRow3 = 0x9523;
    /// <summary><c>$84:9541</c>: background row 4.</summary>
    internal const ushort BackgroundRow4 = 0x9541;
    /// <summary><c>$84:955F</c>: background row 5.</summary>
    internal const ushort BackgroundRow5 = 0x955f;
    /// <summary><c>$84:957D</c>: background row 6.</summary>
    internal const ushort BackgroundRow6 = 0x957d;
    /// <summary><c>$84:959B</c>: background row 7.</summary>
    internal const ushort BackgroundRow7 = 0x959b;
    /// <summary><c>$84:95B9</c>: background row 8.</summary>
    internal const ushort BackgroundRow8 = 0x95b9;
    /// <summary><c>$84:95D7</c>: background row 9.</summary>
    internal const ushort BackgroundRow9 = 0x95d7;
    /// <summary><c>$84:95F5</c>: background row a.</summary>
    internal const ushort BackgroundRowA = 0x95f5;
    /// <summary><c>$84:9613</c>: background row b.</summary>
    internal const ushort BackgroundRowB = 0x9613;
    /// <summary><c>$84:9631</c>: background row c.</summary>
    internal const ushort BackgroundRowC = 0x9631;
    /// <summary><c>$84:964F</c>: background row d.</summary>
    internal const ushort BackgroundRowD = 0x964f;
    /// <summary><c>$84:966D</c>: cartridge-unused background row E.</summary>
    internal const ushort BackgroundRowEUnused = 0x966d;
    /// <summary><c>$84:968B</c>: cartridge-unused background row F.</summary>
    internal const ushort BackgroundRowFUnused = 0x968b;
    /// <summary><c>$84:96A9</c>: clear ceiling block.</summary>
    internal const ushort ClearCeilingBlock = 0x96a9;
    /// <summary><c>$84:96B1</c>: clear ceiling tube.</summary>
    internal const ushort ClearCeilingTube = 0x96b1;
    /// <summary><c>$84:96BF</c>: clear bottom middle side tube.</summary>
    internal const ushort ClearBottomMiddleSideTube = 0x96bf;
    /// <summary><c>$84:96CB</c>: clear bottom middle tubes.</summary>
    internal const ushort ClearBottomMiddleTubes = 0x96cb;
    /// <summary><c>$84:96EF</c>: clear bottom left tube.</summary>
    internal const ushort ClearBottomLeftTube = 0x96ef;
    /// <summary><c>$84:9703</c>: clear bottom right tube.</summary>
    internal const ushort ClearBottomRightTube = 0x9703;
    /// <summary><c>$84:9717</c>: first byte of the following glass draw region.</summary>
    internal const ushort EndExclusive = 0x9717;

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        BoundaryDraws().Concat(BackgroundDraws()).Concat(RegularDraws());

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        TryGetBackground(pointer, out draw) || TryGetRegular(pointer, out draw) || TryGetBoundary(pointer, out draw);

    internal static string VisualId(ushort pointer) => pointer switch
    {
        FillWall => "fill-wall",
        EscapeDoor => "escape-door",
        BackgroundRow2 => "background-row-2",
        BackgroundRow3 => "background-row-3",
        BackgroundRow4 => "background-row-4",
        BackgroundRow5 => "background-row-5",
        BackgroundRow6 => "background-row-6",
        BackgroundRow7 => "background-row-7",
        BackgroundRow8 => "background-row-8",
        BackgroundRow9 => "background-row-9",
        BackgroundRowA => "background-row-a",
        BackgroundRowB => "background-row-b",
        BackgroundRowC => "background-row-c",
        BackgroundRowD => "background-row-d",
        BackgroundRowEUnused => "background-row-e-unused",
        BackgroundRowFUnused => "background-row-f-unused",
        ClearCeilingBlock => "clear-ceiling-block",
        ClearCeilingTube => "clear-ceiling-tube",
        ClearBottomMiddleSideTube => "clear-bottom-middle-side-tube",
        ClearBottomMiddleTubes => "clear-bottom-middle-tubes",
        ClearBottomLeftTube => "clear-bottom-left-tube",
        ClearBottomRightTube => "clear-bottom-right-tube",
        _ => throw new InvalidDataException(
            $"Mother Brain fake-death draw ${pointer:X4} has no visual ID."),
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
    internal readonly record struct RegularDraw(ushort Pointer, int Height, int Width, bool Ceiling, bool Fill, int Side)
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
            if (Fill) return Pointer == BackgroundRowEUnused ? (ushort)0x8319 : (ushort)0x8044;
            if (Side != 0 && run == 1) return 0x00ff;
            return Ceiling ? (block == 0 ? (ushort)0x12fc : (ushort)0x00ff) :
                (block == count - 1 ? (ushort)0x1339 : (ushort)0x00ff);
        }
    }

    internal static bool TryDescribeRegular(ushort pointer, out RegularDraw draw)
    {
        draw = pointer switch
        {
            BackgroundRowEUnused or BackgroundRowFUnused => new(pointer, 13, 1, false, true, 0),
            ClearCeilingBlock => new(pointer, 2, 1, true, false, 0),
            ClearCeilingTube => new(pointer, 5, 1, true, false, 0),
            ClearBottomMiddleSideTube => new(pointer, 4, 1, false, false, 0),
            ClearBottomMiddleTubes => new(pointer, 7, 2, false, false, 0),
            ClearBottomLeftTube => new(pointer, 5, 2, false, false, 1),
            ClearBottomRightTube => new(pointer, 5, 2, false, false, -1),
            _ => default,
        };
        return draw.Pointer != 0;
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
        for (int pointer = BackgroundRowEUnused; pointer < EndExclusive;)
        {
            TryGetRegular((ushort)pointer, out var draw);
            yield return draw;
            foreach (var run in draw.Runs.ToArray()) pointer += 4 + run.LevelWords.Length * 2;
        }
    }

    // Visual cells still require an independent artwork disposition under #1165.
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
        int offset = pointer - BackgroundRow2;
        bool owned = offset >= 0 && pointer <= BackgroundRowD && offset % 30 == 0;
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
        for (int pointer = BackgroundRow2; pointer <= BackgroundRowD; pointer += 30)
        {
            TryGetBackground((ushort)pointer, out var draw);
            yield return draw;
        }
    }

    // Escape-door visual cells remain pending independent artwork review under #1165.
    private static readonly ushort[] EscapeDoorVisuals = [0x222,0x1af,0x1d0,0x220,0x223,0x1eb,0x1d0,0x221];

    /// <summary>
    /// 84:94A3/94B1: a three-cell wall centered on the origin, or a two-column
    /// four-cell door. Both use vertical runs. Wall caps mirror tile 30F around
    /// center tile 340 and are solid. The door's first column has a door parent
    /// above three vertical extensions; its second column is air.
    /// </summary>
    internal readonly record struct BoundaryDraw(ushort Pointer)
    {
        internal int Count(int run)
        {
            if ((uint)run >= 2) throw new IndexOutOfRangeException();
            return Pointer == EscapeDoor ? 4 : 2 - run;
        }
        internal sbyte NextX(int run)
        {
            Count(run);
            return Pointer == EscapeDoor && run == 0 ? (sbyte)1 : (sbyte)0;
        }
        internal sbyte NextY(int run)
        {
            Count(run);
            return Pointer == FillWall && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        internal ushort WordAt(int run, int block)
        {
            if ((uint)block >= Count(run)) throw new IndexOutOfRangeException();
            if (Pointer == FillWall)
                return (ushort)(0x8000 | (run == 0 && block == 0 ? 0x340 : 0x30f | (run == 1 ? 0x800 : 0)));
            RoomCollisionType collision = run == 1 ? RoomCollisionType.Air :
                block == 0 ? RoomCollisionType.DoorBlock : RoomCollisionType.VerticalExtension;
            return (ushort)(((int)collision << 12) | EscapeDoorVisuals[run * 4 + block]);
        }
    }

    internal static bool TryDescribeBoundary(ushort pointer, out BoundaryDraw draw)
    {
        bool owned = pointer is FillWall or EscapeDoor;
        draw = owned ? new(pointer) : default;
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
        TryGetBoundary(FillWall, out var wall);
        yield return wall;
        TryGetBoundary(EscapeDoor, out var door);
        yield return door;
    }
}
