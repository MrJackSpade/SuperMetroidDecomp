using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Samus.Abilities.XrayRevealDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The reveal shape the X-ray scope runs for each collision kind and behaviour.</summary>
public static class XrayReveals
{
    /// <summary>Returns the game-authored reveal shape for one collision kind and behaviour.</summary>
    /// <remarks>
    /// Nine collision kinds have reveals (air, special air, horizontal extension, spike, special,
    /// shootable, vertical extension, grapple and bombable blocks); every other kind reveals nothing, whatever its
    /// behaviour. Air and the
    /// extensions reveal the same for every behaviour; the reacting kinds read the block's reaction. The blocks each copy
    /// shape shows are installed pictures (#1596).
    /// </remarks>
    public static XrayRevealShape? Find(RoomCollisionType type, RoomBlockBehavior behavior) =>
        Find(type, behavior.ReactionUnder(type));

    /// <summary>The X-ray reveal shape each collision type shows for its block reaction, or null for one that reveals nothing.</summary>
    private static IdentityTable<RoomCollisionType, Func<RoomCollisionType, BlockReaction?, XrayRevealShape?>> RevealsByType => field ??= new(new Dictionary<RoomCollisionType, Func<RoomCollisionType, BlockReaction?, XrayRevealShape?>>
    {
        // Air reveals the same copy-one shape whatever its behaviour.
        [RoomCollisionType.Air] = static (type, reaction) => XrayRevealShape.CopyOne,
        [RoomCollisionType.SpecialAir] = static (type, reaction) => Lookup(SpecialAirReveals, type, reaction),
        // A horizontal extension reveals as one whatever its behaviour.
        [RoomCollisionType.HorizontalExtension] = static (type, reaction) => XrayRevealShape.HorizontalExtension,
        [RoomCollisionType.SpikeBlock] = static (type, reaction) => Lookup(SpikeBlockReveals, type, reaction),
        [RoomCollisionType.SpecialBlock] = static (type, reaction) => Lookup(SpecialBlockReveals, type, reaction),
        [RoomCollisionType.ShootableBlock] = static (type, reaction) => Lookup(ShootableBlockReveals, type, reaction),
        // A vertical extension reveals as one whatever its behaviour.
        [RoomCollisionType.VerticalExtension] = static (type, reaction) => XrayRevealShape.VerticalExtension,
        [RoomCollisionType.GrappleBlock] = static (type, reaction) => Lookup(GrappleBlockReveals, type, reaction),
        [RoomCollisionType.BombableBlock] = static (type, reaction) => Lookup(BombableBlockReveals, type, reaction),
        [RoomCollisionType.Slope] = static (type, reaction) => null,
        [RoomCollisionType.SpikeAir] = static (type, reaction) => null,
        [RoomCollisionType.ShootableAir] = static (type, reaction) => null,
        [RoomCollisionType.UnrevealedAir] = static (type, reaction) => null,
        [RoomCollisionType.BombableAir] = static (type, reaction) => null,
        [RoomCollisionType.SolidBlock] = static (type, reaction) => null,
        [RoomCollisionType.DoorBlock] = static (type, reaction) => null,
    });

    /// <summary>
    /// Returns the reveal shape for one collision kind and the reaction it reads, null for a kind that reads none or a block
    /// with no reaction of its kind.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reaction"/> is not of the family <paramref name="type"/> reads.</exception>
    public static XrayRevealShape? Find(RoomCollisionType type, BlockReaction? reaction) => RevealsByType[type](type, reaction);

    /// <summary>
    /// The shape a reacting kind's reveal table selects for <paramref name="reaction"/>, or none when the table has no entry
    /// or the block has no reaction.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reaction"/> is not of the family <paramref name="type"/> reads.</exception>
    private static XrayRevealShape? Lookup(IReadOnlyDictionary<BlockReaction, XrayRevealShape> table, RoomCollisionType type,
        BlockReaction? reaction)
    {
        if (reaction is null)
            return null;
        if (reaction.Family != BlockReactionFamilies.Of(type))
            throw new ArgumentException($"The reaction {reaction} is not one {type} reads.", nameof(reaction));
        return table.TryGetValue(reaction, out XrayRevealShape shape) ? shape : null;
    }
}
