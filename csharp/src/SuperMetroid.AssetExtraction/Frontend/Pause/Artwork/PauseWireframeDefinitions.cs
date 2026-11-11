namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>Native pause wireframe patch geometry, independent of inventory selection.</summary>
public static class PauseWireframeDefinitions
{
    /// <summary>The supported editable pause-wireframe JSON schema version.</summary>
    public const int Version = 1;

    /// <summary>The embedded editable pause-wireframe asset file name.</summary>
    public const string FileName = "pause-wireframes.json";

    /// <summary>The four complete Power/Varia and regular/Hi-Jump artwork variants.</summary>
    public const int Count = 4;
    /// <summary>$82:B20C copies seventeen rows of eight words from bank $82.</summary>
    public const int Columns = 8, Rows = 17, Cells = Columns * Rows;
    /// <summary>$82:B20C starts the wireframe at tile (12,7) of the equipment page.</summary>
    public const int FirstPageTile = 7 * PageColumns + 12;
    /// <summary>The tiles in one row of the equipment page.</summary>
    public const int PageColumns = 32;
    /// <summary>The tiles of the equipment page, one 32x32 tilemap.</summary>
    public const int PageTiles = PageColumns * 32;
}
