using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The room object and terrain change of each of Kraid's room mutations.</summary>
internal static class KraidRoomMutationDefinitions
{
    /// <summary>The room object each mutation is.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<KraidRoomMutation, RoomObjectHeaderId>
        {
            [KraidRoomMutation.CrumbleCeilingIntoBackground1] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground1,
            [KraidRoomMutation.CrumblePlatformVariant1] = RoomObjectHeaderId.CrumbleKraidPlatformVariant1,
            [KraidRoomMutation.CrumbleCeilingIntoBackground2] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground2,
            [KraidRoomMutation.CrumblePlatformVariant2] = RoomObjectHeaderId.CrumbleKraidPlatformVariant2,
            [KraidRoomMutation.CrumbleCeilingIntoBackground3] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground3,
            [KraidRoomMutation.ClearCeiling] = RoomObjectHeaderId.ClearKraidCeiling,
            [KraidRoomMutation.ClearSpikes] = RoomObjectHeaderId.ClearKraidSpikes,
            [KraidRoomMutation.CrumbleSpikes] = RoomObjectHeaderId.CrumbleKraidSpikes,
        };

    /// <summary>Updates each crumble image, background or spike image shows.</summary>
    internal const int CrumbleFrameDuration = 3;

    /// <summary>The passes of the spike crumble, each crumbling two columns.</summary>
    internal const int SpikePassCount = 11;

    /// <summary>One block to the right: the spike crumble's step to its next column.</summary>
    private static readonly Offset16<RoomBlocks> NextColumn = new(1, 0);

    /// <summary>One crumble image or final block, shown for the crumble's frame duration.</summary>
    private static TerrainChangeFrame Shown(KraidRoomDraw draw) => new(RoomObjectDraw.Of(draw), CrumbleFrameDuration);

    /// <summary>The three crumble images and then <paramref name="final"/>, the first image drawn after <paramref name="moveBefore"/>.</summary>
    private static TerrainChangeFrame[] CrumbleInto(KraidRoomDraw final, Offset16<RoomBlocks> moveBefore) =>
    [
        Shown(KraidRoomDraw.CrumbleFirst) with { MoveBefore = moveBefore },
        Shown(KraidRoomDraw.CrumbleSecond),
        Shown(KraidRoomDraw.CrumbleThird),
        Shown(final),
    ];

    /// <summary>A spike pass after the first: its two columns, each one block right of the column before.</summary>
    private static readonly TerrainChangeFrame[] LaterSpikePass =
        [.. CrumbleInto(KraidRoomDraw.SpikeFirst, NextColumn), .. CrumbleInto(KraidRoomDraw.SpikeSecond, NextColumn)];

    /// <summary>
    /// The change each mutation makes: a ceiling block crumbles into its background; a platform variant only sets its
    /// block to its background; the clears draw once; the spike crumble crumbles eleven pairs of columns left to right.
    /// </summary>
    internal static readonly IReadOnlyDictionary<KraidRoomMutation, TerrainChange> Changes =
        new Dictionary<KraidRoomMutation, TerrainChange>
        {
            [KraidRoomMutation.CrumbleCeilingIntoBackground1] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackground1, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.CrumblePlatformVariant1] = new(1, [Shown(KraidRoomDraw.CeilingBackground2)]),
            [KraidRoomMutation.CrumbleCeilingIntoBackground2] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackground2, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.CrumblePlatformVariant2] = new(1, [Shown(KraidRoomDraw.CeilingBackground3)]),
            [KraidRoomMutation.CrumbleCeilingIntoBackground3] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackground3, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.ClearCeiling] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(KraidRoomDraw.ClearCeiling)),
            [KraidRoomMutation.ClearSpikes] = TerrainChange.DrawnOnce(RoomObjectDraw.Of(KraidRoomDraw.ClearSpikes)),
            [KraidRoomMutation.CrumbleSpikes] = new(1,
            [
                .. CrumbleInto(KraidRoomDraw.SpikeFirst, Offset16<RoomBlocks>.Zero),
                .. CrumbleInto(KraidRoomDraw.SpikeSecond, NextColumn),
                .. Enumerable.Repeat(LaterSpikePass, SpikePassCount - 1).SelectMany(pass => pass),
            ]),
        };
}
