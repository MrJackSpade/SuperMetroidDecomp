using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Samus.Abilities.XrayRevealDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The reveal the X-ray scope shows for each collision kind and behaviour.</summary>
internal static class XrayReveals
{
    /// <summary>Returns the game-authored reveal for one collision kind and behaviour.</summary>
    /// <remarks>
    /// Issue #1026: the outer selector in the original data is nine
    /// little-endian (level-word high nibble, behaviour-table pointer) pairs,
    /// then a terminator. Its exact mapping is
    /// 0:D2FC, 3:D306, 5:D310, A:D318, B:D322, C:D3CC, D:D462, E:D46A,
    /// F:D484; nibbles 1, 2, 4, 6, 7, 8, and 9 have no reveal. The
    /// dispatcher masks the block's type nibble and scans these
    /// four-byte records to the sentinel before searching behaviour. Thus the bounded
    /// algorithm is sparse nibble classification, already expressed by this
    /// switch, not a numeric formula or a runtime pointer table. Inspection of the original data
    /// and the independent oracle agree for all 16 types times
    /// 256 behaviour values, including the no-match cases; each selected behaviour table
    /// has its own lookup and proof.
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
    /// Issue #1032: the special-block table has 20
    /// key/pointer pairs (00..0F, 82..85), a terminator, and command
    /// operands. For 00..07, behaviour mod 4 selects one, wide,
    /// tall, or square copies of tile 188; 08..0D are one tile 188; 0E..0F
    /// are one tile 182; 82..85 use the distinct Brinstar-only tile 182 command.
    /// Every other behaviour returns no reveal. The dimension pattern is exact,
    /// but tile and room-policy choices are authored, so this grouped switch
    /// is clearer than a general formula. The original keys/pointers and the
    /// independent 256-case special-block oracle including operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindSpecialBlock(int bts) =>
        SpecialBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the authored shootable-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1033: the shootable-block table has 16
    /// key/pointer pairs (00..0F), a terminator, and command operands.
    /// For 00..07, behaviour mod 4 chooses one tile 82, wide
    /// 150/151, tall 152/184, or square 153/154/185/186.
    /// Behaviour 08..09 chooses one tile 87; 0A..0F chooses one tile 159; all others
    /// return no reveal. Dimension groups are regular, while tile identities
    /// are authored, so the bounded grouped switch remains clearest. Direct
    /// original keys/pointers and the independent 256-case shootable-block oracle
    /// including command operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindShootableBlock(int bts) =>
        ShootableBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the grapple-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1034: the grapple-block table has three
    /// key/pointer pairs (00, 01, 02), a terminator, then copy-one
    /// commands with tile 155 for 00 and 183 for both 01 and 02.
    /// All other behaviour bytes return no reveal. The exact bounded rule is
    /// the 0 versus 1..2 classification already expressed below; tile
    /// identities remain authored. The original keys/pointers and the
    /// independent 256-case grapple-block oracle including operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindGrappleBlock(int bts) =>
        GrappleBlockReveals.GetValueOrDefault(bts, null);

    /// <summary>Finds the bombable-block reveal for one unsigned behaviour byte.</summary>
    /// <remarks>
    /// Issue #1035: the bombable-block table has eight
    /// key/pointer pairs for behaviour 00..07, a terminator, and matching
    /// copy commands. Within 00..07, the low two behaviour bits
    /// select one, wide, tall, or square dimensions; every copied tile is
    /// tile 88. Behaviour 08..FF has no reveal. Checked range plus low-two-bit
    /// classification is the complete bounded rule already represented
    /// by this switch. The original keys/pointers and the independent
    /// 256-case bombable-block oracle including operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindBombableBlock(int bts) =>
        BombableBlockReveals.GetValueOrDefault(bts, null);
}
