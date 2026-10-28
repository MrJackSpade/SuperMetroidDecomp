using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

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

    /// <summary>The JSON key of the Hyper Beam label; separate from ordinary beam controls and placed at the Wave slot by the native Hyper pointer table.</summary>
    public const string HyperKey = "Beam.Hyper";
    /// <summary>Hyper mode's pointer table places its only nonblank patch in the Wave slot.</summary>
    public const int HyperBeamItem = 2;
    /// <summary>The equipment-page cell the Plasma label starts at, $284.</summary>
    public const int PlasmaCell = 0x284;
    /// <summary>The ninth Boots-length Plasma word is subsequently owned by the wireframe at cell $28C.</summary>
    public const int PlasmaWireframeOverlapCell = 0x28c;

    /// <summary>Equipment label identities alias the corresponding selector controls. Native
    /// 82:C08C..C0A6 selects Charge/Ice/Wave/Spazer/Plasma, suit/misc and boot labels in
    /// the same category/item order. Reserve controls have no ordinary inventory label.</summary>
    public static PauseEquipmentLabelId Label(PauseEquipmentCategory category, int item)
    {
        if (!Enum.IsDefined(category) || (uint)item >= ItemCount(category))
            throw new ArgumentOutOfRangeException(nameof(item), $"No pause equipment label exists for category {category}, item {item}.");
        return category switch
        {
            PauseEquipmentCategory.Beams => item switch
            {
                0 => PauseEquipmentLabelId.Charge,
                1 => PauseEquipmentLabelId.Ice,
                2 => PauseEquipmentLabelId.Wave,
                3 => PauseEquipmentLabelId.Spazer,
                4 => PauseEquipmentLabelId.Plasma,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Suits => item switch
            {
                0 => PauseEquipmentLabelId.Varia,
                1 => PauseEquipmentLabelId.Gravity,
                2 => PauseEquipmentLabelId.MorphBall,
                3 => PauseEquipmentLabelId.Bombs,
                4 => PauseEquipmentLabelId.SpringBall,
                5 => PauseEquipmentLabelId.ScrewAttack,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Boots => item switch
            {
                0 => PauseEquipmentLabelId.HiJump,
                1 => PauseEquipmentLabelId.SpaceJump,
                2 => PauseEquipmentLabelId.SpeedBooster,
                _ => throw new ArgumentOutOfRangeException(nameof(item)),
            },
            PauseEquipmentCategory.Reserves => throw new ArgumentOutOfRangeException(nameof(category)),
            _ => throw new ArgumentOutOfRangeException(nameof(category)),
        };
    }

    /// <summary>The JSON key of an ordinary label, which is its selector control's anchor name.</summary>
    public static string Key(PauseEquipmentCategory category, int item) => Name(Label(category, item));

    /// <summary>The JSON key of a label: its selector control's anchor name, or <see cref="HyperKey"/>.</summary>
    public static string Name(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) return HyperKey;
        PauseEquipmentLabelKey entry = Labels().Single(candidate => candidate.Label == label);
        return PauseSelectorDefinitions.Anchor(entry.Category, entry.Item);
    }

    /// <summary>Whether a label is one of the five ordinary beam labels.</summary>
    internal static bool IsOrdinaryBeam(PauseEquipmentLabelId label) => label is PauseEquipmentLabelId.Charge or
        PauseEquipmentLabelId.Ice or PauseEquipmentLabelId.Wave or PauseEquipmentLabelId.Spazer or PauseEquipmentLabelId.Plasma;

    /// <summary>Uses the reviewed category contract, including zero labels for reserves.</summary>
    public static int ItemCount(PauseEquipmentCategory category) => PauseEquipmentCategories.Get(category).ItemCount;

    /// <summary>The ordinary label view of the selector anchors, derived once rather than per lookup.</summary>
    public static IReadOnlyList<PauseEquipmentLabelKey> Labels() => labels.Value;

    /// <summary>Non-reserve selector anchors projected once into the fourteen ordinary inventory label identities.</summary>
    private static readonly Lazy<PauseEquipmentLabelKey[]> labels = new(() =>
        PauseSelectorDefinitions.Anchors()
            .Where(anchor => anchor.Category != PauseEquipmentCategory.Reserves)
            .Select(anchor => new PauseEquipmentLabelKey(anchor.Category, anchor.Item, Label(anchor.Category, anchor.Item))).ToArray());

    /// <summary>Returns the ordinary category strip width in sixteen-bit tile words, not bytes; Hyper's stored width and Plasma's button-patch overrun are separate contracts.</summary>
    /// <param name="category">Native equipment category: one denotes beams; every other integer returns the equipment width without category validation.</param>
    /// <returns>Five for the beam category, otherwise nine.</returns>
    public static int WordCount(PauseEquipmentCategory category) => category == PauseEquipmentCategory.Beams ? BeamWords : EquipmentWords;
    /// <summary>$82:BF32-C018: all collected label tile words use BG palette two.</summary>
    private const int LabelPalette = 2;
    /// <summary>Leading inventory marker character $FF before each label's text.</summary>
    private const int Marker = 0xff;
    /// <summary>$82:BF42 and other label tails use character $D4 as the padded text background.</summary>
    private const int Padding = 0xd4;
    /// <summary>$82:BF34-BF38: contiguous CHARG fragment begins at atlas $D8.</summary>
    private const int ChargePrefix = 0xd8;
    /// <summary>CHARGE label's final E is separately stored at atlas $E7.</summary>
    private const int ChargeEnding = 0xe7;
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
    /// <summary>$82:BF96: Morphing Ball's AL fragment begins at atlas $10F.</summary>
    private const int BallMiddle = 0x10f;
    /// <summary>$82:BF98: final L of Morphing Ball is separately packed at atlas $11F.</summary>
    private const int BallEnding = 0x11f;
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
    /// <summary>$82:C00A-C00E: HYPER text begins at atlas $137.</summary>
    private const int HyperText = 0x137;
    /// <summary>$82:C010: Hyper label's distinct empty endcap at atlas $12F.</summary>
    private const int HyperEndcap = 0x12f;

    /// <summary>Uses five source words for ordinary beams and nine for other labels, including stored Hyper.</summary>
    internal static int StockWordCount(PauseEquipmentLabelId label)
    {
        _ = StockPageByte(label);
        return IsOrdinaryBeam(label) ? BeamWords : EquipmentWords;
    }

    /// <summary>$82:C06C-C086: label columns4/21 and category rows16/9/19, with the suit-to-misc gap.</summary>
    internal static int StockPageByte(PauseEquipmentLabelId label)
    {
        if (label == PauseEquipmentLabelId.Hyper) label = Label(PauseEquipmentCategory.Beams, HyperBeamItem);
        return Ensure.Listed(destinations.Value, label);
    }

    /// <summary>Native label destinations derived once from category columns and rows for per-cell lookup.</summary>
    private static readonly Lazy<Dictionary<PauseEquipmentLabelId, int>> destinations = new(() =>
        Labels().ToDictionary(label => label.Label, label =>
        {
            Position16<TilemapCells> first = label.Category switch
            {
                PauseEquipmentCategory.Beams => new Position16<TilemapCells>(4, 16),
                PauseEquipmentCategory.Suits => new Position16<TilemapCells>(21, checked((ushort)(9 + (label.Item >= 2 ? 2 : 0)))),
                PauseEquipmentCategory.Boots => new Position16<TilemapCells>(21, 19),
                PauseEquipmentCategory.Reserves => throw new InvalidDataException("Unknown equipment label category."),
                _ => throw new InvalidDataException("Unknown equipment label category."),
            };
            return ((first.Y + label.Item) * TilemapColumns + first.X) * sizeof(ushort);
        }));

    /// <summary>Assembles named equipment text from its packed atlas runs, shared suffixes and padding.</summary>
    /// <remarks>The referenced glyph pixels remain independently authored interface artwork.</remarks>
    internal static BackgroundTile StockTile(PauseEquipmentLabelId label, int index)
    {
        if ((uint)index >= StockWordCount(label)) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return new BackgroundTile(Marker, LabelPalette, highPriority: false);
        int text = index - 1;
        int glyph = label switch
        {
            PauseEquipmentLabelId.Charge => text < 3 ? ChargePrefix + text : ChargeEnding,
            PauseEquipmentLabelId.Ice => Run(IceText, 2, text),
            PauseEquipmentLabelId.Wave => Run(WaveText, 3, text),
            PauseEquipmentLabelId.Spazer => SpazerText + text,
            PauseEquipmentLabelId.Plasma => PlasmaText + text,
            PauseEquipmentLabelId.Varia => text < 3 ? VariaText + text : Run(SuitText, 3, text - 3),
            PauseEquipmentLabelId.Gravity => text < 4 ? GravityText + text : Run(SuitText, 3, text - 4),
            PauseEquipmentLabelId.MorphBall => text < 4 ? MorphingPrefix + text
                : text < 6 ? MorphingBallMiddle + text - 4 : text == 6 ? BallMiddle : BallEnding,
            PauseEquipmentLabelId.Bombs => Run(BombsText, 3, text),
            PauseEquipmentLabelId.SpringBall => Run(SpringBallText, 7, text),
            PauseEquipmentLabelId.ScrewAttack => Run(ScrewAttackText, 7, text),
            PauseEquipmentLabelId.HiJump => Run(HiJumpText, 7, text),
            PauseEquipmentLabelId.SpaceJump => Run(SpaceJumpText, 6, text),
            PauseEquipmentLabelId.SpeedBooster => SpeedBoosterText + text,
            PauseEquipmentLabelId.Hyper => text < 3 ? HyperText + text : text == 3 ? HyperEndcap : Padding,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        return new BackgroundTile(glyph, LabelPalette, highPriority: false);
    }

    /// <summary>Advances through a contiguous glyph run, then pads the remaining strip with the native background tile.</summary>
    private static int Run(int first, int length, int index) => index < length ? first + index : Padding;
}
