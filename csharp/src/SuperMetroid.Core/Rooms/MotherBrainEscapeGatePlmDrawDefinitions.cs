namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The three physical four-block gate images chosen by the Mother Brain
/// escape-gate program. Level words remain immutable collision definitions;
/// visual block references can be separated for authoring independently.
/// </summary>
internal static class MotherBrainEscapeGatePlmDrawDefinitions
{
    /// <summary>Fully open vertical gate draw at $84:9473.</summary>
    internal const ushort Open = 0x9473;
    /// <summary>Half-closed vertical gate draw at $84:947F.</summary>
    internal const ushort HalfClosed = 0x947f;
    /// <summary>Fully closed vertical gate draw at $84:948B.</summary>
    internal const ushort Closed = 0x948b;

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
            [Open] = Draw(Open, [0x80ff, 0x80ff, 0x80ff, 0x80ff]),
            [HalfClosed] = Draw(HalfClosed, [0x830f, 0x80ff, 0x80ff, 0x830f]),
            [Closed] = Draw(Closed, [0x830f, 0x8ae8, 0x82e8, 0x830f]),
        };

    private static RoomPlmShotBlockDrawDefinitions.DrawList Draw(
        ushort pointer,
        ushort[] words) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(0x8004, words, 0, 0),
        });
}
