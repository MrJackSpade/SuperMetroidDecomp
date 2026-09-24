namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Complete native restoration words for linked Samus-contact crumble blocks.
/// The type-B parent and type-5/type-D children are physical room mutations;
/// artwork replacement may only alter their visual block composition.
/// </summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitions
{
    /// <summary>Horizontal parent and child restoration at $84:A4A1.</summary>
    internal const ushort Horizontal = 0xa4a1;
    /// <summary>Vertical parent and child restoration at $84:A4A9.</summary>
    internal const ushort Vertical = 0xa4a9;
    /// <summary>Four-block parent and children restoration at $84:A4B1.</summary>
    internal const ushort Square = 0xa4b1;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    private static IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Build() =>
        new Dictionary<ushort, RoomPlmShotBlockDrawDefinitions.DrawList>
        {
            [Horizontal] = new(Horizontal,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(2, new ushort[] { 0xb0bc, 0x50bc }, 0, 0),
                }),
            [Vertical] = new(Vertical,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(0x8002, new ushort[] { 0xb0bc, 0xd0bc }, 0, 0),
                }),
            [Square] = new(Square,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(2, new ushort[] { 0xb0bc, 0x50bc }, 0, 1),
                    new(2, new ushort[] { 0xd0bc, 0xd0bc }, 0, 0),
                }),
        };
}
