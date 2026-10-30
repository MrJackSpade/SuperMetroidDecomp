using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Equipment-page base artwork and the compiled footprints owned by live menu state.</summary>
public static class PauseEquipmentBaseDefinitions
{
    /// <summary>The supported editable equipment-page base schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded equipment-page base tilemap asset file name.</summary>
    public const string FileName = "pause-equipment-base.json";

    /// <summary>The 32-column, 32-row tilemap geometry and its calculated 1024-cell extent.</summary>
    public const int Columns = 32, Rows = 32, Cells = Columns * Rows;
    /// <summary>$B6:E800 template border artwork: side $140, corner $141, horizontal $142 and title end $143.</summary>
    internal const int PanelSide = 0x140, PanelCorner = 0x141, PanelHorizontal = 0x142, PanelTitleEnd = 0x143;
    /// <summary>$B6:E800 template title fragments: SUPPLY $107, BEAM $F9, SUIT $F6, MISC $1B0 and BOOTS $A0.</summary>
    internal const int SupplyTitle = 0x107, BeamTitle = 0xf9, SuitTitle = 0xf6, MiscTitle = 0x1b0, BootsTitle = 0xa0;
    /// <summary>$B6:E95A-E965: six-character SAMUS title begins at $13A.</summary>
    internal const int SamusTitle = 0x13a;
    /// <summary>$B6:E952-E96D title ornament uses repeated bar $BE and end $BD.</summary>
    internal const int TitleBar = 0xbe, TitleBarEnd = 0xbd;
    /// <summary>$B6:E902-EB02: reserve arrow tip/stem/elbow characters $14C/$15C/$16C/$16F.</summary>
    internal const int ArrowTip = 0x14c, ArrowStem = 0x15c, ArrowTurn = 0x16c, ArrowJoin = 0x16f;
    /// <summary>$B6:EB06-EB0F: repeated empty reserve-gauge segment $FC and endcap $FE.</summary>
    internal const int EmptyGauge = 0xfc, GaugeEnd = 0xfe;

    /// <summary>The five framed sections in the authored equipment-page layout.</summary>
    internal enum PanelKind
    {
        /// <summary>The reserve-tank mode and amount section.</summary>
        Supply,
        /// <summary>The left-column beam equipment section.</summary>
        Beam,
        /// <summary>The upper right-column suit section.</summary>
        Suit,
        /// <summary>The middle right-column miscellaneous equipment section.</summary>
        Misc,
        /// <summary>The lower right-column boot section.</summary>
        Boots,
    }
    /// <summary>Right-column panels whose left border joins the beam panel's guide line.</summary>
    internal static readonly PanelKind[] GuidedPanels = [PanelKind.Suit, PanelKind.Misc, PanelKind.Boots];
    /// <summary>Inclusive frame bounds and title rendering choices for one equipment-page section.</summary>
    [IndependentFlag("the panel text's BG priority bit, a fact of the authored panel")]
    internal readonly record struct PanelShape(int Left, int Top, int Right, int Bottom,
        int TextColumn, int TextWidth, int TextTile, int TextPalette, bool TextPriority);

    /// <summary>$B6:EBC0-EC54: two atlas rows form the diagonal beam guide.</summary>
    internal const int GuideDiagonal = 0x145, AtlasRowStride = 16;
    /// <summary>$B6:EAA8/EBA8/EC52/ED68: border junction for equipment guide strokes.</summary>
    internal const int GuideBorderJunction = 0x154;
}
