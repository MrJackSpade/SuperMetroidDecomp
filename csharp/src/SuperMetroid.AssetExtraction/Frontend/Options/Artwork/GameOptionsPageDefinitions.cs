using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Options.Artwork;

/// <summary>
/// The options pages as the cartridge lays them out (#1467): their cells, the controller labels' boxes, the
/// language-label rows and toggle choice boxes the cartridge recolours, and the palettes it recolours them with. Only the import reads them; it pictures each page, label, language highlight and toggle choice once, and the
/// game places those pictures.
/// </summary>
public static class GameOptionsPageDefinitions
{
    /// <summary>Cells in each options-page row.</summary>
    public const int MenuTilemapWidth = 32;
    /// <summary>Rows in each options page, including rows outside the visible screen.</summary>
    public const int MenuTilemapHeight = 32;
    /// <summary>Number of row-major tile cells in each 32-by-32 options page.</summary>
    public const int PageCellCount = MenuTilemapWidth * MenuTilemapHeight;
    /// <summary>Height in characters of the atlas addressed by authored background cells.</summary>
    public const int CharacterRows = 32;
    /// <summary>Width in tile cells of a controller-button label patch: three.</summary>
    public const int ControllerLabelWidth = ControllerLabels.WidthInTiles;
    /// <summary>Height in tile cells of a controller-button label patch: two.</summary>
    public const int ControllerLabelHeight = ControllerLabels.HeightInTiles;
    /// <summary>Six row-major tile cells required for each controller-button glyph patch.</summary>
    public const int ControllerLabelCellCount = ControllerLabelWidth * ControllerLabelHeight;
    /// <summary>Size of one controller-label glyph patch in tile cells.</summary>
    public static Extent16<CharacterCells> ControllerLabelSize => new(ControllerLabelWidth, ControllerLabelHeight);

    /// <summary>The controller page's button-label boxes.</summary>
    public static class ControllerLabels
    {
        /// <summary>Columns of one button label.</summary>
        public const int WidthInTiles = 3;
        /// <summary>Rows of one button label.</summary>
        public const int HeightInTiles = 2;
        /// <summary>The column every label's upper-left cell is in.</summary>
        public const int AnchorColumn = 23;
        /// <summary>The row of the first action's label.</summary>
        public const int FirstAnchorRow = 5;
        /// <summary>Rows between consecutive actions' labels.</summary>
        public const int AnchorRowSpacing = 3;
    }

    /// <summary>The primary page's language-label rows, whose palette shows the active language.</summary>
    public static class LanguageRegions
    {
        /// <summary>The page byte the English label's first row starts at.</summary>
        public const int FirstByteOffset = 648;
        /// <summary>Bytes of each English label row.</summary>
        public const int EnglishByteCount = 24;
        /// <summary>Bytes of each Japanese label row.</summary>
        public const int JapaneseByteCount = 50;
        /// <summary>Rows between the English and Japanese labels.</summary>
        public const int LanguageRowSpacing = 3;
    }

    /// <summary>The Icon Cancel and Moonwalk toggles' choice boxes.</summary>
    public static class SpecialToggles
    {
        /// <summary>Bytes recoloured in each choice row: six cells, palette bits only.</summary>
        public const int PaletteRegionByteCount = 12;
        /// <summary>The page byte Icon Cancel's enabled choice starts at.</summary>
        public const int FirstByteOffset = 480;
        /// <summary>Tilemap rows between the two toggles.</summary>
        public const int SettingRowSpacing = 6;
        /// <summary>Cells between a toggle's enabled and disabled choices.</summary>
        public const int ChoiceCellSpacing = 7;
    }

    /// <summary>The background palettes that select and dim menu text.</summary>
    public static class TilePalettes
    {
        /// <summary>The palette of the active language or the selected on/off choice.</summary>
        public const int Selected = 0;
        /// <summary>The palette of the inactive language or the unselected on/off choice.</summary>
        public const int Unselected = 1;
    }
}
