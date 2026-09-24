namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete bank-$84 draw lists selected by station animation programs. The native
/// level words are physical room mutations; replaceable artwork remains separate.
/// </summary>
internal static class RoomPlmStationDrawDefinitions
{
    /// <summary>First map-station frame draw list at $84:9F25.</summary>
    private const ushort MapFirst = 0x9f25;
    /// <summary>First energy-station frame draw list at $84:9F6D.</summary>
    private const ushort EnergyFirst = 0x9f6d;
    /// <summary>First missile-station frame draw list at $84:9F91.</summary>
    private const ushort MissileFirst = 0x9f91;
    /// <summary>Save-pod idle draw list at $84:9A3F.</summary>
    private const ushort SaveIdle = 0x9a3f;
    /// <summary>First active save-pod draw list at $84:9A9F.</summary>
    private const ushort SaveActive = 0x9a9f;
    /// <summary>Second active save-pod draw list at $84:9A6F.</summary>
    private const ushort SaveAlternate = 0x9a6f;
    /// <summary>Retracted right-side map access draw at $84:9F49.</summary>
    private const ushort MapRightRetracted = 0x9f49;
    /// <summary>Extended right-side map access draw at $84:9F55.</summary>
    private const ushort MapRightExtended = 0x9f55;
    /// <summary>Retracted left-side map access draw at $84:9F5B.</summary>
    private const ushort MapLeftRetracted = 0x9f5b;
    /// <summary>Extended left-side map access draw at $84:9F67.</summary>
    private const ushort MapLeftExtended = 0x9f67;
    /// <summary>Retracted right-side resource access draw at $84:9FB5.</summary>
    private const ushort ResourceRightRetracted = 0x9fb5;
    /// <summary>Extended right-side resource access draw at $84:9FBB.</summary>
    private const ushort ResourceRightExtended = 0x9fbb;
    /// <summary>Retracted left-side resource access draw at $84:9FC1.</summary>
    private const ushort ResourceLeftRetracted = 0x9fc1;
    /// <summary>Extended left-side resource access draw at $84:9FC7.</summary>
    private const ushort ResourceLeftExtended = 0x9fc7;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>();
        for (int frame = 0; frame < 3; frame++)
        {
            Add(lists, checked((ushort)(MapFirst + frame * 12)),
                new(1, new ushort[] { checked((ushort)(0x810c + frame * 0x20)) }, -1, 0),
                new(1, new ushort[] { checked((ushort)(0x810b + frame * 0x20)) }, 0, 0));
            Add(lists, checked((ushort)(EnergyFirst + frame * 12)),
                new(1, new ushort[] { checked((ushort)(0x80c4 + frame)) }, 0, -1),
                new(1, new ushort[] { checked((ushort)(0x10a4 + frame)) }, 0, 0));
            Add(lists, checked((ushort)(MissileFirst + frame * 12)),
                new(1, new ushort[] { checked((ushort)(0x80c7 + frame)) }, 0, -1),
                new(1, new ushort[] { checked((ushort)(0x10a7 + frame)) }, 0, 0));
        }

        AddSavePod(lists, SaveIdle, 0xb859, 0x8c59, 0x005b, 0x045b, 0x8059, 0x8459);
        AddSavePod(lists, SaveActive, 0x885a, 0x8c5a, 0x005c, 0x045c, 0x805a, 0x845a);
        AddSavePod(lists, SaveAlternate, 0x8859, 0x8c59, 0x005b, 0x045b, 0x8059, 0x8459);
        Add(lists, MapRightRetracted,
            new(1, new ushort[] { 0x8128 }, -3, 0),
            new(1, new ushort[] { 0x8528 }, 0, 0));
        Add(lists, MapRightExtended,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x8129 }, 0, 0));
        Add(lists, MapLeftRetracted,
            new(1, new ushort[] { 0x8528 }, 3, 0),
            new(1, new ushort[] { 0x8128 }, 0, 0));
        Add(lists, MapLeftExtended,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x8529 }, 0, 0));
        Add(lists, ResourceRightRetracted,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0xb4c3 }, 0, 0));
        Add(lists, ResourceRightExtended,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x84c1 }, 0, 0));
        Add(lists, ResourceLeftRetracted,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0xb0c3 }, 0, 0));
        Add(lists, ResourceLeftExtended,
            new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x80c1 }, 0, 0));
        return lists;
    }

    private static void AddSavePod(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer,
        ushort floorLeft,
        ushort floorRight,
        ushort shaftLeft,
        ushort shaftRight,
        ushort capLeft,
        ushort capRight) =>
        Add(lists, pointer,
            new(2, new ushort[] { floorLeft, floorRight }, 0, -1),
            new(2, new ushort[] { shaftLeft, shaftRight }, 0, -2),
            new(2, new ushort[] { shaftLeft, shaftRight }, 0, -3),
            new(2, new ushort[] { shaftLeft, shaftRight }, 0, -4),
            new(2, new ushort[] { shaftLeft, shaftRight }, 0, -5),
            new(2, new ushort[] { capLeft, capRight }, 0, 0));

    private static void Add(
        Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList> lists,
        ushort pointer,
        params RoomPlmShotBlockDrawDefinitions.Run[] runs)
    {
        if (!lists.TryAdd(pointer, new(pointer, runs)))
            throw new InvalidDataException($"Duplicate compiled station draw list ${pointer:X4}.");
    }
}
