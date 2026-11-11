using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>The installed gameplay-HUD document's file name and cell extents, which only the import reads (#1502).</summary>
public static class GameplayHudDocumentDefinitions
{
    /// <summary>Installed JSON filename for the gameplay HUD's artwork and layout.</summary>
    public const string FileName = "gameplay-hud.json";
    /// <summary>Cells across each of the HUD's rows.</summary>
    public const int Columns = 32;
    /// <summary>Rows of cells the HUD's values are drawn over, below the static top row.</summary>
    public const int DrawnRows = 3;
    /// <summary>The HUD's rows of cells: the static top row, then the rows its values are drawn over.</summary>
    public const int Rows = DrawnRows + 1;
    /// <summary>Ninety-six cells in the three rows the HUD's values are drawn over; the static top row is excluded.</summary>
    public const int CellCount = Columns * DrawnRows;
    /// <summary>The drawn rows' size in cells.</summary>
    public static Extent16<CharacterCells> Size => new(Columns, DrawnRows);
    /// <summary>Thirty-two cells in the static minimap-border row.</summary>
    public const int TopRowCellCount = Columns;
}
