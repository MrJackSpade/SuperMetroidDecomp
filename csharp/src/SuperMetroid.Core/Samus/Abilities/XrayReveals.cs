using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Samus.Abilities.XrayRevealDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The reveal the X-ray scope shows for each collision kind and behaviour.</summary>
internal static class XrayReveals
{
    /// <summary>Returns the game-authored reveal for one collision kind and behaviour.</summary>
    /// <remarks>
    /// Issue #1026: nine collision kinds have reveals (air, special air, horizontal extension, spike, special,
    /// shootable, vertical extension, grapple and bombable blocks); every other kind reveals nothing. The original data
    /// and the independent oracle agree for all 16 kinds times 256 behaviour values, including the no-match cases.
    /// Air and the extensions reveal the same for every behaviour; the reacting kinds read the block's reaction.
    /// </remarks>
    public static XrayRevealDefinition? Find(RoomCollisionType type, RoomBlockBehavior behavior) => type switch
    {
        RoomCollisionType.Air => Air,
        RoomCollisionType.SpecialAir => FindSpecialAir(behavior.Reaction),
        RoomCollisionType.HorizontalExtension => HorizontalExtension,
        RoomCollisionType.SpikeBlock => FindSpikeBlock(behavior.Reaction),
        RoomCollisionType.SpecialBlock => FindSpecialBlock(behavior.Reaction),
        RoomCollisionType.ShootableBlock => FindShootableBlock(behavior.Reaction),
        RoomCollisionType.VerticalExtension => VerticalExtension,
        RoomCollisionType.GrappleBlock => FindGrappleBlock(behavior.Reaction),
        RoomCollisionType.BombableBlock => FindBombableBlock(behavior.Reaction),
        RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
            RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or RoomCollisionType.BombableAir or
            RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock => null,
        _ => throw new InvalidOperationException($"Undefined RoomCollisionType {type}."),
    };

    /// <summary>Finds the special-air reveal for one reaction: the one-entry special-air table.</summary>
    private static XrayRevealDefinition? FindSpecialAir(BlockReaction reaction) =>
        SpecialAirReveals.GetValueOrDefault(reaction, null);

    /// <summary>Finds the spike-block reveal for one reaction: the one-entry spike-block table.</summary>
    private static XrayRevealDefinition? FindSpikeBlock(BlockReaction reaction) =>
        SpikeBlockReveals.GetValueOrDefault(reaction, null);

    /// <summary>Finds the authored special-block reveal for one reaction.</summary>
    /// <remarks>
    /// Issue #1032: twenty special-block behaviours have reveals
    /// (00..0F, 82..85). For 00..07, behaviour mod 4 selects one, wide,
    /// tall, or square copies of tile 188; 08..0D are one tile 188; 0E..0F
    /// are one tile 182; 82..85 use the distinct Brinstar-only tile 182 command.
    /// Every other behaviour returns no reveal. The dimension pattern is exact,
    /// but tile and room-policy choices are authored. The original data and the
    /// independent 256-case special-block oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindSpecialBlock(BlockReaction reaction) =>
        SpecialBlockReveals.GetValueOrDefault(reaction, null);

    /// <summary>Finds the authored shootable-block reveal for one reaction.</summary>
    /// <remarks>
    /// Issue #1033: sixteen shootable-block behaviours have reveals (00..0F).
    /// For 00..07, behaviour mod 4 chooses one tile 82, wide
    /// 150/151, tall 152/184, or square 153/154/185/186.
    /// Behaviour 08..09 chooses one tile 87; 0A..0F chooses one tile 159; all others
    /// return no reveal. Dimension groups are regular, while tile identities
    /// are authored. The original data and the independent 256-case
    /// shootable-block oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindShootableBlock(BlockReaction reaction) =>
        ShootableBlockReveals.GetValueOrDefault(reaction, null);

    /// <summary>Finds the grapple-block reveal for one reaction.</summary>
    /// <remarks>
    /// Issue #1034: three grapple-block behaviours have reveals: one tile 155
    /// for 00 and one tile 183 for both 01 and 02. All other behaviours return
    /// no reveal. The original data and the independent 256-case grapple-block
    /// oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindGrappleBlock(BlockReaction reaction) =>
        GrappleBlockReveals.GetValueOrDefault(reaction, null);

    /// <summary>Finds the bombable-block reveal for one reaction.</summary>
    /// <remarks>
    /// Issue #1035: eight bombable-block behaviours have reveals (00..07):
    /// the low two behaviour bits select one, wide, tall, or square
    /// dimensions, every tile being tile 88. Behaviour 08..FF has no reveal.
    /// The original data and the independent 256-case bombable-block oracle
    /// agree.
    /// </remarks>
    private static XrayRevealDefinition? FindBombableBlock(BlockReaction reaction) =>
        BombableBlockReveals.GetValueOrDefault(reaction, null);
}
