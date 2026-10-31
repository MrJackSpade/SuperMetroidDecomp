
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Compiled form of the complete revealed-block dispatcher at $91:D2D6-$91:D4D9.
/// Definitions retain native command identities because the tilemap builder uses their
/// distinct copy dimensions and Brinstar-only behavior.
/// </summary>
internal static class XrayRevealDefinitions
{
    /// <summary>Blank metatile selected by negative-coordinate extension exits at $91:CE95/$91:CED7.</summary>
    internal const int BlankMetatile = 255;

    /// <summary>Behaviour $46 is the only special-air value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1028: pinned NTSC J/U v1.0 ROM $91:D306..D30F contains
    /// ($0046, $D30C), terminal $FFFF, then copy-one command $CF36 and
    /// tile $00FF. Native $91:CE08 compares the zero-extended behaviour byte,
    /// so equality with $46 is the complete bounded algorithm over 0..255;
    /// every other behaviour returns no reveal. Direct ROM words and all 256
    /// special-air cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    internal const int SpecialAirRevealBehavior = 70;

    /// <summary>Behaviour $0E is the only spike-block value admitted by the native table.</summary>
    /// <remarks>
    /// Issue #1030: pinned NTSC J/U v1.0 ROM $91:D318..D321 contains
    /// ($000E, $D31E), terminal $FFFF, then copy-one command $CF36 and
    /// tile $005F. Native $91:CE08 compares zero-extended behaviour, making
    /// equality with $0E the exact bounded rule over 0..255; other behaviour
    /// values return no reveal. Direct ROM words and all 256 spike-block
    /// cases in the independent X-ray reveal oracle agree.
    /// </remarks>
    internal const int SpikeRevealBehavior = 14;

    /// <summary>Air reveals one $00FF tile for every behaviour byte.</summary>
    /// <remarks>
    /// Issue #1027: pinned NTSC J/U v1.0 ROM $91:D2FC..D305 has the sole
    /// wildcard $FF00 to $D302, then $FFFF; $D302 holds copy-one command
    /// $CF36 and tile $00FF. Native $91:CE08 checks the wildcard before the
    /// unsigned behaviour byte, so the exact bounded rule for behaviour 0..255 is this
    /// constant definition. Direct ROM words and the independent 256-case
    /// air slice of the X-ray reveal oracle agree; no pointer table is needed.
    /// </remarks>
    internal static readonly XrayRevealDefinition Air = One(255);
    /// <summary>Horizontal extension command for every behaviour byte.</summary>
    /// <remarks>
    /// Issue #1029: pinned NTSC J/U v1.0 ROM $91:D310..D317 has wildcard
    /// $FF00 to $D316, terminal $FFFF, then the operand-free horizontal
    /// extension command $CEBB. Native $91:CE08 checks the wildcard first,
    /// making this constant definition exact for all unsigned behaviour 0..255.
    /// Direct ROM words and the independent 256-case horizontal-extension
    /// oracle agree; the neighboring spike table is not part of this lookup.
    /// </remarks>
    internal static readonly XrayRevealDefinition HorizontalExtension =
        new(XrayRevealCommand.HorizontalExtension, 0, 0, 0, 0);
    /// <summary>Vertical extension command for every behaviour byte.</summary>
    /// <remarks>
    /// Issue #1031: pinned NTSC J/U v1.0 ROM $91:D462..D469 has wildcard
    /// $FF00 to $D468, terminal $FFFF, then operand-free vertical extension
    /// command $CE79. Native $91:CE08 checks the wildcard first, making
    /// this constant definition exact for all unsigned behaviour 0..255.
    /// Direct ROM words and the independent 256-case vertical-extension
    /// oracle agree; adjacent grapple entries do not belong to this lookup.
    /// </remarks>
    internal static readonly XrayRevealDefinition VerticalExtension =
        new(XrayRevealCommand.VerticalExtension, 0, 0, 0, 0);

    /// <summary>The value each key selects; FindSpecialAir looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpecialAirReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [SpecialAirRevealBehavior] = Air,
    };

    /// <summary>The value each key selects; FindSpikeBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpikeBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [SpikeRevealBehavior] = One(95),
    };

    /// <summary>The value each key selects; FindSpecialBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> SpecialBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(188),
        [4] = One(188),
        [8] = One(188),
        [9] = One(188),
        [10] = One(188),
        [11] = One(188),
        [12] = One(188),
        [13] = One(188),
        [1] = Wide(188, 188),
        [5] = Wide(188, 188),
        [2] = Tall(188, 188),
        [6] = Tall(188, 188),
        [3] = Square(188, 188, 188, 188),
        [7] = Square(188, 188, 188, 188),
        [14] = One(182),
        [15] = One(182),
        [130] = new(XrayRevealCommand.CopyBrinstar, 182, 0, 0, 0),
        [131] = new(XrayRevealCommand.CopyBrinstar, 182, 0, 0, 0),
        [132] = new(XrayRevealCommand.CopyBrinstar, 182, 0, 0, 0),
        [133] = new(XrayRevealCommand.CopyBrinstar, 182, 0, 0, 0),
    };

    /// <summary>The value each key selects; FindShootableBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> ShootableBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(82),
        [4] = One(82),
        [1] = Wide(150, 151),
        [5] = Wide(150, 151),
        [2] = Tall(152, 184),
        [6] = Tall(152, 184),
        [3] = Square(153, 154, 185, 186),
        [7] = Square(153, 154, 185, 186),
        [8] = One(87),
        [9] = One(87),
        [10] = One(159),
        [11] = One(159),
        [12] = One(159),
        [13] = One(159),
        [14] = One(159),
        [15] = One(159),
    };

    /// <summary>The value each key selects; FindGrappleBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> GrappleBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(155),
        [1] = One(183),
        [2] = One(183),
    };

    /// <summary>The value each key selects; FindBombableBlock looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, XrayRevealDefinition?> BombableBlockReveals = new Dictionary<int, XrayRevealDefinition?>
    {
        [0] = One(88),
        [4] = One(88),
        [1] = Wide(88, 88),
        [5] = Wide(88, 88),
        [2] = Tall(88, 88),
        [6] = Tall(88, 88),
        [3] = Square(88, 88, 88, 88),
        [7] = Square(88, 88, 88, 88),
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
