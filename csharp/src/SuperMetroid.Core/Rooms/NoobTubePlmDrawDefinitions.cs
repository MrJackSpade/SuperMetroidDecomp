using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>The seven bank-$84 n00b-tube draw lists, valued by their native address.</summary>
internal enum NoobTubeDraw : ushort
{
    /// <summary>Intact projectile-trigger block at $84:98D1.</summary>
    Intact = 0x98d1,
    /// <summary>First damaged origin block at $84:98D7.</summary>
    Damaged = 0x98d7,
    /// <summary>Opened origin block at $84:98DD.</summary>
    Opened = 0x98dd,
    /// <summary>Four-row cleared tube at $84:98E3.</summary>
    Cleared = 0x98e3,
    /// <summary>Late broken-tube panel at $84:9953.</summary>
    BrokenLate = 0x9953,
    /// <summary>Three-row opened tube at $84:9991.</summary>
    OpenedRows = 0x9991,
    /// <summary>Full broken-tube panel at $84:99E5.</summary>
    BrokenFull = 0x99e5,
}

/// <summary>
/// Physical bank-$84 draw lists selected by the n00b-tube PLM. The native
/// coroutine still owns power-bomb gating, shard release, room FX, the event
/// bit and Samus's input lock; these definitions only replace immutable draw
/// payload reads.
/// </summary>
internal static class NoobTubePlmDrawDefinitions
{
    /// <summary>
    /// Seven named NTSC layouts at 84:98D1..9A3E. Wide runs are twelve cells:
    /// symmetric pipe edges surround air; the floor adds a second solid edge cell.
    /// Panel rows use adjacent tiles, vertically reflected below the centre.
    /// Continuations are absolute offsets from the PLM origin, not accumulated steps.
    /// </summary>
    internal readonly record struct Draw(NoobTubeDraw Pointer)
    {
        internal int RunCount => Pointer switch
        {
            NoobTubeDraw.Cleared or NoobTubeDraw.BrokenFull => 4,
            NoobTubeDraw.BrokenLate or NoobTubeDraw.OpenedRows => 3,
            NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened => 1,
            _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Pointer}."),
        };

        private void CheckRun(int run)
        {
            if ((uint)run >= (uint)RunCount) throw new IndexOutOfRangeException();
        }

        internal int WordCount(int run)
        {
            CheckRun(run);
            return Pointer is NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened ||
                (Pointer is NoobTubeDraw.BrokenLate or NoobTubeDraw.BrokenFull && run == 0) ? 1 : 12;
        }

        private int Row(int run) => Pointer switch
        {
            NoobTubeDraw.BrokenLate => run + 3,
            NoobTubeDraw.BrokenFull => run + 2,
            NoobTubeDraw.Intact or NoobTubeDraw.Damaged or NoobTubeDraw.Opened or
                NoobTubeDraw.Cleared or NoobTubeDraw.OpenedRows => run,
            _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Pointer}."),
        };

        internal sbyte NextY(int run)
        {
            CheckRun(run);
            return run == RunCount - 1 ? (sbyte)0 : (sbyte)Row(run + 1);
        }

        internal ushort WordAt(int run, int cell)
        {
            if ((uint)cell >= (uint)WordCount(run)) throw new IndexOutOfRangeException();
            if (WordCount(run) == 1)
                return Pointer switch
                {
                    NoobTubeDraw.Intact => 0xc540, // Projectile trigger, mirrored intact glass tile.
                    NoobTubeDraw.Damaged => 0x8540, // Same glass tile, solid collision after activation.
                    NoobTubeDraw.Opened => 0x8141, // Solid upper rim at the origin.
                    NoobTubeDraw.BrokenLate or NoobTubeDraw.BrokenFull => 0x0141, // Broken frames clear collision at the origin.
                    NoobTubeDraw.Cleared or NoobTubeDraw.OpenedRows =>
                        throw new InvalidOperationException($"NoobTubeDraw {Pointer} has no single-word run."),
                    _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {Pointer}."),
                };
            int row = Row(run);
            int edgeDistance = Math.Min(cell, 11 - cell);
            if (edgeDistance > (row == 5 ? 1 : 0)) return 0x00ff;
            int tile = row switch
            {
                0 => 0x141,
                5 => 0x14e + edgeDistance,
                _ => 0x322 + Math.Min(row - 1, 4 - row),
            };
            int flips = (cell >= 6 ? 0x400 : 0) | (row is 3 or 4 ? 0x800 : 0);
            int collision = row is 0 or 5 ? 0x8000 : 0;
            return (ushort)(collision | flips | tile);
        }
    }

    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        bool owned = Enum.IsDefined((NoobTubeDraw)pointer);
        draw = owned ? new((NoobTubeDraw)pointer) : default;
        return owned;
    }

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw pointer in Pointers())
            {
                TryGet((ushort)pointer, out var list);
                yield return list;
            }
        }
    }

    private static IEnumerable<NoobTubeDraw> Pointers()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Damaged;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.Cleared;
        yield return NoobTubeDraw.BrokenLate;
        yield return NoobTubeDraw.OpenedRows;
        yield return NoobTubeDraw.BrokenFull;
    }

    // Temporary artwork import/export DTOs; gameplay calculates cells directly.
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int cell = 0; cell < words.Length; cell++) words[cell] = draw.WordAt(run, cell);
            runs[run] = new((ushort)words.Length, words, 0, draw.NextY(run));
        }
        list = new(pointer, runs);
        return true;
    }

    internal static string VisualId(ushort pointer) =>
        VisualId(ClosedNativeWords.Decode<NoobTubeDraw>(pointer, "n00b-tube draw with a visual ID"));

    internal static string VisualId(NoobTubeDraw pointer) => pointer switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Damaged => "damaged",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.Cleared => "cleared",
        NoobTubeDraw.BrokenLate => "broken-late",
        NoobTubeDraw.OpenedRows => "opened-rows",
        NoobTubeDraw.BrokenFull => "broken-full",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {pointer}."),
    };

    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (NoobTubeDraw pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal)) return TryGet((ushort)pointer, out list);
        list = default;
        return false;
    }
}
