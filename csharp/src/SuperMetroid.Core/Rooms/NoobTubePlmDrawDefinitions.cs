namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw lists selected by the n00b-tube PLM. The native
/// coroutine still owns power-bomb gating, shard release, room FX, the event
/// bit and Samus's input lock; these definitions only replace immutable draw
/// payload reads.
/// </summary>
internal static class NoobTubePlmDrawDefinitions
{
    /// <summary>Intact projectile-trigger block at $84:98D1.</summary>
    private const ushort Intact = 0x98d1;
    /// <summary>First damaged origin block at $84:98D7.</summary>
    private const ushort Damaged = 0x98d7;
    /// <summary>Opened origin block at $84:98DD.</summary>
    private const ushort Opened = 0x98dd;
    /// <summary>Four-row cleared tube at $84:98E3.</summary>
    private const ushort Cleared = 0x98e3;
    /// <summary>Late broken-tube panel at $84:9953.</summary>
    private const ushort BrokenLate = 0x9953;
    /// <summary>Three-row opened tube at $84:9991.</summary>
    private const ushort OpenedRows = 0x9991;
    /// <summary>Full broken-tube panel at $84:99E5.</summary>
    private const ushort BrokenFull = 0x99e5;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(7);
        Add(lists, Intact, R(0x0001, [0xc540]));
        Add(lists, Damaged, R(0x0001, [0x8540]));
        Add(lists, Opened, R(0x0001, [0x8141]));
        Add(lists, Cleared,
            R(0x000c, EdgedRow(0x8141, 0x8541), nextY: 1),
            R(0x000c, EdgedRow(0x0322, 0x0722), nextY: 2),
            R(0x000c, EdgedRow(0x0323, 0x0723), nextY: 3),
            R(0x000c, EdgedRow(0x0b23, 0x0f23)));
        Add(lists, BrokenLate,
            R(0x0001, [0x0141], nextY: 4),
            R(0x000c, EdgedRow(0x0b22, 0x0f22), nextY: 5),
            R(0x000c, FinalRow()));
        Add(lists, OpenedRows,
            R(0x000c, EdgedRow(0x8141, 0x8541), nextY: 1),
            R(0x000c, EdgedRow(0x0322, 0x0722), nextY: 2),
            R(0x000c, EdgedRow(0x0323, 0x0723)));
        Add(lists, BrokenFull,
            R(0x0001, [0x0141], nextY: 3),
            R(0x000c, EdgedRow(0x0b23, 0x0f23), nextY: 4),
            R(0x000c, EdgedRow(0x0b22, 0x0f22), nextY: 5),
            R(0x000c, FinalRow()));
        return lists;
    }

    private static ushort[] EdgedRow(ushort first, ushort last)
    {
        ushort[] words = Enumerable.Repeat((ushort)0x00ff, 12).ToArray();
        words[0] = first;
        words[^1] = last;
        return words;
    }

    private static ushort[] FinalRow()
    {
        ushort[] words = EdgedRow(0x814e, 0x854e);
        words[1] = 0x814f;
        words[^2] = 0x854f;
        return words;
    }

    private static RoomPlmShotBlockDrawDefinitions.Run R(
        ushort directionAndCount, ushort[] words, sbyte nextY = 0)
    {
        if ((directionAndCount & 0x7fff) != words.Length)
            throw new InvalidDataException("N00b-tube draw count differs from its payload.");
        return new(directionAndCount, words, 0, nextY);
    }

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer, params RoomPlmShotBlockDrawDefinitions.Run[] runs)
    {
        if (runs.Length is < 1 or > 4 ||
            runs[^1].NextX != 0 || runs[^1].NextY != 0)
            throw new InvalidDataException($"N00b-tube draw ${pointer:X4} has invalid runs.");
        if (!lists.TryAdd(pointer, new(pointer, runs)))
            throw new InvalidDataException($"Duplicate n00b-tube draw ${pointer:X4}.");
    }
}
