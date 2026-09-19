namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Compiled form of the complete revealed-block dispatcher at $91:D2D6-$91:D4D9.
/// Definitions retain native command identities because the tilemap builder uses their
/// distinct copy dimensions and Brinstar-only behavior.
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>BTS $46 is the only special-air value admitted by the native table.</summary>
    private const byte SpecialAirRevealBts = 0x46;

    /// <summary>BTS $0E is the only spike-block value admitted by the native table.</summary>
    private const byte SpikeRevealBts = 0x0e;

    private static readonly XrayRevealDefinition Air = One(0x00ff);
    private static readonly XrayRevealDefinition HorizontalExtension =
        new(XrayRevealCodePointers.HorizontalExtension, 0, 0, 0, 0);
    private static readonly XrayRevealDefinition VerticalExtension =
        new(XrayRevealCodePointers.VerticalExtension, 0, 0, 0, 0);

    /// <summary>Returns the cartridge-authored reveal for one collision type/BTS pair.</summary>
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
