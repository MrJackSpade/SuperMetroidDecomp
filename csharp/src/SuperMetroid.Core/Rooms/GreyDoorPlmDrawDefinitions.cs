namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical four-block draw lists for grey door caps and the four shared clear-cap
/// frames. Bomb Torizo's special resident door uses the ordinary right-facing art.
/// </summary>
internal static class GreyDoorPlmDrawDefinitions
{
    /// <summary>Shared left-facing door-clear frame at $84:A677.</summary>
    private const ushort ClearLeft = 0xa677;
    /// <summary>Shared right-facing door-clear frame at $84:A683.</summary>
    private const ushort ClearRight = 0xa683;
    /// <summary>Shared upward-facing door-clear frame at $84:A68F.</summary>
    private const ushort ClearUp = 0xa68f;
    /// <summary>Shared downward-facing door-clear frame at $84:A69B.</summary>
    private const ushort ClearDown = 0xa69b;
    /// <summary>Grey left-facing door, first draw at $84:A6A7.</summary>
    private const ushort GreyLeft = 0xa6a7;
    /// <summary>Grey right-facing door, first draw at $84:A6D7.</summary>
    private const ushort GreyRight = 0xa6d7;
    /// <summary>Grey upward-facing door, first draw at $84:A707.</summary>
    private const ushort GreyUp = 0xa707;
    /// <summary>Grey downward-facing door, first draw at $84:A737.</summary>
    private const ushort GreyDown = 0xa737;

    internal const int DrawListBytes = 12;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer)
    {
        if (pointer == ClearLeft) return "clear-left";
        if (pointer == ClearRight) return "clear-right";
        if (pointer == ClearUp) return "clear-up";
        if (pointer == ClearDown) return "clear-down";
        foreach ((ushort first, string name) in new[]
                 {
                     (GreyLeft, "grey-left"), (GreyRight, "grey-right"),
                     (GreyUp, "grey-up"), (GreyDown, "grey-down"),
                 })
        {
            int offset = pointer - first;
            if (offset >= 0 && offset < 4 * DrawListBytes &&
                offset % DrawListBytes == 0)
                return $"{name}-frame-{offset / DrawListBytes}";
        }
        throw new InvalidDataException($"Grey-door draw ${pointer:X4} has no visual ID.");
    }

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists.Values)
        {
            if (string.Equals(id, VisualId(candidate.Pointer), StringComparison.Ordinal))
            {
                list = candidate;
                return true;
            }
        }
        list = default;
        return false;
    }

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(20);
        Add(lists, ClearLeft, 0x8004, [0x0082, 0x00a2, 0x08a2, 0x0882]);
        Add(lists, ClearRight, 0x8004, [0x0482, 0x04a2, 0x0ca2, 0x0c82]);
        Add(lists, ClearUp, 0x0004, [0x0484, 0x0483, 0x0083, 0x0084]);
        Add(lists, ClearDown, 0x0004, [0x0c84, 0x0c83, 0x0883, 0x0884]);
        AddFourFrames(lists, GreyLeft, 0x8004,
            [0xc0ae, 0xd0ce, 0xd8ce, 0xd8ae],
            [0x80af, 0x80cf, 0x88cf, 0x88af],
            [0x80b0, 0x80d0, 0x88d0, 0x88b0],
            [0x80b1, 0x00d1, 0x08d1, 0x88b1]);
        AddFourFrames(lists, GreyRight, 0x8004,
            [0xc4ae, 0xd4ce, 0xdcce, 0xdcae],
            [0x84af, 0x84cf, 0x8ccf, 0x8caf],
            [0x84b0, 0x84d0, 0x8cd0, 0x8cb0],
            [0x84b1, 0x84d1, 0x8cd1, 0x8cb1]);
        AddFourFrames(lists, GreyUp, 0x0004,
            [0xc4b3, 0x54b2, 0x50b2, 0x50b3],
            [0x84d3, 0x84d2, 0x80d2, 0x80d3],
            [0x84b5, 0x84b4, 0x80b4, 0x80b5],
            [0x84d5, 0x84d4, 0x80d4, 0x80d5]);
        AddFourFrames(lists, GreyDown, 0x0004,
            [0xccb3, 0x5cb2, 0x58b2, 0x58b3],
            [0x8cd3, 0x8cd2, 0x88d2, 0x88d3],
            [0x8cb5, 0x8cb4, 0x88b4, 0x88b5],
            [0x8cd5, 0x0cd4, 0x08d4, 0x88d5]);
        return lists;
    }

    private static void AddFourFrames(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort firstPointer, ushort directionAndCount, params ushort[][] frames)
    {
        if (frames.Length != 4)
            throw new InvalidDataException("A grey cap needs exactly four draw frames.");
        for (int frame = 0; frame < frames.Length; frame++)
            Add(lists, checked((ushort)(firstPointer + frame * DrawListBytes)),
                directionAndCount, frames[frame]);
    }

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer, ushort directionAndCount, ushort[] words)
    {
        if (words.Length != 4)
            throw new InvalidDataException("A door-cap draw list needs four blocks.");
        if (!lists.TryAdd(pointer, new(pointer,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(directionAndCount, words, 0, 0),
                })))
            throw new InvalidDataException($"Duplicate grey-door draw ${pointer:X4}.");
    }
}
