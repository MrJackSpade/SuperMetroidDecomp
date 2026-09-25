namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical block draws selected by Bomb Torizo's resident hand PLM. These
/// level words and run offsets are cartridge mechanics, not editable art.
/// </summary>
internal static class BombTorizoHandPlmDrawDefinitions
{
    /// <summary>Intact Chozo hand and surrounding stone at $84:9877.</summary>
    internal const ushort Intact = 0x9877;
    /// <summary>Final five-run cleared-hand area at $84:989D.</summary>
    internal const ushort Cleared = 0x989d;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer) => pointer switch
    {
        Intact => "intact",
        Cleared => "cleared",
        _ => throw new InvalidDataException(
            $"Bomb Torizo hand draw ${pointer:X4} has no visual ID."),
    };

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
        RoomPlmShotBlockDrawDefinitions.DrawList> Build() =>
        new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>
        {
            [Intact] = new(Intact,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(0x0002, new ushort[] { 0x8065, 0x8066 }, -1, 0),
                new(0x0001, new ushort[] { 0x8064 }, 0, -1),
                new(0x0002, new ushort[] { 0x8045, 0x8046 }, -1, 1),
                new(0x0003, new ushort[] { 0x8047, 0x8048, 0x8049 }, 0, 0),
            }),
            [Cleared] = new(Cleared,
            new RoomPlmShotBlockDrawDefinitions.Run[]
            {
                new(0x0002, new ushort[] { 0x00ff, 0x00ff }, -2, 0),
                new(0x0002, new ushort[] { 0x00ff, 0x00ff }, -2, 1),
                new(0x0004, new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff }, -2, -2),
                new(0x0004, new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff }, -2, -1),
                new(0x0004, new ushort[] { 0x00ff, 0x00ff, 0x00ff, 0x00ff }, 0, 0),
            }),
        };
}
