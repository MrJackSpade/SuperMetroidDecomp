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
    /// <summary>$82:BF32-C018: all collected label tile words use BG palette two.</summary>
    internal const int LabelPalette = 2;
    /// <summary>Leading inventory marker character $FF before each label's text.</summary>
    internal const int Marker = 0xff;
    /// <summary>$82:BF42 and other label tails use character $D4 as the padded text background.</summary>
    internal const int Padding = 0xd4;
    /// <summary>$82:BF34-BF38: contiguous CHARG fragment begins at atlas $D8.</summary>
    internal const int ChargePrefix = 0xd8;
    /// <summary>CHARGE label's final E is separately stored at atlas $E7.</summary>
    internal const int ChargeEnding = 0xe7;
    /// <summary>$82:BF3E-BF40: ICE at atlas $DB-DC.</summary>
    internal const int IceText = 0xdb;
    /// <summary>$82:BF48-BF4C: WAVE at atlas $DD-DF.</summary>
    internal const int WaveText = 0xdd;
    /// <summary>$82:BF52-BF58: SPAZER at atlas $E8-EB.</summary>
    internal const int SpazerText = 0xe8;
    /// <summary>$82:BF5C-BF62: PLASMA at atlas $EC-EF.</summary>
    internal const int PlasmaText = 0xec;
    /// <summary>$82:BF66-BF6A: VARIA at atlas $100-102, followed by the shared SUIT fragment.</summary>
    internal const int VariaText = 0x100;
    /// <summary>$82:BF78-BF7E: GRAVITY at atlas $D0-D3, followed by the shared SUIT fragment.</summary>
    internal const int GravityText = 0xd0;
    /// <summary>$82:BF6C-BF70 and BF80-BF84: shared SUIT at atlas $103-105.</summary>
    internal const int SuitText = 0x103;
    /// <summary>$82:BF8A-BF90: MORPHIN at atlas $120-123.</summary>
    internal const int MorphingPrefix = 0x120;
    /// <summary>$82:BF92-BF94: G/B continuation at atlas $117-118.</summary>
    internal const int MorphingBallMiddle = 0x117;
    /// <summary>$82:BF96: Morphing Ball's AL fragment begins at atlas $10F.</summary>
    internal const int BallMiddle = 0x10f;
    /// <summary>$82:BF98: final L of Morphing Ball is separately packed at atlas $11F.</summary>
    internal const int BallEnding = 0x11f;
    /// <summary>$82:BF9C-BFA0: BOMBS at atlas $D5-D7.</summary>
    internal const int BombsText = 0xd5;
    /// <summary>$82:BFAE-BFBA: SPRING BALL at atlas $110-116.</summary>
    internal const int SpringBallText = 0x110;
    /// <summary>$82:BFC2-BFCE: SCREW ATTACK at atlas $E0-E6.</summary>
    internal const int ScrewAttackText = 0xe0;
    /// <summary>$82:BFD4-BFE0: HI-JUMP BOOTS at atlas $130-136.</summary>
    internal const int HiJumpText = 0x130;
    /// <summary>$82:BFE6-BFF0: SPACE JUMP at atlas $F0-F5.</summary>
    internal const int SpaceJumpText = 0xf0;
    /// <summary>$82:BFF8-C006: SPEED BOOSTER at atlas $124-12B.</summary>
    internal const int SpeedBoosterText = 0x124;
    /// <summary>$82:C00A-C00E: HYPER text begins at atlas $137.</summary>
    internal const int HyperText = 0x137;
    /// <summary>$82:C010: Hyper label's distinct empty endcap at atlas $12F.</summary>
    internal const int HyperEndcap = 0x12f;
}
