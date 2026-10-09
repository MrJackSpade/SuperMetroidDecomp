namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Downward-gate physical draws at $84:A517..A56A and A5D7..A626. Six vertical
/// five-cell frames fill one additional shootable row per step; the top tile is
/// stationary at the endpoints and moving in between. Four trigger colors use
/// consecutive descending tiles, mirrored on the right. Gameplay evaluates cells
/// directly; temporary DTOs preserve the artwork import/export interface.
/// </summary>
internal static class DownwardGatePlmDrawDefinitions
{
    /// <summary>$84:A517: first five-block open-gate column, fourteen bytes per frame.</summary>
    private const ushort ResidentFirst = 0xa517;
    /// <summary>$84:A5D7: blue-left trigger; color pairs occupy twenty bytes.</summary>
    private const ushort TriggerLeftFirst = 0xa5d7;

    /// <summary>Compact geometry and selection data for one resident-gate frame or colored trigger draw list.</summary>
    /// <param name="Column">True for the five-cell vertical resident-gate column; false for a colored trigger tile.</param>
    /// <param name="Frame">Resident animation frame 0–5, or trigger color index in blue, green, red, yellow order.</param>
    /// <param name="Right">For trigger draws, selects the mirrored right-side tile; ignored for resident columns.</param>
    internal readonly record struct Draw(bool Column, int Frame, bool Right)
    {
        /// <summary>Number of horizontal runs needed to represent this draw in the shared PLM artwork format.</summary>
        internal int RunCount => Column || Right ? 1 : 2;

        /// <summary>Returns the number of tile words in a run, validating the run index.</summary>
        internal int WordCount(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return Column ? 5 : Right ? 2 : 1;
        }
        /// <summary>Packs the run's word count and vertical-column bit into the shared draw-list header word.</summary>
        internal ushort DirectionAndCount(int run) =>
            (ushort)(WordCount(run) | (Column ? 0x8000 : 0));

        /// <summary>Returns the horizontal displacement to the following run, including the left-trigger second run offset.</summary>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new IndexOutOfRangeException();
            return !Column && !Right && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Calculates one native tile word from the selected frame, run, and word position.</summary>
        internal ushort WordAt(int run, int word)
        {
            if ((uint)word >= WordCount(run)) throw new IndexOutOfRangeException();
            if (Column)
                return word == 0 ? (Frame is 0 or 5 ? (ushort)0xc0d6 : (ushort)0xc0d7) :
                    (ushort)(0xff | (word <= Frame ? 0xc000 : 0));
            if (run == 0 && word == 0) return 0x80d6;
            return (ushort)(0xc0db - Frame + (Right ? 0x400 : 0));
        }
    }

    /// <summary>Accepts only six column starts and eight trigger starts; gaps belong to upward gates.</summary>
    internal static bool TryDescribe(ushort pointer, out Draw draw)
    {
        draw = default;
        int columnOffset = pointer - ResidentFirst;
        if (columnOffset >= 0 && columnOffset <= 70 && columnOffset % 14 == 0)
        {
            draw = new(true, columnOffset / 14, false);
            return true;
        }
        int triggerOffset = pointer - TriggerLeftFirst;
        if (triggerOffset >= 0 && triggerOffset <= 72 && triggerOffset % 20 is 0 or 12)
        {
            draw = new(false, triggerOffset / 20, triggerOffset % 20 == 12);
            return true;
        }
        return false;
    }

    /// <summary>Enumerates the native list pointers for six resident frames and both sides of four trigger colors.</summary>
    private static IEnumerable<ushort> Pointers()
    {
        for (int frame = 0; frame < 6; frame++) yield return (ushort)(ResidentFirst + frame * 14);
        for (int color = 0; color < 4; color++)
        {
            yield return (ushort)(TriggerLeftFirst + color * 20);
            yield return (ushort)(TriggerLeftFirst + color * 20 + 12);
        }
    }

    /// <summary>Projects every downward-gate draw into the common shot-block artwork representation.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (ushort pointer in Pointers())
            {
                TryGet(pointer, out var draw);
                yield return draw;
            }
        }
    }

    /// <summary>Builds the common draw-list representation when <paramref name="pointer"/> names a downward-gate list.</summary>
    internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(pointer, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int word = 0; word < words.Length; word++) words[word] = draw.WordAt(run, word);
            runs[run] = new(draw.DirectionAndCount(run), words, draw.NextX(run), 0);
        }
        list = new(pointer, runs);
        return true;
    }

    /// <summary>Returns the stable asset ID for a resident animation frame or colored trigger pointer.</summary>
    internal static string VisualId(ushort pointer)
    {
        if (!TryDescribe(pointer, out var draw))
            throw new InvalidDataException($"Downward gate draw list ${pointer:X4} has no visual ID.");
        if (draw.Column) return $"column-frame-{draw.Frame}";
        // Keep published artwork IDs: the historic green/red names differ from native order.
        string color = draw.Frame switch { 0 => "blue", 1 => "green", 2 => "red", _ => "yellow" };
        return $"{color}-{(draw.Right ? "right" : "left")}-trigger";
    }

    /// <summary>Resolves a stable asset ID using ordinal matching and returns its common draw-list representation.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (ushort pointer in Pointers())
            if (string.Equals(id, VisualId(pointer), StringComparison.Ordinal))
                return TryGet(pointer, out list);
        list = default;
        return false;
    }
}
