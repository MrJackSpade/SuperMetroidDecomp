using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>Reads what the bomb-block responses do with a bombable block's reaction (#1722).</summary>
public static class BombableReactions
{
    /// <summary>
    /// Whether <paramref name="reaction"/> is one of the bomb-block breaks: a reaction whose block Samus's breaking contact
    /// sets off as a collision bomb block (<see cref="BlockReactionChangeDefinitions.CollisionBomb"/>); no reaction is none.
    /// </summary>
    public static bool IsBreak(BombableReaction? reaction) =>
        reaction is { } read && BlockReactionChangeDefinitions.CollisionBomb.ContainsKey(read);
}
