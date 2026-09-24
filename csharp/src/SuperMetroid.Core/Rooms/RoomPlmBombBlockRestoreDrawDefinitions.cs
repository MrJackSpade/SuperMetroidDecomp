namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete native level-word restoration lists for linked bomb blocks. Their
/// shape shares the compiled PLM row/column representation with shot blocks,
/// while the distinct type-F parent and type-5/type-D children stay immutable.
/// </summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitions
{
    /// <summary>Horizontal parent and child restoration at $84:A4C7.</summary>
    internal const ushort Horizontal = 0xa4c7;
    /// <summary>Vertical parent and child restoration at $84:A4CF.</summary>
    internal const ushort Vertical = 0xa4cf;
    /// <summary>Four-block parent and child restoration at $84:A4D7.</summary>
    internal const ushort Square = 0xa4d7;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build()
    {
        var lists = new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>
        {
            [Horizontal] = new(Horizontal,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(2, new ushort[] { 0xf058, 0x5058 }, 0, 0),
                }),
            [Vertical] = new(Vertical,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(0x8002, new ushort[] { 0xf058, 0xd058 }, 0, 0),
                }),
            [Square] = new(Square,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(2, new ushort[] { 0xf058, 0x5058 }, 0, 1),
                    new(2, new ushort[] { 0xd058, 0xd058 }, 0, 0),
                }),
        };
        return lists;
    }
}
