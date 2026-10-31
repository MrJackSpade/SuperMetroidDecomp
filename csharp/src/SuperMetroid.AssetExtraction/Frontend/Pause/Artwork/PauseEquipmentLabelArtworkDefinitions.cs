namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The editable equipment label document's schema and the pause atlas tiles the labels are pictured from (#1467); the
/// labels' identities and widths, which the game reads, are <c>PauseEquipmentLabelDefinitions</c>.
/// </summary>
public static class PauseEquipmentLabelArtworkDefinitions
{
    /// <summary>Supported installed label schema version, requiring fourteen ordinary inventory labels, the Hyper label, and a nine-cell blank strip.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for editable equipment-page label artwork, destinations, and the unequipped-label palette selector.</summary>
    public const string FileName = "pause-equipment-labels.json";
    /// <summary>Equipment-page tilemap height in eight-pixel BG tile rows; complete label application requires all 32 rows.</summary>
    public const int TilemapRows = 32;
    /// <summary>Stock BG palette selector three for collected but unequipped labels, replacing only tile-word palette bits and leaving the artwork, flips, and priority intact.</summary>
    public const int DisabledPalette = 3;
    /// <summary>The equipment-page cell the Plasma label starts at, $284.</summary>
    public const int PlasmaCell = 644;
    /// <summary>The ninth Boots-length Plasma word is subsequently owned by the wireframe at cell $28C.</summary>
    public const int PlasmaWireframeOverlapCell = 652;
    /// <summary>$82:BF32-C018: all collected label tile words use BG palette two.</summary>
    internal const int LabelPalette = 2;
    /// <summary>Leading inventory marker character $FF before each label's text.</summary>
    internal const int Marker = 255;
    /// <summary>$82:BF42 and other label tails use character $D4 as the padded text background.</summary>
    internal const int Padding = 212;
    /// <summary>$82:BF34-BF38: contiguous CHARG fragment begins at atlas $D8.</summary>
    internal const int ChargePrefix = 216;
    /// <summary>CHARGE label's final E is separately stored at atlas $E7.</summary>
    internal const int ChargeEnding = 231;
    /// <summary>$82:BF3E-BF40: ICE at atlas $DB-DC.</summary>
    internal const int IceText = 219;
    /// <summary>$82:BF48-BF4C: WAVE at atlas $DD-DF.</summary>
    internal const int WaveText = 221;
    /// <summary>$82:BF52-BF58: SPAZER at atlas $E8-EB.</summary>
    internal const int SpazerText = 232;
    /// <summary>$82:BF5C-BF62: PLASMA at atlas $EC-EF.</summary>
    internal const int PlasmaText = 236;
    /// <summary>$82:BF66-BF6A: VARIA at atlas $100-102, followed by the shared SUIT fragment.</summary>
    internal const int VariaText = 256;
    /// <summary>$82:BF78-BF7E: GRAVITY at atlas $D0-D3, followed by the shared SUIT fragment.</summary>
    internal const int GravityText = 208;
    /// <summary>$82:BF6C-BF70 and BF80-BF84: shared SUIT at atlas $103-105.</summary>
    internal const int SuitText = 259;
    /// <summary>$82:BF8A-BF90: MORPHIN at atlas $120-123.</summary>
    internal const int MorphingPrefix = 288;
    /// <summary>$82:BF92-BF94: G/B continuation at atlas $117-118.</summary>
    internal const int MorphingBallMiddle = 279;
    /// <summary>$82:BF96: Morphing Ball's AL fragment begins at atlas $10F.</summary>
    internal const int BallMiddle = 271;
    /// <summary>$82:BF98: final L of Morphing Ball is separately packed at atlas $11F.</summary>
    internal const int BallEnding = 287;
    /// <summary>$82:BF9C-BFA0: BOMBS at atlas $D5-D7.</summary>
    internal const int BombsText = 213;
    /// <summary>$82:BFAE-BFBA: SPRING BALL at atlas $110-116.</summary>
    internal const int SpringBallText = 272;
    /// <summary>$82:BFC2-BFCE: SCREW ATTACK at atlas $E0-E6.</summary>
    internal const int ScrewAttackText = 224;
    /// <summary>$82:BFD4-BFE0: HI-JUMP BOOTS at atlas $130-136.</summary>
    internal const int HiJumpText = 304;
    /// <summary>$82:BFE6-BFF0: SPACE JUMP at atlas $F0-F5.</summary>
    internal const int SpaceJumpText = 240;
    /// <summary>$82:BFF8-C006: SPEED BOOSTER at atlas $124-12B.</summary>
    internal const int SpeedBoosterText = 292;
    /// <summary>$82:C00A-C00E: HYPER text begins at atlas $137.</summary>
    internal const int HyperText = 311;
    /// <summary>$82:C010: Hyper label's distinct empty endcap at atlas $12F.</summary>
    internal const int HyperEndcap = 303;
}
