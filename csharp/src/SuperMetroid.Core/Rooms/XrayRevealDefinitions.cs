namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Compiled form of the complete revealed-block dispatcher at $91:D2D6-$91:D4D9.
/// Definitions retain native command identities because the tilemap builder uses their
/// distinct copy dimensions and Brinstar-only behavior.
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>BTS $46 is the only special-air value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1028: pinned NTSC J/U v1.0 ROM $91:D306..D30F contains
    /// ($0046, $D30C), terminal $FFFF, then copy-one command $CF36 and
    /// tile $00FF. Native $91:CE08 compares the zero-extended BTS byte,
    /// so equality with $46 is the complete bounded algorithm over 0..255;
    /// every other BTS returns no reveal. Direct ROM words and all 256
    /// special-air cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    private const byte SpecialAirRevealBts = 0x46;

    /// <summary>BTS $0E is the only spike-block value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1030: pinned NTSC J/U v1.0 ROM $91:D318..D321 contains
    /// ($000E, $D31E), terminal $FFFF, then copy-one command $CF36 and
    /// tile $005F. Native $91:CE08 compares zero-extended BTS, making
    /// equality with $0E the exact bounded rule over 0..255; other BTS
    /// values return no reveal. Direct ROM words and all 256 spike-block
    /// cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    private const byte SpikeRevealBts = 0x0e;

    /// <summary>Air reveals one $00FF tile for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1027: pinned NTSC J/U v1.0 ROM $91:D2FC..D305 has the sole
    /// wildcard $FF00 to $D302, then $FFFF; $D302 holds copy-one command
    /// $CF36 and tile $00FF. Native $91:CE08 checks the wildcard before the
    /// unsigned BTS byte, so the exact bounded rule for BTS 0..255 is this
    /// constant definition. Direct ROM words and the independent 256-case
    /// air slice of the X-ray reveal oracle agree; no pointer table is needed.
    /// </remarks>
    private static readonly XrayRevealDefinition Air = One(0x00ff);
    /// <summary>Horizontal extension command for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1029: pinned NTSC J/U v1.0 ROM $91:D310..D317 has wildcard
    /// $FF00 to $D316, terminal $FFFF, then the operand-free horizontal
    /// extension command $CEBB. Native $91:CE08 checks the wildcard first,
    /// making this constant definition exact for all unsigned BTS 0..255.
    /// Direct ROM words and the independent 256-case horizontal-extension
    /// oracle agree; the neighboring spike table is not part of this lookup.
    /// </remarks>
    private static readonly XrayRevealDefinition HorizontalExtension =
        new(XrayRevealCodePointers.HorizontalExtension, 0, 0, 0, 0);
    /// <summary>Vertical extension command for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1031: pinned NTSC J/U v1.0 ROM $91:D462..D469 has wildcard
    /// $FF00 to $D468, terminal $FFFF, then operand-free vertical extension
    /// command $CE79. Native $91:CE08 checks the wildcard first, making
    /// this constant definition exact for all unsigned BTS 0..255.
    /// Direct ROM words and the independent 256-case vertical-extension
    /// oracle agree; adjacent grapple entries do not belong to this lookup.
    /// </remarks>
    private static readonly XrayRevealDefinition VerticalExtension =
        new(XrayRevealCodePointers.VerticalExtension, 0, 0, 0, 0);

    /// <summary>Returns the cartridge-authored reveal for one collision type/BTS pair.</summary>
    /// <remarks>
    /// Issue #1026: the outer selector in the pinned NTSC J/U v1.0 ROM is nine
    /// little-endian (level-word high nibble, bank-$91 BTS-table pointer) pairs at
    /// $91:D2D6..D2F9, then $FFFF at $91:D2FA. Its exact mapping is
    /// 0:D2FC, 3:D306, 5:D310, A:D318, B:D322, C:D3CC, D:D462, E:D46A,
    /// F:D484; nibbles 1, 2, 4, 6, 7, 8, and 9 have no reveal. The native
    /// $91:CDD6 dispatcher masks the level word with $F000 and scans these
    /// four-byte records to the sentinel before searching BTS. Thus the bounded
    /// algorithm is sparse nibble classification, already expressed by this
    /// switch, not a numeric formula or a runtime pointer table. Direct ROM
    /// inspection and the independent ROM oracle agree for all 16 types times
    /// 256 BTS values, including the no-match cases; each selected BTS table
    /// has its own lookup and proof.
    /// </remarks>
    public static XrayRevealDefinition? Find(RoomCollisionType type, byte bts) => type switch
    {
        RoomCollisionType.Air => Air,
        RoomCollisionType.SpecialAir when bts == SpecialAirRevealBts => Air,
        RoomCollisionType.HorizontalExtension => HorizontalExtension,
        RoomCollisionType.SpikeBlock when bts == SpikeRevealBts => One(0x005f),
        RoomCollisionType.SpecialBlock => FindSpecialBlock(bts),
        RoomCollisionType.ShootableBlock => FindShootableBlock(bts),
        RoomCollisionType.VerticalExtension => VerticalExtension,
        RoomCollisionType.GrappleBlock => FindGrappleBlock(bts),
        RoomCollisionType.BombableBlock => FindBombableBlock(bts),
        _ => null,
    };

    /// <summary>Finds the authored special-block reveal for one unsigned BTS byte.</summary>
    /// <remarks>
    /// Issue #1032: pinned NTSC J/U v1.0 ROM $91:D322..D3CB has 20
    /// key/pointer pairs (00..0F, 82..85), $FFFF at $D372, and command
    /// operands through $D3CB. For 00..07, BTS mod 4 selects one, wide,
    /// tall, or square copies of tile $00BC; 08..0D are one $00BC; 0E..0F
    /// are one $00B6; 82..85 use the distinct Brinstar-only $00B6 command.
    /// Every other BTS returns no reveal. The dimension pattern is exact,
    /// but tile and room-policy choices are authored, so this grouped switch
    /// is clearer than a general formula. Direct ROM keys/pointers and the
    /// independent 256-case special-block oracle including operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindSpecialBlock(byte bts) => bts switch
    {
        0 or 4 or 8 or 9 or 0x0a or 0x0b or 0x0c or 0x0d => One(0x00bc),
        1 or 5 => Wide(0x00bc, 0x00bc),
        2 or 6 => Tall(0x00bc, 0x00bc),
        3 or 7 => Square(0x00bc, 0x00bc, 0x00bc, 0x00bc),
        0x0e or 0x0f => One(0x00b6),
        0x82 or 0x83 or 0x84 or 0x85 =>
            new(XrayRevealCodePointers.CopyBrinstar, 0x00b6, 0, 0, 0),
        _ => null,
    };

    /// <summary>Finds the authored shootable-block reveal for one unsigned BTS byte.</summary>
    /// <remarks>
    /// Issue #1033: pinned NTSC J/U v1.0 ROM $91:D3CC..D461 has 16
    /// key/pointer pairs (00..0F), $FFFF at $D40C, and command operands
    /// through $D461. For 00..07, BTS mod 4 chooses one $0052, wide
    /// $0096/$0097, tall $0098/$00B8, or square $0099/$009A/$00B9/$00BA.
    /// BTS 08..09 chooses one $0057; 0A..0F chooses one $009F; all others
    /// return no reveal. Dimension groups are regular, while tile identities
    /// are authored, so the bounded grouped switch remains clearest. Direct
    /// ROM keys/pointers and the independent 256-case shootable-block oracle
    /// including command operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindShootableBlock(byte bts) => bts switch
    {
        0 or 4 => One(0x0052),
        1 or 5 => Wide(0x0096, 0x0097),
        2 or 6 => Tall(0x0098, 0x00b8),
        3 or 7 => Square(0x0099, 0x009a, 0x00b9, 0x00ba),
        8 or 9 => One(0x0057),
        >= 0x0a and <= 0x0f => One(0x009f),
        _ => null,
    };

    /// <summary>Finds the grapple-block reveal for one unsigned BTS byte.</summary>
    /// <remarks>
    /// Issue #1034: pinned NTSC J/U v1.0 ROM $91:D46A..D483 has three
    /// key/pointer pairs (00, 01, 02), $FFFF at $D476, then copy-one
    /// commands with tile $009B for 00 and $00B7 for both 01 and 02.
    /// All other BTS bytes return no reveal. The exact bounded rule is
    /// the 0 versus 1..2 classification already expressed below; tile
    /// identities remain authored. Direct ROM keys/pointers and the
    /// independent 256-case grapple-block oracle including operands agree.
    /// </remarks>
    private static XrayRevealDefinition? FindGrappleBlock(byte bts) => bts switch
    {
        0 => One(0x009b),
        1 or 2 => One(0x00b7),
        _ => null,
    };

    private static XrayRevealDefinition? FindBombableBlock(byte bts) => bts switch
    {
        0 or 4 => One(0x0058),
        1 or 5 => Wide(0x0058, 0x0058),
        2 or 6 => Tall(0x0058, 0x0058),
        3 or 7 => Square(0x0058, 0x0058, 0x0058, 0x0058),
        _ => null,
    };

    private static XrayRevealDefinition One(ushort tile) =>
        new(XrayRevealCodePointers.CopyOne, tile, 0, 0, 0);

    private static XrayRevealDefinition Wide(ushort left, ushort right) =>
        new(XrayRevealCodePointers.CopyWide, left, right, 0, 0);

    private static XrayRevealDefinition Tall(ushort top, ushort bottom) =>
        new(XrayRevealCodePointers.CopyTall, top, 0, bottom, 0);

    private static XrayRevealDefinition Square(
        ushort topLeft,
        ushort topRight,
        ushort bottomLeft,
        ushort bottomRight) =>
        new(XrayRevealCodePointers.CopySquare, topLeft, topRight, bottomLeft, bottomRight);
}
