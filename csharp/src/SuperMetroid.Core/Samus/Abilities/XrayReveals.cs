using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Samus.Abilities.XrayRevealDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The reveal shape the X-ray scope runs for each collision kind and behaviour.</summary>
internal static class XrayReveals
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

    /// <summary>
    /// Returns the reveal shape for one collision kind and the reaction it reads, null for a kind that reads none or a block
    /// with no reaction of its kind.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reaction"/> is not the reaction <paramref name="type"/> reads.</exception>
    public static XrayRevealShape? Find(RoomCollisionType type, Enum? reaction) => type switch
    {
        // Air reveals the same copy-one shape whatever its behaviour.
        RoomCollisionType.Air => XrayRevealShape.CopyOne,
        RoomCollisionType.SpecialAir => Lookup(SpecialAirReveals, reaction),
        // A horizontal extension reveals as one whatever its behaviour.
        RoomCollisionType.HorizontalExtension => XrayRevealShape.HorizontalExtension,
        RoomCollisionType.SpikeBlock => Lookup(SpikeBlockReveals, reaction),
        RoomCollisionType.SpecialBlock => Lookup(SpecialBlockReveals, reaction),
        RoomCollisionType.ShootableBlock => Lookup(ShootableBlockReveals, reaction),
        // A vertical extension reveals as one whatever its behaviour.
        RoomCollisionType.VerticalExtension => XrayRevealShape.VerticalExtension,
        RoomCollisionType.GrappleBlock => Lookup(GrappleBlockReveals, reaction),
        RoomCollisionType.BombableBlock => Lookup(BombableBlockReveals, reaction),
        RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
            RoomCollisionType.ShootableAir or RoomCollisionType.UnrevealedAir or RoomCollisionType.BombableAir or
            RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock => null,
        _ => throw new InvalidOperationException($"Undefined RoomCollisionType {type}."),
    };

    /// <summary>
    /// The shape a reaction table selects for <paramref name="reaction"/>, or none when the table has no entry or the block
    /// has no reaction.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="reaction"/> is not the table's reaction type.</exception>
    private static XrayRevealShape? Lookup<TReaction>(IReadOnlyDictionary<TReaction, XrayRevealShape> table, Enum? reaction)
        where TReaction : struct, Enum => reaction switch
        {
            null => null,
            TReaction read => table.TryGetValue(read, out XrayRevealShape shape) ? shape : null,
            _ => throw new ArgumentException($"The reaction {reaction} is not a {typeof(TReaction).Name}.", nameof(reaction)),
        };
}
