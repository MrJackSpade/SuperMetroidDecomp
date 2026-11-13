using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The gameplay-HUD documents' file names and cell extents (#1502, #1638): the installed layout the game reads, and the artwork
/// only the import reads, which is import data.
/// </summary>
public static class GameplayHudDocumentDefinitions
{
    /// <summary>Installed JSON filename for the gameplay HUD's layout (<see cref="GameplayHudLayoutDocument"/>).</summary>
    public const string FileName = "gameplay-hud.json";
    /// <summary>Supported gameplay-HUD layout schema revision.</summary>
    public const int LayoutVersion = 1;
    /// <summary>Import-data JSON filename for the gameplay HUD's artwork (<see cref="GameplayHudPresentationDocument"/>).</summary>
    public const string ArtworkFileName = "gameplay-hud-artwork.json";
    /// <summary>Cells across each of the HUD's rows.</summary>
    public const int Columns = 32;
    /// <summary>Rows of cells the HUD's values are drawn over, below the static top row.</summary>
    public const int DrawnRows = 3;
    /// <summary>Ninety-six cells in the three rows the HUD's values are drawn over; the static top row is excluded.</summary>
    public const int CellCount = Columns * DrawnRows;
    /// <summary>The drawn rows' size in cells.</summary>
    public static Extent16<CharacterCells> Size => new(Columns, DrawnRows);
    /// <summary>Thirty-two cells in the static minimap-border row.</summary>
    public const int TopRowCellCount = Columns;
}
