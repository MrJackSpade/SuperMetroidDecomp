using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Where Botwoon's wall object is spawned, and the terrain change of each wall mutation.</summary>
internal static partial class BotwoonWallMutationDefinitions
{
    /// <summary>The room block both wall mutations are spawned at: the wall's top block.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(15, 4);

    /// <summary>
    /// The scroll cells the wall opens, aligned to their tops, as it starts to crumble or when an already-defeated room
    /// clears it: the first two screens of the room's top row, in the order they open.
    /// </summary>
    internal static IReadOnlyList<Position16<RoomScreens>> OpenedScrollCells { get; } = [new(0, 0), new(1, 0)];

    /// <summary>Rows the crumble crumbles, top to bottom.</summary>
    internal const int CrumbleRows = 9;

    /// <summary>Updates each crumble image shows.</summary>
    internal const int CrumbleFrameDuration = 4;

    /// <summary>The sound each crumbling row starts with.</summary>
    internal static SoundEffectId CrumbleSound { get; } = EnemySound.BlockCrumble;

    /// <summary>One block down: the crumble's step to its next row.</summary>
    private static readonly Offset16<RoomBlocks> NextRow = new(0, 1);

    /// <summary>
    /// One row's crumble: the four images of a single block's breakup, shared with the respawning shot block, the first
    /// drawn after <paramref name="moveBefore"/> with the crumble sound.
    /// </summary>
    private static TerrainChangeFrame[] CrumbleRow(Offset16<RoomBlocks> moveBefore) =>
    [
        new(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Rock), CrumbleFrameDuration, moveBefore, CrumbleSound),
        new(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Pieces), CrumbleFrameDuration),
        new(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Dust), CrumbleFrameDuration),
        new(ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Gone), CrumbleFrameDuration),
    ];

    /// <summary>How long the crumble's setup holds it before the wall begins to fall, in updates (#2182).</summary>
    internal const int CrumbleSetupHold = 64;

    /// <summary>
    /// The crumble: its setup holds it for <see cref="CrumbleSetupHold"/> updates before the wall begins to fall, then it
    /// crumbles the wall one row at a time; <see cref="BotwoonWallCrumbleObject"/> opens the first two scroll cells as the
    /// first row starts.
    /// </summary>
    internal static TerrainChange Crumble { get; } = new(CrumbleSetupHold,
    [
        .. CrumbleRow(Offset16<RoomBlocks>.Zero),
        .. Enumerable.Repeat(NextRow, CrumbleRows - 1).SelectMany(CrumbleRow),
    ]);

    /// <summary>The clear of an already-defeated room: the cleared wall blocks, drawn on the next update.</summary>
    internal static TerrainChange Clear { get; } = TerrainChange.DrawnOnce(BotwoonWallObjectDrawDefinitions.Clear);
}
