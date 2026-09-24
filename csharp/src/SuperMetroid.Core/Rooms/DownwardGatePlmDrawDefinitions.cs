namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete bank-$84 physical level-word draw lists for the downward-gate column
/// and its four left/right shot-trigger pairs. Gate actor sprites are bank-$86 art.
/// </summary>
internal static class DownwardGatePlmDrawDefinitions
{
    /// <summary>First five-block closed-gate column at $84:A517.</summary>
    private const ushort ResidentFirst = 0xa517;
    /// <summary>First left-side gate-trigger draw at $84:A5D7.</summary>
    private const ushort TriggerLeftFirst = 0xa5d7;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();
    private static readonly IReadOnlyDictionary<ushort, string> VisualIds = BuildVisualIds();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer) =>
        VisualIds.TryGetValue(pointer, out string? id)
            ? id
            : throw new InvalidDataException($"Downward gate draw list ${pointer:X4} has no visual ID.");

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach ((ushort pointer, string candidate) in VisualIds)
            if (string.Equals(id, candidate, StringComparison.Ordinal))
                return Lists.TryGetValue(pointer, out list);
        list = default;
        return false;
    }

    private static IReadOnlyDictionary<ushort, string> BuildVisualIds()
    {
        var ids = new Dictionary<ushort, string>();
        for (int frame = 0; frame < 6; frame++)
            ids.Add(checked((ushort)(ResidentFirst + frame * 14)), $"column-frame-{frame}");
        string[] colors = ["blue", "green", "red", "yellow"];
        for (int color = 0; color < colors.Length; color++)
        {
            ushort left = checked((ushort)(TriggerLeftFirst + color * 20));
            ids.Add(left, $"{colors[color]}-left-trigger");
            ids.Add(checked((ushort)(left + 12)), $"{colors[color]}-right-trigger");
        }
        if (ids.Count != Lists.Count)
            throw new InvalidDataException("Downward gate visual IDs do not cover all draw lists.");
        return ids;
    }

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>();
        for (int frame = 0; frame < 6; frame++)
        {
            ushort[] words = new ushort[5];
            words[0] = frame is 0 or 5 ? (ushort)0xc0d6 : (ushort)0xc0d7;
            for (int row = 1; row < words.Length; row++)
                words[row] = row <= frame ? (ushort)0xc0ff : (ushort)0x00ff;
            ushort pointer = checked((ushort)(ResidentFirst + frame * 14));
            Add(lists, pointer,
                new RoomPlmShotBlockDrawDefinitions.Run(0x8005, words, 0, 0));
        }

        for (int color = 0; color < 4; color++)
        {
            ushort left = checked((ushort)(TriggerLeftFirst + color * 20));
            ushort right = checked((ushort)(left + 12));
            ushort sideWord = checked((ushort)(0xc0db - color));
            Add(lists, left,
                new(1, new ushort[] { 0x80d6 }, -1, 0),
                new(1, new ushort[] { sideWord }, 0, 0));
            Add(lists, right,
                new RoomPlmShotBlockDrawDefinitions.Run(2,
                    new ushort[] { 0x80d6, checked((ushort)(sideWord + 0x0400)) }, 0, 0));
        }

        return lists;
    }

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer,
        params RoomPlmShotBlockDrawDefinitions.Run[] runs)
    {
        if (!lists.TryAdd(pointer, new(pointer, runs)))
            throw new InvalidDataException($"Duplicate compiled gate draw list ${pointer:X4}.");
    }
}
