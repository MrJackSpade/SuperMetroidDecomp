using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Core.Assets;

/// <summary>Semantic identities and native extraction sources for pause equipment labels.</summary>
public static class PauseEquipmentLabelDefinitions
{
    /// <summary>Supported installed label schema version, requiring fourteen ordinary inventory labels, the Hyper label, and a nine-cell blank strip.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for editable equipment-page label artwork, destinations, and the unequipped-label palette selector.</summary>
    public const string FileName = "pause-equipment-labels.json";
    /// <summary>Equipment-page tilemap width in eight-pixel BG tile cells; each row contains 32 little-endian sixteen-bit tile words.</summary>
    public const int TilemapColumns = 32;
    /// <summary>Equipment-page tilemap height in eight-pixel BG tile rows; complete label application requires all 32 rows.</summary>
    public const int TilemapRows = 32;
    /// <summary>Five tile words in an ordinary beam label and in the displayed Hyper patch, including the leading inventory marker cell.</summary>
    public const int BeamWords = 5;
    /// <summary>Nine tile words in suit, miscellaneous equipment, boot, blank, and stored Hyper strips; also the native button-handler overrun width for Plasma.</summary>
    public const int EquipmentWords = 9;
    /// <summary>Stock BG palette selector three for collected but unequipped labels, replacing only tile-word palette bits and leaving the artwork, flips, and priority intact.</summary>
    public const int DisabledPalette = 3;

    /// <summary><c>kEquipmentScreenTilemap_Blank</c> at $82:C01A.</summary>
    public const ushort BlankSource = 0xc01a;

    /// <summary><c>kHyperBeamWeaponsTilemaps</c> at $82:C0A8.</summary>
    public const int HyperPointerTable = 0x82c0a8;

    /// <summary>Case-sensitive installed label identity for Hyper Beam; separate from ordinary beam controls and placed at the Wave slot by the native Hyper pointer table.</summary>
    public const string HyperKey = "Beam.Hyper";
    /// <summary>Case-sensitive Plasma label identity; its ordinary five-word strip may extend into the following native Varia source words during a nine-word button patch.</summary>
    public const string PlasmaKey = "Beam.Plasma";
    /// <summary>Case-sensitive Varia Suit label identity; its first four words also supply the deliberate continuation after Plasma's ordinary five-word source.</summary>
    public const string VariaKey = "Equipment.Varia";
    /// <summary>Hyper mode's pointer table places its only nonblank patch in the Wave slot.</summary>
    public const int HyperBeamItem = 2;
    /// <summary>Native Plasma label destination at equipment-page cell $284.</summary>
    public const int NativePlasmaDestinationCell = 0x284;
    /// <summary>The ninth Boots-length Plasma word is subsequently owned by the wireframe at cell $28C.</summary>
    public const int NativePlasmaWireframeOverlapCell = 0x28c;

    /// <summary>Equipment label identities alias the corresponding selector controls. Native
    /// 82:C08C..C0A6 selects Charge/Ice/Wave/Spazer/Plasma, suit/misc and boot labels in
    /// the same category/item order. Reserve controls have no ordinary inventory label.</summary>
    public static string Key(PauseEquipmentCategory category, int item)
    {
        if (!Enum.IsDefined(category) || (uint)item >= ItemCount(category))
            throw new ArgumentOutOfRangeException(nameof(item), $"No pause equipment label exists for category {category}, item {item}.");
        return PauseSelectorDefinitions.Anchor(category, item);
    }

    /// <summary>Uses the reviewed category contract, including zero labels for reserves.</summary>
    public static int ItemCount(PauseEquipmentCategory category) => PauseEquipmentCategories.Get(category).ItemCount;

    /// <summary>The ordinary label view of the selector anchors, derived once rather than per lookup.</summary>
    public static IReadOnlyList<(PauseEquipmentCategory Category, int Item, string Key)> Labels() => labels.Value;

    private static readonly Lazy<(PauseEquipmentCategory Category, int Item, string Key)[]> labels = new(() =>
        PauseSelectorDefinitions.Anchors()
            .Where(anchor => anchor.Category != PauseEquipmentCategory.Reserves)
            .Select(anchor => (anchor.Category, anchor.Item, anchor.Name)).ToArray());

