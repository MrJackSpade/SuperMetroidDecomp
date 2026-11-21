using static SuperMetroid.Core.Rooms.RoomObjects.ShotBlockObjectDraws;
using Stage = SuperMetroid.Core.Rooms.RoomObjects.BlockBreakupStage;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The block reactions' shared beats, stated once (#2557): a block breaking up into rock, pieces and dust and leaving its
/// space open, and, for a block that grows back, the same images in reverse and its restored picture.
/// </summary>
internal static class BlockBreakups
{
    /// <summary>A reaction that shows <paramref name="draw"/> for one update, starting on the update after it is set off.</summary>
    /// <param name="draw">What the reveal shows.</param>
    internal static BlockReactionChange Reveal(RoomObjectDraw draw) => new(1, [new(draw, 1)]);

    /// <summary>
    /// A normal bomb's one-update reveal of a crumble block of <paramref name="arrangement"/>: a single block shows its
    /// bombed reveal, a larger one the contact crumble's restored picture.
    /// </summary>
    /// <param name="arrangement">The crumble block's blocks.</param>
    internal static BlockReactionChange CrumbleReveal(BlockArrangement arrangement) => Reveal(arrangement == BlockArrangement.Single
        ? BombedRevealObjectDraws.Family.Of(RoomObjectBombedRevealDraw.CrumbleSingle)
        : LinkedRestoreObjectFamily.ContactCrumbleDraws.Of(new ContactCrumbleRestoreDraw(arrangement)));

    /// <summary>A block of <paramref name="arrangement"/> breaking up for good: rock, pieces and dust, then its open space for one update.</summary>
    /// <param name="delay">The updates the reaction waits before its first image.</param>
    /// <param name="arrangement">The blocks the reaction breaks.</param>
    /// <param name="pace">How long the rock, pieces and dust stay.</param>
    /// <param name="sound">The sound the rock comes with; null for none.</param>
    internal static BlockReactionChange Permanent(int delay, BlockArrangement arrangement, BlockBreakupPace pace, BlockBreakupSound? sound) =>
        new(delay, [.. Breaking(arrangement, pace, sound), new(Breakup(arrangement, Stage.Gone), 1)]);

    /// <summary>
    /// A block of <paramref name="arrangement"/> breaking up, staying open for <paramref name="openUpdates"/>, then growing
    /// back through dust, pieces and rock at the steady pace before <paramref name="restore"/> shows its block again.
    /// </summary>
    /// <param name="delay">The updates the reaction waits before its first image.</param>
    /// <param name="arrangement">The blocks the reaction breaks.</param>
    /// <param name="pace">How long the rock, pieces and dust stay as the block breaks.</param>
    /// <param name="sound">The sound the rock comes with; null for none.</param>
    /// <param name="openUpdates">How long the block's space stays open.</param>
    /// <param name="restore">How the block's picture comes back.</param>
    internal static BlockReactionChange Respawning(int delay, BlockArrangement arrangement, BlockBreakupPace pace, BlockBreakupSound? sound,
        int openUpdates, BlockRestore restore) =>
        new(delay,
        [
            .. Breaking(arrangement, pace, sound),
            new(Breakup(arrangement, Stage.Gone), openUpdates),
            new(Breakup(arrangement, Stage.Dust), BlockBreakupPace.Steady.Dust),
            new(Breakup(arrangement, Stage.Pieces), BlockBreakupPace.Steady.Pieces),
            new(Breakup(arrangement, Stage.Rock), BlockBreakupPace.Steady.Rock),
            restore.Of(arrangement),
        ]);

    /// <summary>The rock, pieces and dust of a breaking block, the rock with its sound.</summary>
    private static BlockReactionFrame[] Breaking(BlockArrangement arrangement, BlockBreakupPace pace, BlockBreakupSound? sound) =>
    [
        new(Breakup(arrangement, Stage.Rock), pace.Rock, sound?.Effect) { SoundPriority = sound?.Priority ?? SoundRequestPriority.Normal },
        new(Breakup(arrangement, Stage.Pieces), pace.Pieces),
        new(Breakup(arrangement, Stage.Dust), pace.Dust),
    ];
}
