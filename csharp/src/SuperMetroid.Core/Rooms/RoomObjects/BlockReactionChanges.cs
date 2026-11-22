using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Selects a block reaction by the block's reaction or crumble block from <see cref="BlockReactionChangeDefinitions"/>.</summary>
internal static partial class BlockReactionChanges
{
    /// <summary>
    /// The collision bomb block a bomb block's break sets off when Samus's contact breaks it (#2554), or none for a reaction
    /// that breaks nothing: its room object does nothing, and an inert block's contact never reaches it.
    /// </summary>
    /// <param name="reaction">The bombable block's reaction.</param>
    /// <param name="change">The collision bomb block the break sets off, when it is one.</param>
    /// <returns>Whether <paramref name="reaction"/> is one of the eight bomb-block breaks.</returns>
    internal static bool TryCollisionBomb(BombableReaction reaction, [NotNullWhen(true)] out BlockReactionChange? change) =>
        BlockReactionChangeDefinitions.CollisionBomb.TryGetValue(reaction, out change);

    /// <summary>The crumble reveal a normal bomb sets off for a crumble block, by the crumble's size.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a crumble block.</exception>
    internal static LastingBlockReactionChange CrumbleReveal(SpecialBlockReaction? reaction) => From(BlockReactionChangeDefinitions.CrumbleReveal, reaction);

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
    internal static BlockReactionChange RespawningShot(ShootableReaction? reaction) => From(BlockReactionChangeDefinitions.RespawningShot, reaction);

    /// <summary>The permanent shot block a permanent break sets off, by its size.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is not a permanent break.</exception>
    internal static LastingBlockReactionChange PermanentShot(ShootableReaction? reaction) => From(BlockReactionChangeDefinitions.PermanentShot, reaction);

    /// <summary>The change <paramref name="changes"/> gives <paramref name="reaction"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reaction"/> is none, or not among the selection.</exception>
    internal static TChange From<TReaction, TChange>(IReadOnlyDictionary<TReaction, TChange> changes, TReaction? reaction)
        where TReaction : struct, Enum
        where TChange : class =>
        reaction is { } read && changes.TryGetValue(read, out TChange? change)
            ? change
            : throw new ArgumentOutOfRangeException(nameof(reaction), reaction, "The reaction sets off none of these changes.");
}
