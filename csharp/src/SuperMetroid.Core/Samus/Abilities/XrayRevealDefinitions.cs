
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Compiled form of the complete revealed-block dispatcher at $91:D2D6-$91:D4D9.
/// Definitions retain native command identities because the tilemap builder uses their
/// distinct copy dimensions and Brinstar-only behavior.
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>Blank metatile selected by negative-coordinate extension exits at $91:CE95/$91:CED7.</summary>
    internal const ushort BlankMetatile = 0x00ff;

    /// <summary>BTS $46 is the only special-air value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1028: pinned NTSC J/U v1.0 ROM $91:D306..D30F contains
    /// ($0046, $D30C), terminal $FFFF, then copy-one command $CF36 and
    /// tile $00FF. Native $91:CE08 compares the zero-extended BTS byte,
    /// so equality with $46 is the complete bounded algorithm over 0..255;
    /// every other BTS returns no reveal. Direct ROM words and all 256
    /// special-air cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    internal const byte SpecialAirRevealBts = 0x46;

    /// <summary>BTS $0E is the only spike-block value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1030: pinned NTSC J/U v1.0 ROM $91:D318..D321 contains
    /// ($000E, $D31E), terminal $FFFF, then copy-one command $CF36 and
    /// tile $005F. Native $91:CE08 compares zero-extended BTS, making
    /// equality with $0E the exact bounded rule over 0..255; other BTS
    /// values return no reveal. Direct ROM words and all 256 spike-block
    /// cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    internal const byte SpikeRevealBts = 0x0e;

    /// <summary>Air reveals one $00FF tile for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1027: pinned NTSC J/U v1.0 ROM $91:D2FC..D305 has the sole
    /// wildcard $FF00 to $D302, then $FFFF; $D302 holds copy-one command
    /// $CF36 and tile $00FF. Native $91:CE08 checks the wildcard before the
    /// unsigned BTS byte, so the exact bounded rule for BTS 0..255 is this
    /// constant definition. Direct ROM words and the independent 256-case
    /// air slice of the X-ray reveal oracle agree; no pointer table is needed.
    /// </remarks>
    internal static readonly XrayRevealDefinition Air = One(0x00ff);
    /// <summary>Horizontal extension command for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1029: pinned NTSC J/U v1.0 ROM $91:D310..D317 has wildcard
    /// $FF00 to $D316, terminal $FFFF, then the operand-free horizontal
    /// extension command $CEBB. Native $91:CE08 checks the wildcard first,
    /// making this constant definition exact for all unsigned BTS 0..255.
    /// Direct ROM words and the independent 256-case horizontal-extension
    /// oracle agree; the neighboring spike table is not part of this lookup.
    /// </remarks>
    internal static readonly XrayRevealDefinition HorizontalExtension =
        new(XrayRevealCommand.HorizontalExtension, 0, 0, 0, 0);
    /// <summary>Vertical extension command for every BTS byte.</summary>
    /// <remarks>
    /// Issue #1031: pinned NTSC J/U v1.0 ROM $91:D462..D469 has wildcard
    /// $FF00 to $D468, terminal $FFFF, then operand-free vertical extension
    /// command $CE79. Native $91:CE08 checks the wildcard first, making
    /// this constant definition exact for all unsigned BTS 0..255.
    /// Direct ROM words and the independent 256-case vertical-extension
    /// oracle agree; adjacent grapple entries do not belong to this lookup.
    /// </remarks>
    internal static readonly XrayRevealDefinition VerticalExtension =
        new(XrayRevealCommand.VerticalExtension, 0, 0, 0, 0);

    /// <summary>The value each key selects; FindSpecialAir looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpecialAirReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [SpecialAirRevealBts] = Air,
    };

    /// <summary>The value each key selects; FindSpikeBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpikeBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [SpikeRevealBts] = One(0x005f),
    };

    /// <summary>The value each key selects; FindSpecialBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpecialBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(0x00bc),
        [4] = One(0x00bc),
        [8] = One(0x00bc),
        [9] = One(0x00bc),
        [0x0a] = One(0x00bc),
        [0x0b] = One(0x00bc),
        [0x0c] = One(0x00bc),
        [0x0d] = One(0x00bc),
        [1] = Wide(0x00bc, 0x00bc),
        [5] = Wide(0x00bc, 0x00bc),
        [2] = Tall(0x00bc, 0x00bc),
        [6] = Tall(0x00bc, 0x00bc),
        [3] = Square(0x00bc, 0x00bc, 0x00bc, 0x00bc),
        [7] = Square(0x00bc, 0x00bc, 0x00bc, 0x00bc),
        [0x0e] = One(0x00b6),
        [0x0f] = One(0x00b6),
        [0x82] = new(XrayRevealCommand.CopyBrinstar, 0x00b6, 0, 0, 0),
        [0x83] = new(XrayRevealCommand.CopyBrinstar, 0x00b6, 0, 0, 0),
        [0x84] = new(XrayRevealCommand.CopyBrinstar, 0x00b6, 0, 0, 0),
        [0x85] = new(XrayRevealCommand.CopyBrinstar, 0x00b6, 0, 0, 0),
    };

    /// <summary>The value each key selects; FindShootableBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> ShootableBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(0x0052),
        [4] = One(0x0052),
        [1] = Wide(0x0096, 0x0097),
        [5] = Wide(0x0096, 0x0097),
        [2] = Tall(0x0098, 0x00b8),
        [6] = Tall(0x0098, 0x00b8),
        [3] = Square(0x0099, 0x009a, 0x00b9, 0x00ba),
        [7] = Square(0x0099, 0x009a, 0x00b9, 0x00ba),
        [8] = One(0x0057),
        [9] = One(0x0057),
        [10] = One(0x009f),
        [11] = One(0x009f),
        [12] = One(0x009f),
        [13] = One(0x009f),
        [14] = One(0x009f),
        [15] = One(0x009f),
    };

    /// <summary>The value each key selects; FindGrappleBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> GrappleBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(0x009b),
        [1] = One(0x00b7),
        [2] = One(0x00b7),
    };

    /// <summary>The value each key selects; FindBombableBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> BombableBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(0x0058),
        [4] = One(0x0058),
        [1] = Wide(0x0058, 0x0058),
        [5] = Wide(0x0058, 0x0058),
        [2] = Tall(0x0058, 0x0058),
        [6] = Tall(0x0058, 0x0058),
        [3] = Square(0x0058, 0x0058, 0x0058, 0x0058),
        [7] = Square(0x0058, 0x0058, 0x0058, 0x0058),
    };

    /// <summary>Copies one authored tile to the X-ray overlay at the inspected block.</summary>
    internal static XrayRevealDefinition One(int tile) =>
        new(XrayRevealCommand.CopyOne, tile, 0, 0, 0);

    /// <summary>Copies a left and right tile across two horizontal overlay cells.</summary>
    internal static XrayRevealDefinition Wide(int left, int right) =>
        new(XrayRevealCommand.CopyWide, left, right, 0, 0);

    /// <summary>Copies a top and bottom tile across two vertical overlay cells.</summary>
    internal static XrayRevealDefinition Tall(int top, int bottom) =>
        new(XrayRevealCommand.CopyTall, top, 0, bottom, 0);

    /// <summary>Copies four authored tiles into a two-by-two overlay block.</summary>
    internal static XrayRevealDefinition Square(
        int topLeft,
        int topRight,
        int bottomLeft,
        int bottomRight) =>
        new(XrayRevealCommand.CopySquare, topLeft, topRight, bottomLeft, bottomRight);
}
