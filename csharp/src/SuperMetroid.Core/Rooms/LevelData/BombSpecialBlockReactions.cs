using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What a normal bomb sets off at a special block (#1742).</summary>
public static class BombSpecialBlockReactions
{
    /// <summary>The plain solid special blocks a normal bomb shows as Speed Booster blocks in every area.</summary>
    private static readonly BombRevealedSpeedBlockLookalikes<SpecialBlockReaction, LastingBlockReactionChange> SpeedBlockLookalikes =
        new([SpecialBlockReaction.BombRevealsSpeedBlock], SpeedBoosterBlockObjectDefinitions.BombReveal);

    /// <summary>
    /// The crumble reveal, the Speed Booster block's reveal, or the reaction that does nothing but hold its place for an update,
    /// for a special block with <paramref name="reaction"/>. Brinstar's own Speed Booster blocks are revealed as Speed
    /// Booster blocks; in the other areas those blocks stay plain solid blocks (#1805).
    /// </summary>
    /// <param name="reaction">The block's reaction.</param>
    internal static LastingBlockReactionChange ChangeFor(SpecialBlockReaction reaction)
    {
        SpecialBlockReaction read = reaction;
        if (SpeedBlockLookalikes.TryShowAsSpeedBlock(read, out LastingBlockReactionChange? shown))
            return shown;
        return Ensure.Listed(BombSpecialBlockReactionDefinitions.Responses, read) switch
        {
            SpecialBlockBombResponse.CrumbleReveal => BlockReactionChanges.CrumbleReveal(read),
            SpecialBlockBombResponse.SpeedBlockReveal => SpeedBoosterBlockObjectDefinitions.BombReveal,
            SpecialBlockBombResponse.Nothing => LastingBlockReactionChange.MadeAtSpawn,
            var response => throw new InvalidOperationException($"Undefined {nameof(SpecialBlockBombResponse)} {response}."),
        };
    }
}