    /// <summary>Returns the ordinary category strip width in sixteen-bit tile words, not bytes; Hyper's stored width and Plasma's button-patch overrun are separate contracts.</summary>
    /// <param name="category">Native equipment category: one denotes beams; every other integer returns the equipment width without category validation.</param>
    /// <returns>Five for the beam category, otherwise nine.</returns>
    public static int WordCount(PauseEquipmentCategory category) => category == PauseEquipmentCategory.Beams ? BeamWords : EquipmentWords;
    /// <summary>$82:BF32-C018: all collected labels use BG palette two and the marker character $FF.</summary>
    private const int LabelPalette = 2, Marker = 0xff;
    /// <summary>$82:BF42 and other label tails use character $D4 as the padded text background.</summary>
    private const int Padding = 0xd4;
    /// <summary>$82:BF34-BF38: contiguous CHARG fragment at atlas $D8-DA; E is stored at $E7.</summary>
    private const int ChargePrefix = 0xd8, ChargeEnding = 0xe7;
    /// <summary>$82:BF3E-BF40: ICE at atlas $DB-DC.</summary>
    private const int IceText = 0xdb;
    /// <summary>$82:BF48-BF4C: WAVE at atlas $DD-DF.</summary>
    private const int WaveText = 0xdd;
    /// <summary>$82:BF52-BF58: SPAZER at atlas $E8-EB.</summary>
    private const int SpazerText = 0xe8;
    /// <summary>$82:BF5C-BF62: PLASMA at atlas $EC-EF.</summary>
    private const int PlasmaText = 0xec;
    /// <summary>$82:BF66-BF6A: VARIA at atlas $100-102, followed by the shared SUIT fragment.</summary>
    private const int VariaText = 0x100;
    /// <summary>$82:BF78-BF7E: GRAVITY at atlas $D0-D3, followed by the shared SUIT fragment.</summary>
    private const int GravityText = 0xd0;
    /// <summary>$82:BF6C-BF70 and BF80-BF84: shared SUIT at atlas $103-105.</summary>
    private const int SuitText = 0x103;
    /// <summary>$82:BF8A-BF90: MORPHIN at atlas $120-123.</summary>
    private const int MorphingPrefix = 0x120;
    /// <summary>$82:BF92-BF94: G/B continuation at atlas $117-118.</summary>
    private const int MorphingBallMiddle = 0x117;
    /// <summary>$82:BF96-BF98: AL and final L fragments at atlas $10F/$11F.</summary>
    private const int BallMiddle = 0x10f, BallEnding = 0x11f;
    /// <summary>$82:BF9C-BFA0: BOMBS at atlas $D5-D7.</summary>
    private const int BombsText = 0xd5;
    /// <summary>$82:BFAE-BFBA: SPRING BALL at atlas $110-116.</summary>
    private const int SpringBallText = 0x110;
    /// <summary>$82:BFC2-BFCE: SCREW ATTACK at atlas $E0-E6.</summary>
    private const int ScrewAttackText = 0xe0;
    /// <summary>$82:BFD4-BFE0: HI-JUMP BOOTS at atlas $130-136.</summary>
    private const int HiJumpText = 0x130;
    /// <summary>$82:BFE6-BFF0: SPACE JUMP at atlas $F0-F5.</summary>
    private const int SpaceJumpText = 0xf0;
    /// <summary>$82:BFF8-C006: SPEED BOOSTER at atlas $124-12B.</summary>
    private const int SpeedBoosterText = 0x124;
    /// <summary>$82:C00A-C010: HYPER at atlas $137-139 with its distinct empty endcap at $12F.</summary>
    private const int HyperText = 0x137, HyperEndcap = 0x12f;

    internal static int StockWordCount(string key)
    {
        _ = StockDestinationByte(key);
        return key.StartsWith("Beam.", StringComparison.Ordinal) && key != HyperKey ? BeamWords : EquipmentWords;
    }

    /// <summary>$82:C06C-C086: label columns4/21 and category rows16/9/19, with the suit-to-misc gap.</summary>
    internal static int StockDestinationByte(string key)
    {
        if (key == HyperKey) key = Key(PauseEquipmentCategory.Beams, HyperBeamItem);
        return destinations.Value.TryGetValue(key, out int destination) ? destination
            : throw new ArgumentOutOfRangeException(nameof(key));
    }

    // Every label's destination by the rule above, derived once: lookups run per tilemap cell.
    private static readonly Lazy<Dictionary<string, int>> destinations = new(() =>
        Labels().ToDictionary(label => label.Key, label =>
        {
            (int column, int firstRow) = label.Category switch
            {
                PauseEquipmentCategory.Beams => (4, 16),
                PauseEquipmentCategory.Suits => (21, 9 + (label.Item >= 2 ? 2 : 0)),
                PauseEquipmentCategory.Boots => (21, 19),
                _ => throw new InvalidDataException("Unknown equipment label category."),
            };
            return ((firstRow + label.Item) * TilemapColumns + column) * sizeof(ushort);
        }, StringComparer.Ordinal));

    /// <summary>Assembles named equipment text from its packed atlas runs, shared suffixes and padding.</summary>
    /// <remarks>The referenced glyph pixels remain independently authored interface artwork.</remarks>
    internal static ushort StockWord(string key, int index)
    {
        if ((uint)index >= StockWordCount(key)) throw new IndexOutOfRangeException();
        if (index == 0) return (ushort)(LabelPalette << 10 | Marker);
        int text = index - 1;
        int glyph = key switch
        {
            "Beam.Charge" => text < 3 ? ChargePrefix + text : ChargeEnding,
            "Beam.Ice" => Run(IceText, 2, text),
            "Beam.Wave" => Run(WaveText, 3, text),
            "Beam.Spazer" => SpazerText + text,
            PlasmaKey => PlasmaText + text,
            VariaKey => text < 3 ? VariaText + text : Run(SuitText, 3, text - 3),
            "Equipment.Gravity" => text < 4 ? GravityText + text : Run(SuitText, 3, text - 4),
            "Equipment.MorphBall" => text < 4 ? MorphingPrefix + text
                : text < 6 ? MorphingBallMiddle + text - 4 : text == 6 ? BallMiddle : BallEnding,
            "Equipment.Bombs" => Run(BombsText, 3, text),
            "Equipment.SpringBall" => Run(SpringBallText, 7, text),
            "Equipment.ScrewAttack" => Run(ScrewAttackText, 7, text),
            "Boots.HiJump" => Run(HiJumpText, 7, text),
            "Boots.SpaceJump" => Run(SpaceJumpText, 6, text),
            "Boots.SpeedBooster" => SpeedBoosterText + text,
            HyperKey => text < 3 ? HyperText + text : text == 3 ? HyperEndcap : Padding,
            _ => throw new ArgumentOutOfRangeException(nameof(key)),
        };
        return (ushort)(LabelPalette << 10 | glyph);
    }

    private static int Run(int first, int length, int index) => index < length ? first + index : Padding;
}
