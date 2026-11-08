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
    /// </remarks>
    public static XrayRevealDefinition? Find(RoomCollisionType type, RoomBlockBehavior behavior) => Find(type, behavior.Value);

    /// <summary>The reveal for one collision kind and behaviour value, the key the reveal tables are authored by.</summary>
    private static XrayRevealDefinition? Find(RoomCollisionType type, int bts) => type switch
    {
        RoomCollisionType.Air => Air,
        RoomCollisionType.SpecialAir => FindSpecialAir(bts),
        RoomCollisionType.HorizontalExtension => HorizontalExtension,
        RoomCollisionType.SpikeBlock => FindSpikeBlock(bts),
        RoomCollisionType.SpecialBlock => FindSpecialBlock(bts),
        RoomCollisionType.ShootableBlock => FindShootableBlock(bts),
        RoomCollisionType.VerticalExtension => VerticalExtension,
        RoomCollisionType.GrappleBlock => FindGrappleBlock(bts),
        RoomCollisionType.BombableBlock => FindBombableBlock(bts),
        RoomCollisionType.Slope or RoomCollisionType.SpikeAir or
            RoomCollisionType.ShootableAir or RoomCollisionType.UnusedAir or RoomCollisionType.BombableAir or
            RoomCollisionType.SolidBlock or RoomCollisionType.DoorBlock => null,
        _ => throw new InvalidOperationException($"Undefined RoomCollisionType {type}."),
    };

    /// <summary>Finds the special-air reveal for one unsigned behaviour byte: the one-entry special-air table.</summary>
    private static XrayRevealDefinition? FindSpecialAir(int bts) =>
        SpecialAirReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the spike-block reveal for one unsigned behaviour byte: the one-entry spike-block table.</summary>
    private static XrayRevealDefinition? FindSpikeBlock(int bts) =>
        SpikeBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the authored special-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1032: twenty special-block behaviours have reveals
    /// (00..0F, 82..85). For 00..07, behaviour mod 4 selects one, wide,
    /// tall, or square copies of tile 188; 08..0D are one tile 188; 0E..0F
    /// are one tile 182; 82..85 use the distinct Brinstar-only tile 182 command.
    /// Every other behaviour returns no reveal. The dimension pattern is exact,
    /// but tile and room-policy choices are authored. The original data and the
    /// independent 256-case special-block oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindSpecialBlock(int bts) =>
        SpecialBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the authored shootable-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1033: sixteen shootable-block behaviours have reveals (00..0F).
    /// For 00..07, behaviour mod 4 chooses one tile 82, wide
    /// 150/151, tall 152/184, or square 153/154/185/186.
    /// Behaviour 08..09 chooses one tile 87; 0A..0F chooses one tile 159; all others
    /// return no reveal. Dimension groups are regular, while tile identities
    /// are authored. The original data and the independent 256-case
    /// shootable-block oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindShootableBlock(int bts) =>
        ShootableBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the grapple-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1034: three grapple-block behaviours have reveals: one tile 155
    /// for 00 and one tile 183 for both 01 and 02. All other behaviours return
    /// no reveal. The original data and the independent 256-case grapple-block
    /// oracle agree.
    /// </remarks>
    private static XrayRevealDefinition? FindGrappleBlock(int bts) =>
        GrappleBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the bombable-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1035: eight bombable-block behaviours have reveals (00..07):
    /// the low two behaviour bits select one, wide, tall, or square
    /// dimensions, every tile being tile 88. Behaviour 08..FF has no reveal.
    /// The original data and the independent 256-case bombable-block oracle
    /// agree.
    /// </remarks>
    private static XrayRevealDefinition? FindBombableBlock(int bts) =>
        BombableBlockReveals.GetValueOrDefault(bts, null);
}
