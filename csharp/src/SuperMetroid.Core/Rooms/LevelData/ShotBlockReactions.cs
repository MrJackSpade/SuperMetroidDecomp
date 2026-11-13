using static SuperMetroid.Core.Rooms.LevelData.ShotBlockReactionDefinitions;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the shot-block dispatcher does with a shootable block's reaction.</summary>
public static class ShotBlockReactions
{
    /// <summary>
    /// The dispatcher's reaction for <paramref name="reaction"/>, or null for an area reaction or a reaction the dispatcher
    /// does not handle (the door, collectible, gate and animal-escape reactions are dispatched before it).
    /// </summary>
    public static ShotBlockReaction? Of(ShootableReaction reaction) =>
        Handled.TryGetValue(reaction, out ShotBlockReaction handled) ? handled : null;

    /// <summary>Whether <paramref name="reaction"/> is one of the table's leading reactions every weapon's scan dispatches.</summary>
    public static bool IsLeading(ShootableReaction reaction) => LeadingReactions.Contains(reaction);

    /// <summary>Whether <paramref name="reaction"/> is one the room's area gives shootable blocks.</summary>
    public static bool IsArea(ShootableReaction reaction) => AreaReactions.Contains(reaction);
}
