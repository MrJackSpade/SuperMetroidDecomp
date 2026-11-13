using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Selects a block reaction by the block's reaction or crumble block from <see cref="BlockReactionChangeDefinitions"/>.</summary>
internal static class BlockReactionChanges
{
    /// <summary>The collision bomb block a bomb block's break sets off when Samus's contact breaks it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a break.</exception>
    internal static BlockReactionChange CollisionBomb(BombableReaction reaction) => From(BlockReactionChangeDefinitions.CollisionBomb, reaction);

    /// <summary>The bomb reaction a Power Bomb sets off for a bomb block's break.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a break.</exception>
    internal static BlockReactionChange ReactionBomb(BombableReaction reaction) => From(BlockReactionChangeDefinitions.ReactionBomb, reaction);

    /// <summary>The bomb block a normal bomb sets off for a bomb block's break, without the reaction sound.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a break.</exception>
    internal static BlockReactionChange BombBreak(BombableReaction reaction) => From(BlockReactionChangeDefinitions.BombBreak, reaction);

    /// <summary>The crumble reveal a normal bomb sets off for a crumble block, by the crumble's size.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a crumble block.</exception>
    internal static BlockReactionChange CrumbleReveal(SpecialBlockReaction reaction) => From(BlockReactionChangeDefinitions.CrumbleReveal, reaction);

    /// <summary>How <paramref name="crumble"/> breaks up once Samus lands on it.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="crumble"/> names no crumble block.</exception>
    internal static BlockReactionChange ContactCrumble(ContactCrumbleBlock crumble) => crumble switch
    {
        ContactCrumbleBlock.Respawning1x1 => BlockReactionChangeDefinitions.ContactCrumble1x1Respawning,
        ContactCrumbleBlock.Respawning2x1 => BlockReactionChangeDefinitions.ContactCrumble2x1Respawning,
        ContactCrumbleBlock.Respawning1x2 => BlockReactionChangeDefinitions.ContactCrumble1x2Respawning,
        ContactCrumbleBlock.Respawning2x2 => BlockReactionChangeDefinitions.ContactCrumble2x2Respawning,
        ContactCrumbleBlock.Permanent1x1 => BlockReactionChangeDefinitions.ContactCrumble1x1Permanent,
        ContactCrumbleBlock.Permanent2x1 => BlockReactionChangeDefinitions.ContactCrumble2x1Permanent,
        ContactCrumbleBlock.Permanent1x2 => BlockReactionChangeDefinitions.ContactCrumble1x2Permanent,
        ContactCrumbleBlock.Permanent2x2 => BlockReactionChangeDefinitions.ContactCrumble2x2Permanent,
        _ => throw new ArgumentOutOfRangeException(nameof(crumble), crumble, "Undefined contact crumble block."),
    };

    /// <summary>The respawning shot block a respawning break sets off, by its size.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a respawning break.</exception>
    internal static BlockReactionChange RespawningShot(ShootableReaction reaction) => From(BlockReactionChangeDefinitions.RespawningShot, reaction);

    /// <summary>The permanent shot block a permanent break sets off, by its size.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a permanent break.</exception>
    internal static BlockReactionChange PermanentShot(ShootableReaction reaction) => From(BlockReactionChangeDefinitions.PermanentShot, reaction);

    /// <summary>The change <paramref name="changes"/> gives <paramref name="reaction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not among the selection.</exception>
    private static BlockReactionChange From<TReaction>(IReadOnlyDictionary<TReaction, BlockReactionChange> changes, TReaction reaction)
        where TReaction : struct, Enum =>
        changes.TryGetValue(reaction, out BlockReactionChange? change)
            ? change
            : throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The reaction sets off none of these changes.");
}
