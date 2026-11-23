namespace SuperMetroid.AssetExtraction.Map.Artwork;

/// <summary>Presentation schema geometry: the editable per-area map documents and the cells they place.</summary>
public static partial class MapPresentationFormat
{
    /// <summary>Supported per-area map JSON schema revision, checked before reading the cells; 2 names each cell by what it shows (#1513).</summary>
    public const int Version = 2;
}
