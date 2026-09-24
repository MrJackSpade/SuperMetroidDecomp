namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Immutable bank-$84 level-word mutations drawn by the ordinary shot-block PLMs.
/// Each word contains both collision type and a visual block reference; keeping the
/// entire native word here prevents artwork replacement from changing terrain rules.
/// </summary>
internal static class RoomPlmShotBlockDrawDefinitions
{
    /// <summary>First one-block breakup draw list, <c>$84:A345</c>.</summary>
    internal const ushort SingleFrame0 = 0xa345;
    /// <summary>First horizontal-pair breakup draw list, <c>$84:A35D</c>.</summary>
    internal const ushort HorizontalFrame0 = 0xa35d;
    /// <summary>First vertical-pair breakup draw list, <c>$84:A37D</c>.</summary>
    internal const ushort VerticalFrame0 = 0xa37d;
    /// <summary>First square breakup draw list, <c>$84:A39D</c>.</summary>
    internal const ushort SquareFrame0 = 0xa39d;
    /// <summary>Final two-block horizontal collision-parent restoration, <c>$84:A47B</c>.</summary>
    internal const ushort RestoreHorizontal = 0xa47b;
    /// <summary>Final two-block vertical collision-parent restoration, <c>$84:A483</c>.</summary>
    internal const ushort RestoreVertical = 0xa483;
    /// <summary>Final four-block square collision-parent restoration, <c>$84:A48B</c>.</summary>
    internal const ushort RestoreSquare = 0xa48b;

    /// <summary>One native record: direction/count, complete level words, then the signed offset to the next record.</summary>
    internal readonly record struct Run(
        ushort DirectionAndCount,
        ReadOnlyMemory<ushort> LevelWords,
        sbyte NextX,
        sbyte NextY);

    internal readonly record struct DrawList(ushort Pointer, ReadOnlyMemory<Run> Runs);

    private static readonly ushort[] BreakFrames = [0x0053, 0x0054, 0x0055, 0x00ff];
    private static readonly IReadOnlyDictionary<ushort, DrawList> Lists = Build();

    internal static IEnumerable<DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer, out DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort, DrawList> Build()
    {
        var lists = new Dictionary<ushort, DrawList>(19);
        for (int frame = 0; frame < BreakFrames.Length; frame++)
        {
            ushort word = BreakFrames[frame];
            Add(lists, checked((ushort)(SingleFrame0 + frame * 6)),
                new Run(1, new ushort[] { word }, 0, 0));
            Add(lists, checked((ushort)(HorizontalFrame0 + frame * 8)),
                new Run(2, new ushort[] { word, word }, 0, 0));
            Add(lists, checked((ushort)(VerticalFrame0 + frame * 8)),
                new Run(0x8002, new ushort[] { word, word }, 0, 0));
            Add(lists, checked((ushort)(SquareFrame0 + frame * 16)),
                new Run(2, new ushort[] { word, word }, 0, 1),
                new Run(2, new ushort[] { word, word }, 0, 0));
        }

        // The final draw of a multi-block respawning shot block restores native parent
        // collision nibbles and child directions, not just its original appearance.
        Add(lists, RestoreHorizontal,
            new Run(2, new ushort[] { 0xc096, 0x5097 }, 0, 0));
        Add(lists, RestoreVertical,
            new Run(0x8002, new ushort[] { 0xc098, 0xd0b8 }, 0, 0));
        Add(lists, RestoreSquare,
            new Run(2, new ushort[] { 0xc099, 0x509a }, 0, 1),
            new Run(2, new ushort[] { 0xd0b9, 0xd0ba }, 0, 0));
        return lists;
    }

    private static void Add(Dictionary<ushort, DrawList> lists, ushort pointer, params Run[] runs)
    {
        if (!lists.TryAdd(pointer, new DrawList(pointer, runs)))
            throw new InvalidDataException($"Duplicate compiled shot-block draw list ${pointer:X4}.");
    }
}
