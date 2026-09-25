namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Physical bank-$84 draw layouts used by the translated Chozo statue PLMs.
/// The crumble plug shares the already-compiled shot-block breakup frames.
/// Complete level words, run geometry and signed offsets are cartridge mechanics;
/// installed artwork may replace only their visual block references.
/// </summary>
internal static class ChozoStatuePlmDrawDefinitions
{
    /// <summary>Blank Lower Norfair hand after the acid-lowered event, $84:A2B5.</summary>
    internal const ushort LowerNorfairClearedHand = 0xa2b5;
    /// <summary>Wrecked Ship statue's clear-slope-access draw, $84:9CC5.</summary>
    internal const ushort ClearSlopeAccess = 0x9cc5;
    /// <summary>Wrecked Ship statue's block-slope-access draw, $84:9D0F.</summary>
    internal const ushort BlockSlopeAccess = 0x9d0f;

    private static readonly IReadOnlyDictionary<ushort,
        RoomPlmShotBlockDrawDefinitions.DrawList> Lists = Build();

    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All => Lists.Values;

    internal static bool TryGet(ushort pointer,
        out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        Lists.TryGetValue(pointer, out list);

    internal static string VisualId(ushort pointer) => pointer switch
    {
        LowerNorfairClearedHand => "lower-norfair-cleared-hand",
        ClearSlopeAccess => "wrecked-ship-clear-slope-access",
        BlockSlopeAccess => "wrecked-ship-block-slope-access",
        _ => throw new InvalidDataException($"Chozo statue draw ${pointer:X4} has no visual ID."),
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
            [LowerNorfairClearedHand] = new(LowerNorfairClearedHand,
                new RoomPlmShotBlockDrawDefinitions.Run[]
                {
                    new(1, new ushort[] { 0x00ff }, 0, 0),
                }),
            [ClearSlopeAccess] = SlopeAccess(ClearSlopeAccess,
                0x012b, 0x112b, 0x0111, 0x019b, 0x0129, 0x1129,
                0x01bb, 0x0129, 0x01bb, 0x11bb),
            [BlockSlopeAccess] = SlopeAccess(BlockSlopeAccess,
                0xa12b, 0x812b, 0x8111, 0x819b, 0x8129, 0x8129,
                0x81bb, 0x8129, 0x81bb, 0x81bb),
        };

    private static RoomPlmShotBlockDrawDefinitions.DrawList SlopeAccess(
        ushort pointer, ushort longRow, ushort longRowEnd,
        ushort secondRow, ushort secondRowTransition, ushort secondRowEnd,
        ushort secondRowLast, ushort thirdRowStart, ushort thirdRowEnd,
        ushort fourthRow, ushort fifthRow) => new(pointer,
        new RoomPlmShotBlockDrawDefinitions.Run[]
        {
            new(14, Enumerable.Repeat(longRow, 13).Append(longRowEnd).ToArray(), 0, 5),
            new(9, Enumerable.Repeat(secondRow, 5)
                .Concat(new ushort[] { secondRowTransition, secondRowEnd,
                    secondRowEnd, secondRowLast }).ToArray(), 5, 6),
            new(2, new ushort[] { thirdRowStart, thirdRowEnd }, 5, 7),
            new(1, new ushort[] { fourthRow }, 5, 8),
            new(1, new ushort[] { fifthRow }, 0, 0),
        });
}
