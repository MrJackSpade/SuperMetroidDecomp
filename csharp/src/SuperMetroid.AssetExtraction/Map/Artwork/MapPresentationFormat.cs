namespace SuperMetroid.AssetExtraction.Map.Artwork;

/// <summary>Presentation schema geometry: the editable per-area map documents and the cells they place.</summary>
public static class MapPresentationFormat
{
    /// <summary>Supported per-area map JSON schema revision, checked before reading the cells; 2 names each cell by what it shows (#1513).</summary>
    public const int Version = 2;
    /// <summary>Number of 8-pixel picture rows in the shared 64-pixel-high map pictures, bounding editable tile-row values.</summary>
    public const int AtlasRows = 8;
    /// <summary>Eight selectable colour variants a document cell may name.</summary>
    public const int PaletteCount = 8;
}
