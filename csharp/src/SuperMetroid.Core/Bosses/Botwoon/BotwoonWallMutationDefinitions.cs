using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Where Botwoon's wall object is spawned, and the terrain change of each wall mutation.</summary>
internal static class BotwoonWallMutationDefinitions
{
    /// <summary>The room block both wall mutations are spawned at: the wall's top block.</summary>
    internal static readonly Position16<RoomBlocks> Block = new(15, 4);

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
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 0), CrumbleFrameDuration, moveBefore, CrumbleSound),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 1), CrumbleFrameDuration),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 2), CrumbleFrameDuration),
        new(ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, 3), CrumbleFrameDuration),
    ];

    /// <summary>
    /// The crumble: its setup holds it for 64 updates before the wall begins to fall, then it crumbles the wall one row
    /// at a time; <see cref="BotwoonWallCrumbleObject"/> opens the first two scroll cells as the first row starts.
    /// </summary>
    internal static TerrainChange Crumble { get; } = new(64,
    [
        .. CrumbleRow(Offset16<RoomBlocks>.Zero),
        .. Enumerable.Repeat(NextRow, CrumbleRows - 1).SelectMany(CrumbleRow),
    ]);

    /// <summary>The clear of an already-defeated room: the nine cleared wall blocks, drawn on the next update.</summary>
    internal static TerrainChange Clear { get; } = TerrainChange.DrawnOnce(BotwoonWallObjectDrawDefinitions.Clear);
}
