using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Cartridge values and helpers of <see cref="BlockReactionChanges"/> that only verification reads.</summary>
internal static class BlockReactionChangesConstants
{
    /// <summary>The bomb reaction a Power Bomb sets off for a bomb block's break.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a break.</exception>
    internal static BlockReactionChange ReactionBomb(BombableReaction? reaction) =>
        reaction is { } read && BlockReactionChangeDefinitions.ReactionBomb.TryGetValue(read, out BlockReactionChange? change)
            ? change
            : throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The reaction sets off no bomb reaction.");
}
