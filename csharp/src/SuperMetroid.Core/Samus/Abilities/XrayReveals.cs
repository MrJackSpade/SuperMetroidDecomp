using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Samus.Abilities.XrayRevealDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The reveal command the X-ray scope runs for each collision kind and behaviour.</summary>
internal static class XrayReveals
{
    /// <summary>Returns the game-authored reveal command for one collision kind and behaviour.</summary>
    /// <remarks>
    /// Issue #1026: nine collision kinds have reveals (air, special air, horizontal extension, spike, special,
    /// shootable, vertical extension, grapple and bombable blocks); every other kind reveals nothing. The authored data
    /// and the independent oracle agree for all 16 kinds times 256 behaviour values, including the no-match cases.
    /// Air and the extensions reveal the same for every behaviour; the reacting kinds read the block's reaction. The
    /// blocks each copy command shows are installed pictures (#1596).
    /// </remarks>
    public static XrayRevealCommand? Find(RoomCollisionType type, RoomBlockBehavior behavior) => type switch
    {
        // Issue #1027: the air table has the sole wildcard entry, a copy-one command, for every behaviour.
        RoomCollisionType.Air => XrayRevealCommand.CopyOne,
        RoomCollisionType.SpecialAir => Lookup(SpecialAirReveals, behavior.Reaction),
        // Issue #1029: the horizontal-extension table has a wildcard entry for every behaviour.
        RoomCollisionType.HorizontalExtension => XrayRevealCommand.HorizontalExtension,
        RoomCollisionType.SpikeBlock => Lookup(SpikeBlockReveals, behavior.Reaction),
        RoomCollisionType.SpecialBlock => FindSpecialBlock(behavior.Reaction),
        RoomCollisionType.ShootableBlock => FindShootableBlock(behavior.Reaction),
        // Issue #1031: the vertical-extension table has a wildcard entry for every behaviour.
        RoomCollisionType.VerticalExtension => XrayRevealCommand.VerticalExtension,
        RoomCollisionType.GrappleBlock => FindGrappleBlock(behavior.Reaction),
        RoomCollisionType.BombableBlock => FindBombableBlock(behavior.Reaction),
        RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
            RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or RoomCollisionType.BombableAir or
            RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock => null,
        _ => throw new InvalidOperationException($"Undefined RoomCollisionType {type}."),
    };

    /// <summary>Finds the special-block reveal command for one reaction.</summary>
    /// <remarks>
    /// Issue #1032: twenty special-block behaviours have reveals (00..0F, 82..85). For 00..07, behaviour mod 4 selects
    /// one, wide, tall, or square copies; 08..0F are one-block copies; 82..85 use the distinct Brinstar-only command.
    /// Every other behaviour returns no reveal. The authored data and the independent 256-case special-block oracle agree.
    /// </remarks>
    private static XrayRevealCommand? FindSpecialBlock(BlockReaction reaction) =>
        Lookup(SpecialBlockReveals, reaction);

    /// <summary>Finds the shootable-block reveal command for one reaction.</summary>
    /// <remarks>
    /// Issue #1033: sixteen shootable-block behaviours have reveals (00..0F). For 00..07, behaviour mod 4 chooses one,
    /// wide, tall, or square copies; 08..0F are one-block copies; all others return no reveal. The authored data and
    /// the independent 256-case shootable-block oracle agree.
    /// </remarks>
    private static XrayRevealCommand? FindShootableBlock(BlockReaction reaction) =>
        Lookup(ShootableBlockReveals, reaction);

    /// <summary>Finds the grapple-block reveal command for one reaction.</summary>
    /// <remarks>
    /// Issue #1034: three grapple-block behaviours (00..02) have one-block reveals. All other behaviours return no
    /// reveal. The authored data and the independent 256-case grapple-block oracle agree.
    /// </remarks>
    private static XrayRevealCommand? FindGrappleBlock(BlockReaction reaction) =>
        Lookup(GrappleBlockReveals, reaction);

    /// <summary>Finds the bombable-block reveal command for one reaction.</summary>
    /// <remarks>
    /// Issue #1035: eight bombable-block behaviours have reveals (00..07): the low two behaviour bits select one, wide,
    /// tall, or square dimensions. Behaviour 08..FF has no reveal. The authored data and the independent 256-case
    /// bombable-block oracle agree.
    /// </remarks>
    private static XrayRevealCommand? FindBombableBlock(BlockReaction reaction) =>
        Lookup(BombableBlockReveals, reaction);

    /// <summary>The command a reaction table selects for <paramref name="reaction"/>, or none when the table has no entry.</summary>
    private static XrayRevealCommand? Lookup(IReadOnlyDictionary<BlockReaction, XrayRevealCommand> table, BlockReaction reaction) =>
        table.TryGetValue(reaction, out XrayRevealCommand command) ? command : null;
}
