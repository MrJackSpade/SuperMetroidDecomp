using static SuperMetroid.Core.Rooms.LevelData.ShotBlockReactionDefinitions;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the shot-block dispatcher does with a shootable block's reaction.</summary>
public static class ShotBlockReactions
{
    /// <summary>
    /// The dispatcher's reaction for <paramref name="reaction"/>, or null for an area reaction or a common entry the
    /// dispatcher does not handle (the door, collectible, gate and animal-escape entries are dispatched before it).
    /// </summary>
    public static ShotBlockReaction? Of(BlockReaction reaction)
    {
        if (reaction.IsAreaSpecific)
            return null;
        if (reaction.Entry < LeadingEntries.Count)
            return LeadingEntries[reaction.Entry];
        return reaction.Entry is <= LastFollowingUnusedEntry or LoneUnusedEntry
            ? ShotBlockReaction.Unused
            : null;
    }

    /// <summary>Whether <paramref name="reaction"/> is the collision probe.</summary>
    public static bool IsCollisionProbe(BlockReaction reaction) => Of(reaction) == ShotBlockReaction.CollisionProbe;

    /// <summary>
    /// The block shape a break reaction selects: zero through three for one block, two wide, two tall and two by two.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The reaction is not a respawning or permanent break.</exception>
    public static int BreakShape(BlockReaction reaction) => Of(reaction) switch
    {
        ShotBlockReaction.RespawningBreak => reaction.Entry,
        ShotBlockReaction.PermanentBreak => reaction.Entry - FirstPermanentBreakEntry,
        ShotBlockReaction.RespawningPowerBombBreak or ShotBlockReaction.PermanentPowerBombBreak or
            ShotBlockReaction.RespawningSuperMissileBreak or ShotBlockReaction.PermanentSuperMissileBreak or
            ShotBlockReaction.Nothing or ShotBlockReaction.CollisionProbe or ShotBlockReaction.Unused or null =>
            throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The reaction is not a shot-block break."),
        _ => throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "Undefined shot-block reaction."),
    };
}
