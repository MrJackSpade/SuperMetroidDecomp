using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The terrain change of each of Kraid's room mutations.</summary>
internal static class KraidRoomMutationDefinitions
{
    /// <summary>Updates each crumble image, background or spike image shows.</summary>
    internal const int CrumbleFrameDuration = 3;

    /// <summary>The passes of the spike crumble, each crumbling two columns.</summary>
    internal const int SpikePassCount = 11;

    /// <summary>One block to the right: the spike crumble's step to its next column.</summary>
    private static readonly Offset16<RoomBlocks> NextColumn = new(1, 0);

    /// <summary>One stage of a crumbling block or the picture it leaves, shown for the crumble's frame duration.</summary>
    private static TerrainChangeFrame Shown(RoomObjectDraw draw) => new(draw, CrumbleFrameDuration);

    /// <summary>The picture a crumbled block leaves, shown for the crumble's frame duration.</summary>
    private static TerrainChangeFrame Shown(KraidRoomDraw draw) => Shown(KraidRoomObjectDraws.Family.Of(draw));

    /// <summary>The rock, pieces and dust of a crumbling block and then <paramref name="final"/>, the rock drawn after <paramref name="moveBefore"/>.</summary>
    private static TerrainChangeFrame[] CrumbleInto(KraidRoomDraw final, Offset16<RoomBlocks> moveBefore) =>
    [
        Shown(KraidRoomObjectDraws.Crumble(BlockBreakupStage.Rock)) with { MoveBefore = moveBefore },
        Shown(KraidRoomObjectDraws.Crumble(BlockBreakupStage.Pieces)),
        Shown(KraidRoomObjectDraws.Crumble(BlockBreakupStage.Dust)),
        Shown(final),
    ];

    /// <summary>A spike pass after the first: its two columns, each one block right of the column before.</summary>
    private static readonly TerrainChangeFrame[] LaterSpikePass =
        [.. CrumbleInto(KraidRoomDraw.SpikeBackgroundLeftColumn, NextColumn), .. CrumbleInto(KraidRoomDraw.SpikeBackgroundRightColumn, NextColumn)];

    /// <summary>
    /// The change each mutation makes: a ceiling block crumbles into its background; a platform variant only sets its
    /// block to its background; the clears draw once; the spike crumble crumbles its pairs of columns (<see cref="SpikePassCount"/>) left to right.
    /// </summary>
    internal static readonly IdentityTable<KraidRoomMutation, TerrainChange> Changes = new(new Dictionary<KraidRoomMutation, TerrainChange>
        {
            [KraidRoomMutation.CrumbleCeilingAtLeftEdge] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackgroundLeftEdge, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.CrumblePlatformInOddColumn] = new(1, [Shown(KraidRoomDraw.CeilingBackgroundOddColumn)]),
            [KraidRoomMutation.CrumbleCeilingInOddColumn] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackgroundOddColumn, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.CrumblePlatformInEvenColumn] = new(1, [Shown(KraidRoomDraw.CeilingBackgroundEvenColumn)]),
            [KraidRoomMutation.CrumbleCeilingInEvenColumn] = new(1, CrumbleInto(KraidRoomDraw.CeilingBackgroundEvenColumn, Offset16<RoomBlocks>.Zero)),
            [KraidRoomMutation.ClearCeiling] = TerrainChange.DrawnOnce(KraidRoomObjectDraws.Family.Of(KraidRoomDraw.ClearCeiling)),
            [KraidRoomMutation.ClearSpikes] = TerrainChange.DrawnOnce(KraidRoomObjectDraws.Family.Of(KraidRoomDraw.ClearSpikes)),
            [KraidRoomMutation.CrumbleSpikes] = new(1,
            [
                .. CrumbleInto(KraidRoomDraw.SpikeBackgroundLeftColumn, Offset16<RoomBlocks>.Zero),
                .. CrumbleInto(KraidRoomDraw.SpikeBackgroundRightColumn, NextColumn),
                .. Enumerable.Repeat(LaterSpikePass, SpikePassCount - 1).SelectMany(pass => pass),
            ]),
        });
}
