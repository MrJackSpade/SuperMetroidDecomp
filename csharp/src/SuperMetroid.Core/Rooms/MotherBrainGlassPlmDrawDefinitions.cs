namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw layouts selected by Mother Brain's glass PLM. The
/// multi-run records include signed offsets from the PLM origin; the native
/// instruction program still owns damage thresholds, shards, and timing.
/// </summary>
internal static class MotherBrainGlassPlmDrawDefinitions
{
    /// <summary>Initial single glass block at $84:9717.</summary>
    private const ushort Initial = 0x9717;
    /// <summary>First full pane damage frame at $84:971D.</summary>
    private const ushort PaneDamage1 = 0x971d;
    /// <summary>Second full pane damage frame at $84:9731.</summary>
    private const ushort PaneDamage2 = 0x9731;
    /// <summary>Three-block pane transition at $84:9745.</summary>
    private const ushort PaneTransition = 0x9745;
    /// <summary>First shifted pane damage frame at $84:974F.</summary>
    private const ushort ShiftedPane1 = 0x974f;
    /// <summary>Second shifted pane damage frame at $84:9769.</summary>
    private const ushort ShiftedPane2 = 0x9769;
    /// <summary>Reduced shifted pane frame at $84:9781.</summary>
    private const ushort ShiftedPane3 = 0x9781;
    /// <summary>First four-run shatter frame at $84:978F.</summary>
    private const ushort Shatter1 = 0x978f;
    /// <summary>Second four-run shatter frame at $84:97B7.</summary>
    private const ushort Shatter2 = 0x97b7;
    /// <summary>Third four-run shatter frame at $84:97E7.</summary>
    private const ushort Shatter3 = 0x97e7;
    /// <summary>Four-run empty-glass frame at $84:9817.</summary>
    private const ushort Cleared = 0x9817;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>(11);
        Add(lists, Initial, R(0x0001, [0xc6c0]));
        Add(lists, PaneDamage1,
            R(0x8004, [0xc2c7, 0xd2c9, 0xdac9, 0x5ac7], -1, 1),
            R(0x8002, [0xd2c8, 0xdac8]));
        Add(lists, PaneDamage2,
            R(0x8004, [0xc2c7, 0xd2cb, 0xdacb, 0x5ac7], -1, 1),
            R(0x8002, [0xd2ca, 0xdaca]));
        Add(lists, PaneTransition,
            R(0x8003, [0xc2c7, 0x02cc, 0x0acc]));
        Add(lists, ShiftedPane1,
            R(0x0001, [0xc2c7], -3, 0),
            R(0x8004, [0x82cd, 0x86c9, 0x8ec9, 0x8acd], -2, 1),
            R(0x8002, [0x86c8, 0x8ec8]));
        Add(lists, ShiftedPane2,
            R(0x0001, [0xc2c7], -3, 1),
            R(0x8003, [0x86cb, 0x8ecb, 0x8acd], -2, 1),
            R(0x8002, [0x86ca, 0x8eca]));
        Add(lists, ShiftedPane3,
            R(0x0001, [0xc2c7], -3, 1),
            R(0x8002, [0x06cc, 0x0ecc]));
        Add(lists, Shatter1,
            R(0x8004, [0xc2ce, 0x02cf, 0x0acf, 0x5ace], -3, 0),
            R(0x8004, [0x86ce, 0x06cf, 0x0ecf, 0x8ece], -2, 1),
            R(0x8002, [0xd6d0, 0xded0], -1, 1),
            R(0x8002, [0xd2d0, 0xdad0]));
        Add(lists, Shatter2,
            R(0x8004, [0xc2ce, 0x00ff, 0x00ff, 0x5ace], -3, 0),
            R(0x8004, [0x86ce, 0x00ff, 0x00ff, 0x8ece], -2, 0),
            R(0x8004, [0x52c2, 0xd2c3, 0xdac3, 0xd2c4], -1, 0),
            R(0x8004, [0x56c2, 0xd6c3, 0xdec3, 0xd6c4]));
        Add(lists, Shatter3,
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff], -3, 0),
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff], -2, 0),
            R(0x8004, [0x02d2, 0x02d3, 0x0ad3, 0x02d4], -1, 0),
            R(0x8004, [0x06d2, 0x06d3, 0x0ed3, 0x06d4]));
        Add(lists, Cleared,
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff], -3, 0),
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff], -2, 0),
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff], -1, 0),
            R(0x8004, [0x00ff, 0x00ff, 0x00ff, 0x00ff]));
        return lists;
    }

    private static RoomPlmShotBlockDrawDefinitions.Run R(
        ushort directionAndCount, ushort[] words, sbyte nextX = 0, sbyte nextY = 0)
    {
        if ((directionAndCount & 0x7fff) != words.Length)
            throw new InvalidDataException("Mother Brain glass draw count differs from its payload.");
        return new(directionAndCount, words, nextX, nextY);
    }

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer, params RoomPlmShotBlockDrawDefinitions.Run[] runs)
    {
        if (runs.Length is < 1 or > 4 ||
            runs[^1].NextX != 0 || runs[^1].NextY != 0)
            throw new InvalidDataException($"Mother Brain glass draw ${pointer:X4} has invalid runs.");
        if (!lists.TryAdd(pointer, new(pointer, runs)))
            throw new InvalidDataException($"Duplicate Mother Brain glass draw ${pointer:X4}.");
    }
}
