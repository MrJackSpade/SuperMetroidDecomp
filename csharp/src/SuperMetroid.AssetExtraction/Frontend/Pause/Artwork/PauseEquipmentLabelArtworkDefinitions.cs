namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The editable equipment label document's schema and the cartridge's layout of the label strips on the equipment page
/// (#1467, #1515). The game places each label's picture whole and knows none of this.
/// </summary>
public static class PauseEquipmentLabelArtworkDefinitions
{
    /// <summary>Equipment-page tilemap width in eight-pixel tile cells; each row contains 32 little-endian sixteen-bit tile words.</summary>
    public const int TilemapColumns = 32;
    /// <summary>Five tile words in an ordinary beam label and in the displayed Hyper patch, including the leading inventory marker cell.</summary>
    public const int BeamWords = 5;
    /// <summary>Nine tile words in suit, miscellaneous equipment, boot, blank, and stored Hyper strips.</summary>
    public const int EquipmentWords = 9;
    /// <summary>The beam item whose place the cartridge's Hyper label table gives Hyper's only nonblank strip: Wave's.</summary>
    public const int HyperBeamItem = 2;
    /// <summary>Supported installed label schema version, requiring fourteen ordinary inventory labels, the Hyper label, and a nine-cell blank strip.</summary>
    public const int Version = 1;
    /// <summary>Installed JSON filename for editable equipment-page label artwork, destinations, and the unequipped-label palette selector.</summary>
    public const string FileName = "pause-equipment-labels.json";
    /// <summary>The JSON key of the Hyper Beam label; separate from ordinary beam controls and placed at the Wave slot by the game's Hyper label selection.</summary>
    public const string HyperKey = "Beam.Hyper";
    /// <summary>Equipment-page tilemap height in eight-pixel BG tile rows; complete label application requires all 32 rows.</summary>
    public const int TilemapRows = 32;
    /// <summary>Stock BG palette selector three for collected but unequipped labels, replacing only tile-word palette bits and leaving the artwork, flips, and priority intact.</summary>
    public const int DisabledPalette = 3;
}
