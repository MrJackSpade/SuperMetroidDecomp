namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Eight native floor/ceiling plant block layouts at $84:9E0D..9F24. Their
/// complete level words and signed three-run geometry are immutable physical
/// data; the visible word portion is supplied by replaceable assets.
/// </summary>
internal static class SamusEaterPlmDrawDefinitions
{
    /// <summary>Native floor-idle draw list at $84:9E0D.</summary>
    internal const ushort FloorIdle = 0x9e0d;
    /// <summary>Native floor-chew-1 draw list at $84:9E45.</summary>
    internal const ushort FloorChew1 = 0x9e45;
    /// <summary>Native floor-chew-2 draw list at $84:9E61.</summary>
    internal const ushort FloorChew2 = 0x9e61;
    /// <summary>Native floor-chew-3 draw list at $84:9E7D.</summary>
    internal const ushort FloorChew3 = 0x9e7d;
    /// <summary>Native ceiling-idle draw list at $84:9E99.</summary>
    internal const ushort CeilingIdle = 0x9e99;
    /// <summary>Native ceiling-chew-1 draw list at $84:9ED1.</summary>
    internal const ushort CeilingChew1 = 0x9ed1;
    /// <summary>Native ceiling-chew-2 draw list at $84:9EED.</summary>
    internal const ushort CeilingChew2 = 0x9eed;
    /// <summary>Native ceiling-chew-3 draw list at $84:9F09.</summary>
    internal const ushort CeilingChew3 = 0x9f09;

    private static readonly RoomPlmShotBlockDrawDefinitions.DrawList[] Lists =
    [
        new(FloorIdle, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x35a1, 0x85a0 }, -2, 0),
            new(0x0002, new ushort[] { 0x81a0, 0x51a1 }, -2, -1),
            new(0x0004, new ushort[] { 0x2180, 0x2181, 0x2581, 0x2580 }, 0, 0),
        }),
        new(FloorChew1, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x05a3, 0x85a2 }, -2, 0),
            new(0x0002, new ushort[] { 0x81a2, 0x01a3 }, -2, -1),
            new(0x0004, new ushort[] { 0x2182, 0x2183, 0x2583, 0x2582 }, 0, 0),
        }),
        new(FloorChew2, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x05a5, 0x85a4 }, -2, 0),
            new(0x0002, new ushort[] { 0x81a4, 0x01a5 }, -2, -1),
            new(0x0004, new ushort[] { 0x2184, 0x2185, 0x2585, 0x2584 }, 0, 0),
        }),
        new(FloorChew3, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x05a7, 0x85a6 }, -2, 0),
            new(0x0002, new ushort[] { 0x81a6, 0x01a7 }, -2, -1),
            new(0x0004, new ushort[] { 0x2186, 0x2187, 0x2587, 0x2586 }, 0, 0),
        }),
        new(CeilingIdle, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x3da1, 0x8da0 }, -2, 0),
            new(0x0002, new ushort[] { 0x89a0, 0x59a1 }, -2, 1),
            new(0x0004, new ushort[] { 0x2980, 0x2981, 0x2d81, 0x2d80 }, 0, 0),
        }),
        new(CeilingChew1, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x0da3, 0x8da2 }, -2, 0),
            new(0x0002, new ushort[] { 0x89a2, 0x09a3 }, -2, 1),
            new(0x0004, new ushort[] { 0x2982, 0x2983, 0x2d83, 0x2d82 }, 0, 0),
        }),
        new(CeilingChew2, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x0da5, 0x8da4 }, -2, 0),
            new(0x0002, new ushort[] { 0x89a4, 0x09a5 }, -2, 1),
            new(0x0004, new ushort[] { 0x2984, 0x2985, 0x2d85, 0x2d84 }, 0, 0),
        }),
        new(CeilingChew3, new RoomPlmShotBlockDrawDefinitions.Run[] {
            new(0x0002, new ushort[] { 0x0da7, 0x8da6 }, -2, 0),
            new(0x0002, new ushort[] { 0x89a6, 0x09a7 }, -2, 1),
            new(0x0004, new ushort[] { 0x2986, 0x2987, 0x2d87, 0x2d86 }, 0, 0),
        }),
    ];

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists;

    internal static string VisualId(ushort pointer) => pointer switch
    {
        FloorIdle => "floor-idle",
        FloorChew1 => "floor-chew-1",
        FloorChew2 => "floor-chew-2",
        FloorChew3 => "floor-chew-3",
        CeilingIdle => "ceiling-idle",
        CeilingChew1 => "ceiling-chew-1",
        CeilingChew2 => "ceiling-chew-2",
        CeilingChew3 => "ceiling-chew-3",
        _ => throw new InvalidDataException($"Samus Eater draw ${pointer:X4} has no visual ID."),
    };

    internal static bool TryGetByVisualId(string id,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists)
        {
            if (!string.Equals(VisualId(candidate.Pointer), id,
                    StringComparison.Ordinal))
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list)
    {
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList candidate in Lists)
        {
            if (candidate.Pointer != pointer)
                continue;
            list = candidate;
            return true;
        }
        list = default;
        return false;
    }
}
