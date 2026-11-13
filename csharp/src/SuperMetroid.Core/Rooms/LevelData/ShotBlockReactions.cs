using static SuperMetroid.Core.Rooms.LevelData.ShotBlockReactionDefinitions;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the shot-block dispatcher does with a shootable block's reaction.</summary>
public static class ShotBlockReactions
{
    /// <summary>
    /// The dispatcher's reaction for <paramref name="reaction"/>, or null for no reaction or a reaction the dispatcher does
    /// not handle (the door, collectible, gate and animal-escape reactions are dispatched before it).
    /// </summary>
    public static ShotBlockReaction? Of(ShootableReaction? reaction) =>
        reaction is { } read && Handled.TryGetValue(read, out ShotBlockReaction handled) ? handled : null;

    /// <summary>Whether <paramref name="reaction"/> is one of the table's leading reactions every weapon's scan dispatches.</summary>
    public static bool IsLeading(ShootableReaction? reaction) => reaction is { } read && LeadingReactions.Contains(read);

    /// <summary>What the shot-block dispatcher makes of a shootable reaction.</summary>
    extension(ShootableReaction reaction)
    {
        /// <summary>
        /// Whether the reaction sets its one-update object off only in a shootable block: in shootable air a weapon, a bomb
        /// or the grapple beam passes it with no reaction at all.
        /// </summary>
        public bool SetsOffOnlyInSolidBlocks => SolidBlockOnlyReactions.Contains(reaction);
    }
}
