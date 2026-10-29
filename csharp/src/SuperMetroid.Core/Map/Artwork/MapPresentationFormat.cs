namespace SuperMetroid.Core.Map.Artwork;

/// <summary>Presentation schema geometry: the editable per-area map documents and the cells they place.</summary>
public static class MapPresentationFormat
{
    /// <summary>Supported per-area map JSON schema revision, checked before compiling the cells.</summary>
    public const int Version = 1;
    /// <summary>Number of 8-pixel picture columns in the shared map pictures; picture row times 32 plus column names a cell's shape.</summary>
    public const int AtlasColumns = 32;
    /// <summary>Number of 8-pixel picture rows in the shared map pictures, bounding editable tile-row values.</summary>
    public const int AtlasRows = MapCellShape.Count / AtlasColumns;
    /// <summary>Eight selectable colour variants a document cell may name.</summary>
    public const int PaletteCount = 8;
}
