namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Five bounded physical block-draw layouts for Crocomire's bridge and
/// three-column arena wall at $84:9B5B..9BF6.
/// </summary>
internal static class CrocomireArenaPlmDrawDefinitions
{
    /// <summary><c>$84:9B5B</c>: ten cleared bridge blocks.</summary>
    internal const ushort ClearBridge = 0x9b5b;
    /// <summary><c>$84:9B73</c>: one crumbling bridge block.</summary>
    internal const ushort CrumbleBridgeBlock = 0x9b73;
    /// <summary><c>$84:9B79</c>: one cleared bridge block.</summary>
    internal const ushort ClearBridgeBlock = 0x9b79;
    /// <summary><c>$84:9B7F</c>: three columns of clear wall blocks.</summary>
    internal const ushort ClearInvisibleWall = 0x9b7f;
    /// <summary><c>$84:9BBB</c>: three columns of solid wall blocks.</summary>
    internal const ushort CreateInvisibleWall = 0x9bbb;
    /// <summary><c>$84:9BF7</c>: first byte of the following eye-door draw region.</summary>
    internal const ushort EndExclusive = 0x9bf7;

    private static readonly Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw) =>
        Lists.TryGetValue(pointer, out draw);

    internal static string VisualId(ushort pointer) => pointer switch
    {
        ClearBridge => "clear-bridge",
        CrumbleBridgeBlock => "crumble-bridge-block",
        ClearBridgeBlock => "clear-bridge-block",
        ClearInvisibleWall => "clear-invisible-wall",
        CreateInvisibleWall => "create-invisible-wall",
        _ => throw new InvalidDataException(
            $"Crocomire arena draw ${pointer:X4} has no visual ID."),
    };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList draw)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists.Values)
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

    private static Dictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var result = new Dictionary<ushort,
            RoomPlmShotBlockDrawDefinitions.DrawList>();
        Add(ClearBridge, new RoomPlmShotBlockDrawDefinitions.Run(10,
            Enumerable.Repeat((ushort)0x0080, 10).ToArray(), 0, 0));
        Add(CrumbleBridgeBlock, new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x810b }, 0, 0));
        Add(ClearBridgeBlock, new RoomPlmShotBlockDrawDefinitions.Run(1, new ushort[] { 0x0080 }, 0, 0));
        Add(ClearInvisibleWall,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x0080, 0x0107, 0x0127, 0x0107, 0x0127, 0x0147, 0x0080, 0x0080 }, 1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x0080, 0x0108, 0x0128, 0x0108, 0x0128, 0x0148, 0x0080, 0x0080 }, 2, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x0080, 0x0109, 0x0129, 0x0109, 0x0129, 0x0149, 0x0080, 0x0080 }, 0, 0));
        Add(CreateInvisibleWall,
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x8080, 0x8107, 0x8127, 0x8107, 0x8127, 0x8147, 0x8080, 0x8080 }, 1, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x8080, 0x8108, 0x8128, 0x8108, 0x8128, 0x8148, 0x8080, 0x8080 }, 2, 0),
            new RoomPlmShotBlockDrawDefinitions.Run(0x8008, new ushort[]
                { 0x8080, 0x8109, 0x8129, 0x8109, 0x8129, 0x8149, 0x8080, 0x8080 }, 0, 0));
        return result;

        void Add(ushort pointer,
            params RoomPlmShotBlockDrawDefinitions.Run[] runs)
        {
            if (!result.TryAdd(pointer,
                    new RoomPlmShotBlockDrawDefinitions.DrawList(pointer, runs)))
                throw new InvalidDataException(
                    $"Duplicate Crocomire draw ${pointer:X4}.");
        }
    }
}
