namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>The pause equipment labels' page layout and widths the game reads; their artwork is the import's (#1467).</summary>
public static class PauseEquipmentLabelDefinitions
{
    /// <summary>Equipment-page tilemap width in eight-pixel BG tile cells; each row contains 32 little-endian sixteen-bit tile words.</summary>
    public const int TilemapColumns = 32;
    /// <summary>Five tile words in an ordinary beam label and in the displayed Hyper patch, including the leading inventory marker cell.</summary>
    public const int BeamWords = 5;
    /// <summary>Nine tile words in suit, miscellaneous equipment, boot, blank, and stored Hyper strips; also the button-handler overrun width for Plasma.</summary>
    public const int EquipmentWords = 9;

    /// <summary>Hyper mode's pointer table places its only nonblank patch in the Wave slot.</summary>
    public const int HyperBeamItem = 2;
}
