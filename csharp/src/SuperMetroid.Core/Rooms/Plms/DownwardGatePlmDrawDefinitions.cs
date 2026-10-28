namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Downward-gate physical draws. Six vertical
/// five-cell frames fill one additional shootable row per step; the top tile is
/// stationary at the endpoints and moving in between. Four trigger colors use
/// consecutive descending tiles, mirrored on the right. Gameplay evaluates cells
/// directly; temporary DTOs preserve the artwork import/export interface.
/// </summary>
internal static class DownwardGatePlmDrawDefinitions
{

    /// <summary>Decoded column step or colored trigger shape; physical block words are calculated from this native draw identity.</summary>
    internal readonly record struct Draw(DownwardGatePart Part, [property: IntegerQuantity("gate animation frame index")] int Frame)
    {
        /// <summary>Whether the list writes the gate column.</summary>
        internal bool Column => Part == DownwardGatePart.Column;

        /// <summary>Whether the list writes a right-hand trigger.</summary>
        internal bool Right => Part == DownwardGatePart.RightTrigger;

        /// <summary>Columns and right triggers occupy one run; left triggers include a separate neighboring block.</summary>
        internal int RunCount => Column || Right ? 1 : 2;
        /// <summary>Columns write five vertical cells; right triggers write two cells; each left-trigger run writes one.</summary>
        internal int WordCount(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return Column ? 5 : Right ? 2 : 1;
        }
        /// <summary>The gate column writes down its column; trigger runs write along their row.</summary>
        internal PlmRunDirection Direction => Column ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal;
        /// <summary>Moves left between the two left-trigger runs; other runs stay in their column.</summary>
        internal sbyte NextX(int run)
        {
            if ((uint)run >= RunCount) throw new ArgumentOutOfRangeException(nameof(run));
            return !Column && !Right && run == 0 ? (sbyte)-1 : (sbyte)0;
        }
        /// <summary>Calculates the stationary/moving top tile, filled shootable rows, or color-specific trigger tile.</summary>
        internal ushort WordAt(int run, int word)
        {
            if ((uint)word >= WordCount(run)) throw new ArgumentOutOfRangeException(nameof(word));
            if (Column)
                return word == 0 ? (Frame is 0 or 5 ? (ushort)0xc0d6 : (ushort)0xc0d7) :
                    (ushort)(0xff | (word <= Frame ? 0xc000 : 0));
            if (run == 0 && word == 0) return 0x80d6;
            return (ushort)(0xc0db - Frame + (Right ? 0x400 : 0));
        }
    }

    /// <summary>The shape of one of the six column stages or eight trigger draws; a trigger's frame is its color's tile step.</summary>
    internal static bool TryDescribe(PlmDraw request, out Draw draw)
    {
        if (request is not PlmFrameDraw<DownwardGateDraw> { Frame: var gate })
        {
            draw = default;
            return false;
        }
        draw = gate switch
        {
            DownwardGateDraw.Open => new(DownwardGatePart.Column, 0),
            DownwardGateDraw.Lowered1 => new(DownwardGatePart.Column, 1),
            DownwardGateDraw.Lowered2 => new(DownwardGatePart.Column, 2),
            DownwardGateDraw.Lowered3 => new(DownwardGatePart.Column, 3),
            DownwardGateDraw.Lowered4 => new(DownwardGatePart.Column, 4),
            DownwardGateDraw.Closed => new(DownwardGatePart.Column, 5),
            DownwardGateDraw.TriggerBlueLeft => new(DownwardGatePart.LeftTrigger, 0),
            DownwardGateDraw.TriggerBlueRight => new(DownwardGatePart.RightTrigger, 0),
            DownwardGateDraw.TriggerRedLeft => new(DownwardGatePart.LeftTrigger, 1),
            DownwardGateDraw.TriggerRedRight => new(DownwardGatePart.RightTrigger, 1),
            DownwardGateDraw.TriggerGreenLeft => new(DownwardGatePart.LeftTrigger, 2),
            DownwardGateDraw.TriggerGreenRight => new(DownwardGatePart.RightTrigger, 2),
            DownwardGateDraw.TriggerYellowLeft => new(DownwardGatePart.LeftTrigger, 3),
            DownwardGateDraw.TriggerYellowRight => new(DownwardGatePart.RightTrigger, 3),
            _ => throw new InvalidOperationException($"Undefined {nameof(DownwardGateDraw)} {gate}."),
        };
        return true;
    }

    /// <summary>Enumerates six column stages followed by four left/right trigger color pairs.</summary>
    private static IEnumerable<PlmDraw> Draws()
    {
        foreach (DownwardGateDraw gate in Enum.GetValues<DownwardGateDraw>())
            yield return PlmDraw.Of(gate);
    }

    /// <summary>Projects calculated physical shapes into temporary draw records for the artwork interface.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (PlmDraw draw in Draws())
            {
                TryGet(draw, out var list);
                yield return list;
            }
        }
    }

    /// <summary>Reconstructs one supported native draw list from its column or trigger geometry.</summary>
    internal static bool TryGet(PlmDraw request, out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        list = default;
        if (!TryDescribe(request, out var draw)) return false;
        var runs = new RoomPlmShotBlockDrawDefinitions.Run[draw.RunCount];
        for (int run = 0; run < runs.Length; run++)
        {
            var words = new ushort[draw.WordCount(run)];
            for (int word = 0; word < words.Length; word++) words[word] = draw.WordAt(run, word);
            runs[run] = new(draw.Direction, words, new(draw.NextX(run), 0));
        }
        list = new(request, runs);
        return true;
    }

    /// <summary>Maps a draw to its published artwork identity, preserving historic green/red labels.</summary>
    internal static string VisualId(PlmDraw request)
    {
        if (!TryDescribe(request, out var draw))
            throw new InvalidDataException($"Downward gate draw list {request} has no visual ID.");
        if (draw.Column) return $"column-frame-{draw.Frame}";
        // Keep published artwork IDs: the historic green/red names differ from native order.
        string color = draw.Frame switch { 0 => "blue", 1 => "green", 2 => "red", _ => "yellow" };
        return $"{color}-{(draw.Right ? "right" : "left")}-trigger";
    }

    /// <summary>Looks up a published visual name against supported native column and trigger draws.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmShotBlockDrawDefinitions.FindByVisualId(Draws(), VisualId, TryGet, id, out list);
}
