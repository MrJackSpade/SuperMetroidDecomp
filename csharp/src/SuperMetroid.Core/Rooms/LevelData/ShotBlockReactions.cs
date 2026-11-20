using static SuperMetroid.Core.Rooms.LevelData.ShotBlockReactionDefinitions;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the shot-block response does with a shootable block's reaction.</summary>
public static class ShotBlockReactions
{
    /// <summary>
    /// The shot-block response's reaction for <paramref name="reaction"/>, when the response handles it (the door,
    /// collectible, gate and animal-escape reactions run before it).
    /// </summary>
    /// <param name="reaction">The block's shootable reaction.</param>
    /// <param name="handled">The response's reaction.</param>
    /// <returns>Whether the response handles the reaction.</returns>
    public static bool TryOf(ShootableReaction reaction, out ShotBlockReaction handled) => Handled.TryGetValue(reaction, out handled);

    /// <summary>Whether <paramref name="reaction"/> is one of the reactions every weapon's scan hands to the shot-block response.</summary>
    public static bool IsWeaponReaction(ShootableReaction? reaction) => reaction is { } read && WeaponReactions.Contains(read);

    /// <summary>What the shot-block response makes of a shootable reaction.</summary>
    extension(ShootableReaction reaction)
    {
        /// <summary>
        /// Whether the reaction sets its one-update object off only in a shootable block: in shootable air a weapon, a bomb
        /// or the grapple beam passes it with no reaction at all.
        /// </summary>
        public bool SetsOffOnlyInSolidBlocks => SolidBlockOnlyReactions.Contains(reaction);
    }
}
